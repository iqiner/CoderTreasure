using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace DebatchControlEmulator
{
    public partial class ContolBox : Form
    {
        private Dictionary<string, bool> lightList = new Dictionary<string, bool>();
        private Timer flashLightTimer = new Timer();
        private SocketListener listener;
        private int columnCount = 5;

        public ContolBox()
        {
            InitializeComponent();
            List<string> lightIDs = ConfigurationManager.AppSettings["Lights"]
                                                        .Split(new char[] { ',' })
                                                        .Distinct()
                                                        .ToList();
            lightIDs.ForEach(lightID =>
            {
                if (!lightList.ContainsKey(lightID))
                {
                    lightList.Add(lightID, false);
                }
            });
            string ip = ConfigurationManager.AppSettings["IP"].ToString();
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"].ToString());
            int count = Convert.ToInt32(ConfigurationManager.AppSettings["ColumnCount"].ToString());
            columnCount = Math.Min(count, lightIDs.Count);

            IPAddress address = IPAddress.Parse(ip);
            listener = new SocketListener(address, port);
            listener.ReceiveSignalEventHandler += listener_ReceiveSignalEventHandler;

            this.flashLightTimer.Interval = 250;
            this.flashLightTimer.Tick += delegate
            {
                foreach (string lightID in lightIDs)
                {
                    bool isON = this.lightList[lightID];
                    Button button = this.lightPanel.Controls.Find("light_" + lightID, true)[0] as Button;
                    Color color = button.BackColor;
                    if (isON)
                    {
                        if (color == Color.Red)
                        {
                            color = Form.DefaultBackColor;
                        }
                        else
                        {
                            color = Color.Red;
                        }
                    }
                    else
                    {
                        color = Form.DefaultBackColor;
                    }
                    button.BackColor = color;
                }
                Application.DoEvents();
            };
            this.flashLightTimer.Start();
        }

        private void listener_ReceiveSignalEventHandler(string realCommand)
        {
            string lightIDArray = CommandHelper.GetLightAddress(realCommand);
            CommandType commandType = CommandHelper.GetCommandType(realCommand);

            if (commandType != CommandType.ZCommand && commandType != CommandType.TCommand && commandType != CommandType.OCommand)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        this.lbMessage.Text = "Accept Command: " + realCommand;
                    }));
                }
            }

            string replyCommand = string.Empty;
            if (commandType == CommandType.TCommand)
            {
                replyCommand = CommandHelper.GetReplyOCommand();
                this.listener.SendReply(replyCommand);
            }
            else if (commandType == CommandType.DCommand)
            {
                string lightIDTemp = lightIDArray;
                while (lightIDTemp.Length > 0)
                {
                    string lightID = lightIDTemp.Substring(0, 4);
                    this.FlashLight(lightID, false);
                    lightIDTemp = lightIDTemp.Substring(4);
                }
            }
            else if (commandType == CommandType.LCommand)
            {
                string lightID = lightIDArray;
                this.FlashLight(lightID, true);
            }
            else if (commandType == CommandType.PCommand)
            {
                string lightIDTemp = lightIDArray;
                while (lightIDTemp.Length > 0)
                {
                    string lightID = lightIDTemp.Substring(0, 9).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    this.FlashLight(lightID, true);
                    lightIDTemp = lightIDTemp.Substring(9);
                }
            }
            else if (commandType == CommandType.ZCommand)
            {
                Sequencer.RestSequence();
            }
        }

        private void ContolBox_Load(object sender, EventArgs e)
        {
            this.PaintLights();
            this.listener.Start();
        }

        private Panel InitLight(string lightID, int width, int height)
        {
            Panel panel = new Panel();
            panel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            panel.Name = "panel_" + lightID;
            panel.Width = width;
            panel.Height = height;
            panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panel.Margin = new System.Windows.Forms.Padding(4,4,0,0);

            TableLayoutPanel table = new TableLayoutPanel();
            table.Name = "table_" + lightID;
            table.Parent = panel;
            table.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            table.Width = width;
            table.Height = height;
            table.RowCount = 3;
            table.ColumnCount = 1;
            table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            table.Margin = new System.Windows.Forms.Padding(0);

            Label label = new Label();
            label.Text = lightID;
            label.Parent = table;
            label.Name = "label_" + lightID;
            label.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            label.Margin = new System.Windows.Forms.Padding(1);

            Button lightButton = new Button();
            lightButton.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lightButton.Name = "light_" + lightID;
            lightButton.Parent = table;
            lightButton.BackColor = Form.DefaultBackColor;
            lightButton.MinimumSize = new Size(0,0);
            lightButton.AutoSize = true;
            lightButton.Margin = new System.Windows.Forms.Padding(0,0,2,0);

            Button confirmButton = new Button();
            confirmButton.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            confirmButton.Parent = table;
            confirmButton.Name = "confirm_" + lightID;
            confirmButton.Text = "Confirm";
            confirmButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            confirmButton.Click += new System.EventHandler(this.confirmButton_Click);
            confirmButton.MinimumSize = new Size(0, 0);
            confirmButton.AutoSize = true;
            confirmButton.Margin = new System.Windows.Forms.Padding(0, 0, 2, 2);

            table.Controls.Add(label);
            table.Controls.Add(lightButton);
            table.Controls.Add(confirmButton);

            return panel;
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            string lightID = (sender as Button).Name.Split('_')[1];
            string replyMessage = CommandHelper.GetReplyTCommand(lightID);
            this.listener.SendReply(replyMessage);
            this.FlashLight(lightID, false);
        }

        private void PaintLights()
        {
            int width = this.LightSize.Width;
            int height = this.LightSize.Height;
            lightList.Keys.ToList().ForEach(lightID =>
            {
                Panel panel = InitLight(lightID, width, height);
                panel.Parent = this.lightPanel;
                this.lightPanel.Controls.Add(panel);
            });
        }

        private Size LightSize
        {
            get
            {
                int row = this.lightList.Keys.Count % columnCount == 0 ? this.lightList.Keys.Count / columnCount : this.lightList.Keys.Count / columnCount + 1;

                int width = (this.lightPanel.Width - (columnCount + 1) * 4) / columnCount;
                int height = (this.lightPanel.Height - (row + 1) * 4) / row;

                return new Size(width, height);
            }
        }

        private void FlashLight(string lightID, bool isON)
        {
            this.lightList[lightID] = isON;
        }

        private void ContolBox_Resize(object sender, EventArgs e)
        {
            int width = this.LightSize.Width;
            int height = this.LightSize.Height;
            lightList.Keys.ToList().ForEach(lightID =>
            {
                Panel panel = this.lightPanel.Controls.Find("panel_" + lightID, true)[0] as Panel;
                panel.Width = width;
                panel.Height = height;
                TableLayoutPanel table = this.lightPanel.Controls.Find("table_" + lightID, true)[0] as TableLayoutPanel;
                table.Width = width;
                table.Height = height;
            });
        }
    }
}

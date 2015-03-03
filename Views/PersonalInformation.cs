using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Models;
using Models.Interfaces;
using Controllers;
using Models.Enums;

namespace Views
{
    public partial class PersonalInformation : Form, IView
    {
        private PersonController m_PersonController;

        public PersonalInformation()
        {
            InitializeComponent();
            InitializeControl();
        }

        public void SetController(IController controller)
        {
            this.m_PersonController = controller as PersonController;
            if (this.m_PersonController == null)
            {
                throw new ArgumentException("Controller not match View.");
            }

            this.BindingModeWithContorl();
        }

        private void BindingModeWithContorl()
        {
            this.txtName.DataBindings.Clear();
            this.txtAge.DataBindings.Clear();
            this.cmbSex.DataBindings.Clear();
            this.txtSalary.DataBindings.Clear();

            this.m_PersonController.InitModel();
            
            this.txtName.DataBindings.Add("Text", this.m_PersonController.Model, "Name", true, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            this.txtAge.DataBindings.Add("Text", this.m_PersonController.Model, "Age", true, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            this.cmbSex.DataBindings.Add("Text", this.m_PersonController.Model, "Sex", true, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            this.txtSalary.DataBindings.Add("Text", this.m_PersonController.Model, "Salary", true, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
        }

        private void InitializeControl()
        {
            this.cmbSex.DataSource = Enum.GetValues(typeof(HumanSex));
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            int id = 0;
            if (!int.TryParse(this.txtSearch.Text.Trim(), out id) || id <= 0)
            {
                MessageBox.Show("Please input a valid ID.");
                this.BindingModeWithContorl();
                return;
            }
            try
            {
                this.m_PersonController.LoadPerson(id);
                this.txtSearch.Text = string.Empty;
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message);
                this.BindingModeWithContorl();
                this.txtSearch.SelectAll();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                this.m_PersonController.SavePerson();
                MessageBox.Show("Save Success.");
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                this.m_PersonController.UpdatePerson();
                MessageBox.Show("Update Success.");
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                this.m_PersonController.DeletePerson();
                MessageBox.Show("Delete Success.");
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.BindingModeWithContorl();
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.btnSearch.PerformClick();
            }
        }
    }
}

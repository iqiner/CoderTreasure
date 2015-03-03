using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Export
{
    class Program
    {
        void Main()
        {
            string path = @"D:/ABC/DGM_PTST_LKPTBLE.csv";
            using (StreamReader sr = File.OpenText(path))
            {
                string baseFileName = "DHLGMSortCodeMatrixData_";
                sr.ReadLine();
                int count = 0;
                int fileNumber = 1;
                StringBuilder sBuilder = new StringBuilder(30000);
                string baseDirectory = @"D:\rc50\CLR\21839\InserData\";
                while (!sr.EndOfStream)
                {
                    string[] sArr = sr.ReadLine().Split(',');
                    sBuilder.AppendFormat("INSERT INTO dbo.[DHLGMSortCodeMatrix]([Location],[Product],[MailType],[Dest_ZIP5],[E1_PrimaryOutBound],[E2_PrimaryInbound],[E3_ZIP5],[E4_DestinationTerminal],[E5_MailType],[E6_SortCodeVersion]) VALUES('{0}',{1},{2},'{3}','{4}','{5}','{6}','{7}',{8},{9})", sArr[0], sArr[1], sArr[2], sArr[3], sArr[4], sArr[5], sArr[6], sArr[7], sArr[8], sArr[9]).AppendLine();
                    count++;
                    if (count >= 5000)
                    {
                        count = 0;
                        using (StreamWriter sw = File.CreateText(baseDirectory + baseFileName + fileNumber.ToString("00") + ".sql"))
                        {
                            sw.Write("USE [fedex]\r\nGO\r\n" + sBuilder.ToString());
                            fileNumber++;
                        }
                        sBuilder.Remove(0, sBuilder.Length);
                    }
                }
                if (count > 0)
                {
                    using (StreamWriter sw = File.CreateText(baseDirectory + baseFileName + fileNumber.ToString("00") + ".sql"))
                    {
                        sw.Write("USE [fedex]\r\nGO\r\n" + sBuilder.ToString());
                        fileNumber++;
                    }
                    sBuilder.Remove(0, sBuilder.Length);
                }

            }
        }

    }
}

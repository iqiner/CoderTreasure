using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ExportFromFile
{

    class Program
    {
        static void Main(string[] args)
        {
            //string sourcePathForOldVersion = @"C:\Documents and Settings\sd45\Desktop\old.csv";
            string sourcePathForNewVersion = @"C:\Documents and Settings\sd45\Desktop\new.csv";
            string destinationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Exports\";

            #region Read Source Data

            //List<Entity> oldList = ReadFile(sourcePathForOldVersion);
            List<Entity> newList = ReadFile(sourcePathForNewVersion);
            //List<Entity> resultListPart_I = new List<Entity>();
            //List<Entity> resultListPart_II = new List<Entity>();

            //newList.ForEach(entity =>
            //{
            //    if (oldList.Find(item => item.ZipCode == entity.ZipCode && item.SortCode == entity.SortCode && item.GroundZone == entity.GroundZone && item.GroundTransitDays == entity.GroundTransitDays) != null)
            //    {
            //        resultListPart_I.Add(entity);
            //    }
            //    else
            //    {
            //        resultListPart_II.Add(entity);
            //    }
            //});

            #endregion

            #region Save To File

            //SaveFile(destinationDirectory, oldList, "Old");
            //SaveFile(destinationDirectory, newList, "New");
            //SaveFile(destinationDirectory, resultListPart_I, "New_I");
            //SaveFile(destinationDirectory, resultListPart_II, "New_II");

            #endregion

            #region Generate XML for connectship

            GenerateMiscData(destinationDirectory, newList,"MiscData");
            
            #endregion


        }

        private static void GenerateMiscData(string destinationDirectory, List<Entity> newList,string fileName)
        {
            MiscData MiscData = new MiscData();
            MiscData.SourceList = newList;
            string xmlstring = MiscData.ToString();
            using (StreamWriter writer = new StreamWriter(destinationDirectory + fileName + ".xml"))
            {
                writer.Write(xmlstring);
            }
        }

        private static void SaveFile(string destinationDirectory, List<Entity> entityList, string fileName)
        {
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (Entity entity in entityList)
            {
                i++;
                string sql = string.Format("Insert dbo.OZZO_OnTracZoneAndTransitDay(ZipCode,SortCode,GroundZone,GroundTransitDays,InDate,InUser,LastEditDate,LastEditUser) values ('{0}','{1}',{2},{3},GETDATE(),'SD45',GETDATE(),'SD45')", entity.ZipCode, entity.SortCode, entity.GroundZone, entity.GroundTransitDays);
                if (i % 5000 == 0)
                {
                    using (StreamWriter writer = new StreamWriter(destinationDirectory + fileName + "_" + i / 5000 + ".sql"))
                    {
                        writer.Write(sb.ToString());
                    }
                    sb.Length = 0;
                }
                sb.AppendLine(sql);
            }
            if (sb.Length > 0)
            {
                using (StreamWriter writer = new StreamWriter(destinationDirectory + fileName + "_" + ((i / 5000) + 1) + ".sql"))
                {
                    writer.Write(sb.ToString());
                }
            }
        }

        private static List<Entity> ReadFile(string sourcePath)
        {
            List<Entity> list = new List<Entity>();

            using (StreamReader streamReader = new StreamReader(sourcePath))
            {
                string line;
                int rowID = 0;
                while (!String.IsNullOrEmpty(line = streamReader.ReadLine()))
                {
                    if (rowID == 0)
                    {
                        rowID++;
                        continue;
                    }
                    string[] fields = line.Split(',');
                    string zipCode = fields[0];
                    string sortCode = fields[1];
                    int groundZone = Convert.ToInt32(fields[4]);
                    int groundTransitDays = Convert.ToInt32(fields[7]);
                    Entity entity = new Entity { ZipCode = zipCode, SortCode = sortCode, GroundZone = groundZone, GroundTransitDays = groundTransitDays };
                    list.Add(entity);
                    rowID++;
                }
            }
            return list;
        }
    }
}

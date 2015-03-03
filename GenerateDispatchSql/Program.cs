using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GenerateDispatchSql
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("PLEASE INPUT WAREHOUSE NUMBER, SPLITE BY COMMA. FOR EXAMPLE: 07,08: ");
                Console.ForegroundColor = ConsoleColor.White;
                string inputString = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Green;

                List<string> warehouseNumberList = inputString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                List<CatalogBase> catalogInstanceList = new List<CatalogBase>();
                warehouseNumberList.ForEach(WHNumber =>
                {
                    try
                    {
                        catalogInstanceList.Add(CatalogInstanceFactory.GetInstance(WHNumber));
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.Message);
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                });
                if (catalogInstanceList.Count > 0)
                {
                    string path = Utils.GenerateForms("0000", catalogInstanceList.ToArray());
                    Console.WriteLine("Success! " + path);
                }

                Console.Write("EXIT ? Y/N ");
                Console.ForegroundColor = ConsoleColor.White;
                string key = Console.ReadLine().ToUpper();
                Console.ForegroundColor = ConsoleColor.Green;
                if (key == "Y")
                {
                    break;
                }
            }
        }
    }
}

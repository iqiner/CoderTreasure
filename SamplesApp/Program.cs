using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Shipping;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Newegg.Oversea.DataAccess;
using NPOI.XSSF;
using NPOI.SS;
using NPOI.HSSF;
using System.Dynamic;

namespace SamplesApp
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> list = new List<int> { 1, 2, 3, 4, 5 };
            List<int> listcopy = list;
            Thread thread = new Thread(new ThreadStart(() => {
                while (true)
                {
                    try
                    {
                        Console.WriteLine(list == listcopy);
                        foreach (var a in listcopy)
                        {
                            Console.WriteLine(list.FirstOrDefault(l => l == 2));
                            Console.WriteLine(list.Count);
                            Console.WriteLine(list == listcopy);
                            //Console.WriteLine("Thread1:"+a);
                            Thread.Sleep(500);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        //System.Diagnostics.Trace.WriteLine(ex.Message);
                    }
                }
            }));
            thread.Start();

            Thread thread2 = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(2000);
                //list.Clear();
                list = new List<int> { 6, 7, 8, 9 };
                foreach (var a in list)
                {
                    Console.WriteLine("Thread2:"+a);
                    
                }
                //list.Clear();
                //list = new List<int> { 2, 2, 8, 9 };
            }));
            thread2.Start();

            Console.ReadKey();

            //dynamic itemEntity = new ItemEntity();
            
            //Console.WriteLine("ItemNumber:" + itemEntity.ItemNumber);
            //Console.WriteLine("PromationPrice:" + itemEntity.PromationPrice);
            //Console.WriteLine("PromationPrice:" + itemEntity.GOOO);
            //Console.ReadKey();
            //var a = new MiscData();
            //var x = a.ToString();
            //using (StreamWriter writer = new StreamWriter(@"C:\Documents and Settings\sd45\Desktop\misc.xml"))
            //{
            //    writer.Write(x);
            //}
            //return;
            //string connStr = @"USER ID=WHDbo;PASSWORD =2Dev4WH;DATA SOURCE =NewSql;INITIAL CATALOG =Newegg";
            /*
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "Insert into newegg.dbo.temp_sure(x) values(@X)";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@X", SqlDbType.Float);
                cmd.Parameters["@X"].Value = 12.235F;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            */

            //DataCommand command = DataCommandManager.GetDataCommand("NewSql.Test");
            ////X entity = new X { x = 12.235F};
            ////command.ExecuteNonQuery(entity);

            //object x = command.ExecuteScalar<string>();
            //Console.WriteLine(x == DBNull.Value );
            //Console.ReadKey();

            //Persons ps = new Persons();
            //ps.PersonList = new List<Person>();

            //ps.PersonList.Add(new Person { Name = "D", Age = 10, Childs = new List<Child> { new Child { Name = "D.c1", Age = 1 }, new Child { Name = "D.c2", Age = 2 } } });
            //ps.PersonList.Add(new Person { Name = "J", Age = 12, Sex = Sex.Female });

            //House house = new House
            //{
            //    //Person = new Person { Name = "D", Age = 10, Childs = new List<Child> { new Child { Name = "D.c1", Age = 1 }, new Child { Name = "D.c2", Age = 2 } } },
            //    Persons = ps
            //};
            //Serialize(house);
        }

        private static void Serialize<T>(T persons)
        {
            string xmlString = String.Empty;
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (Stream stream = new MemoryStream())
            {
                serializer.Serialize(stream, persons);
                stream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(stream))
                {
                    xmlString = reader.ReadToEnd();
                }
            }
            Console.Write(xmlString);

            //XmlSerializer serializer = new XmlSerializer(typeof(Persons));
            FileStream fs = new FileStream("a.xml", FileMode.Create);
            using (fs)
            {
                serializer.Serialize(fs, persons);
            }
        }

        public static void Deserialize()
        {
            XmlSerializer ser =
                new XmlSerializer(typeof(Persons));
            FileStream fs = new FileStream("a.xml", FileMode.Open);
            using (fs)
            {
                Persons p2 = (Persons)ser.Deserialize(fs);
                Console.WriteLine("Count {0}", p2.PersonList.Count);
                foreach (Person de in p2.PersonList)
                {
                    Console.WriteLine("Name {0} Age: {1}", de.Name, de.Age);
                }
            }
            Console.ReadLine();
        }
    }


	public class ItemEntity : System.Dynamic.DynamicObject
	{
		/// <summary>
		/// 可以使用LINQ TO XML或者将XML数据构造在一个指定的容器中，用来判断是否存在
		/// </summary>
		private static Dictionary<string, object> itemPropery = new Dictionary<string, object>();
		static ItemEntity()
		{
			itemPropery.Add("ItemNumber", 1029394);
			itemPropery.Add("PromationPrice", 100);
		}
		public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
		{
			if (itemPropery.ContainsKey(binder.Name))
			{
				result = itemPropery[binder.Name];
				return true;
			}
			result = null;
			return false;
		}
	}



    public class X
    {
        public double x
        {
            get;
            set;
        }
    }

    public static class Extension
    {
        public static T Init<T>(ITest entity, XmlNode node) where T : ITest
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object[] attributies = property.GetCustomAttributes(typeof(XmlAttributeNameAttribute), false);
                string nodeAttributeName = property.Name;
                foreach (Attribute attribute in attributies)
                {
                    if (attribute is XmlAttributeNameAttribute)
                    {
                        if (attribute != null)
                        {
                            XmlAttributeNameAttribute _attr = attribute as XmlAttributeNameAttribute;
                            nodeAttributeName = _attr.Name;
                        }
                    }
                }

                if (node.Attributes[nodeAttributeName] != null)
                {
                    Type targetType = property.PropertyType;
                    if (targetType.IsGenericType)
                    {
                        targetType = targetType.GetGenericArguments()[0];
                    }
                    object value = Convert.ChangeType(node.Attributes[nodeAttributeName].Value, targetType);
                    property.SetValue(entity, value, null);
                }
            }
            return (T)((object)entity);
        }
    }

    public class XmlAttributeNameAttribute : Attribute
    {
        public string Name { get; set; }
    }

    public interface ITest
    {
    }

    public enum Sex
    {
        Male,
        Female
    }

    [Serializable]
    public class Person : ITest
    {
        //[XmlAttributeName(Name = "Names")]
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        //[XmlAttributeName(Name = "Age")]
        [XmlAttribute(AttributeName="Age")]
        public int Age { get; set; }

        [XmlAttribute(AttributeName = "SEX")]
        public Sex Sex { get; set; }

        [XmlAttributeName]
        public List<Child> Childs { get; set; }
    }

    public class Child
    {
        //[XmlAttributeName(Name = "Names")]
        public string Name { get; set; }
        //[XmlAttributeName(Name = "Age")]
        public int? Age { get; set; }
    }

    [XmlRoot(ElementName = "Dropship.dbo.Person")]
    //[Serializable]
    public class Persons
    {
        [XmlElement(ElementName="Person")]
        public List<Person> PersonList { get; set; }
    }

    public class House
    {
        public Person Person { get; set; }
        
        public Persons Persons { get; set; }
    }



    public class Transit
    {
        public int ZipCode { get; set; }
        public int Days { get; set; }
        public int this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return this.ZipCode;
                }
                else if (index == 1)
                {
                    return this.Days;
                }
                else
                {
                    throw new ArgumentException("Can not find the specific property name.");
                }

            }
            set
            {
                if (index == 0)
                {
                    this.ZipCode = value;
                }
                else if (index == 1)
                {
                    this.Days = value;
                }
                else
                {
                    throw new ArgumentException("Can not find the specific property name.");
                }
            }

        }

    }

    public class MiscData
    {
        private const string connectString = @"USER ID=WHDBO;PASSWORD =2Dev4WH;DATA SOURCE =s7sql01;INITIAL CATALOG =codecenter";

        public IList<Transit> GetDataFromDB()
        {
            IList<Transit> list = new List<Transit>();
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = @"select ZipCode,SortCode,GroundZone, GroundTransitDays as Days from codecenter.dbo.OZZO_OnTracZoneAndTransitDay order by zipcode asc";
                cmd.CommandText = @"
select zipcode,GroundTransitDays as Days from codeCenter.dbo.zipcodeforDynamex 
union all
select zipcode,GroundTransitDays as Days from codeCenter.dbo.zipcodeforDynamex1
union all
select zipcode,GroundTransitDays as Days from codeCenter.dbo.zipcodeforDynamex2
order by zipcode asc
                    ";
                cmd.CommandType = CommandType.Text;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Transit { ZipCode = reader.GetInt32(reader.GetOrdinal("ZipCode")), Days = reader.GetInt32(reader.GetOrdinal("Days")) });
                    }
                }
                conn.Close();
            }
            return list;
        }

        public IList<Transit> GetDataFromExcel(string filePath, bool ignoreHeadRow, params int[] selectedColumn)
        {
            IList<Transit> list = new List<Transit>();
            /*
            using (FileStream fs = new FileStream(filePath,FileMode.Open,FileAccess.Read))
            {
                HSSFWorkbook workbook = new HSSFWorkbook(fs);
                
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    ISheet sheet = workbook.GetSheetAt(i);
                    for (int j = 0; j < sheet.LastRowNum; j++)
                    {
                        if (ignoreHeadRow && j == 0)
                        {
                            continue;
                        }

                        IRow row = sheet.GetRow(j);
                        if (row != null)
                        {
                            try
                            {
                                Transit transit = new Transit();
                                for (int k = 0; k < selectedColumn.Length; k++)
                                {
                                    ICell cell = row.GetCell(selectedColumn[k]);
                                    transit[k] = Convert.ToInt32(cell.ToString());
                                }
                            }
                            catch { }
                        }

                    }
                }
            }
            */
            return list.OrderBy(item => item.ZipCode).ToList();
        }

        public string MainTemplate(string transitList, string zoneList)
        {
            string template = @"
<MiscData>
	<configurationList name=""OnTrac"">
		<namepair name=""SATURDAY_DELIVERY"" value=""TRUE"" />
	</configurationList>
	<packagingList name=""OnTrac"">
		<packaging symbol=""CUSTOM"" />
	</packagingList>
	<TnTList name=""OnTrac"">
{0}                     
	</TnTList>
	<zoneList name=""OnTrac"">
		<zone country=""UNITED_STATES"" zoneAssigned=""01"">
{1}			
		</zone>
	</zoneList>
</MiscData>";
            return string.Format(template, transitList, zoneList);
        }

        public string MainTemplate(string zoneList)
        {
            string template = @"
<MiscData>
	<zoneList name=""OnTrac"">
		<zone country=""UNITED_STATES"" zoneAssigned=""01"">
{0}			
		</zone>
	</zoneList>
</MiscData>";
            return string.Format(template, zoneList);
        }

        public string TransitTemplate(int days, int startZipCode, int endZipCode)
        {
            string template = @"        <TimeInTransit country=""UNITED_STATES"" commitment=""{0}"">
			<PostalCode startPostalCode=""{1}"" endPostalCode=""{2}"" />
		</TimeInTransit>";

            return string.Format(template, (TranmitDays)days, startZipCode, endZipCode);
        }

        public string ZoneTemplate(int start, int end)
        {
            string template = @"            <PostalCode startPostalCode=""{0}"" endPostalCode=""{1}""/>";
            return string.Format(template, start.ToString().PadLeft(5, '0'), end.ToString().PadLeft(5, '0'));
        }

        public override string ToString()
        {
            StringBuilder transitList = new StringBuilder();
            StringBuilder zoneList = new StringBuilder();
            //IList<Transit> list = GetDataFromDB();
            IList<Transit> list = GetDataFromExcel(@"C:\Users\sd45\Desktop\Newegg Inc Radius Maps with Zip code list Jan 20 2012.xlsx", true, 0);
            if (list.Count == 0)
            {
                return string.Empty;
            }
            if (list.Count == 1)
            {
                transitList.AppendLine(TransitTemplate(list[0].Days, list[0].ZipCode, list[0].ZipCode));
                zoneList.AppendLine(ZoneTemplate(list[0].ZipCode, list[0].ZipCode));
            }
            else
            {
                int start = list[0].ZipCode;
                int end = start + 1;
                int day = list[0].Days;
                for (int i = 1; i < list.Count; i++)
                {
                    Transit item = list[i];

                    if (item.ZipCode == end && item.Days == day)
                    {
                        end = item.ZipCode + 1;
                        if (i == list.Count - 1)
                        {
                            transitList.AppendLine(TransitTemplate(day, start, end - 1));
                            zoneList.AppendLine(ZoneTemplate(start, end - 1));
                        }
                        continue;
                    }
                    else
                    {
                        transitList.AppendLine(TransitTemplate(day, start, end - 1));
                        zoneList.AppendLine(ZoneTemplate(start, end - 1));
                        start = item.ZipCode;
                        end = item.ZipCode + 1;
                        day = item.Days;
                        if (i == list.Count - 1)
                        {
                            transitList.AppendLine(TransitTemplate(day, start, end - 1));
                            zoneList.AppendLine(ZoneTemplate(start, end - 1));
                        }
                    }
                }
            }

            //return MainTemplate(transitList.ToString(), zoneList.ToString());
            return MainTemplate(zoneList.ToString());
        }
    }

    enum TranmitDays
    {
        NEXTDAY = 1,
        TWODAY = 2
    }

    public class ExportData
    {
        private const string connectString = @"USER ID=WHDBO;PASSWORD =2Dev4WH;DATA SOURCE =s7sql01;INITIAL CATALOG =codecenter";

        public string GenerateScript(string field, string tableFullName)
        {
            string sql = String.Format(@"select {0} from {1} order by zipcode asc", field, tableFullName);
            StringBuilder sb = new StringBuilder();
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string[] fieldName = field.Split(',');
                        string values = "";
                        for (int i = 0; i < fieldName.Length; i++)
                        {
                            object value = reader.GetValue(reader.GetOrdinal(fieldName[i]));
                            if (reader.GetProviderSpecificFieldType(i).FullName == "System.Data.SqlTypes.SqlString")
                            {
                                values += String.Format("'{0}',", value.ToString());
                            }
                            else
                            {
                                values += value.ToString() + ",";
                            }
                        }
                        values = values.TrimEnd(',');
                        string insertSql = string.Format(@"insert {0}({1}) values({2})", tableFullName, field, values);
                        sb.AppendLine(insertSql);
                    }
                }
                conn.Close();
            }
            return sb.ToString();
        }

        public void get()
        {
            string sql = this.GenerateScript("ZipCode,SortCode", "codecenter.dbo.OZZO_OnTracSortCode");
            using (StreamWriter writer = new StreamWriter(@"C:\MiscData\script.sql"))
            {
                writer.Write(sql);
            }
        }
    }
}

#region FileHeader
/********************************************************************************
** Copyright (C) 2010 Newegg. All rights reserved.
**
**
** File Name:           XMLEntity
** Creator:             Sure.J.Deng
** Create date:         11/2/2012 12:57:36 
** CLR Version:         3.5 
** NameSpace:           ExportFromFile 
** Description:         TODO: the class description
** Latest Modifier:     sd45
** Latest Modify date:  11/2/2012 12:57:36 
**
**
** Version number:      1.0.0.0
*********************************************************************************/
#endregion

namespace ExportFromFile
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Entity
    {
        public string ZipCode { get; set; }
        public string SortCode { get; set; }
        public int GroundZone { get; set; }
        public int GroundTransitDays { get; set; }
    }

    public class Transit
    {
        public int ZipCode { get; set; }
        public int Days { get; set; }
        public int GroundZone { get; set; }
    }

    public class MiscData
    {
        public List<Entity> SourceList { get; set; }

        public List<Transit> GetData()
        {
            List<Transit> list= new List<Transit>();
            SourceList.ForEach(item =>
            {
                list.Add(new Transit { ZipCode = Convert.ToInt32(item.ZipCode), Days = item.GroundTransitDays, GroundZone=item.GroundZone });
            });
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
{1}	</zoneList>
</MiscData>";
            return string.Format(template, transitList, zoneList);
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
            return string.Format(template, start, end);
        }

        public string ZoneListTemplate(string country, int zoneID,string zoneList)
        {
            string template = @"		<zone country=""{0}"" zoneAssigned=""{1}"">
{2}		</zone>";
            return string.Format(template,country,zoneID,zoneList);
        }

        public override string ToString()
        {
            
            StringBuilder zoneList = new StringBuilder();
            List<Transit> list = GetData();
            string transitList = this.BuildMiscDate(MiscType.Transit,list);

            list.GroupBy(item => item.GroundZone).ToList().ForEach(g => {
                List<Transit> glist = g.ToList();
                zoneList.AppendLine(this.ZoneListTemplate("UNITED_STATES", g.Key, this.BuildMiscDate(MiscType.Zone, g.ToList())));
            });

            return MainTemplate(transitList, zoneList.ToString());
        }

        public string BuildMiscDate(MiscType type, List<Transit> sourceList)
        {
            StringBuilder result = new StringBuilder();
            if (sourceList.Count == 0)
            {
                return string.Empty;
            }
            if (sourceList.Count == 1)
            {
                if (type == MiscType.Transit)
                {
                    result.AppendLine(TransitTemplate(sourceList[0].Days, sourceList[0].ZipCode, sourceList[0].ZipCode));
                }
                else
                {
                    result.AppendLine(ZoneTemplate(sourceList[0].ZipCode, sourceList[0].ZipCode));
                }
            }
            else
            {
                int start = sourceList[0].ZipCode;
                int end = start + 1;
                int day = sourceList[0].Days;
                for (int i = 1; i < sourceList.Count; i++)
                {
                    Transit item = sourceList[i];

                    if (item.ZipCode == end && item.Days == day)
                    {
                        end = item.ZipCode + 1;
                        if (i == sourceList.Count - 1)
                        {
                            if (type == MiscType.Transit)
                            {
                                result.AppendLine(TransitTemplate(day, start, end - 1));
                            }
                            else
                            {
                                result.AppendLine(ZoneTemplate(start, end - 1));
                            }
                        }
                        continue;
                    }
                    else
                    {
                        if (type == MiscType.Transit)
                        {
                            result.AppendLine(TransitTemplate(day, start, end - 1));
                        }
                        else
                        {
                            result.AppendLine(ZoneTemplate(start, end - 1));
                        }
                        start = item.ZipCode;
                        end = item.ZipCode + 1;
                        day = item.Days;
                        if (i == sourceList.Count - 1)
                        {
                            if (type == MiscType.Transit)
                            {
                                result.AppendLine(TransitTemplate(day, start, end - 1));
                            }
                            else
                            {
                                result.AppendLine(ZoneTemplate(start, end - 1));
                            }
                        }
                    }
                }
            }
            return result.ToString();
        }
    }

    public enum MiscType
    {
        Transit,
        Zone
    }

    public enum TranmitDays
    {
        NEXTDAY = 1,
        TWODAY = 2
    }
}

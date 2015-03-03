using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace Shipping
{
    public class OnTrac
    {
        public static string Request()
        {
            try
            {
                string url = "http://www.shipontrac.net/OnTracAPItest/ShipmentRequest.ashx";

                string postData = GetShipmentRequestXML();

                byte[] postByteData = new UTF8Encoding().GetBytes(postData);
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "text/xml;charset=utf-8";
                request.ContentLength = postByteData.Length;
                request.Proxy = new WebProxy("http://s1firewall:8080/") { UseDefaultCredentials = true, Credentials = new NetworkCredential("sd45","Newegg@317521","abs_corp")};

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postByteData, 0, postByteData.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    const int bufferSize = 1024;
                    byte[] buffer = new byte[bufferSize];
                    StringBuilder sb = new StringBuilder();
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        int count = responseStream.Read(buffer, 0, buffer.Length); ;
                        while (count > 0)
                        {
                            sb.Append(Encoding.UTF8.GetString(buffer,0,count));
                            count = responseStream.Read(buffer, 0, buffer.Length);
                        }                       
                    }

                    return sb.ToString();
                    //string xmlStr = sb.ToString();
                    //XmlDocument xml = new XmlDocument();
                    //xml.LoadXml(xmlStr);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return "";
            }
        }

        private static string GetShipmentRequestXML()
        {
            string requestXML = @"<?xml version=""1.0""?>
                                <OntracShipmentRequest>
                                <account>37</account>
                                <password>testpass</password>
                                <requestReference>test</requestReference>
                                <Package>
                                <PackageData>
                                <UID></UID>
                                <ShipperData>
                                <name>FULFILLMENT CIRCLE</name>
                                <address>343 third street</address>
                                <suite>suite 17</suite>
                                <city>sparks</city>
                                <state>nv</state>
                                <zip>tt</zip>
                                <phone>(415) 350-2608</phone>
                                <contact>Tim</contact>
                                </ShipperData>
                                <DeliveryData>
                                <name>Tom Blangke</name>
                                <address>7703 Apple Valley Drive</address>
                                <address2>Suite 12</address2>
                                <address3></address3>
                                <city>BOTHELL</city>
                                <state>WA</state>
                                <zip>90210</zip>
                                <phone>(415) 372-2608</phone>
                                <contact>Sam</contact>
                                </DeliveryData>
                                <PackageDetail>
                                <shipDate>
                                </shipDate>
                                <reference>10-00</reference>
                                <tracking />
                                <service>C</service>
                                <declared>35</declared>
                                <cod>25</cod>
                                <cod_Type>1</cod_Type>
                                <saturdayDelivery>false</saturdayDelivery>
                                <signatureRqd>false</signatureRqd>
                                <type>P</type>
                                <weight>3</weight>
                                <billTo />
                                <instructions />
                                <shipEmail />
                                <delEmail />
                                <labelType>5</labelType>
                                </PackageDetail>
                                </PackageData>
                                </Package>
                                </OntracShipmentRequest>";
            return requestXML;
        }
    }
}

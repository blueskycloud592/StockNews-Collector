using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

using NewsCollector;

namespace NewsCollector.Service
{
    public class XmlService
    {
        //======================================================
        public static string GetNewsBuildDate(string html)
        {
            if (string.IsNullOrEmpty(html))
                return "";

            //Load xml
            StringBuilder result = new StringBuilder();
            XDocument xdoc = XDocument.Parse(html);
            if (xdoc != null)
            {
                // a)利用email id抓到title.
                var query = (from list in xdoc.Descendants("lastBuildDate")
                             select new 
                             {
                                 Key = list.Name.ToString(),
                                 Value = list.Value,
                             }).ToDictionary(t => t.Key, t => t.Value).FirstOrDefault();
                return query.Value;
            }
            return null;
        }
        //======================================================
        public static List<NewsClass> GetNewsContent(string html)
        {
            /*
            <item>
                <title>
                    <![CDATA[營收：和勤(1586)2月營收1億2777萬元，月增率-14.31%，年增率18.46%]]>
                </title>
                <link>
                    https://tw.rd.yahoo.com/referurl/stock/rssback/*https://tw.finance.yahoo.com/news...
                </link>
                <pubDate>
                    Fri, 06 Mar 2015 06:25:43 GMT
                </pubDate>
                <description>
                    <![CDATA[
                    【財訊快報／編輯部】和勤(1586)自結104年2月營收1億2777萬6000元，月增率-14.31%，年增率...]]>
                </description>
            </item>
            */
            try
            { 
                //Load xml
                StringBuilder result = new StringBuilder();
                XDocument xdoc = XDocument.Parse(html);
                if (xdoc != null)
                {
                    // a)取出所有item並建立資料列表
                    var query = (from list in xdoc.Descendants("item")
                                    select new NewsClass
                                    {
                                        StockNumber = "",
                                        StockName = "",
                                        Title = list.Element("title").Value,
                                        Description = list.Element("description").Value,
                                        Link = list.Element("link").Value,
                                        pubDate = DateTime.Parse(list.Element("pubDate").Value),
                                        saveIt = false,
                                    });


                    return query.ToList();
                }
            }
            catch
            {

            }
            return null;
        }
        //======================================================

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Mail;
using System.Web;
using System.Configuration;
using System.Drawing;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using NewsCollector;

namespace Utility
{
    public class Util
    {     
        //--讀取StockList資料 [SQLite]
        public static DataSet getStockList()
        {
            //---判斷目錄
            if (Directory.Exists("../db3") == false)
                Directory.CreateDirectory("../db3");

            string fileName = "../db3/stockList_news.db3";

            //--檔案不存在,試著新建DB及資料表.[失敗時,回傳null]
            if (File.Exists(fileName) == false)    //****注意!stockList因為有LastUpdate所以不能隨便刪除!!
            {
                string ma = "../db3/ma.db3";
                if (File.Exists(ma) == false)
                    return null;

                //---從最近的ma.db3的stocklist中取出所有股票名單
                SQLiteConnection sqConnection = new SQLiteConnection("Data Source=" + ma);
                //取出StockList資料
                SQLiteDataAdapter myAdapter = new SQLiteDataAdapter(@"SELECT * FROM StockList", sqConnection);
                myAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                DataSet myDataSet = new DataSet();
                myAdapter.Fill(myDataSet, "StockList");
                if (myDataSet.Tables["StockList"].Rows.Count <= 0)
                    return null;

                //===新建stocklist.db3檔
                SQLiteConnection.CreateFile(fileName);
                //----
                DbProviderFactory factory = SQLiteFactory.Instance;
                DbConnection conn = factory.CreateConnection();
                try
                {
                    conn.ConnectionString = "Data Source=" + fileName;
                    conn.Open();
                    //======建立StockList資料表
                    string sql = "create table [StockList] ([StockID] PRIMARY KEY, [StockName], [LastUpdate])";
                    DbCommand cmd = conn.CreateCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    //====查詢並取回資料
                    cmd = conn.CreateCommand();
                    cmd.Connection = conn;
                    for (int i = 0; i < 3; i++)
                        cmd.Parameters.Add(cmd.CreateParameter());
                    //--不用trans的效率極差!
                    DbTransaction trans = conn.BeginTransaction();
                    try
                    {
                        //-----將類別Inert到SQLite中
                        for (int i = 0; i < myDataSet.Tables["StockList"].Rows.Count; i++)
                        {
                            cmd.CommandText = "insert into [StockList] values (?,?,?)";
                            cmd.Parameters[0].Value = myDataSet.Tables["StockList"].Rows[i]["stockid"].ToString();
                            cmd.Parameters[1].Value = myDataSet.Tables["StockList"].Rows[i]["name"].ToString();
                            cmd.Parameters[2].Value = DateTime.Now.AddDays(-3); //先設為3天前
                            cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                    }

                    conn.Close();
                }
                catch
                {
                    conn.Close();
                    File.Delete(fileName);
                    return null;
                }
            }
            //========
            SQLiteConnection sqConnection2 = new SQLiteConnection("Data Source=" + fileName);
            //取出StockList資料
            SQLiteDataAdapter myAdapter2 = new SQLiteDataAdapter(@"SELECT * FROM StockList", sqConnection2);
            myAdapter2.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            DataSet myDataSet2 = new DataSet();
            myAdapter2.Fill(myDataSet2, "StockList");
            if (myDataSet2.Tables["StockList"].Rows.Count <= 0)
                return null;
            return myDataSet2;//.Tables["StockList"];
        }

        //========================================================================
        public static string GetWebresourceFile(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 5000; //5秒
                request.UserAgent = string.Format("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/536.2 (KHTML, like Gecko) Chrome/36.{2}.{0}.{1} Safari/536.{1}", DateTime.Now.Millisecond, DateTime.Now.Second, DateTime.Now.Minute);
                WebResponse response = request.GetResponse();
                Stream resStream = response.GetResponseStream();
                StreamReader sr = new StreamReader(resStream, System.Text.Encoding.Default);//.UTF8);
                string SourceCode = sr.ReadToEnd();
                resStream.Close();
                sr.Close();
                return SourceCode;
            }
            catch
            {
                return "";
            }
        }
        //========================================================================

        public static List<NewsClass> getStockNews(int day=-3)
        {
            //---判斷目錄
            string fileName = string.Format("../db3/news{0}.db3", DateTime.Today.ToString("yyyyMM"));

            //--檔案不存在,試著新建DB及資料表.[失敗時,回傳null]
            if (File.Exists(fileName) == true)    //****注意!stockList因為有LastUpdate所以不能隨便刪除!!
            {
                SQLiteConnection sqConnection2 = new SQLiteConnection("Data Source=" + fileName);
                //取出StockList資料
                SQLiteDataAdapter myAdapter2 = new SQLiteDataAdapter(@"SELECT * FROM StockNews Where [pubDate]>@lastDate order by [StockId] asc, [pubDate] desc", sqConnection2);
                myAdapter2.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                myAdapter2.SelectCommand.Parameters.AddWithValue("@lastDate", DateTime.Today.AddDays(day));
                DataSet myDataSet2 = new DataSet();
                myAdapter2.Fill(myDataSet2, "StockNews");
                if (myDataSet2.Tables["StockNews"].Rows.Count <= 0)
                    return null;
                var newsList = new List<NewsClass>();
                foreach (DataRow row in myDataSet2.Tables["StockNews"].Rows)
                {
                    var item = new NewsClass()
                    {
                        StockNumber = row["StockID"].ToString(),
                        StockName = "",
                        Title = row["Title"].ToString(),
                        Description = row["Description"].ToString(),
                        Link = row["Link"].ToString(),
                        NewsContent = row["NewsContent"].ToString(),
                        pubDate = DateTime.Parse(row["pubDate"].ToString()),
                        FocusScore = byte.Parse(row["FocusScore"].ToString()),
                        GoodScore = byte.Parse(row["GoodScore"].ToString()),
                        BadScore = byte.Parse(row["BadScore"].ToString()),
                        saveIt = true,
                    };
                    newsList.Add(item);
                }

                return newsList;
            }
            return null;
        }

        public static bool SaveNews(List<NewsClass> newslist)
        {
            string fileName = string.Format("../db3/news{0}.db3", DateTime.Today.ToString("yyyyMM"));
            //--檔案不存在,試著新建DB及資料表.[失敗時,回傳null]
            DbProviderFactory factory = SQLiteFactory.Instance;
            if (File.Exists(fileName) == false)    //****注意!stockList因為有LastUpdate所以不能隨便刪除!!
            {
                //===新建stockList_news.db3檔
                SQLiteConnection.CreateFile(fileName);
                //----
                DbConnection conn = factory.CreateConnection();
                try
                {
                    conn.ConnectionString = "Data Source=" + fileName;
                    conn.Open();
                    //======建立StockList資料表
                    string sql = "create table [StockNews] ([StockID], [Title], [Description], [Link], [NewsContent], [pubDate], [FocusScore], [GoodScore], [BadScore])";
                    DbCommand cmd = conn.CreateCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
                catch
                {

                }
                conn.Close();
            }
            //======把資料丟進去~
            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = "Data Source=" + fileName;
                conn.Open();
                //--
                DbCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                for (int i = 0; i < 9; i++)
                    cmd.Parameters.Add(cmd.CreateParameter());
                //--不用trans的效率極差!
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    //-----將類別Inert到SQLite中
                    foreach (var news in newslist)                    
                    {
                        if (news.saveIt == false)
                            continue;
                        //---
                        cmd.CommandText = "insert into [StockNews] values (?,?,?,?,?,?,?,?,?)";
                        cmd.Parameters[0].Value = news.StockNumber;
                        cmd.Parameters[1].Value = news.Title;
                        cmd.Parameters[2].Value = news.Description;
                        cmd.Parameters[3].Value = news.Link;
                        cmd.Parameters[4].Value = news.NewsContent;
                        cmd.Parameters[5].Value = news.pubDate;
                        cmd.Parameters[6].Value = news.FocusScore;
                        cmd.Parameters[7].Value = news.GoodScore;
                        cmd.Parameters[8].Value = news.BadScore;
                        cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                }
                conn.Close();
            }

            return true;
            
            ///保留//////////////////////////////////////////////////
            //b)利用 https://tw.finance.yahoo.com/rss/s/2448 抓個股新聞
            //分析後：再用LastUpdate > 最近更新時間,來判斷是否有新News?
            //沒就不處理,有則抓取並記錄到DB中(先用一月一檔試試)

            //存到news201503.db中,並更新update.db的時間
            //news要記錄:時間,股號名稱,主題,詳細URL,並建一個簡單的label字串來做索引
        }
        //---更新News最後的儲存時間 (全部都會更新)
        public static bool UpdateLastDate(DateTime lastTime)
        {
            string fileName = "../db3/stockList_news.db3";
            if (File.Exists(fileName) == false)
                return false;
            //========
            DbProviderFactory factory = SQLiteFactory.Instance;
            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = "Data Source=" + fileName;
                conn.Open();
                //--
                DbCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                for (int i = 0; i < 2; i++)
                    cmd.Parameters.Add(cmd.CreateParameter());
                //--不用trans的效率極差!
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    cmd.CommandText = "update [StockList] SET [LastUpdate] = (?)";// WHERE [StockID] = (?)";
                    cmd.Parameters[0].Value = lastTime;
                    //cmd.Parameters[1].Value = stockid;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                }
                catch//(Exception ex)
                {
                    trans.Rollback();
                }
                conn.Close();
            }
            return true;
        }
        public static bool UpdateDataset(DataSet stockDS)
        {
            string fileName = "../db3/stockList_news.db3";
            if (File.Exists(fileName) == false)
                return false;
            //========
            SQLiteConnection sqConnection = new SQLiteConnection("Data Source=" + fileName);
            //取出StockList資料
            SQLiteDataAdapter myAdapter = new SQLiteDataAdapter(@"SELECT * FROM StockList", sqConnection);
            SQLiteCommandBuilder mycommandbuilder = new SQLiteCommandBuilder(myAdapter);
            myAdapter.Update(stockDS, "StockList");
            return true;
        }

        //--簡單檢查連線狀況
        public static bool ConnectionExists()
        {
            try
            {
                System.Net.Sockets.TcpClient clnt = new System.Net.Sockets.TcpClient("mis.twse.com.tw", 80);
                clnt.Close();
                return true;
            }
            catch //(System.Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 修改配置文件中某項的值
        /// </summary>
        public static void SetConfig(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[key] != null)
                config.AppSettings.Settings[key].Value = value;
            else
                config.AppSettings.Settings.Add(key, value);

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        /// <summary>
        /// 讀取配置文件某項的值
        /// </summary>
        public static string GetConfig(string key)
        {
            string _value = string.Empty;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[key] != null)
            {
                _value = config.AppSettings.Settings[key].Value;
            }
            return _value;
        }        

        #region StockClass & 持股
        public static List<StockClass> readStockClass()
        {
            string fileName = "../db3/stock.db3";
            if (File.Exists(fileName) == false)
                return null;
            //========讀取db內容
            DataSet myDataSet = new DataSet();
            try
            {
                //--開啟SQLie => 讀取Class資料
                SQLiteConnection sqConnection = new SQLiteConnection("Data Source=" + fileName);
                //---
                SQLiteDataAdapter myAdapter = new SQLiteDataAdapter(
                            @"SELECT * FROM StockClass ORDER BY SC_Sort ASC", sqConnection);
                myAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                myAdapter.Fill(myDataSet, "Class");
                if (myDataSet.Tables["Class"].Rows.Count <= 0)
                    return null;
                //--取得Class的StockList
                myAdapter = new SQLiteDataAdapter(
                            @"SELECT * FROM StockList ORDER BY Sort,ClassID ASC", sqConnection);
                myAdapter.Fill(myDataSet, "List");
            }
            catch
            {
                return null;
            }
            //---將DataSet的資料轉為List資料.
            List<StockClass> stockClass = new List<StockClass>();
            for (int i = 0; i < myDataSet.Tables["Class"].Rows.Count; i++)
            {
                StockClass Class = new StockClass();
                Class.ID = int.Parse(myDataSet.Tables["Class"].Rows[i]["SC_Sort"].ToString()); //--用sort當ID.
                Class.Name = myDataSet.Tables["Class"].Rows[i]["SC_Name"].ToString();
                Class.Note = myDataSet.Tables["Class"].Rows[i]["SC_Note"].ToString();
                Class.stockList = new List<StockList>();
                if (myDataSet.Tables["List"].Rows.Count > 0)
                {
                    for (int j = 0; j < myDataSet.Tables["List"].Rows.Count; j++)
                    {
                        if ( myDataSet.Tables["List"].Rows[j]["ClassID"].ToString() == myDataSet.Tables["Class"].Rows[i]["SC_SORT"].ToString() )
                        {
                            Class.stockList.Add(new StockList
                            {
                                Name = myDataSet.Tables["List"].Rows[j]["StockName"].ToString(),
                                Number = myDataSet.Tables["List"].Rows[j]["StockID"].ToString()
                            });
                        }
                    }
                }
                stockClass.Add(Class);
            }
            return stockClass;
        }
        public static List<MyStock> readMyStock()
        {
            string fileName = "../db3/mystock.db3";
            if (File.Exists(fileName) == false)
                return null;
            //========讀取db內容
            DataSet myDataSet = new DataSet();
            try
            {
                //--開啟SQLie => 讀取Class資料
                SQLiteConnection sqConnection = new SQLiteConnection("Data Source=" + fileName);
                //---
                SQLiteDataAdapter myAdapter = new SQLiteDataAdapter(
                            @"SELECT * FROM MyStock", sqConnection); //不排序...(越上面代表持股越久)
                //@"SELECT * FROM MyStock ORDER BY stockid ASC", sqConnection);
                myAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                myAdapter.Fill(myDataSet, "MyStock");
                if (myDataSet.Tables["MyStock"].Rows.Count <= 0)
                    return null;
            }
            catch
            {
                return null;
            }
            //---將DataSet的資料轉為List資料.
            List<MyStock> myStock = new List<MyStock>();
            for (int i = 0; i < myDataSet.Tables["MyStock"].Rows.Count; i++)
            {
                MyStock mylist = new MyStock();
                mylist.stockid = myDataSet.Tables["MyStock"].Rows[i]["stockid"].ToString();
                mylist.buymode = bool.Parse(myDataSet.Tables["MyStock"].Rows[i]["buymode"].ToString());
                mylist.snum = int.Parse(myDataSet.Tables["MyStock"].Rows[i]["snum"].ToString());
                mylist.inprice = float.Parse(myDataSet.Tables["MyStock"].Rows[i]["inprice"].ToString());
                mylist.outprice = float.Parse(myDataSet.Tables["MyStock"].Rows[i]["outprice"].ToString());
                mylist.outmode = int.Parse(myDataSet.Tables["MyStock"].Rows[i]["outmode"].ToString());
                mylist.note = myDataSet.Tables["MyStock"].Rows[i]["note"].ToString();
                myStock.Add(mylist);
            }
            return myStock;
        }
        #endregion
    }
}

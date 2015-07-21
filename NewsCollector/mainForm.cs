using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Utility;
using NewsCollector.Service;

namespace NewsCollector
{
    public partial class mainForm : Form
    {
        private BackgroundWorker newsWorker;

        private System.Windows.Forms.Timer PollingClock = null;

        private List<NewsClass> allNewsList = null;
        private List<StockClass> stockClass = null;
        private List<MyStock> myStock = null;

        //==========
        #region Init, Load, Timer
        public mainForm()
        {
            InitializeComponent();
        }
        private void mainForm_Load(object sender, EventArgs e) 
        {
            cb_score.SelectedIndex = 1; //score>=1
            cb_loadday.SelectedIndex = 1; //3天
            cb_maingroup.SelectedIndex = 3; //持股+前2Group

            tb_keyword.Text = Util.GetConfig("Keywords").Trim();

            //---多緒行緒元件 
            newsWorker = new BackgroundWorker();
            newsWorker.WorkerReportsProgress = true;
            newsWorker.WorkerSupportsCancellation = true;
            newsWorker.DoWork += new DoWorkEventHandler(newsWorker_Run);
            newsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(newsWorker_RunWorkerCompleted);
            //更新UI => 由ReportProgress()引發
            newsWorker.ProgressChanged += new ProgressChangedEventHandler(newsWorker_ProgressChanged);

            //---Timer
            PollingClock = new System.Windows.Forms.Timer();
            PollingClock.Interval = 600000; //每10分觸發一次(600秒).
            PollingClock.Tick += new EventHandler(PollingEvent);

            //---
            allNewsList = new List<NewsClass>();
            stockClass = Util.readStockClass();
            myStock = Util.readMyStock();

            if(myStock==null && stockClass==null)
            {
                cb_maingroup.Enabled = false;
            }
        }
        //---------
        private void mainForm_Closing(object sender, FormClosingEventArgs e) 
        {
            if(MessageBox.Show("確定關閉?", this.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }
        
        //---------
        private void PollingEvent(object sender, System.EventArgs e)
        {
            //--600秒Timer
            if (PollingClock != null && btn_load.Enabled == true && btn_go.Enabled==true)
            {
                PollingClock.Stop();
                btn_load.Enabled = false;
                btn_go.Enabled = false;
                //---Thread
                newsWorker.RunWorkerAsync();
            }
        }
        #endregion

        #region 多緒行緒元件: BackWorker
        void newsWorker_Run(object sender, DoWorkEventArgs e)
        {
           //--檢查連線
            if(Util.ConnectionExists()==false)
            {
                newsWorker.ReportProgress(0, "沒有網路連線...?!");
                return;
            }
            //取得個股名單+最近更新時間(不存在則建立)
            //a)從最近的ma.db取得名單,並建立另一個update.db檔(記ALL Stocks名單+每支最後的更新時間)
            //  **注意!stockList因為有LastUpdate所以不能隨便刪除!!
            var Stock_DS = Util.getStockList();
            var StockList = Stock_DS.Tables["StockList"];
            if (StockList == null || StockList.Rows.Count <= 0)
            {
                btn_go.Enabled = true;
                return; //沒資料
            }
            var ignoreHeader = Util.GetConfig("IgnoreHeader").Split(',');
            
            //======依Stock下載
            List<NewsClass> newsBuck = new List<NewsClass>();
            int total_news = 0, failcount=0;
            for (int i = 0; i < StockList.Rows.Count; i++)
            {
                string stockid = StockList.Rows[i]["StockID"].ToString();
                int number = 0;
                if (int.TryParse(stockid, out number) == false || number < 1000)
                {
                    continue;
                }
                //--2015~2016~2017...
                if (stockid == DateTime.Today.Year.ToString())
                {
                    continue;
                }
                DateTime lastUpdate = DateTime.Parse(StockList.Rows[i]["LastUpdate"].ToString());
                if(lastUpdate.AddMinutes(15) > DateTime.Now)
                {
                    continue; //在15分內有更新過就pass (雖然每10分會Polling檢查一次)
                }                
                //====測試RSS分析
                string html = Util.GetWebresourceFile("https://tw.finance.yahoo.com/rss/s/" + stockid);

                var lastbuild = XmlService.GetNewsBuildDate(html);
                DateTime lastBuildDate = DateTime.Now;
                if (string.IsNullOrEmpty(lastbuild) || !DateTime.TryParse(lastbuild, out lastBuildDate))
                {
                    failcount++;//失敗
                    continue;
                }
                //---lastBuildDate=RSS產生時間 > Rows=程式最後處理時間
                if (lastBuildDate < lastUpdate)
                {
                    continue; //判斷是否有新RSS? 
                }
                //-----取回一連串Items (List<NewsClass>)
                var newslist = XmlService.GetNewsContent(html);
                if (newslist == null || newslist.Count <= 0)
                {
                    failcount++;//失敗或沒items
                    continue;
                }
                //******
                var ignoreStr = Util.GetConfig("Ignore");
                var focusStr = Util.GetConfig("Focus");
                var goodStr = Util.GetConfig("Good");
                var badStr = Util.GetConfig("Bad");
                foreach (var item in newslist)
                {
                    //old news
                    if (item.pubDate <= lastUpdate)
                        continue;
                    item.StockNumber = StockList.Rows[i]["StockID"].ToString();
                    item.StockName = StockList.Rows[i]["StockName"].ToString();

                    //***分析&記錄區***
                    //利用Ignore把不重要的內容過濾掉.
                    if (HowMuchKeywords(item, ignoreStr) > 0)
                    {
                        continue;
                    }
                    //---去掉特定的(無意義)開頭詞
                    if (ignoreHeader.Length > 0)
                    {
                        foreach (var ighstr in ignoreHeader)
                        {
                            int index = item.Title.IndexOf(ighstr, 0);
                            if (index >= 0)
                            {
                                item.Title = item.Title.Substring(index+ighstr.Length);
                                break;
                            }
                        }
                    }

                    //利用Good&Bad關鍵字,為新聞做個重要性評分
                    item.FocusScore = (byte)HowMuchKeywords(item, focusStr);
                    item.GoodScore = (byte)HowMuchKeywords(item, goodStr);
                    item.BadScore = (byte)HowMuchKeywords(item, badStr);
                    //夠重要的新聞?!
                    int score = item.FocusScore + item.GoodScore + item.BadScore;
                    if (score > 0)
                    {
                        //利用item.Link下載詳情並分析?!//string newshtml = Util.GetWebresourceFile(item.Link);

                        //*******
                    }
                    if (!item.Title.Contains(item.StockName) && !item.Description.Contains(item.StockName) && score < 2)
                    {
                        continue;//item.FocusScore++; //---Title有帶股名+1基本分(算容易)
                    }
                    //---也簡單秀到listview中.
                    //***在stockeyes中,則讀取news201503.db的個股新聞,並秀到視窗中.
                    if (score > 0)
                    {
                        newsWorker.ReportProgress(1, item);  
                    }
                    newsBuck.Add(item);
                    allNewsList.Add(item);
                    total_news++;
                    item.saveIt = true;
                }
                //===(更新:個股在stockList.db3的LastUpdate)
                StockList.Rows[i]["LastUpdate"] = lastBuildDate;
                if(lastBuildDate.AddDays(2) < DateTime.Today)
                {
                    StockList.Rows[i]["LastUpdate"] = lastBuildDate.AddDays(1);
                }
                //Util.SaveNews(newslist); //停用
            }
            if (total_news > 0)
            {
                //===批次儲存(全處理完再寫入DB3,減少寫檔次數)
                Util.SaveNews(newsBuck);      
                Util.UpdateDataset(Stock_DS); //批次回寫(lastUpdateDate)

                newsWorker.ReportProgress(0, string.Format("更新:{0} 條(-{1})", total_news, failcount));
            }
        }
        void newsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btn_load.Enabled = true;
            btn_go.Enabled = true;

            //--600秒Timer
            if (PollingClock != null)
            {
                PollingClock.Start(); //背景處理完才Run Timer(才不會因處理超過10分而重複觸發的問題)
            }

            /*//loop 
            if (sender != null)
            {
                ((BackgroundWorker)sender).RunWorkerAsync(); 
            }
            */
        }
        void newsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {            
            if(e.ProgressPercentage==0)
            {
                SetListViewMsg((string)e.UserState);
            }
            if(e.ProgressPercentage==1)
            {
                SetListViewNews((NewsClass)e.UserState);
            }

            //參數範例
            //newsWorker.ReportProgress(1,xx)  
            // newsWorker.ReportProgress(1,new string[] {xx,xx})  
            //string[] str = (string[])e.UserState

        }
        #endregion

        #region 處理區
        private void loadNews()
        {
            int score = cb_score.SelectedIndex;
            if (score < 0)
                return;

            allNewsList = new List<NewsClass>();
            //----------
            listView_News.Items.Clear();
            //----------------載入過去3天or一個月的新聞
            var Stock_DS = Util.getStockList();
            var StockList = Stock_DS.Tables["StockList"];
            if (StockList == null || StockList.Rows.Count <= 0)
                return; //沒資料

            int day = getLoadDays();
            var newsList = Util.getStockNews(day);
            if (newsList == null || newsList.Count <= 0)
                return;

            string keyword = tb_keyword.Text.Trim();

            Util.SetConfig("Keywords",keyword); //save

            //---加上skip時,一支股只會出現一條新聞(過濾keyword太多相似新聞:例如營收)
            bool skipSameId = cb_skip.Checked;

            //輸出
            int totalcount = 0;
            string stockid = "0050";
            foreach (var news in newsList)
            {
                if (cb_maingroup.SelectedIndex > 0)
                {
                    if (!isMyStock(news.StockNumber) && !isMainGroup(news.StockNumber,cb_maingroup.SelectedIndex-1))
                        continue;
                }

                DataRow[] row = StockList.Select(string.Format("Stockid='{0}'", news.StockNumber));
                if (row.Length == 1)
                {
                    news.StockName = row[0]["StockName"].ToString();
                }
                if (skipSameId && stockid == news.StockNumber )
                {
                    if( news.Title.Contains(news.StockName) || stockid==DateTime.Today.Year.ToString())
                    {
                        continue;
                    }
                }
                //--過濾score
                if ((news.FocusScore + news.GoodScore + news.BadScore) < score)
                    continue;
                //--過濾keyword
                if (!string.IsNullOrEmpty(keyword))
                {
                    bool passit = false;
                    var keywords = keyword.Split(',');
                    foreach(var key in keywords)
                    {
                        if (key.Contains("月增") || key.Contains("年增")) //Command不處理
                        {
                            if (EarningUP(news.Title, key) == false)
                            {
                                passit = true;
                                break;
                            }
                        }

                        //---------------
                        if (!news.Title.Contains(key.Trim()) && !news.Description.Contains(key.Trim()))
                        {
                            passit = true;
                            break;
                        }                        
                    }
                    if (passit)
                    {
                        continue;
                    }
                }
                allNewsList.Add(news);
                SetListViewNews(news);
                stockid = news.StockNumber;
                totalcount++;
            }
            SetListViewMsg("Total:" + totalcount);
        }

        private void showNews()
        {
            int score = cb_score.SelectedIndex;
            if (score < 0)
                return;

            //----------
            listView_News.Items.Clear();
            if (allNewsList == null || allNewsList.Count <= 0)
                return;

            string keyword = tb_keyword.Text.Trim();

            //---加上skip時,一支股只會出現一條新聞(過濾keyword太多相似新聞:例如營收)
            bool skipSameId = cb_skip.Checked;

            //輸出
            int totalcount = 0;
            string stockid = "0050";
            foreach (var news in allNewsList)
            {
                if (cb_maingroup.SelectedIndex > 0)
                {
                    if (!isMyStock(news.StockNumber) && !isMainGroup(news.StockNumber, cb_maingroup.SelectedIndex - 1))
                        continue;
                }
                if (skipSameId && stockid == news.StockNumber)
                {
                    if (news.Title.Contains(news.StockName) || stockid == DateTime.Today.Year.ToString())
                    {
                        continue;
                    }
                }
                //--過濾score
                if ((news.FocusScore + news.GoodScore + news.BadScore) < score)
                    continue;
                //--過濾keyword
                if (!string.IsNullOrEmpty(keyword))
                {
                    bool passit = false;
                    var keywords = keyword.Split(',');
                    foreach (var key in keywords)
                    {
                        if (key.Contains("skip")) //Command不處理
                            continue;
                        //---------------
                        if (!news.Title.Contains(key.Trim()) && !news.Description.Contains(key.Trim()))
                        {
                            passit = true;
                            break;
                        }
                    }
                    if (passit)
                    {
                        continue;
                    }
                }
                SetListViewNews(news);
                stockid = news.StockNumber;
                totalcount++;
            }
            SetListViewMsg("Total:" + totalcount);
        }
        private int getLoadDays()
        {
            switch(cb_loadday.SelectedIndex)
            {
                case 0: //今天
                    return 0;
                case 1: //昨天
                    return -1;
                case 2:
                    return -2;
                case 3: //昨天
                    return -10;
                default:
                    return -30;
            }

        }

        private int HowMuchKeywords(NewsClass news, string keyword)
        {
            int count = 0;
            if(!string.IsNullOrEmpty(keyword))
            {
                var keywords = keyword.Split(',');
                foreach(var item in keywords)
                {
                    //---特殊word: 有些news會寫月增(率)-xx%,這不能加到Good中.(判為不合格)
                    if (item.Contains("月增") || item.Contains("年增"))
                    {
                        //--月增年增:加判斷"率"這個字,而且必接+數字.
                        if (EarningUP(news.Title, item) == true)
                        {
                            count++;
                        }
                    }
                    else
                    {
                        if (news.Title.Contains(item) == true || news.Description.Contains(item) == true)
                        {
                            count++; //每個字,在title或desc中只統計一次.
                        }
                    }
                }
            }            
            return count;
        }

        private bool EarningUP(string item, string keyword)
        {
            //--月增年增:加判斷"率"這個字,而且必接+數字才會是true.
            int startindex = item.IndexOf(keyword);
            if(startindex > 0)
            {
                startindex+=keyword.Length;
                if (keyword.Contains("率")==false && item.Contains(keyword + "率") == true)
                {
                    startindex++; //3個字
                }
                if (item.Length <= startindex)
                    return false;
                char word = item[startindex];
                if (word > 48 && word <= 57) //1~9 不看0
                {
                    return true;
                }
            }
            return false;
        }

        private bool isMyStock(string number)
        {
            if (myStock == null || myStock.Count <= 0)
                return false;
            if (myStock.FirstOrDefault(t => t.stockid==number) == null)
                return false;

            return true;
        }
        private bool isMainGroup(string number, int mode)
        {
            if (mode <= 0)
                return false;
            if (stockClass == null || stockClass.Count < 3)
                return false;
            if (mode >= 1)
            {
                if (stockClass[0].stockList == null || stockClass[0].stockList.Count <= 0)
                    return false;
                if (stockClass[0].stockList.FirstOrDefault(t => t.Number == number) != null)
                    return true;
            }
            if (mode >= 2)
            {
                if (stockClass[1].stockList == null || stockClass[1].stockList.Count <= 0)
                    return false;
                if (stockClass[1].stockList.FirstOrDefault(t => t.Number == number) != null)
                    return true;
            }
            if (mode >= 3)
            {
                if (stockClass[2].stockList == null || stockClass[2].stockList.Count <= 0)
                    return false;
                if (stockClass[2].stockList.FirstOrDefault(t => t.Number == number) != null)
                    return true;
            }

            return false;
        }

        #endregion

        #region 輸出UI、按鈕Click
        public void SetListViewMsg(string str)
        {
            ListViewItem item = new ListViewItem(DateTime.Now.ToString("MM/dd HH:mm:ss"));
            item.UseItemStyleForSubItems = false;
            item.SubItems.Add("-");
            item.SubItems.Add(str);
            listView_News.Items.Add(item);
            listView_News.EnsureVisible(listView_News.Items.Count - 1);
        }
        public void SetListViewNews(NewsClass news)
        {
            //ListViewItem item = new ListViewItem(DateTime.Now.ToString("MM/dd HH:mm:ss"));
            ListViewItem item = new ListViewItem(news.pubDate.ToString("MM/dd HH:mm:ss"));
            item.UseItemStyleForSubItems = false;
            item.Tag = news;//news.Link;
            item.SubItems.Add(news.StockNumber+" "+news.StockName);
            //---持股
            if (isMyStock(news.StockNumber)==true)
            {
                item.SubItems[item.SubItems.Count - 1].ForeColor = Color.White;
                item.SubItems[item.SubItems.Count - 1].BackColor = Color.RoyalBlue;
            }
            // 前3群組
            else if (isMainGroup(news.StockNumber,3) == true)
            {
                item.SubItems[item.SubItems.Count - 1].ForeColor = Color.Black;
                item.SubItems[item.SubItems.Count - 1].BackColor = Color.Gold;
            }            
            item.SubItems.Add(news.Title);
            /*
            var keyword = tb_keyword.Text.Trim();
            if(!string.IsNullOrEmpty(keyword)) //符合的變個底色
            {
                int count = keyword.Split(',').Length;
                if(HowMuchKeywords(news,keyword)>=count)
                { }
            }
            */
            if (news.FocusScore + news.GoodScore + news.BadScore > 0)
            {
                item.SubItems.Add((news.FocusScore + news.GoodScore + news.BadScore).ToString());
                if(news.FocusScore + news.GoodScore + news.BadScore >= 3)
                {
                    item.SubItems[item.SubItems.Count - 1].BackColor = Color.MistyRose;
                }
            }
            listView_News.Items.Add(item);
            listView_News.EnsureVisible(listView_News.Items.Count - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        private void btn_load_Click(object sender, EventArgs e)
        {
            btn_load.Enabled = false;
            btn_go.Enabled = false;
            loadNews();
            btn_load.Enabled = true;
            btn_go.Enabled = true;
        }
        private void btn_go_Click(object sender, EventArgs e)
        {
            SetListViewMsg("Start Updating..");

            PollingEvent(null, null);
        }
        private void listView_News_DoubleClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                if(((ListView)sender).SelectedItems.Count > 0)
                {
                    NewsClass news = (NewsClass)((ListView)sender).SelectedItems[0].Tag;
                    string url = (string)news.Link;
                    var realurl = url.Split('*');
                    if(realurl.Length==2 && realurl[1].StartsWith("http"))
                    {
                        url = realurl[1];
                    }
                    //---有按著Shift:直接打開營收
                    if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                    {
                        url = string.Format("https://tw.stock.yahoo.com/d/s/earning_{0}.html",news.StockNumber);
                    }
                    try
                    {
                        //System.Diagnostics.Process.Start(@"%AppData%\..\Local\Google\Chrome\Application\chrome.exe", "http:\\www.yahoo.com");
                        System.Diagnostics.Process.Start("chrome.exe", url);
                    }
                    catch
                    {
                        System.Diagnostics.Process.Start("iexplore.exe", url);
                    }                    
                } 
            }
        }
        private void btn_openwindow_Click(object sender, EventArgs e)
        {

            if (((ListView)listView_News).SelectedItems.Count > 0)
            {
                NewsClass news = (NewsClass)((ListView)listView_News).SelectedItems[0].Tag;
                string url = (string)news.Link;
                var realurl = url.Split('*');
                if(realurl.Length==2 && realurl[1].StartsWith("http"))
                {
                    url = realurl[1];
                }
                //---有按著Shift:直接打開營收
                if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    url = string.Format("https://tw.stock.yahoo.com/d/s/earning_{0}.html",news.StockNumber);
                }
                try
                {
                    //System.Diagnostics.Process.Start(@"%AppData%\..\Local\Google\Chrome\Application\chrome.exe", "http:\\www.yahoo.com");
                    System.Diagnostics.Process.Start("chrome.exe", url);
                }
                catch
                {
                    System.Diagnostics.Process.Start("iexplore.exe", url);
                }                    
            } 
        }


        private void tb_load_OnKeydown(object sender, KeyEventArgs e)
        {
            if (btn_load.Enabled == true && e.KeyCode == Keys.Return)
            {
                btn_load_Click(null, null);
            }
        }

        private void cb_mygrouponly_CheckedChanged(object sender, EventArgs e)
        {
            if (btn_load.Enabled == true)
            {
                showNews();
            }
        }

        #endregion


    }
}

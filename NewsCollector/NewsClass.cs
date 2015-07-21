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

namespace NewsCollector
{
    public class NewsClass
    {
        public string StockNumber { get; set; }
        public string StockName { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public DateTime pubDate { get; set; }

        public bool saveIt { get; set; }
        //-------------
        public byte FocusScore { get; set; } // 重要Keyword
        public byte GoodScore { get; set; } //利多分數:利用keyword為這個新聞評分
        public byte BadScore { get; set; } //利空
        public string NewsContent { get; set; }  //分數夠高時,就下載詳細Data記錄
    }

    //==========

    public class StockClass
    {
        public int ID { get; set; }                 //<排序編號0~9+> 
        public string Name { get; set; }            //--類別名稱 [10]
        public string Note { get; set; }            //--類別筆記欄
        public List<StockList> stockList { get; set; }  //--類別的個股名單
    }
    public class StockList
    {
        public string Number { get; set; } //代號 [0]
        public string Name { get; set; } //名稱 [36]
    }

    public class MyStock
    {
        public string stockid { get; set; }
        public string name { get; set; }
        public bool buymode { get; set; }   //--多單為true;空單為false
        public int snum { get; set; }    // 張數
        public float inprice { get; set; }  // 成本價
        public float balance { get; set; }  // 損益
        public float outprice { get; set; } // 停損價
        public int outmode { get; set; }    // 停損選項
        public string note { get; set; }    // 操作記事(個股)
    }

}

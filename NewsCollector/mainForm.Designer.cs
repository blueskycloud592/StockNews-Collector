namespace NewsCollector
{
    partial class mainForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(mainForm));
            this.btn_go = new System.Windows.Forms.Button();
            this.listView_News = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cb_maingroup = new System.Windows.Forms.ComboBox();
            this.cb_loadday = new System.Windows.Forms.ComboBox();
            this.cb_skip = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_keyword = new System.Windows.Forms.TextBox();
            this.cb_score = new System.Windows.Forms.ComboBox();
            this.btn_load = new System.Windows.Forms.Button();
            this.btn_openwindow = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_go
            // 
            this.btn_go.Location = new System.Drawing.Point(587, 377);
            this.btn_go.Name = "btn_go";
            this.btn_go.Size = new System.Drawing.Size(75, 23);
            this.btn_go.TabIndex = 1;
            this.btn_go.Text = "Go";
            this.btn_go.UseVisualStyleBackColor = true;
            this.btn_go.Click += new System.EventHandler(this.btn_go_Click);
            // 
            // listView_News
            // 
            this.listView_News.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listView_News.FullRowSelect = true;
            this.listView_News.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView_News.Location = new System.Drawing.Point(12, 12);
            this.listView_News.MultiSelect = false;
            this.listView_News.Name = "listView_News";
            this.listView_News.Size = new System.Drawing.Size(650, 356);
            this.listView_News.TabIndex = 4;
            this.listView_News.UseCompatibleStateImageBehavior = false;
            this.listView_News.View = System.Windows.Forms.View.Details;
            this.listView_News.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView_News_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "DateTime";
            this.columnHeader1.Width = 80;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "股名";
            this.columnHeader2.Width = 100;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Title";
            this.columnHeader3.Width = 400;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Score";
            this.columnHeader4.Width = 45;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cb_maingroup);
            this.groupBox1.Controls.Add(this.cb_loadday);
            this.groupBox1.Controls.Add(this.cb_skip);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tb_keyword);
            this.groupBox1.Controls.Add(this.cb_score);
            this.groupBox1.Controls.Add(this.btn_load);
            this.groupBox1.Location = new System.Drawing.Point(12, 370);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(449, 86);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            // 
            // cb_maingroup
            // 
            this.cb_maingroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_maingroup.FormattingEnabled = true;
            this.cb_maingroup.Items.AddRange(new object[] {
            "None",
            "持股Only",
            "持股+First Group",
            "持股+1+2 Group",
            "持股+1~3 Group"});
            this.cb_maingroup.Location = new System.Drawing.Point(149, 48);
            this.cb_maingroup.Name = "cb_maingroup";
            this.cb_maingroup.Size = new System.Drawing.Size(107, 20);
            this.cb_maingroup.TabIndex = 20;
            // 
            // cb_loadday
            // 
            this.cb_loadday.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_loadday.FormattingEnabled = true;
            this.cb_loadday.Items.AddRange(new object[] {
            "今日News",
            "昨天至今",
            "3天內",
            "10天",
            "30天"});
            this.cb_loadday.Location = new System.Drawing.Point(13, 48);
            this.cb_loadday.Name = "cb_loadday";
            this.cb_loadday.Size = new System.Drawing.Size(110, 20);
            this.cb_loadday.TabIndex = 19;
            // 
            // cb_skip
            // 
            this.cb_skip.AutoSize = true;
            this.cb_skip.Checked = true;
            this.cb_skip.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_skip.Location = new System.Drawing.Point(312, 51);
            this.cb_skip.Name = "cb_skip";
            this.cb_skip.Size = new System.Drawing.Size(45, 16);
            this.cb_skip.TabIndex = 16;
            this.cb_skip.Text = "Skip";
            this.cb_skip.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(93, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 12);
            this.label1.TabIndex = 15;
            this.label1.Text = "Keyword:";
            // 
            // tb_keyword
            // 
            this.tb_keyword.Location = new System.Drawing.Point(149, 21);
            this.tb_keyword.Name = "tb_keyword";
            this.tb_keyword.Size = new System.Drawing.Size(208, 22);
            this.tb_keyword.TabIndex = 14;
            this.tb_keyword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tb_load_OnKeydown);
            // 
            // cb_score
            // 
            this.cb_score.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_score.FormattingEnabled = true;
            this.cb_score.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.cb_score.Location = new System.Drawing.Point(13, 21);
            this.cb_score.Name = "cb_score";
            this.cb_score.Size = new System.Drawing.Size(75, 20);
            this.cb_score.TabIndex = 13;
            // 
            // btn_load
            // 
            this.btn_load.Location = new System.Drawing.Point(366, 21);
            this.btn_load.Name = "btn_load";
            this.btn_load.Size = new System.Drawing.Size(75, 23);
            this.btn_load.TabIndex = 12;
            this.btn_load.Text = "Load";
            this.btn_load.UseVisualStyleBackColor = true;
            this.btn_load.Click += new System.EventHandler(this.btn_load_Click);
            // 
            // btn_openwindow
            // 
            this.btn_openwindow.ForeColor = System.Drawing.Color.Blue;
            this.btn_openwindow.Location = new System.Drawing.Point(478, 377);
            this.btn_openwindow.Name = "btn_openwindow";
            this.btn_openwindow.Size = new System.Drawing.Size(75, 23);
            this.btn_openwindow.TabIndex = 13;
            this.btn_openwindow.Text = "Open";
            this.btn_openwindow.UseVisualStyleBackColor = true;
            this.btn_openwindow.Click += new System.EventHandler(this.btn_openwindow_Click);
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 474);
            this.Controls.Add(this.btn_openwindow);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listView_News);
            this.Controls.Add(this.btn_go);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "mainForm";
            this.Text = "News Collector";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.mainForm_Closing);
            this.Load += new System.EventHandler(this.mainForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_go;
        private System.Windows.Forms.ListView listView_News;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cb_skip;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_keyword;
        private System.Windows.Forms.ComboBox cb_score;
        private System.Windows.Forms.Button btn_load;
        private System.Windows.Forms.ComboBox cb_loadday;
        private System.Windows.Forms.ComboBox cb_maingroup;
        private System.Windows.Forms.Button btn_openwindow;
    }
}


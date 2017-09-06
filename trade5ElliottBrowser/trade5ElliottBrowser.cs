using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WnFTechnicalIndicators;


namespace trade5ElliottBrowser
{
    public enum ModeOfInfo
    {
        Product,
        JobHealth,  // Not Implemented
        IniEdit
    }


    public delegate void ehVoid();
    public delegate void ehInt1(int i);
    public delegate void ehString1(string p);
    public delegate void ehControl(object sender, System.EventArgs e);


    public partial class trade5ElliottBrowser : Form
    {
        public trade5ElliottBrowser()
        {
            InitializeComponent();

            if (!WnFElliottBrowser.LoadIni())
                throw new Exception("Error loading application configuration file");

            symbolsOn = new Dictionary<string, Dictionary<CandlePeriod, WnFCandles>>();
            chartsDic = new Dictionary<string, Dictionary<CandlePeriod, CChartPanel>>();
            periodDic = new Dictionary<CandlePeriod, bool>();

            czoom = Properties.Settings.Default.czoom;
            if (czoom == 0)
            {
                czoom = 144;
                Properties.Settings.Default.czoom = czoom;
            }
            toolStripTBZoom.Text = czoom.ToString();
        }

        public void OnAfterDisplay(int i)
        {
            chartsPanelFlag -= (int)Math.Pow(2, i);
            if (chartsPanelFlag == 0) busy_loading = false;
        }

        private IWnFOpenAPI _factory;
        private string symbolOnDisplay;
        private Dictionary<string, Dictionary<CandlePeriod, CChartPanel>> chartsDic;
        private Dictionary<string, Dictionary<CandlePeriod, WnFCandles>> symbolsOn;
        private Dictionary<string, string> symbolsDic;
        private Dictionary<CandlePeriod, bool> periodDic;
        private Dictionary<CandlePeriod, ToolStripButton> btnPeriods;
        private TableLayoutPanel chartsPanel;
        private TabControl symbolsTabs;
        private ToolStripButton btnPrefered;
        private const string defaultPeriods = "D";
        private int czoom;
        private bool busy_loading;
        private int chartsPanelFlag;
        private ehVoid d_msg;


        delegate void d_SetCtrl(Control ctrl, string str);

        private void SetCtrl(Control ctrl, string str)
        {
            if (ctrl.InvokeRequired)
            {
                d_SetCtrl ci = new d_SetCtrl(SetCtrl);
                ctrl.Invoke(ci, ctrl, str);
            }
            else
            {
                if (ctrl.GetType() == typeof(SplitContainer))
                    ctrl.Visible = true;
                else if (ctrl.GetType() == typeof(TableLayoutPanel))
                    ctrl.Show();
                else
                    ctrl.Text = str;
            }
        }

        private void OnSymbolSelected(CommodityType type, string symbol)
        {
            SelectSymbol(symbol, type);
        }

        private void PrepareSymbolsPanel(Dictionary<string, string> dic, bool refresh = false)
        {
            if (!refresh)
            {
                symbolsTabs = new TabControl();

                symbolsTabs.TabPages.Add("Stocks");
                CSymbolsPanel p = new CSymbolsPanel();
                p.Symbols = dic;
                p.SymbolSelected += this.OnSymbolSelected;
                p.Parent = symbolsTabs.TabPages[0];
                p.Dock = DockStyle.Fill;

                symbolsTabs.Parent = splitContainer2.Panel1;
                symbolsTabs.Dock = DockStyle.Fill;
                symbolsTabs.SelectedTab = symbolsTabs.TabPages[0];
            }
            else
            {
                CSymbolsPanel p = (CSymbolsPanel)symbolsTabs.TabPages[0].Controls[0];
                p.Symbols = dic;
            }
        }

        private void OnDataLoading()
        {
            while (busy_loading)
            {
                System.Threading.Thread.Sleep(200);
                toolStripSLblMessage.Text += ".";
            }

            toolStripSLblMessage.Text += " OK";
            SetCtrl(splitContainer2, null);
        }

        private void SelectSymbol(string symbol, CommodityType type = CommodityType.Stocks, WnFCandles ch = null)
        {
            if (symbol == symbolOnDisplay) return;

            splitContainer2.Panel2.Controls.RemoveAt(0); splitContainer2.Panel1Collapsed = true;

            if (!toolStripCBSymbol.AutoCompleteCustomSource.Contains(symbol))
            {
                chartsDic[symbol] = new Dictionary<CandlePeriod, CChartPanel>();
                if (!toolStripCBSymbol.Items.Contains(symbol)) toolStripCBSymbol.Items.Add(symbol);
                toolStripCBSymbol.AutoCompleteCustomSource.Add(symbol);
            }

            if (!symbolsOn.ContainsKey(symbol))
            {
                symbolsOn[symbol] = new Dictionary<CandlePeriod, WnFCandles>();
                if (ch != null)
                    symbolsOn[symbol][(CandlePeriod)ch.Period] = ch;
            }

            toolStripCBSymbol.Text = symbol;
            this.Text = Properties.Settings.Default.tm + " - " + symbolsDic[symbol];
        }

        private CChartPanel CreateChart(string symbol, CandlePeriod p)
        {
            if (!chartsDic[symbol].ContainsKey(p))
            {
                chartsDic[symbol][p] = new CChartPanel(symbolsOn.Single(kv => kv.Key == symbol), (int)p);
                chartsDic[symbol][p].AfterDisplay -= OnAfterDisplay;
                chartsDic[symbol][p].AfterDisplay += OnAfterDisplay;
                chartsDic[symbol][p].EmbededInMultiChart = true;
                chartsDic[symbol][p].ParentCZoom = toolStripTBZoom;
            }

            return chartsDic[symbol][p];
        }

        private TableLayoutPanel CreateChartsPanel(int cnt)
        {
            int rcnt = 1, ccnt = 1, chart_no = 0;
            if (cnt < 3)
            {
                ccnt = cnt;
            }
            else
            {
                rcnt = 2;
                ccnt = (int)((cnt + 1) / 2);
            }

            TableLayoutPanel cpanel = new TableLayoutPanel();
            cpanel.ColumnCount = ccnt;
            for (int i = 0; i < ccnt; i++)
                cpanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)100.0 / ccnt));
            cpanel.RowCount = rcnt;
            for (int i = 0; i < rcnt; i++)
                cpanel.RowStyles.Add(new RowStyle(SizeType.Percent, (float)100.0 / rcnt));

            Dictionary<CandlePeriod, CChartPanel> dic = chartsDic[symbolOnDisplay];
            foreach (CandlePeriod p in periodDic.Keys)
            {
                if (periodDic[p])
                {
                    CChartPanel panel = dic[p];
                    cpanel.Controls.Add(panel, chart_no % ccnt, chart_no / ccnt);
                    panel.Dock = DockStyle.Fill;
                    chart_no++;
                }
            }

            if (chartsPanelFlag == 0) busy_loading = false;
            return cpanel;
        }

        private void RefreshChartPanel()
        {
            busy_loading = true;
            toolStripSLblMessage.Text = "Loading chart";
            splitContainer2.Visible = false;
            d_msg.BeginInvoke(null, null);

            if (splitContainer2.Panel2.Controls.Count > 0) splitContainer2.Panel2.Controls.RemoveAt(0);

            int no_charts = 0;
            Dictionary<CandlePeriod, CChartPanel> dic = chartsDic[symbolOnDisplay];
            chartsPanelFlag = 0;

            foreach (CandlePeriod p in periodDic.Keys)
            {
                if (periodDic[p])
                {
                    if (!dic.ContainsKey(p))
                    {
                        CreateChart(symbolOnDisplay, p);
                        chartsPanelFlag += (int)Math.Pow(2, no_charts);
                    }
                    no_charts++;
                }
            }

            if (no_charts == 0) return;
            chartsPanel = CreateChartsPanel(no_charts);
            chartsPanel.Parent = splitContainer2.Panel2;
            chartsPanel.Dock = DockStyle.Fill;
        }

        private void SetFormLocation()
        {
            if (Properties.Settings.Default.form_loc.X < SystemInformation.VirtualScreen.Left)
                this.Location = new Point(SystemInformation.VirtualScreen.Left, Properties.Settings.Default.form_loc.Y);
            else if (Properties.Settings.Default.form_loc.X >= SystemInformation.VirtualScreen.Right)
                this.Location = new Point(SystemInformation.VirtualScreen.Right - this.Width, Properties.Settings.Default.form_loc.Y);
            else
                this.Location = Properties.Settings.Default.form_loc;
        }

        private void InitPeriodButtons()
        {
            btnPeriods = new Dictionary<CandlePeriod, ToolStripButton>();
            toolStripBtnM.Tag = CandlePeriod.M;
            toolStripBtnW.Tag = CandlePeriod.W;
            toolStripBtnD.Tag = CandlePeriod.D;
            toolStripBtn1H.Tag = CandlePeriod.m60;
            toolStripBtn30m.Tag = CandlePeriod.m30;
            toolStripBtn15m.Tag = CandlePeriod.m15;
            toolStripBtn5m.Tag = CandlePeriod.m5;
            toolStripBtn3m.Tag = CandlePeriod.m3;

            btnPeriods[CandlePeriod.M] = toolStripBtnM;
            btnPeriods[CandlePeriod.W] = toolStripBtnW;
            btnPeriods[CandlePeriod.D] = toolStripBtnD;
            btnPeriods[CandlePeriod.m60] = toolStripBtn1H;
            btnPeriods[CandlePeriod.m30] = toolStripBtn30m;
            btnPeriods[CandlePeriod.m15] = toolStripBtn15m;
            btnPeriods[CandlePeriod.m5] = toolStripBtn5m;
            btnPeriods[CandlePeriod.m3] = toolStripBtn3m;

            foreach (ToolStripButton b in btnPeriods.Values)
            {
                b.Enabled = _factory.SupportedPeriods().Contains((int)b.Tag);
                b.Visible = b.Enabled;
            }

            SetPreferedPeriod();
            SetDefaultPeriods();
        }

        private void SetDefaultPeriods()
        {
            string str;
            string[] arr;
            Dictionary<CandlePeriod, bool> dic = new Dictionary<CandlePeriod, bool>();

            str = Properties.Settings.Default.defaultPeriods;
        retry:
            arr = str.Split(',');

            foreach (string i in arr)
            {
                CandlePeriod p;
                if (Enum.TryParse(i, false, out p)) dic[p] = true;
            }

            if (dic.Count == 0)
            {
                str = defaultPeriods; goto retry;
            }

            foreach (CandlePeriod p in Enum.GetValues(typeof(CandlePeriod))) periodDic[p] = false;
            foreach (CandlePeriod p in dic.Keys) periodDic[p] = true;
            foreach (ToolStripButton btn in btnPeriods.Values)
                btn.Checked = periodDic[(CandlePeriod)Enum.Parse(typeof(CandlePeriod), btn.Tag.ToString())];
        }

        private void SetPreferedPeriod(ToolStripButton btn = null)
        {
            btnPrefered = (btn == null) ? btnPeriods[CandlePeriod.D] : btn;
        }

        private void SaveDefaultPeriods()
        {
            if (toolStripBtnToggle.Checked)
            {
                string str = "";

                foreach (CandlePeriod p in periodDic.Keys)
                {
                    if (periodDic[p]) str += p + " ";
                }
                str = str.Trim().Replace(' ', ',');

                Properties.Settings.Default["DefaultPeriods"] = str;
            }
            else
            {
                Properties.Settings.Default["PreferedPeriod"] = ((CandlePeriod)btnPrefered.Tag).ToString();
            }
        }

        private void trade5ElliottBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.form_loc = this.Location;
            Properties.Settings.Default.Save();
        }

        private void trade5ElliottBrowser_Load(object sender, EventArgs e)
        {
            d_msg = OnDataLoading;

            if (toolStripSplitAPI.DropDownItems.Count == 1)
                toolStripSplitAPI.Tag = toolStripSplitAPI.DropDownItems[0].Tag;

            SetFormLocation();
        }

        private CandlePeriod ButtonPeriod(ToolStripButton btn)
        {
            return (CandlePeriod)btn.Tag;
        }

        private void OnSymbolSelected(string symbol, CommodityType type = CommodityType.Stocks)
        {
            splitContainer2.Panel1Collapsed = true;
            SelectSymbol(symbol, type);
        }

        private void OnPeriodBtnClick(object sender, EventArgs e)
        {
            if (toolStripSLblOnOff.Text == "Data Provider") return;    // ==> When not logged on, events are cancelled.

            ToolStripButton btn = (ToolStripButton)sender;
            if (!toolStripBtnToggle.Checked)
            {
                if (!btn.Checked) OnSinglePeriodBtnClick(sender, e); return;
            }
            btn.Checked = !btn.Checked;
            periodDic[ButtonPeriod(btn)] = btn.Checked;
            if (symbolOnDisplay != null) RefreshChartPanel();
        }

        private void OnSinglePeriodBtnClick(object sender, EventArgs e)
        {
            ToolStripButton btn = (ToolStripButton)sender;

            CandlePeriod p = ButtonPeriod(btn);
            foreach (ToolStripButton b in btnPeriods.Values)
            {
                b.Checked = (btn == b);
                periodDic[ButtonPeriod(b)] = (btn == b);
            }
            btnPrefered = btnPeriods[p];

            if (symbolOnDisplay == null) return;
            RefreshChartPanel();
        }

        private void toolStripBtnAbout_Click(object sender, EventArgs e)
        {
            Form f = WnFElliottBrowser.GetForm(_factory, ModeOfInfo.Product);
            f.Show();
            f.Location = new Point(this.Location.X + SystemInformation.ToolWindowCaptionHeight, this.Location.Y + SystemInformation.ToolWindowCaptionHeight);
        }

        private void toolStripBtnToggle_Click(object sender, EventArgs e)
        {
            toolStripBtnToggle.Checked = !toolStripBtnToggle.Checked;
            if (toolStripBtnToggle.Checked)
            {
                toolStripBtnToggle.Image = Properties.Resources.Thumbnails;
                toolStripBtnToggle.ToolTipText = "Multiple View";
            }
            else
            {
                toolStripBtnToggle.Image = Properties.Resources.Windows;
                toolStripBtnToggle.ToolTipText = "Single View";
                if (btnPrefered != null) OnSinglePeriodBtnClick(btnPrefered, null);
            }
        }

        private void toolStripBtnSymbols_Click(object sender, EventArgs e)
        {
            if (splitContainer2.Panel1.Controls.Count == 0)
            {
            }
            splitContainer2.Panel1Collapsed = !splitContainer2.Panel1Collapsed;
        }

        private void toolStripCBSymbol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripCBSymbol.Text == symbolOnDisplay) return;
            symbolOnDisplay = toolStripCBSymbol.Text;
            this.Text = Properties.Settings.Default.tm + " - " + symbolsDic[symbolOnDisplay];
            RefreshChartPanel();
        }

        private void toolStripCBSymbol_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                if (symbolsDic.ContainsKey(toolStripCBSymbol.Text)) SelectSymbol(toolStripCBSymbol.Text, CommodityType.None);
        }

        private void toolStripTBZoom_Click(object sender, EventArgs e)
        {

        }

        private void toolStripSplitAPI_ButtonClick(object sender, EventArgs e)
        {
            WnFCandles ch;
            int err = -1;

            if (toolStripSplitAPI.Text == "Data Provider")
            {
                if (Convert.ToString(toolStripSplitAPI.Tag) == QuandlAPI._PROVIDER)
                {
                    WnFElliottBrowser.Factory = _factory = (IWnFOpenAPI)(new QuandlAPI());
                    ((QuandlAPI)_factory).SymbolsUpdate += Trade5ElliottBrowser_SymbolsUpdate;
                }
                else
                    throw new NotImplementedException();

                if (_factory.Check(out err, out ch))
                {
                    toolStripSLblOnOff.Image = Properties.Resources.ArrowGreen;
                    toolStripSLblOnOff.Text = _factory.ProviderName();
                    toolStripSLblMessage.Text = "Checking API settings OK";
                    MessageBox.Show(toolStripSLblMessage.Text, Properties.Settings.Default.tm, MessageBoxButtons.OK);

                    toolStripSplitAPI.Text = quandlToolStripMenuItem.Text;
                    InitPeriodButtons();
                    SelectSymbol(ch.Symbol, CommodityType.None, ch);
                }
                else
                {
                    MessageBox.Show("Error checking API settings: " + ((APIError)err).ToString(), Properties.Settings.Default.tm, MessageBoxButtons.OK);

                    if (err == (int)APIError.APIKeyFile || err == (int)APIError.APIKeyLength)
                    {
                        if (err == (int)APIError.APIKeyFile)
                            WnFElliottBrowser.CreateAPIKeysIni(QuandlAPI._PROVIDER);

                        Form f = WnFElliottBrowser.GetForm(_factory, ModeOfInfo.IniEdit, (int)IniCategory.APIKeys);
                        f.Show();
                        f.Location = new Point(this.Location.X + SystemInformation.ToolWindowCaptionHeight, this.Location.Y + SystemInformation.ToolWindowCaptionHeight);
                    }
                    else if (err == (int)APIError.APIUrlFormat || err == (int)APIError.APIResponseErr)
                    {
                        Form f = WnFElliottBrowser.GetForm(_factory, ModeOfInfo.IniEdit, (int)IniCategory.URLs);
                        f.Show();
                        f.Location = new Point(this.Location.X + SystemInformation.ToolWindowCaptionHeight, this.Location.Y + SystemInformation.ToolWindowCaptionHeight);
                    }
                }
            }
            else
            {
            }
        }

        private void toolStripSplitAPI_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            toolStripSplitAPI.Tag = e.ClickedItem.Tag;
            toolStripSplitAPI_ButtonClick(toolStripSplitAPI, null);
        }

        private void toolStripSplitAPI_TextChanged(object sender, EventArgs e)
        {
            int err;
            _factory.RefreshSymbols(out symbolsDic, out err);
            if (symbolsDic != null)
            {
                toolStripBtnSymbols.Enabled = true;
                toolStripCBSymbol.Enabled = true;
                PrepareSymbolsPanel(symbolsDic, sender == null);
            }
            else
                MessageBox.Show("Error while getting symbols: " + ((APIError)err).ToString(), Properties.Settings.Default.tm, MessageBoxButtons.OK);
        }

        private void Trade5ElliottBrowser_SymbolsUpdate()
        {
            toolStripSplitAPI_TextChanged(null, null);
        }
    }
}

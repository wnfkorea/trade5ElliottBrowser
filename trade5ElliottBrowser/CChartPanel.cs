using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using WnFTechnicalIndicators;
using WnFElliottWave;
using System.Drawing.Printing;

namespace trade5ElliottBrowser
{
    public partial class CChartPanel : UserControl
    {
        enum PriSeries
        {
            OHLC = 1,
            FRTL = 2,
            ESPNfr = 3,
            ESPN = 4
        }

        enum AuxSeries
        {
            None = 0,
            Volume = 1,
            MACD = 2,
            RSI = 4,
            OBV = 8,
            CCI = 16,
            Stoch = 32,
            Disp = 64,
            ADO = 128,
            MA = 256
        }

        enum DMode
        {
            SelectWave = 0,
            Line = 1,
            RetraceRatio = 2
        }

        enum CMode
        {
            Chart = 0,
            FInfo = 1,
            Table = 2,
            PInfo = 3
        }


        public event ehInt1 AfterDisplay;
        delegate void d_GetCandles(string s, int p, CommodityType type, ref Dictionary<CandlePeriod, WnFCandles> dic, WnFDbConnectionWrapper wrpper);


        public CChartPanel()
        {
            InitializeComponent();

            _czoom = Properties.Settings.Default.czoom;
            _d_m = DMode.SelectWave;
            _c_mod = 0;
            _lbl_w = 80;
            _tl_cnt = 0;
            _rr_cnt = 0;

            _info = new ToolTip();
            _pt_s = new Dictionary<int, PointF>();
            _pt_e = new Dictionary<int, PointF>();
            _tl_e = new Dictionary<int, double>();
            _tl_rr = new Dictionary<string, Series>();
            _dLabel = new Label();
            _pLabel = new Label();
            _aLabel = new Label();
            _line_ob = new StripLine();
            _line_os = new StripLine();
            _man_lob = new StripLine();
            _man_los = new StripLine();

            _series = new Dictionary<string, Series>();
            _init_Series();
        }

        public CChartPanel(string s, int p, string t) : this()
        {
            _symbol = s;
            _period = p;
            _title = t;
        }

        public CChartPanel(KeyValuePair<string, Dictionary<CandlePeriod, WnFCandles>> kv, int p) : this(kv.Key, p, string.Empty)
        {
            _chlist = kv.Value;
            _finished = new System.Threading.ManualResetEvent(false);
            d_GetCandles d = WnFElliottBrowser.Factory.GetCandles;
            d.BeginInvoke(kv.Key, p, CommodityType.None, ref _chlist, null, new AsyncCallback(cb_GetCandles), null);
        }

        public bool EmbededInMultiChart
        {
            set
            {
                _mode_multi = value;

                if (!_mode_multi) _symbol_dic = new Dictionary<string, string>();
            }
        }

        public ToolStripTextBox ParentCZoom
        {
            set
            {
                _parent_tb = value;
                _parent_tb.KeyUp += ToolStripNoC_KeyUp;
            }
        }


        private System.Threading.ManualResetEvent _finished;
        private bool _mode_multi;
        private bool _mode_replay;
        private Dictionary<string, string> _symbol_dic;
        private ToolStripTextBox _parent_tb;

        private string _symbol;
        private Dictionary<CandlePeriod, WnFCandles> _chlist;
        private int _period;
        private string _title;
        private DataTable _candles;
        private int _czoom;
        private int _c_mod;

        private Dictionary<WNFA, IndicatorSignal> _obj;
        private int _epm;

        private PriSeries _pseries_idx;
        private Dictionary<string, Series> _series;
        private string _aux_cn;
        private ToolTip _info;
        private DMode _d_m;
        private StripLine _line_ob;
        private StripLine _line_os;
        private StripLine _man_lob;
        private StripLine _man_los;
        private int _w_s;
        private int _w_e;
        private PointF _point;
        private PointF _point_ss;
        private PointF _point_se;
        private Double _y_offset;
        private Double _r_ratio;
        private Dictionary<int, PointF> _pt_s;
        private Dictionary<int, PointF> _pt_e;
        private Dictionary<int, Double> _tl_e;
        private Dictionary<string, Series> _tl_rr;
        private int _tl_cnt;
        private int _rr_cnt;
        private int _pt_pattern_start;
        private int _lbl_w;
        private Boolean _have_mouse;
        private Label _dLabel;
        private Label _pLabel;
        private Label _aLabel;


        private void cb_GetCandles(IAsyncResult result)
        {
            TableLayoutPanel c = (TableLayoutPanel)this.Parent;
            int idx = c.Controls.IndexOf(this);
            if (AfterDisplay != null) AfterDisplay(idx);
            _finished.Set();
        }

        private void _init_Series()
        {
            Series _ohlc = new Series(PriSeries.OHLC.ToString());
            _ohlc.XValueMember = "DateTime";
            _ohlc.YValueMembers = "High,Low,Open,Close";
            _ohlc.YAxisType = AxisType.Primary;
            _ohlc.ChartType = SeriesChartType.Candlestick;
            _ohlc.Color = Color.DarkSlateGray;
            _ohlc.CustomProperties = "PriceUpColor=Red, PriceDownColor=RoyalBlue";
            _ohlc.ToolTip = "Open: #VALY3{N1}\r\n" + "High: #VALY1{N1}\r\n" + "Low: #VALY2{N1}\r\n" + "Close: #VALY4{N1}";
            _series[_ohlc.Name] = _ohlc;

            Series _frtl = new Series(PriSeries.FRTL.ToString());
            _frtl.ChartType = SeriesChartType.Candlestick;
            _frtl.Color = Color.DarkSlateGray;
            _frtl.CustomProperties = "PriceUpColor=Red, PriceDownColor=RoyalBlue";
            _frtl.SmartLabelStyle.Enabled = true;
            _frtl.SmartLabelStyle.IsMarkerOverlappingAllowed = false;
            _frtl.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Bottom;
            _frtl.ToolTip = "Open: #VALY3{N1}\r\n" + "High: #VALY1{N1}\r\n" + "Low: #VALY2{N1}\r\n" + "Close: #VALY4{N1}";
            _series[_frtl.Name] = _frtl;

            Series _espnfr = new Series(PriSeries.ESPNfr.ToString());
            _espnfr.YAxisType = AxisType.Primary;
            _espnfr.ChartType = SeriesChartType.Line;
            _espnfr.Color = Color.White;
            _series[_espnfr.Name] = _espnfr;

            Series _espn = new Series(PriSeries.ESPN.ToString());
            _espn.ChartType = SeriesChartType.Line;
            _espn.Color = Color.Red;
            _series[_espn.Name] = _espn;

            Series _volume = new Series(AuxSeries.Volume.ToString());
            _volume.XValueMember = "DateTime";
            _volume.YValueMembers = AuxSeries.Volume.ToString();
            _volume.YAxisType = AxisType.Secondary;
            _volume.ChartType = SeriesChartType.Column;
            _volume.Color = Color.LightGreen;
            _volume.ToolTip = "#VALY{N0}";
            _series[_volume.Name] = _volume;
        }

        private void _rescaleAxisY(string cn = "")
        {
            int p = 0;
            double mx = 0;
            double mn = 0;

            var _with1 = _chart.ChartAreas[0];

            try
            {
                p = Convert.ToInt32(_with1.AxisX.ScaleView.Position);
                if (p >= _candles.Rows.Count) return;
            }
            catch (OverflowException ex)
            {
                Console.WriteLine(Properties.Settings.Default.tm + " Exception at FChart.RescaleAxisY()\r\n" + ex.Message);
            }

            string f = "row_num >= " + p + " AND row_num <= " + (p + _czoom - 1);

            try
            {
                if (_pseries_idx == PriSeries.ESPN)
                {
                    mx = Convert.ToDouble(_candles.Compute("MAX(ESPN_AP)", f));
                    mn = Convert.ToDouble(_candles.Compute("MIN(ESPN_AP)", f));
                }
                else
                {
                    mx = Convert.ToDouble(_candles.Compute("MAX(High)", f));
                    mn = Convert.ToDouble(_candles.Compute("MIN(Low)", f));
                }
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine(Properties.Settings.Default.tm + " Exception at FChart.RescaleAxisY()\r\n" + ex.Message);
            }

            if (_chart.ChartAreas[0].AxisY.StripLines.Count > 0)
            {
                foreach (StripLine l in _chart.ChartAreas[0].AxisY.StripLines)
                {
                    mx = Math.Max(mx, l.IntervalOffset);
                    mn = Math.Min(mn, l.IntervalOffset);
                }
            }

            if (mx != mn)
            {
                _with1.AxisY.Maximum = mx + 0.05 * (mx - mn);
                _with1.AxisY.Minimum = mn - 0.05 * (mx - mn);
                _with1.AxisY.Minimum -= 0.2 * (_with1.AxisY.Maximum - _with1.AxisY.Minimum);
            }
            else
            {
                _with1.AxisY.Maximum = 1.05 * mx;
                _with1.AxisY.Minimum = 0.95 * mn;
                _with1.AxisY.Minimum -= 0.2 * (_with1.AxisY.Maximum - _with1.AxisY.Minimum);
            }

            if (cn == string.Empty) cn = _aux_cn;
            if (cn != string.Empty)
            {
                if (cn.EndsWith("_SIG")) cn = _chart.Series[_chart.Series.Count - 2].Name;

                _aux_cn = cn;
                try
                {
                    mx = Convert.ToDouble(_candles.Compute("MAX([" + cn + "])", f));
                    mn = Convert.ToDouble(_candles.Compute("MIN([" + cn + "])", f));
                }
                catch (InvalidCastException ex)
                {
                    Console.WriteLine(Properties.Settings.Default.tm + " Exception at FChart.RescaleAxisY()\r\n" + ex.Message);
                }

                if (cn.StartsWith(AuxSeries.OBV.ToString()))
                {
                    _with1.AxisY2.Maximum = mx + 5 * (mx - mn);
                    _with1.AxisY2.Minimum = mn - 0.1 * (mx - mn);
                }
                else if (cn == AuxSeries.Volume.ToString())
                {
                    _with1.AxisY2.Maximum = 6 * mx;
                    _with1.AxisY2.Minimum = 0;
                }
                else
                {
                    _with1.AxisY2.Maximum = mx + 5 * (mx - mn);
                    _with1.AxisY2.Minimum = Math.Min(mn - 0.1 * (mx - mn), 0);
                }
            }
        }

        private void LoadPeriods()
        {
            ToolStripMinSel.Items.Clear();
            foreach (CandlePeriod r in Enum.GetValues(typeof(CandlePeriod)))
            {
                if ((int)r > 60) break;
                ToolStripMinSel.Items.Add(r);
            }
        }

        private void OnTIDropDownItemClicked(object sender, EventArgs e)
        {
            WnFTechnicalIndicators.TI i = (TI)Enum.Parse(typeof(TI), ((ToolStripMenuItem)sender).Tag.ToString());
            string sstr = IndicatorSignal.GetDefaultSigStr(i);
            IndicatorSignal isig = new IndicatorSignal((CandlePeriod)_period, sstr, i);
            AuxSeries ssid = (AuxSeries)Enum.Parse(typeof(AuxSeries), i.ToString());

            if (!_candles.Columns.Contains(sstr))
            {
                _chlist[(CandlePeriod)_period].AddIndicatorSignal(ref isig);
                _candles = _chlist[(CandlePeriod)_period].Indicator(isig.IStr).Data;
            }

            _chart.MouseMove -= Chart_MouseMove;
            System.Windows.Forms.Cursor.Position = new Point(System.Windows.Forms.Cursor.Position.X,
                                                             this.ParentForm.Location.Y + SystemInformation.ToolWindowCaptionHeight + (int)(2.5 * ToolStripPeriod.Height));
            DisplayChart(0, ssid, sstr);
            _chart.MouseMove += Chart_MouseMove;

            ToolStripAMode.Text = string.Empty;
        }

        private void LoadTIs()
        {
            List<ToolStripItem> dditems = new List<ToolStripItem>();
            ToolStripMenuItem dd;
            foreach (WnFTechnicalIndicators.TI i in Enum.GetValues(typeof(WnFTechnicalIndicators.TI)))
            {
                if ((int)i < 4) continue;
                if (i == WnFTechnicalIndicators.TI.CPN) continue;
                dd = new ToolStripMenuItem();
                dd.Text = i.ToString();
                dd.Name = i.ToString();
                dd.Tag = i;
                dd.Click += OnTIDropDownItemClicked;
                dditems.Add(dd);
            }
            ToolStripTI.DropDownItems.AddRange(dditems.ToArray());
        }

        private void SetPeriod(CandlePeriod p)
        {
            ToolStripMenuItem dd;
            _period = (int)p;

            if (p < CandlePeriod.D)
            {
                //ToolStripMinSel.Enabled = True    ==> Uncomment when singleview mode!
                ToolStripMinSel.Text = p.ToString();
                _dLabel.Width = (int)(1.6 * _lbl_w);
                dd = MinuteToolStripMenuItem;
            }
            else
            {
                //ToolStripMinSel.Enabled = False   ==> Uncomment when singleview mode!
                _dLabel.Width = _lbl_w;
                if (p == CandlePeriod.D)
                    dd = DailyToolStripMenuItem;
                else if (p == CandlePeriod.W)
                    dd = WeeklyToolStripMenuItem;
                else
                    dd = MonthlyToolStripMenuItem;
            }

            ToolStripPeriod.Image = dd.Image;
            ToolStripPeriod.Text = dd.Text;
            ToolStripPeriod.Tag = dd.Tag;
        }

        private bool PreDisplay(int amode = 0, AuxSeries ssid = AuxSeries.None)
        {
            _finished.WaitOne();

            if (!_chlist.ContainsKey((CandlePeriod)_period))
                _chlist[(CandlePeriod)_period] = WnFElliottBrowser.Factory.GetCandles(_symbol, _period, CommodityType.None);

            if (amode == 0)
            {
                _pseries_idx = PriSeries.OHLC;
                if (ssid == AuxSeries.None)
                    _candles = _chlist[(CandlePeriod)_period].DOHLCV;
            }
            else
            {
                if (_obj[(WNFA)amode].Indicator == null)
                {
                    IndicatorSignal isig = _obj[(WNFA)amode];
                    _chlist[(CandlePeriod)_period].AddIndicatorSignal(ref isig);
                }
                else if ((WNFA)amode == WNFA.ESPN)
                {
                    if ((int)((TiESPN)_obj[(WNFA)amode].Indicator).AnalysisMode != _epm)
                    {
                        ((TiESPN)_obj[(WNFA)amode].Indicator).AnalysisMode = (EPAM)_epm;
                        _obj[(WNFA)amode].Indicator.Init(_candles);
                    }
                }

                _candles = _chlist[(CandlePeriod)_period].Indicator(_obj[(WNFA)amode].IStr).Data;
                if ((WNFA)amode == WNFA.ESPN)
                {
                    _epm = (int)((TiESPN)_obj[(WNFA)amode].Indicator).AnalysisMode;
                }
            }

            return (_candles.Rows.Count > 0);
        }

        private void PostDisplay()
        {
        }

        private void ShowSPNotations()
        {
            DataRow[] dr = _candles.Select("ESPN_SP IS NOT NULL");
            int ub = dr.Length - 1;
            if (ub < 0) return;

            for (int i = 0; i <= ub; i++)
            {
                int idx = _candles.Rows.IndexOf(dr[i]);
                _chart.Series[0].Points[idx].Label = (string)dr[i]["ESPN_SP"];
            }

            dr = _candles.Select("ESPN_HA IS NOT NULL");
            ub = dr.Length - 1;
            if (ub < 0) return;

            for (int i = 0; i <= ub; i++)
            {
                int idx = _candles.Rows.IndexOf(dr[i]);
                string lbl = _chart.Series[0].Points[idx + 1].Label;
                if (lbl == string.Empty)
                {
                    _chart.Series[0].Points[idx + 1].Label = "[" + dr[i]["ESPN_HA"] + "]";
                }
                else
                {
                    _chart.Series[0].Points[idx + 1].Label = "[" + dr[i]["ESPN_HA"] + "]" + lbl;
                }
            }
        }

        private void ShowPriPart(WnFTechnicalIndicators.TI t, string sstr)
        {
            _chart.DataBind();

            switch (t)
            {
                case TI.FRTL:
                    _series[PriSeries.FRTL.ToString()].Points.DataBind(_candles.AsEnumerable(), "DateTime", "High,Low,Open,Close", "Label=" + sstr);
                    break;
                case TI.PFTW:
                    _series[PriSeries.OHLC.ToString()].Points.DataBind(_candles.AsEnumerable(), "DateTime", "High,Low,Open,Close", "Label=" + sstr + "_S");
                    break;
                case TI.ESPN:
                    _series[PriSeries.ESPN.ToString()].Points.DataBind(_candles.AsEnumerable(), "DateTime", "ESPN_AP", "Label=ESPN_SP, LabelTooltip=ESPN_DS");
                    ShowSPNotations();
                    break;
                case TI.ESPNfr:
                    _series[PriSeries.ESPNfr.ToString()].Points.DataBind(_candles.AsEnumerable(), "DateTime", sstr + "_FP", "Label=" + sstr + "_SP, LabelTooltip=" + sstr + "_DS");
                    break;
                case TI.MA:
                    if (!_series.ContainsKey(sstr))
                    {
                        Series series_indicator = new Series(sstr);
                        series_indicator.YValueMembers = sstr;
                        series_indicator.YAxisType = AxisType.Primary;
                        series_indicator.ChartType = SeriesChartType.Line;
                        _series.Add(series_indicator.Name, series_indicator);
                    }
                    _chart.Series.Add(_series[sstr]);
                    _series[sstr].Points.DataBind(_candles.AsEnumerable(), "DateTime", sstr, "");

                    break;
                default:
                    //Do Nothing
                    break;
            }
        }

        private void ShowAuxPart(WnFTechnicalIndicators.TI t, string sstr, bool is_sig = false)
        {
            Series series_indicator = default(Series);
            if (sstr == string.Empty) return;

            if (_series.ContainsKey(sstr))
            {
                series_indicator = _series[sstr];
            }
            else
            {
                series_indicator = new Series(sstr);
                series_indicator.YValueMembers = sstr;
                series_indicator.YAxisType = AxisType.Secondary;
                if (is_sig)
                {
                    series_indicator.ChartType = SeriesChartType.Line;
                }
                else
                {
                    if (t == TI.MACD || t == TI.ESPN || t == TI.ESPNfr)
                        series_indicator.ChartType = SeriesChartType.RangeColumn;
                    else
                        series_indicator.ChartType = SeriesChartType.Line;
                }
                _series[sstr] = series_indicator;
            }

            _chart.Series.Add(series_indicator);
            _series[sstr].Points.DataBind(_candles.AsEnumerable(), "DateTime", sstr, "");


            if (_candles.Columns.Contains(sstr + "_SIG"))
            {
                ShowAuxPart(t, sstr + "_SIG", true);
                return;
            }

            _chart.ChartAreas[0].AxisX.ScaleView.Position = _candles.Rows.Count - _czoom + 1;
            _rescaleAxisY(sstr);

            switch (t)
            {
                case TI.RSI:
                    SetOBOSLines(70, 30);
                    break;
                case TI.CCI:
                    SetOBOSLines(100, -100);
                    break;
                case TI.Stoch:
                    SetOBOSLines(80, 20);
                    break;
                case TI.Disp:
                    SetOBOSLines(0, 0);
                    break;
                default:
                    //Do Nothing
                    break;
            }
        }

        public void SetModeReplay(bool replay = true)
        {
            _mode_replay = replay;
            if (replay)
            {
                _obj.Clear();
                _obj = new Dictionary<WNFA, IndicatorSignal>();
            }
        }

        private void DisplayChart(int amode = 0, AuxSeries ssid = AuxSeries.None, string aux_sstr = "")
        {
            TI t = default(TI);
            string sstr = null;

            _chart.Series.Clear();
            _chart.DataSource = "";

            if (!PreDisplay(amode, ssid)) return;

            _chart.DataSource = _candles;
            _chart.ResetAutoValues();

            switch (_pseries_idx)
            {
                case PriSeries.ESPN:
                    t = TI.ESPN;
                    sstr = TI.ESPN.ToString();
                    _chart.Series.Add(_series[PriSeries.ESPN.ToString()]);
                    ShowAuxPart(TI.MACD, IndicatorSignal.GetDefaultSigStr(TI.MACD));
                    break;

                case PriSeries.ESPNfr:
                    t = TI.ESPNfr;
                    sstr = IndicatorSignal.GetDefaultSigStr(TI.ESPNfr, (CandlePeriod)_period);
                    _chart.Series.Add(_series[PriSeries.ESPNfr.ToString()]);
                    _chart.Series.Add(_series[PriSeries.OHLC.ToString()]);
                    ShowAuxPart(TI.MACD, IndicatorSignal.GetDefaultSigStr(TI.MACD));
                    break;

                case PriSeries.FRTL:
                    t = TI.PFTW;
                    sstr = TI.PFTW.ToString();
                    _chart.Series.Add(_series[PriSeries.OHLC.ToString()]);
                    _chart.Series.Add(_series[AuxSeries.Volume.ToString()]);
                    _rescaleAxisY(AuxSeries.Volume.ToString());
                    break;

                default:
                    _chart.Series.Add(_series[PriSeries.OHLC.ToString()]);
                    if (ssid == AuxSeries.None || ssid == AuxSeries.MA)
                    {
                        if (ssid == AuxSeries.MA)
                            ShowPriPart(TI.MA, aux_sstr);

                        _chart.ChartAreas[0].AxisX.ScaleView.Position = _candles.Rows.Count - _czoom + 1;
                        if (_candles.Columns.Contains(AuxSeries.Volume.ToString()))
                        {
                            _chart.Series.Add(_series[AuxSeries.Volume.ToString()]);
                            _rescaleAxisY(AuxSeries.Volume.ToString());
                        }
                        else
                        {
                            _rescaleAxisY();
                        }
                    }
                    else
                        ShowAuxPart((TI)Enum.Parse(typeof(WnFTechnicalIndicators.TI), ssid.ToString()), aux_sstr);
                    goto DoPostDisplay;
            }

            ShowPriPart(t, sstr);

            if (t == TI.ESPN || t == TI.ESPNfr || (TechnicalIndicator.GetNoSP(t) > 0))
            {
                _chart.Series[_chart.Series.Count - 2].Color = Color.LightGreen;
                _chart.Series[_chart.Series.Count - 1].Color = Color.SandyBrown;
            }

        DoPostDisplay:
            PostDisplay();
        }

        private void OnButtonAMode(WnFTechnicalIndicators.WNFA mode, System.Object e)
        {
            if (!_obj.ContainsKey(mode))
            {
                WnFTechnicalIndicators.TI i = (TI)Enum.Parse(typeof(WnFTechnicalIndicators.TI), mode.ToString());
                string sstr = IndicatorSignal.GetDefaultSigStr(i, (CandlePeriod)_period);
                _obj[mode] = new IndicatorSignal((CandlePeriod)_period, sstr, i);
            }

            switch (mode)
            {
                case WnFTechnicalIndicators.WNFA.ESPN:
                    _pseries_idx = PriSeries.ESPN;
                    if (e != null && !_mode_replay)
                        if (e.GetType() == typeof(ToolStripItemClickedEventArgs))
                            _epm = (_epm + 1) % 3;

                    break;
                case WnFTechnicalIndicators.WNFA.ESPNfr:
                    _pseries_idx = PriSeries.ESPNfr;

                    break;
                case WnFTechnicalIndicators.WNFA.PFTW:
                    _pseries_idx = PriSeries.FRTL;

                    break;
                default:
                    break;
            }

            DisplayChart(Convert.ToInt32(mode));
        }

        private void SetOBOSLines(double ob, double os, bool primary = false, bool manual = false)
        {
            StripLine l_os = manual ? _man_los : _line_os;
            StripLine l_ob = manual ? _man_lob : _line_ob;

            l_os.IntervalOffset = os;
            l_ob.IntervalOffset = ob;

            var _with1 = _chart.ChartAreas[0];
            if (!_with1.AxisY.StripLines.Contains(l_ob) && !_with1.AxisY.StripLines.Contains(l_ob))
            {
                if (primary)
                {
                    _with1.AxisY.StripLines.Add(l_os);
                    if (os != ob)
                        _with1.AxisY.StripLines.Add(l_ob);
                }
                else
                {
                    _with1.AxisY2.StripLines.Add(l_os);
                    if (os != ob)
                        _with1.AxisY2.StripLines.Add(l_ob);
                }
            }
        }

        private string AnalysisTooltip(string[] arr)
        {
            string tt = string.Empty;
            for (int i = 2; i <= 2; i++)
            {
                string str = arr[i];
                if (!string.IsNullOrEmpty(str))
                {
                    str = str.Trim();
                    if (str != string.Empty && str != "0")
                        tt += str.Replace(" ※", "\r\n※") + "\r\n\r\n";
                }
            }
            return tt;
        }

        private void AnalyzePolyWave()
        {
            TechnicalIndicator indi = default(TechnicalIndicator);
            if (_obj != null)
            {
                WNFA amode = (WNFA)Enum.Parse(typeof(WNFA), (string)ToolStripAMode.Tag);
                if (_obj.ContainsKey(amode)) indi = _obj[amode].Indicator;
            }
            if (indi == null) return;

            if (SplitContainer2.Panel2Collapsed) SplitContainer2.Panel2Collapsed = false;
            LabelSPNotations.Location = new Point((int)_chart.ChartAreas[0].AxisX.ValueToPixelPosition(_chart.ChartAreas[0].AxisX.ScaleView.Position - 1), 5);

            string tt = string.Empty;
            MacroWave ew = default(MacroWave);
            int s = Math.Min(this._w_s, this._w_e);
            int e = Math.Max(this._w_s, this._w_e);
            string[] s_p = null;
            int n = 0;
            ElliottWave li = default(ElliottWave);
            int degree = 0;
            int temp = 0;
            string str = string.Empty;

            if (_pt_pattern_start > 0 && _chart.Series.Count > 2)
                _chart.Series[_chart.Series.Count - 2].Points[_pt_pattern_start].Label = string.Empty;

            switch (indi.Type)
            {
                case TI.ESPNfr:
                    ew = ((TiESPNfr)indi).Analyzer;
                    goto Common;

                case TI.ESPN:
                    ew = ((TiESPN)indi).Analyzer;
                    Common:
                    if (!ew.mw_list.ContainsKey(s) || !ew.mw_list.ContainsKey(e))
                        return;

                    li = new ElliottWave(ew, e, s);
                    li.Linked = false;
                    li.Count(EW_NP.Low, false);
                    s_p = (string[])li.Info();
                    tt = AnalysisTooltip(s_p);
                    n = li.SP(EW_SP.None);  //ew.GetSPNotations(e, s, true);
                    if ((n & (int)EW_SP.LastFive) == (int)EW_SP.LastFive || (n & (int)EW_SP.LastThree) == (int)EW_SP.LastThree)
                    {
                        str = ((ElliottWave)ew.mw_list[e]).mSP;
                        s_p = (string[])ew.get_h1n(s, e, ref temp, ref degree, 5);
                        _pt_pattern_start = (int)temp;
                        if (degree > 1)
                            str += "@@" + _pt_pattern_start + "> " + s_p[2];
                        else
                            str += "@" + _pt_pattern_start + "> " + s_p[2];
                        tt += "\r\n" + str.Replace(" ※", "\r\n※") + "\r\n\r\n";

                        _chart.Series[_chart.Series.Count - 2].Points[_pt_pattern_start].Label = "▷";
                        _chart.Series[_chart.Series.Count - 2].Points[_pt_pattern_start].LabelForeColor = Color.Black;
                    }

                    break;
                default:

                    break;
            }

            LabelSPNotations.Text = tt;
        }

        private void AddTrendLineSeries()
        {
            _tl_cnt += 1;

            string tl_cn = "TrendLine" + _tl_cnt;
            int k1 = Math.Min(_w_s, _w_e);
            int k2 = Math.Max(_w_s, _w_e);
            double x1 = _tl_e[k1];
            double x2 = _tl_e[k2];

            _candles.Columns.Add(tl_cn, typeof(double));
            for (int i = 0; i <= k2 - k1; i++)
            {
                if (k1 + i < _candles.Rows.Count)
                    _candles.Rows[k1 + i][tl_cn] = x1 + i * (x2 - x1) / (k2 - k1);
            }

            Series series_tl = new Series(tl_cn);
            series_tl.XValueMember = "DateTime";
            series_tl.YValueMembers = tl_cn;
            series_tl.YAxisType = AxisType.Primary;
            series_tl.ChartType = SeriesChartType.Line;
            series_tl.BorderWidth = 2;
            series_tl.Color = ToolStripButtonBLR.ForeColor;
            _chart.Series.Add(series_tl);
            _chart.DataBind();
            _tl_rr.Add(tl_cn, series_tl);
        }

        private void AddRatioSeries(bool create)
        {
            if (create)
                _rr_cnt += 1;

            string rr_cn = "Ratio" + _rr_cnt;
            int k1 = Math.Min(_w_s, _w_e);
            int k2 = Math.Max(_w_s, _w_e);
            double x1 = _tl_e[k1];
            double x2 = _tl_e[k2];

            if (create)
            {
                _candles.Columns.Add(rr_cn, typeof(double));
                _candles.Columns.Add(rr_cn + "_Label", typeof(string));
            }

            for (int i = 0; i <= k2 - k1; i++)
            {
                _candles.Rows[k1 + i][rr_cn] = x1 + i * (x2 - x1) / (k2 - k1);
            }

            if (create)
            {
                _candles.Rows[k1][rr_cn + "_Label"] = String.Format("{0:N2}", _candles.Rows[k1][rr_cn]);
                _candles.Rows[k2][rr_cn + "_Label"] = String.Format("{0:N2}", _candles.Rows[k2][rr_cn]);

                Series series_rr = new Series(rr_cn);
                series_rr.XValueMember = "DateTime";
                series_rr.YValueMembers = rr_cn;
                series_rr.YAxisType = AxisType.Primary;
                series_rr.ChartType = SeriesChartType.Line;
                series_rr.BorderWidth = 2;
                series_rr.Color = ToolStripButtonBLR.ForeColor;
                series_rr.LabelForeColor = ToolStripButtonBLR.ForeColor;
                _chart.Series.Add(series_rr);
                _tl_rr.Add(rr_cn, series_rr);
            }
            else
            {
                _candles.Rows[k2][rr_cn + "_Label"] = String.Format("{0:N2} / {1:N2}", _r_ratio, _candles.Rows[k2][rr_cn]);
                SetOBOSLines(x2 + 161.8 * (x2 - x1) / _r_ratio, x2 + 261.8 * (x2 - x1) / _r_ratio, true, true);
            }
            _chart.Series[rr_cn].Points.DataBind(_candles.DefaultView, "DateTime", rr_cn, "Label=" + rr_cn + "_Label");
        }

        private void DeleteLineToSeries(Series s)
        {
            string sn = s.Name;
            _chart.Series.Remove(s);
            _candles.Columns.Remove(sn);
            if (sn.Contains("Ratio")) _candles.Columns.Remove(sn + "_Label");
            _chart.Invalidate();
        }

        private double GetRetraceRatio()
        {
            double r = 0;
            try
            {
                r = _chart.ChartAreas[0].AxisY.PixelPositionToValue(_point.Y) - _chart.ChartAreas[0].AxisY.PixelPositionToValue(_point_se.Y);
                r = (r / _y_offset) * 100;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[CChartPanel.GetRetraceRatio()] exception\r\n" + ex.Message);
            }
            return r;
        }

        private void OnAnalysisMode(System.Object Sender, System.EventArgs e)
        {
            OnButtonAMode((WNFA)((ToolStripItem)Sender).Tag, null);
        }

        private void OnAbout(System.Object Sender, System.EventArgs e)
        {

        }

        private void InitChart()
        {
            var _with1 = _chart.ChartAreas[0];
            _with1.AxisX.MajorGrid.Enabled = false;
            _with1.CursorX.AutoScroll = true;
            _with1.AxisX.ScaleView.Zoomable = true;
            _with1.AxisX.ScaleView.SizeType = DateTimeIntervalType.Auto;
            _with1.AxisX.ScaleView.Zoom(0, _czoom);
            _with1.AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            _with1.AxisX.ScaleView.SmallScrollSize = _czoom;
            _with1.AxisY.MajorGrid.Enabled = false;
            _with1.AxisY.LabelStyle.Format = "N0";
            _with1.AxisY2.MajorGrid.Enabled = false;
            _with1.AxisY2.LabelStyle.Enabled = false;
            _with1.CursorX.LineWidth = 1;
            _with1.CursorX.LineDashStyle = ChartDashStyle.DashDot;
            _with1.CursorX.LineColor = Color.Red;
            _with1.CursorX.SelectionColor = Color.Yellow;
            _with1.CursorY.LineWidth = 1;
            _with1.CursorY.LineDashStyle = ChartDashStyle.DashDot;
            _with1.CursorY.LineColor = Color.Red;
            _with1.CursorY.Interval = 0;

            _line_os.BorderColor = Color.Black;
            _line_os.IntervalOffset = -100;
            _line_os.StripWidth = 0;
            _line_os.BorderWidth = 1;
            _line_os.BorderDashStyle = ChartDashStyle.Dot;
            _line_ob.BorderColor = Color.Black;
            _line_ob.IntervalOffset = 100;
            _line_ob.StripWidth = 0;
            _line_ob.BorderWidth = 1;
            _line_ob.BorderDashStyle = ChartDashStyle.Dot;

            _man_los.BorderColor = Color.Red;
            _man_los.IntervalOffset = -100;
            _man_los.StripWidth = 0;
            _man_los.BorderWidth = 1;
            _man_los.BorderDashStyle = ChartDashStyle.Dot;
            _man_lob.BorderColor = Color.Red;
            _man_lob.IntervalOffset = 100;
            _man_lob.StripWidth = 0;
            _man_lob.BorderWidth = 1;
            _man_lob.BorderDashStyle = ChartDashStyle.Dot;

            _dLabel.Parent = SplitContainer1.Panel2;
            _dLabel.TextAlign = ContentAlignment.MiddleCenter;
            _dLabel.BringToFront();
            _dLabel.Visible = false;
            _pLabel.Parent = SplitContainer1.Panel2;
            _pLabel.Width = Convert.ToInt32(_lbl_w * 0.8);
            _pLabel.TextAlign = ContentAlignment.MiddleCenter;
            _pLabel.BringToFront();
            _pLabel.Visible = false;
            _aLabel.Parent = SplitContainer1.Panel2;
            _aLabel.Width = Convert.ToInt32(_lbl_w * 0.8);
            _aLabel.TextAlign = ContentAlignment.MiddleLeft;
            _aLabel.BringToFront();
            _aLabel.Visible = true;

            DisplayChart();

            _hScrollBar.Minimum = 10;
            _hScrollBar.Maximum = _candles.Rows.Count - 1;
            _hScrollBar.Value = Math.Min(_czoom, _hScrollBar.Maximum);

            _info.InitialDelay = 0;
            _info.ShowAlways = true;
            _have_mouse = false;
        }

        private void OnButtonPrint(bool toImage = false)
        {
            if (toImage)
            {
                string hpath = Properties.Settings.Default.imgPath;
                string fn = string.Format("{0}{1}_{2}_{3}.jpg", hpath.Replace("file:\\", ""), _symbol, _period.ToString(),
                                          DateTime.Now.ToString().Replace(":", string.Empty).Replace("-", string.Empty));
                Bitmap bmp = new Bitmap(_chart.Width, _chart.Height);
                _chart.DrawToBitmap(bmp, _chart.DisplayRectangle);
                bmp.Save(fn, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else
            {
                PrintDialog printDlg = new PrintDialog();
                PrintDocument printDoc = new PrintDocument();
                printDoc = _chart.Printing.PrintDocument;

                printDoc.DocumentName = _title;
                printDlg.Document = printDoc;
                printDlg.Document.DefaultPageSettings.Landscape = true;
                printDlg.Document.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(0, 30, 0, 0);
                printDlg.AllowSelection = true;
                printDlg.AllowSomePages = true;

                if ((printDlg.ShowDialog() == DialogResult.OK))
                    printDoc.Print();
            }
        }

        private void ToolStripOutMode_ButtonClick(object sender, EventArgs e)
        {
            OnButtonPrint(ToolStripOutMode.Text == ToImageToolStripMenuItem.Text);
        }

        private void ToolStripOutMode_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string m = e.ClickedItem.Text;
            ToolStripOutMode.Image = e.ClickedItem.Image;
            ToolStripOutMode.Text = m;
            ToolStripOutMode_ButtonClick(ToolStripOutMode, null);
        }

        private void Chart_AxisViewChanged(object sender, ViewEventArgs e)
        {
            string cn = string.Empty;
            _have_mouse = false;
            if (_chart.Series.Count > 1) cn = _chart.Series[_chart.Series.Count - 1].Name;
            _rescaleAxisY(cn);
        }

        private void Chart_Click(object sender, EventArgs e)
        {
            if (_d_m != DMode.SelectWave || _obj == null || (_pseries_idx != PriSeries.ESPN && _pseries_idx != PriSeries.ESPNfr))
                return;

            Point p = _chart.PointToClient(new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
            HitTestResult result = _chart.HitTest(p.X, p.Y);

            if (result.ChartElementType == ChartElementType.DataPointLabel)
            {
                TechnicalIndicator indi = _obj[(WNFA)Enum.Parse(typeof(WNFA), (string)ToolStripAMode.Tag)].Indicator;
                MacroWave ew = (indi.Type == TI.ESPN) ? ((TiESPN)indi).Analyzer : ((TiESPNfr)indi).Analyzer;
                ElliottWave li = ew.mw_list[result.PointIndex];
                string info = li.get_mDS(); if (string.IsNullOrEmpty(info)) return;

                info = info.Trim(); if (string.IsNullOrEmpty(info)) return;
                if (SplitContainer2.Panel2Collapsed) SplitContainer2.Panel2Collapsed = false;
                LabelSPNotations.Location = new Point((int)_chart.ChartAreas[0].AxisX.ValueToPixelPosition(_chart.ChartAreas[0].AxisX.ScaleView.Position - 1), 5);
                LabelSPNotations.Text = info.Replace(" ※", "\r\n※") + "\r\n\r\n";
            }
        }

        private void Chart_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_d_m == DMode.SelectWave) return;

            HitTestResult htr = _chart.HitTest(e.Location.X, e.Location.Y);
            if (htr.ChartElementType == ChartElementType.DataPoint)
            {
                foreach (Series s in _tl_rr.Values)
                {
                    if (s.Points.Contains((DataPoint)htr.Object))
                    {
                        DeleteLineToSeries(s);
                        return;
                    }
                }
            }
        }

        private void Chart_MouseDown(object sender, MouseEventArgs e)
        {
            if (!SplitContainer2.Panel2Collapsed) SplitContainer2.Panel2Collapsed = true;

            if (e.Button == MouseButtons.Right)
            {
                int px = e.Location.X;
                int py = e.Location.Y;

                ContextMenuStrip1.Items.Clear();
                foreach (WNFA i in Enum.GetValues(typeof(WNFA)))
                {
                    var _with1 = ContextMenuStrip1.Items.Add("Analysis Mode " + i.ToString(), null, OnAnalysisMode);
                    _with1.Tag = i;
                }
                ContextMenuStrip1.Items.Add(new ToolStripSeparator());
                ContextMenuStrip1.Items.Add("Dump Chart", null, ToolStripOutMode_ButtonClick);
                ContextMenuStrip1.Items.Add("Print", null, ToolStripOutMode_ButtonClick);
                ContextMenuStrip1.Items.Add(new ToolStripSeparator());
                ContextMenuStrip1.Items.Add("About", null, OnAbout);
                ContextMenuStrip1.Show(_chart, new System.Drawing.Point(px, py));

            }
            else
            {
                HitTestResult htr = _chart.HitTest(e.Location.X, e.Location.Y);
                if (htr.ChartElementType == ChartElementType.ScrollBarThumbTracker)
                    return;

                _point_ss.X = e.Location.X;
                _point_ss.Y = e.Location.Y;

                switch (_d_m)
                {
                    case DMode.SelectWave:
                        _chart.ChartAreas[0].CursorX.SetCursorPixelPosition(_point, true);

                        try
                        {
                            _w_s = Convert.ToInt32(_chart.ChartAreas[0].CursorX.Position) - 1;
                        }
                        catch (OverflowException ex)
                        {
                            Console.WriteLine("[CChartPanel.Chart_MouseDown()] exception\r\n" + ex.Message);
                        }

                        _chart.ChartAreas[0].CursorX.SetSelectionPixelPosition(default(PointF), default(PointF), false);
                        _chart.ChartAreas[0].CursorX.IsUserEnabled = true;
                        _chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;

                        break;
                    case DMode.Line:
                        _chart.ChartAreas[0].CursorY.SetCursorPixelPosition(_point, true);
                        _chart.ChartAreas[0].CursorX.SetCursorPixelPosition(_point, true);

                        try
                        {
                            _w_s = Convert.ToInt32(_chart.ChartAreas[0].CursorX.Position) - 1;
                        }
                        catch (OverflowException ex)
                        {
                            Console.WriteLine("[CChartPanel.Chart_MouseDown()] exception\r\n" + ex.Message);
                        }

                        _pt_s.Add(_pt_s.Count, _point_ss);
                        _pt_e.Add(_pt_e.Count, _point_ss);
                        _tl_e = new Dictionary<int, double>();
                        _tl_e.Add(_w_s, _chart.ChartAreas[0].CursorY.Position);

                        break;
                    case DMode.RetraceRatio:
                        _chart.ChartAreas[0].CursorY.SetCursorPixelPosition(_point, true);
                        _chart.ChartAreas[0].CursorX.SetCursorPixelPosition(_point, true);

                        if (_point_se.IsEmpty)
                        {
                            try
                            {
                                _w_s = Convert.ToInt32(_chart.ChartAreas[0].CursorX.Position) - 1;
                            }
                            catch (OverflowException ex)
                            {
                                Console.WriteLine("[CChartPanel.Chart_MouseDown()] exception\r\n" + ex.Message);
                            }

                            _pt_s.Add(_pt_s.Count, _point_ss);
                            _pt_e.Add(_pt_e.Count, _point_ss);
                            _tl_e = new Dictionary<int, double>();
                            _tl_e.Add(_w_s, _chart.ChartAreas[0].CursorY.Position);
                        }

                        break;
                }

                _have_mouse = true;
            }
        }

        private void Chart_MouseEnter(object sender, EventArgs e)
        {
            if (_d_m == DMode.SelectWave)
            {
                _dLabel.Visible = true;
                _pLabel.Visible = true;

                _chart.ChartAreas[0].CursorX.LineDashStyle = ChartDashStyle.Dot;
                _chart.ChartAreas[0].CursorY.LineDashStyle = ChartDashStyle.Dot;
            }
        }

        private void Chart_MouseLeave(object sender, EventArgs e)
        {
            _dLabel.Visible = false;
            _pLabel.Visible = false;

            _chart.ChartAreas[0].CursorX.LineDashStyle = ChartDashStyle.NotSet;
            _chart.ChartAreas[0].CursorY.LineDashStyle = ChartDashStyle.NotSet;
        }

        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            Graphics go = _chart.CreateGraphics();
            Pen p = new Pen(Color.Black);

            _point.X = e.Location.X;
            _point.Y = e.Location.Y;

            if (_have_mouse)
            {
                _chart.Invalidate();
                switch (_d_m)
                {
                    case DMode.SelectWave:
                        _chart.ChartAreas[0].CursorX.SetSelectionPixelPosition(_point_ss, _point, true);
                        _chart.ChartAreas[0].CursorX.SelectionColor = Color.Gray;
                        break;

                    case DMode.Line:
                        p.Width = 1.5f;
                        go.DrawLine(p, _point_ss, _point);
                        p.Dispose();
                        go.Dispose();
                        break;

                    case DMode.RetraceRatio:
                        p.Width = 1.5f;

                        if (_point_se.IsEmpty)
                            go.DrawLine(p, _point_ss, _point);
                        else
                        {
                            go.DrawLine(p, _point_ss, _point_se);
                            _r_ratio = GetRetraceRatio();
                            _info.SetToolTip(this._chart, _r_ratio.ToString("N1") + " / " + _chart.ChartAreas[0].AxisY.PixelPositionToValue(_point.Y).ToString("N2"));
                        }

                        p.Dispose();
                        go.Dispose();
                        break;
                }
            }
            else
            {
                _info.SetToolTip(this._chart, string.Empty);
                if (_d_m == DMode.SelectWave)
                {
                    try
                    {
                        _dLabel.Visible = true;
                        _pLabel.Visible = true;

                        _chart.ChartAreas[0].CursorY.SetCursorPixelPosition(_point, true);
                        _chart.ChartAreas[0].CursorX.SetCursorPixelPosition(_point, true);

                        if (_chart.ChartAreas[0].CursorX.Position > 0 & _chart.ChartAreas[0].CursorX.Position <= _candles.Rows.Count)
                        {
                            string d = (string)_candles.Rows[Convert.ToInt32(_chart.ChartAreas[0].CursorX.Position) - 1]["DateTime"];
                            _dLabel.Text = d;
                            _dLabel.Location = new Point(Convert.ToInt32(_point.X - _dLabel.Width / 2), Convert.ToInt32(_chart.Height * 0.93));

                            double v = _chart.ChartAreas[0].CursorY.Position;
                            int px = Convert.ToInt32(_chart.Width * 0.04);
                            if (v >= 1000000)
                                px += 20;
                            else if (v >= 100000)
                                px += 10;
                            else if (v < 1000)
                                px -= 15;

                            _pLabel.Text = v.ToString("N2");
                            _pLabel.Location = new Point(px, Convert.ToInt32(_point.Y - _pLabel.Height / 2));
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine("[CChartPanel.Chart_MouseMove()] exception\r\n" + ex.Message);
                    }
                    _chart.Invalidate();
                }
            }
        }

        private void Chart_MouseUp(object sender, MouseEventArgs e)
        {
            _point.X = e.Location.X;
            _point.Y = e.Location.Y;

            switch (_d_m)
            {
                case DMode.SelectWave:
                    _chart.ChartAreas[0].CursorX.SetCursorPixelPosition(_point, true);

                    try
                    {
                        _w_e = Convert.ToInt32(_chart.ChartAreas[0].CursorX.Position) - 1;
                    }
                    catch (OverflowException ex)
                    {
                        Console.WriteLine("[CChartPanel.Chart_MouseUp()] exception\r\n" + ex.Message);
                    }
                    if (_have_mouse && _w_s != _w_e)
                    {
                        double x1 = 0;
                        double x2 = 0;
                        x1 = _chart.Series[0].Points[_w_s].GetValueByName("Y");
                        x2 = _chart.Series[0].Points[_w_e].GetValueByName("Y");
                        SetOBOSLines(x2 - 0.382 * (x2 - x1), x2 - 0.618 * (x2 - x1), true, true);
                        AnalyzePolyWave();
                    }

                    _chart.ChartAreas[0].CursorX.IsUserEnabled = false;
                    _chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = false;
                    _have_mouse = false;

                    break;
                case DMode.Line:
                    try
                    {
                        _chart.ChartAreas[0].CursorY.SetCursorPixelPosition(_point, true);
                        _chart.ChartAreas[0].CursorX.SetCursorPixelPosition(_point, true);

                        _w_e = Convert.ToInt32(_chart.ChartAreas[0].CursorX.Position) - 1;
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine("[CChartPanel.Chart_MouseUp()] exception\r\n" + ex.Message);
                    }
                    catch (OverflowException ex)
                    {
                        Console.WriteLine("[CChartPanel.Chart_MouseUp()] exception\r\n" + ex.Message);
                    }
                    if (_w_s != _w_e)
                    {
                        _tl_e.Add(_w_e, _chart.ChartAreas[0].CursorY.Position);
                        AddTrendLineSeries();
                    }

                    _have_mouse = false;
                    _pt_s = new Dictionary<int, PointF>();
                    _pt_e = new Dictionary<int, PointF>();

                    break;
                case DMode.RetraceRatio:
                    try
                    {
                        _chart.ChartAreas[0].CursorY.SetCursorPixelPosition(_point, true);
                        _chart.ChartAreas[0].CursorX.SetCursorPixelPosition(_point, true);

                        _w_e = Convert.ToInt32(_chart.ChartAreas[0].CursorX.Position) - 1;
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine("[CChartPanel.Chart_MouseUp()] exception\r\n" + ex.Message);
                    }
                    catch (OverflowException ex)
                    {
                        Console.WriteLine("[CChartPanel.Chart_MouseUp()] exception\r\n" + ex.Message);
                    }

                    if (_point_se.IsEmpty)
                    {
                        _point_se = new PointF(2, 2);
                        _point_se.X = e.Location.X;
                        _point_se.Y = e.Location.Y;
                        _y_offset = _chart.ChartAreas[0].AxisY.PixelPositionToValue(_point_se.Y) - _chart.ChartAreas[0].AxisY.PixelPositionToValue(_point_ss.Y);

                        if (_w_s != _w_e)
                        {
                            _tl_e.Add(_w_e, _chart.ChartAreas[0].CursorY.Position);
                            Chart_Paint(this, null);
                            AddRatioSeries(true);

                            _w_s = _w_e;
                            _pt_s[_pt_s.Count - 1] = _point;
                            _tl_e = new Dictionary<int, double>();
                            _tl_e.Add(_w_s, _chart.ChartAreas[0].CursorY.Position);

                            _chart.Paint -= Chart_Paint;
                        }
                        else
                        {
                            _have_mouse = false;
                            _point_se = new PointF();
                            _pt_s = new Dictionary<int, PointF>();
                            _pt_e = new Dictionary<int, PointF>();
                            _tl_e = new Dictionary<int, double>();
                        }
                    }
                    else
                    {
                        if (_w_s != _w_e)
                        {
                            _tl_e.Add(_w_e, _chart.ChartAreas[0].CursorY.Position);
                            AddRatioSeries(false);
                        }
                        _chart.Paint += Chart_Paint;
                        _have_mouse = false;
                        _point_se = new PointF();
                        _pt_s = new Dictionary<int, PointF>();
                        _pt_e = new Dictionary<int, PointF>();
                        _tl_e = new Dictionary<int, double>();
                    }

                    break;
            }
        }

        private void Chart_MouseWheel(object sender, MouseEventArgs e)
        {
            int noc = Convert.ToInt32(ToolStripNoC.Text);

            noc += e.Delta / Math.Abs(e.Delta);
            noc = Math.Min(Math.Max(noc, 1), _candles.Rows.Count - 1);
            ToolStripNoC.Text = Convert.ToString(noc);
        }

        private void Chart_Paint(object sender, PaintEventArgs e)
        {
            Graphics go = _chart.CreateGraphics();
            Pen p = new Pen(Color.Gray);
            p.Width = 2f;

            foreach (int i in _pt_s.Keys)
            {
                go.DrawLine(p, _pt_s[i], _pt_e[i]);
            }

            p.Dispose();
            go.Dispose();
        }

        private void ToolStripNoC_TextChanged(object sender, EventArgs e)
        {
            string cn;
            int c = _czoom;
            if (ToolStripNoC.Text == string.Empty) return;
            if (int.TryParse(ToolStripNoC.Text, out c) && c >= 10)
            {
                _czoom = c;
                _hScrollBar.ValueChanged -= _hScrollBar_ValueChanged;
                _hScrollBar.Value = _czoom;
                _hScrollBar.ValueChanged += _hScrollBar_ValueChanged;

                _chart.ChartAreas[0].AxisX.ScaleView.Zoom(0, _czoom);
                _chart.ChartAreas[0].AxisX.ScaleView.SmallScrollSize = _czoom;
                _chart.ChartAreas[0].AxisX.ScaleView.Position = _chart.Series[0].Points.Count - _czoom + 1;

                if (_chart.Series.Count > 1)
                {
                    cn = _chart.Series[_chart.Series.Count - 1].Name;
                    if (cn.EndsWith("_SIG")) cn = _chart.Series[_chart.Series.Count - 2].Name;
                    _rescaleAxisY(cn);
                }
            }
        }

        private void _hScrollBar_ValueChanged(object sender, EventArgs e)
        {
            ToolStripNoC.Text = _hScrollBar.Value.ToString();
        }

        private void ToolStripSymbol_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ToolStripButtonBLR_Click(object sender, EventArgs e)
        {
            _d_m = (DMode)(((int)_d_m + 1) % Enum.GetNames(typeof(DMode)).Length);
            switch (_d_m)
            {
                case DMode.SelectWave:
                    ToolStripButtonBLR.Text = "SP Notation";
                    ToolStripButtonColor.Enabled = false;
                    _chart.Cursor = Cursors.Arrow;
                    break;
                case DMode.Line:
                    ToolStripButtonBLR.Text = "Trend Line";
                    ToolStripButtonColor.Enabled = true;
                    _chart.Cursor = Cursors.Cross;
                    break;
                case DMode.RetraceRatio:
                    ToolStripButtonBLR.Text = "Ratio";
                    ToolStripButtonColor.Enabled = true;
                    _chart.Cursor = Cursors.Cross;
                    break;
            }
        }

        private void ToolStripAMode_ButtonClick(object sender, EventArgs e)
        {
            ToolStripMenuItem m = default(ToolStripMenuItem);

            if (ToolStripAMode.Text == ESPNToolStripMenuItem.Text)
                m = ESPNfrToolStripMenuItem;
            else if (ToolStripAMode.Text == ESPNfrToolStripMenuItem.Text)
                m = PFTWToolStripMenuItem;
            else
                m = ESPNToolStripMenuItem;

            OnButtonAMode((WNFA)Enum.Parse(typeof(WNFA), m.Tag.ToString()), e);

            ToolStripAMode.Image = m.Image;
            ToolStripAMode.Text = m.Text;
            ToolStripAMode.Tag = m.Tag;
        }

        private void ToolStripAMode_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string m = e.ClickedItem.Text;
            if (ToolStripAMode.Text == m && m != ESPNToolStripMenuItem.Text) return;

            _chart.MouseMove -= Chart_MouseMove;
            System.Windows.Forms.Cursor.Position = new Point(System.Windows.Forms.Cursor.Position.X,
                                                             (int)(this.ParentForm.Location.Y + SystemInformation.ToolWindowCaptionHeight + 2.5 * ToolStripPeriod.Height));

            ToolStripAMode.Image = e.ClickedItem.Image;
            ToolStripAMode.Text = m;
            OnButtonAMode((WNFA)Enum.Parse(typeof(WNFA), e.ClickedItem.Tag.ToString()), e);

            _chart.MouseMove += Chart_MouseMove;
        }

        private void ToolStripNoC_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int i = -1;
                if (int.TryParse(((ToolStripTextBox)sender).Text, out i) && i > 0)
                    ToolStripNoC.Text = i.ToString();
            }
        }

        private void ToolStripButtonC_Click(object sender, EventArgs e)
        {
            int enum_cnt = Enum.GetValues(typeof(CMode)).Length;
            _c_mod = (_c_mod + 1) % enum_cnt;
            ReCheck:
            switch ((CMode)_c_mod)
            {
                case CMode.Chart:
                    ToolStripButtonC.Checked = false;
                    DataGridViewC.Visible = false;
                    _chart.Visible = true;
                    _dLabel.Visible = true;
                    _pLabel.Visible = true;
                    ToolStripOutMode.Visible = true;
                    break;

                case CMode.FInfo:
                    _c_mod += 1;
                    goto ReCheck;

                case CMode.Table:
                    SplitContainer2.Panel2Collapsed = true;
                    ToolStripButtonC.Checked = true;
                    _chart.Visible = false;
                    DataGridViewC.Dock = DockStyle.Fill;
                    DataGridViewC.DataSource = _candles;
                    DataGridViewC.Visible = true;
                    DataGridViewC.FirstDisplayedScrollingRowIndex = DataGridViewC.RowCount - 1;
                    _dLabel.Visible = false;
                    _pLabel.Visible = false;
                    ToolStripOutMode.Visible = false;
                    break;

                case CMode.PInfo:
                    _c_mod = (_c_mod + 1) % enum_cnt;
                    goto ReCheck;
            }
        }

        private void ToolStripButtonColor_Click(object sender, EventArgs e)
        {
            ColorDialog MyDialog = new ColorDialog();
            MyDialog.AllowFullOpen = false;
            MyDialog.ShowHelp = true;
            MyDialog.Color = ToolStripButtonBLR.ForeColor;

            if ((MyDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK))
            {
                ToolStripButtonBLR.ForeColor = MyDialog.Color;
            }
        }

        private void CChartPanel_Load(object sender, EventArgs e)
        {
            _obj = new Dictionary<WNFA, IndicatorSignal>();
            _point = new PointF(2, 2);
            _point_ss = new PointF(2, 2);

            this._chart.MouseMove -= Chart_MouseMove;
            this.ToolStripNoC.TextChanged -= ToolStripNoC_TextChanged;

            if (!_mode_multi)
            {
                ToolStripPeriod.Enabled = true;
                ToolStripMinSel.Enabled = true;
                ToolStripSymbol.Enabled = true;
                ToolStripSymbol.Visible = true;

                if (_title != string.Empty)
                {
                    _symbol_dic[_title] = _symbol;
                    ToolStripSymbol.Items.Add(_title);
                    ToolStripSymbol.AutoCompleteCustomSource.Add(_title);
                    ToolStripSymbol.SelectedIndexChanged -= ToolStripSymbol_SelectedIndexChanged;
                    ToolStripSymbol.Text = _title;
                    ToolStripSymbol.SelectedIndexChanged += ToolStripSymbol_SelectedIndexChanged;
                }
            }

            LoadPeriods();
            LoadTIs();
            SetPeriod((CandlePeriod)_period);

            ToolStripNoC.Text = Convert.ToString(_czoom);
            ToolStripButtonColor.Enabled = false;
            DataGridViewC.Visible = false;
            InitChart();

            this._chart.MouseMove += Chart_MouseMove;
            this.ToolStripNoC.TextChanged += ToolStripNoC_TextChanged;
        }
    }
}

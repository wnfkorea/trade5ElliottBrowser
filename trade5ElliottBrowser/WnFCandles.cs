using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using WnFTechnicalIndicators;


namespace trade5ElliottBrowser
{
    public struct StockOHLCV
    {
        public string D;
        public double O;
        public double H;
        public double L;
        public double C;
        public double V;

        public StockOHLCV(string _d, double _o, double _v)
        {
            D = _d;
            O = _o;
            H = _o;
            L = _o;
            C = _o;
            V = _v;
        }

        public StockOHLCV(string _d, double _o, double _h, double _l, double _c, double _v)
        {
            D = _d;
            O = _o;
            H = _h;
            L = _l;
            C = _c;
            V = _v;
        }
    }

    public struct CandleMeta
    {
        public string C;
        public CommodityType K;
        public CandlePeriod P;

        public CandleMeta(string _s, CommodityType _k, CandlePeriod _p)
        {
            C = _s;
            K = _k;
            P = _p;
        }
    }


    public class WnFCandles : IDisposable
    {
        protected int _period;
        protected string _stockcode;
        protected CommodityType _ctype;
        protected StockOHLCV _last;
        protected Dictionary<string, List<IndicatorSignal>> _indicators;
        protected DataTable _dohlcv;
        protected string _tstr;

        #region " IDisposable Support "
        /// Keep track of when the object is disposed. 
        protected bool disposed = false;
        /// This method disposes the base object's resources. 
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    /// Insert code to free managed resources.
                    if ((_dohlcv != null)) _dohlcv.Dispose();
                }
                /// Insert code to free unmanaged resources. 
            }
            this.disposed = true;
        }

        /// Do not change or add Overridable to these methods. 
        /// Put cleanup code in Dispose(ByVal disposing As Boolean). 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        public WnFCandles(int p, string s)
        {
            if (!ValidPeriod(p)) throw new ArgumentException("Invalid candle period " + p);
            _period = p;
            _stockcode = s;
        }

        public WnFCandles(int p, string s, CommodityType k) : this(p, s)
        {
            _ctype = k;
        }

        public string Symbol
        {
            get { return _stockcode; }
        }

        public int Period
        {
            get { return _period; }
            set { _period = value; }
        }

        public DataTable DOHLCV
        {
            get { return _dohlcv; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Rows.Count == 0) throw new ArgumentException("Candles vacant");
                _dohlcv = value;
            }
        }

        public string Time
        {
            get { return _tstr; }
        }

        public static DataTable GetWithAutoInc(DataTable dt)
        {
            DataTable r = default(DataTable);
            DataColumn col = default(DataColumn);

            if (dt.Columns.Contains("row_num")) dt.Columns.Remove(dt.Columns["row_num"]);

            col = new DataColumn("row_num", typeof(Int32));
            col.AutoIncrement = true;
            col.AutoIncrementSeed = 0;

            r = new DataTable();
            r.Columns.Add(col);
            r.Load(new DataTableReader(dt));

            return r;
        }

        public static bool ValidPeriod(int p)
        {
            bool valid = true;
            if (!(p == 1 || p == 2 || p == 3 || p == 5 || p == 10 || p == 15 || p == 20 || p == 30 || p == 60 || p == 1440 || p == 10080 || p == 43200))
                valid = false;
            return valid;
        }

        public virtual int FillCandles(int n, string d = "")
        {
            if (_dohlcv == null) throw new InvalidOperationException("Candles nothing");
            if (n < 1) throw new ArgumentException();

            if (d != string.Empty)
            {
                DataRow _dr;
                _dohlcv = _dohlcv.Select("DateTime < '" + d + "'", string.Empty).CopyToDataTable();
                _dr = _dohlcv.Rows[_dohlcv.Rows.Count - 1];
                _last = new StockOHLCV((string)_dr["DateTime"],
                                       Convert.ToDouble(_dr["Open"]), Convert.ToDouble(_dr["High"]), Convert.ToDouble(_dr["Low"]), Convert.ToDouble(_dr["Close"]),
                                       Convert.ToDouble(_dr["Volume"]));
            }

            while (_dohlcv.Rows.Count > n)
                _dohlcv.Rows[0].Delete();

            return _dohlcv.Rows.Count;
        }

        public TechnicalIndicator Indicator(string iStr)
        {
            if (_indicators == null) throw new NullReferenceException();
            if (_indicators.ContainsKey(iStr))
                return _indicators[iStr][0].Indicator;
            else
                throw new ArgumentException();
        }

        public void AddIndicatorSignal(ref IndicatorSignal isig)
        {
            if (_indicators == null) _indicators = new Dictionary<string, List<IndicatorSignal>>();
            if (!_indicators.ContainsKey(isig.IStr)) _indicators[isig.IStr] = new List<IndicatorSignal>();

            foreach (IndicatorSignal i in _indicators[isig.IStr])
            {
                if (i.Same(isig))
                {
                    isig = i;
                    return;
                }
            }

            try
            {
                isig.Indicator = IndicatorSignal.IStrToObj(isig.TI, isig.IStr);
                isig.Indicator.Init(_dohlcv);
                if (isig.Indicator.GetType() == typeof(WnFTechnicalIndicators.TiESPNfr))
                {
                    ((WnFTechnicalIndicators.TiESPNfr)isig.Indicator).GetFrPrices();
                }

                _indicators[isig.IStr].Add(isig);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Exception at WnFChart.AddIndicatorSignal()\r\n" + ex.Message);
            }
        }

        public static string PeriodToString(CandlePeriod cp)
        {
            switch (cp)
            {
                case CandlePeriod.m1:
                    return "1-Minute";
                case CandlePeriod.m2:
                    return "2-Minute";
                case CandlePeriod.m3:
                    return "3-Minute";
                case CandlePeriod.m5:
                    return "5-Minute";
                case CandlePeriod.m10:
                    return "10-Minute";
                case CandlePeriod.m15:
                    return "15-Minute";
                case CandlePeriod.m20:
                    return "20-Minute";
                case CandlePeriod.m30:
                    return "30-Minute";
                case CandlePeriod.m60:
                    return "60-Minute";
                case CandlePeriod.D:
                    return "Daily";
                case CandlePeriod.W:
                    return "Weekly";
                case CandlePeriod.M:
                    return "Monthly";
                default:
                    return string.Empty;
            }
        }
    }
}

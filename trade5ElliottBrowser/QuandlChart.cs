using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WnFTechnicalIndicators;

namespace trade5ElliottBrowser
{
    public class QuandlChart : WnFCandles
    {

        public QuandlChart(int p, string dataset, string database, WnFDbConnectionWrapper _wrpper = null) : base(p, dataset)
        {
            _database = database;
            _api = (QuandlAPI)WnFElliottBrowser.Factory;
            _set_dbconn(_wrpper);
            _init_candles();
        }

        public QuandlChart(int p, string dataset, CommodityType k, string database, WnFDbConnectionWrapper _wrpper = null) : this(p, dataset, database, _wrpper)
        {
            _ctype = k;
        }


        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_dohlcv != null) _dohlcv.Dispose();
                    if (_dbc_wr != null && _dbc_wr_owner)
                    {
                        if (_dbc_wr.Connection != null) _dbc_wr.Connection.Close();
                        _dbc_wr = null;
                    }
                }
            }
            disposed = true;
        }


        private QuandlAPI _api;
        private string _database;
        private WnFDbConnectionWrapper _dbc_wr;
        private bool _dbc_wr_owner;
        private string _tname;

        public int GetCandles()
        {
            QuandlAPI.QuandlError err = default(QuandlAPI.QuandlError);
            DataTable dt = default(DataTable);
            bool append;
            string d1, d2;
            int no_candles = 0;
            string fstr;

            if (!_api.ReadURLString("Candles", out fstr))
            {
                MessageBox.Show("Error reading URL format string for candles.", Properties.Settings.Default.tm, MessageBoxButtons.OK);
                return (int)APIError.APIUrlFormat;
            }

            append = _is_append(out d1);
            d2 = DateTime.Today.ToString("yyyy-MM-dd");
            fstr = _api.Get_URL(_database, _stockcode, ((QuandlAPI.QuandlPeriod)_period).ToString(), d1, "{0}", "{1}");

            if (!append)
            {
                Console.WriteLine("[QuandlChart.GetCandles()] calling _store_candles async for " + _stockcode);
                d_store_candles d = _store_candles;
                d.BeginInvoke(fstr.Replace("&rows={1}", ""), d2, null, null);

                err = _api.GetCandlesTable(string.Format(fstr, "{0}", QuandlAPI._MAX_JSON), d2, out _dohlcv);
                return (err == null) ? 0 :err.ToInt();
            }
            else
            {
                if (DateTime.Parse(d2) < DateTime.Parse(d1)) d2 = string.Empty;

                err = _api.GetCandlesTable(fstr.Replace("&rows={1}", ""), d2, out dt, _tname);
                if (err != null)
                    return err.ToInt();

                no_candles = (dt.Rows.Count > 0) ? _dbc_wr.AppendTable(dt): 1;
                dt = null;
                return no_candles;
            }
        }

        public override int FillCandles(int n, string d = "")
        {
            int _ccnt = 0;
            _dohlcv = new DataTable();
            _ccnt = _dbc_wr.FillRows(_period, _tname, n, ref _dohlcv); if (_ccnt == 0) goto my_exit;

            try
            {
                DataRow dr = _dohlcv.Rows[_ccnt - 1];
                _last = new StockOHLCV(Convert.ToString(dr["DateTime"]),
                                       Convert.ToDouble(dr["Open"]), Convert.ToDouble(dr["High"]), Convert.ToDouble(dr["Low"]), Convert.ToDouble(dr["Close"]),
                                       Convert.ToDouble(dr["Volume"]));
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine("Exception at QuandlChart.FillCandles()\r\n" + ex.Message);
            }

        my_exit:
            return _ccnt;
        }

        private void _set_dbconn(WnFDbConnectionWrapper wrpper)
        {
            _dbc_wr_owner = (wrpper == null);
            if (wrpper == null)
            {
                _dbc_wr = WnFDbConnectionWrapper.GetWrapper((WnF_DBType)Properties.Settings.Default.dbms, string.Format(Properties.Settings.Default.dbConn, _database));
            }
            else
                _dbc_wr = wrpper;
        }

        private void _init_candles()
        {
            _last = _dbc_wr.CheckTable(_period, _stockcode, out _tname);
        }

        private bool _is_append(out string d1)
        {
            bool b = false;
            d1 = QuandlAPI._1ST_DATE;

            if (!string.IsNullOrEmpty(_last.D))
            {
                b = true;
                d1 = _last.D.Replace("/", "-");
            }

            return b;
        }

        delegate void d_store_candles(string fstr, string d2);

        private void _store_candles(string fstr, string d2)
        {
            DataTable dic = default(DataTable);
            QuandlAPI.QuandlError err = default(QuandlAPI.QuandlError);
            WnF_DBType dbt = (WnF_DBType)Properties.Settings.Default.dbms;
            string cstr;

            err = _api.GetCandlesTable(fstr, d2, out dic, _tname);
            if (err != null)
            {
                Console.WriteLine("[QuandlChart._store_candles()] GetCandlesTable failed...\r\n" + err.message);
                return;
            }

#if DEBUG
            dynamic watch = System.Diagnostics.Stopwatch.StartNew();
#endif
            cstr = string.Format(Properties.Settings.Default.dbConn, _database);
            using (WnFDbConnectionWrapper dbcw = WnFDbConnectionWrapper.GetWrapper(dbt, cstr))
                dbcw.InsertTable(dic);

#if DEBUG
            watch.Stop();
            Console.WriteLine("[QuandlChart._store_candles()] " + dic.Rows.Count + " ellapsed " + watch.ElapsedMilliseconds);
#endif
            dic = null;
        }

    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using WnFTechnicalIndicators;

namespace trade5ElliottBrowser
{
    public class QuandlAPI : IWnFOpenAPI
    {
        public enum QuandlPeriod : int
        {
            daily = 1440,
            weekly = 10080,
            monthly = 43200
        }

        [JsonConverter(typeof(TimeSeriesConverter))]
        public class TimeSeries
        {
            public string Date { get; set; }
            public double[] Values { get; set; }
        }

        class TimeSeriesConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(TimeSeries));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JArray ja = JArray.Load(reader);
                TimeSeries ts = new TimeSeries();
                ts.Date = (string)ja[0]; ja.RemoveAt(0);
                ts.Values = ja.Select(jv => (double)jv).ToArray();
                return ts;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                JArray ja = new JArray();
                TimeSeries ts = (TimeSeries)value;
                ja.Add(ts.Date);
                ja.Add(ts.Values);
                ja.WriteTo(writer);
            }
        }

        public class QuandlData
        {
            public string limit
            {
                get { return _limit; }
                set { _limit = value; }
            }

            public string transform
            {
                get { return _transform; }
                set { _transform = value; }
            }

            public int[] column_index
            {
                get { return _column_index; }
                set { _column_index = value; }
            }

            public string[] column_names
            {
                get { return _column_names; }
                set { _column_names = value; }
            }

            public string start_date
            {
                get { return _start_date; }
                set { _start_date = value; }
            }

            public string end_date
            {
                get { return _end_date; }
                set { _end_date = value; }
            }

            public string frequency
            {
                get { return _frequency; }
                set { _frequency = value; }
            }

            public TimeSeries[] data
            {
                get { return _data; }
                set { _data = value; }
            }

            public string collapse
            {
                get { return _collapse; }
                set { _collapse = value; }
            }

            public string order
            {
                get { return _order; }
                set { _order = value; }
            }

            private string _limit;
            private string _transform;
            private int[] _column_index;
            private string[] _column_names;
            private string _start_date;
            private string _end_date;
            private string _frequency;
            private TimeSeries[] _data;
            private string _collapse;
            private string _order;
        }

        public class QuandlDataWrapper
        {
            public QuandlData dataset_data
            {
                get { return _dataset_data; }
                set { _dataset_data = value; }
            }

            private QuandlData _dataset_data;
        }

        public class QuandlError
        {
            public string code
            {
                get { return _code; }
                set { _code = value; }
            }

            public string message
            {
                get { return _message; }
                set { _message = value; }
            }

            public int ToInt()
            {
                int e = (_code == "QECx02") ? (int)APIError.APIResponseErr : (int)APIError.Unknown;
                return e;
            }

            private string _code;
            private string _message;
        }

        public class QuandlException
        {
            public QuandlError quandl_error
            {
                get { return _error; }
                set { _error = value; }
            }

            private QuandlError _error;
        }

        
        public static string _PROVIDER = "Quandl-API";
        public static string _COMP = "NASDAQOMX/COMP";
        public static string _FB = "WIKI/FB";
        public static string _CdMASTER = "quandl_symbols";
        public static string _1ST_DATE = "1975-01-01";
        public static int _MAX_JSON = 600;
        public event ehVoid SymbolsUpdate;


        public QuandlAPI()
        {
            _check_home();
        }

        public string ProviderName()
        {
            return _PROVIDER;
        }

        public string Get_URL(string database, string dataset, string period, string start_date, string end_date, string nrows)
        {
            return string.Format(_url_fstr, database, dataset, period, start_date, end_date, nrows) + _key_postfix();
        }

        public QuandlError GetCandlesDic(string fstr, string end_date, out Dictionary<string, double[]> dic, int n = int.MinValue)
        {
            QuandlError err = default(QuandlError);
            QuandlDataWrapper result = default(QuandlDataWrapper);
            string req_url;
            int ccnt = 0, rcnt = 0;
            dic = new Dictionary<string, double[]>();

            if (n != int.MinValue)
            {
                n = Math.Max(n, _MAX_JSON); fstr += "&rows=" + n;
            }
            else
                n = int.MaxValue;

            req_url = string.Format(fstr, end_date);

            do
            {
                try
                {
                    result = (QuandlDataWrapper)WnFElliottBrowser.GetJSONObjects(req_url, typeof(QuandlDataWrapper));
                }
                catch (WebException ex)
                {
                    int status = (int)((HttpWebResponse)ex.Response).StatusCode;
                    err = GetQuandlError(ex);
                    Console.WriteLine("Exception at QuandlAPI.GetCandlesDic() GetJSONObjects returned {0} after {1} candles.", status, dic.Count);
                    if (dic.Count > 0) dic.Clear();
                    break;
                }

                rcnt += 1;
                ccnt = result.dataset_data.data.Length;
                foreach (TimeSeries l in result.dataset_data.data)
                {
                    end_date = l.Date;
                    dic[end_date] = l.Values;
                }

                req_url = string.Format(fstr, end_date);
            } while (ccnt >= n);

            result = null;
            return err;
        }

        public QuandlError GetCandlesTable(string fstr, string end_date, out DataTable dic, string tname = "")
        {
            QuandlError err = default(QuandlError);
            QuandlDataWrapper result = default(QuandlDataWrapper);
            DataRow r = default(DataRow);
            string req_url = string.Format(fstr, end_date);
            int ccnt = 0, rcnt = 0;
            dic = _candles_table(tname);

            try
            {
                result = (QuandlDataWrapper)WnFElliottBrowser.GetJSONObjects(req_url, typeof(QuandlDataWrapper));
            }
            catch (WebException ex)
            {
                int status = (int)((HttpWebResponse)ex.Response).StatusCode;
                err = GetQuandlError(ex);
                Console.WriteLine("Exception at QuandlAPI.GetCandlesTable() GetJSONObjects returned {0}", status);
            }

            rcnt += 1;
            ccnt = result.dataset_data.data.Length;
            for (int i = result.dataset_data.data.Length-1; i>= 0; i--)
            {
                TimeSeries v = result.dataset_data.data[i];
                r = dic.NewRow();
                r["DateTime"] = v.Date.Replace("-", "/");
                r["Open"] = v.Values[0];
                r["High"] = v.Values[1];
                r["Low"] = v.Values[2];
                r["Close"] = v.Values[3];
                r["Volume"] = v.Values[4];
                dic.Rows.Add(r);
            }

            result = null;
            return err;
        }

        public bool Check(out int _err, out WnFCandles _c)
        {
            _err = 0;
            _c = null;
            _akeyfile = string.Empty;
            _apikeys = LoadAPIKeys(out _akeyfile);
            _url_fstr = string.Empty;

            if (_akeyfile == string.Empty)
            {
                _err = (int)APIError.APIKeyFile;
                return false;
            }
            else if (_apikeys.Count == 0)
            {
                _err = (int)APIError.APIKeyLength;
                return false;
            }
            else if (!ReadURLString("Candles", out _url_fstr))
            {
                _err = (int)APIError.APIUrlFormat;
                return false;
            }
            else
            {
                string symbol, database;
                if(!ReadURLString("DatabaseDefault", out database) || !ReadURLString("DatasetDefault", out symbol))
                {
                    _err = (int)APIError.APIUrlFormat; return false;
                }

                _ch = new QuandlChart((int)CandlePeriod.D, symbol, database);
                _err = _ch.GetCandles();
                if (_err < 0)
                    return false;
                else if (_err > 0)
                    _ch.FillCandles(_MAX_JSON);

                if (_ch.DOHLCV.Rows.Count == 0)
                {
                    _err = (int)APIError.Unknown; return false;
                }
                else
                    _c = _ch;
            }

            return true;
        }

        public Dictionary<int, string> LoadAPIKeys(out string _p)
        {
            _p = _home + System.IO.Path.DirectorySeparatorChar + Properties.Settings.Default.keys_ini;
            if (!File.Exists(_p))
            {
                _p = string.Empty;
                return null;
            }
            else
                return WnFElliottBrowser.LoadAPIKeys(_p);
        }

        public string APIKey(int type = -1)
        {
            throw new NotImplementedException();
        }

        public int[] SupportedPeriods()
        {
            int[] arr = new int[(Enum.GetValues(typeof(QuandlPeriod))).Length];
            Enum.GetValues(typeof(QuandlPeriod)).CopyTo(arr, 0);
            return arr;
        }

        public bool ReadURLString(string k, out string fstr)
        {
            string p = _home + System.IO.Path.DirectorySeparatorChar + Properties.Settings.Default.urls_ini;
            fstr = WnFElliottBrowser.ReadURLString(p, k);
            return (fstr != string.Empty);
        }

        public void GetSectors(out Dictionary<string, string> dic, out int err)
        {
            throw new NotImplementedException();
        }

        public void RefreshSymbols(out Dictionary<string, string> dic, out int err)
        {
            string ppath, database, symbols_fstr;
            dic = new Dictionary<string, string>();
            err = 0;

            if (!ReadURLString("DatabaseDefault", out database) || !ReadURLString("Symbols", out symbols_fstr))
            {
                err = (int) APIError.APIUrlFormat; return;
            }

            ppath = Properties.Settings.Default.symbolsPath;
            if (!string.IsNullOrEmpty(ppath))
            {
                if (File.Exists(ppath) && !File.GetAttributes(ppath).HasFlag(FileAttributes.Directory))
                {
                    string[] parr = ppath.Split(Path.DirectorySeparatorChar);
                    if (parr[parr.Length - 2] == database)
                    {
                        using (Stream input = File.OpenRead(ppath))
                        {
                            ZipArchive zipArchive = new ZipArchive(input, ZipArchiveMode.Read);
                            dic = _zipped_CSV_to_dic(zipArchive);
                        }

                        d_refreshSymbols d = _refresh_symbols;
                        d.BeginInvoke(symbols_fstr, database, ref dic, ref err, null, null);
                        err = 0; return;
                    }
                }
            }

            _refresh_symbols(symbols_fstr, database, ref dic, ref err);
        }

        public string SymbolToName(string s, CommodityType type = CommodityType.None)
        {
            throw new NotImplementedException();
        }

        public string SymbolByName(string n, CommodityType type = CommodityType.None)
        {
            throw new NotImplementedException();
        }

        public WnFCandles GetCandles(string s, int p, CommodityType type = CommodityType.None, WnFDbConnectionWrapper wrpper = null)
        {
            QuandlChart ch = new QuandlChart(p, s, "WIKI", wrpper);
            int err = ch.GetCandles();
            if (err < 0)
                ch.FillCandles(_MAX_JSON);
            else if (err > 0)
                ch.FillCandles(_MAX_JSON);
            return ch;
        }

        public void GetCandles(string s, int p, CommodityType type, ref Dictionary<CandlePeriod, WnFCandles> dic, WnFDbConnectionWrapper wrpper = null)
        {
            WnFCandles ch = new QuandlChart(p, s, "WIKI", wrpper);
            if (((QuandlChart)ch).GetCandles() > 0)
                ch.FillCandles(_MAX_JSON);
            dic[(CandlePeriod)p] = ch;
        }

        public int QuoteItemsToDictionary(string[] c, out Dictionary<int, int> dic)
        {
            int r = 0;
            dic = new Dictionary<int, int>();

            for (int i = 0; i < c.Length; i++)
            {
                int idx = 0;
                int k = 0;

                switch (c[i])
                {
                    case "Date":
                    case "Trade Date":
                        k = 1;
                        break;
                    case "Open":
                        k = 2;
                        break;
                    case "High":
                        k = 4;
                        break;
                    case "Low":
                        k = 8;
                        break;
                    case "Close":
                    case "Last":
                    case "Current":
                    case "Index Value":
                        k = 16;
                        break;
                    case "Volume":
                        k = 32;
                        break;
                }

                if (k > 0)
                {
                    r += k;
                    idx = Convert.ToInt32(Math.Log(k, 2));
                    dic[idx] = i;
                }
            }
            return r;
        }

        public static QuandlError GetQuandlError(WebException ex)
        {
            QuandlException e = default(QuandlException);
            QuandlError er = default(QuandlError);
            string txt = null;

            using (Stream s = ex.Response.GetResponseStream())
            {
                using (StreamReader r = new StreamReader(s))
                {
                    txt = r.ReadToEnd();
                }
            }

            e = (QuandlException)JsonConvert.DeserializeObject(txt, typeof(QuandlException));
            if (e != null) er = e.quandl_error;
            return er;
        }


        private string _home;
        private string _akeyfile;
        private Dictionary<int, string> _apikeys;
        private string _url_fstr;
        private QuandlChart _ch;


        private void _check_home()
        {
            _home = WnFElliottBrowser.HomePath + System.IO.Path.DirectorySeparatorChar + _PROVIDER;
            if (!System.IO.Directory.Exists(_home)) System.IO.Directory.CreateDirectory(_home);
        }

        private string _key_postfix(int type = -1)
        {
            return "&api_key=" + _apikeys[0];
        }

        private DataTable _candles_table(string tname = "")
        {
            DataColumn col;
            DataTable dohlcv = new DataTable();
            if (string.IsNullOrEmpty(tname))
            {
                col = new DataColumn("row_num", typeof(Int32));
                col.AutoIncrement = true;
                col.AutoIncrementSeed = 0;
                dohlcv.Columns.Add(col);
            }
            else
                dohlcv.TableName = tname;

            dohlcv.Columns.Add("DateTime", typeof(string));
            dohlcv.Columns.Add("Open", typeof(double));
            dohlcv.Columns.Add("High", typeof(double));
            dohlcv.Columns.Add("Low", typeof(double));
            dohlcv.Columns.Add("Close", typeof(double));
            dohlcv.Columns.Add("Volume", typeof(double));
            dohlcv.PrimaryKey = new DataColumn[] { dohlcv.Columns["DateTime"] };
            return dohlcv;
        }

        private Dictionary<string, string> _zipped_CSV_to_dic(ZipArchive za)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            using (var unzipped = za.Entries[0].Open())
            {
                using (var reader = new StreamReader(unzipped, Encoding.ASCII))
                {
                    string line, symbol, name;
                    string[] arr, arr1;
                    dic = new Dictionary<string, string>();

                    while ((line = reader.ReadLine()) != null)
                    {
                        arr = line.Trim().Split(',');
                        arr1 = arr[0].Split('/');
                        symbol = arr1.Length > 1 ? arr1[1] : arr[0];
                        name = line.Replace(arr[0] + ",", string.Empty).Replace("\"", string.Empty)
                                   .Replace("Prices, Dividends, Splits and Trading Volume", string.Empty).Trim();
                        dic[symbol] = name;
                    }
                }
            }
            return dic;
        }

        delegate void d_refreshSymbols(string symbols_fstr, string database, ref Dictionary<string, string> dic, ref int err);

        private void _refresh_symbols(string symbols_fstr, string database, ref Dictionary<string, string> dic, ref int err)
        {
            string url, path;
            HttpWebResponse response = default(HttpWebResponse);
            url = string.Format(symbols_fstr, database);

            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                response = webRequest.GetResponse() as HttpWebResponse;

                if (response.ContentType == "application/zip")
                {
                    bool to_serialize = true, overwrite = false;
                    path = _home + Path.DirectorySeparatorChar + database; if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    path += Path.DirectorySeparatorChar + response.ResponseUri.AbsolutePath.Substring(1);
                    Properties.Settings.Default.symbolsPath = path; Properties.Settings.Default.Save();

                    using (var mstr = new MemoryStream())
                    using (var str = response.GetResponseStream())
                    {
                        str.CopyTo(mstr);
                        ZipArchive zipArchive = new ZipArchive(mstr, ZipArchiveMode.Read);

                        if (File.Exists(path))
                        {
                            using (Stream input = File.OpenRead(path))
                            {
                                var serialized = new ZipArchive(input, ZipArchiveMode.Read);
                                using (var md5 = MD5.Create())
                                {
                                    var h1 = System.Text.Encoding.UTF8.GetString(md5.ComputeHash(zipArchive.Entries[0].Open()));
                                    var h2 = System.Text.Encoding.UTF8.GetString(md5.ComputeHash(serialized.Entries[0].Open()));
                                    to_serialize = overwrite = (h1 != h2);
                                }
                            }
                        }

                        if (to_serialize)
                        {
                            mstr.Position = 0;
                            using (Stream output = File.OpenWrite(path))
                                mstr.CopyTo(output);

                            if (overwrite)
                            {
                                if (SymbolsUpdate != null) SymbolsUpdate(); err = 0; goto step1;
                            }
                        }

                        dic = _zipped_CSV_to_dic(zipArchive);
                    }
        step1:
                    err = 0;
                }
                else
                {
                    err = (int)APIError.Unknown;
                }
            }
            catch (WebException ex)
            {
                QuandlError qerror = GetQuandlError(ex);
                err = qerror.ToInt();
            }
        }
    }
}

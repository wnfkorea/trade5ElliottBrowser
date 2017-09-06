using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using WnFTechnicalIndicators;

namespace trade5ElliottBrowser
{
    public enum APIError : int
    {
        APIKeyFile = -1,
        APIKeyLength = -2,
        APIUrlFormat = -3,
        APIResponseErr = -4,
        Unknown = -5
    }

    public enum CommodityType : int
    {
        None = -1,
        Stocks = 0,
        Futures = 1,
        Options = 2,
        Index = 3
    }


    public interface IWnFOpenAPI
    {
        string ProviderName();
        bool Check(out int err, out WnFCandles ch);
        Dictionary<int, string> LoadAPIKeys(out string p);
        string APIKey(int type = -1);
        int[] SupportedPeriods();
        void GetSectors(out Dictionary<string, string> dic, out int err);
        void RefreshSymbols(out Dictionary<string, string> dic, out int err);
        string SymbolToName(string s, CommodityType type);
        string SymbolByName(string n, CommodityType type);
        WnFCandles GetCandles(string s, int p, CommodityType type, WnFDbConnectionWrapper wrpper = null);
        void GetCandles(string s, int p, CommodityType type, ref Dictionary<CandlePeriod, WnFCandles> dic, WnFDbConnectionWrapper wrpper);
        int QuoteItemsToDictionary(string[] c, out Dictionary<int, int> dic);
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;


namespace trade5ElliottBrowser
{
    public enum IniCategory : int
    {
        Application = 0,
        URLs = 1,
        APIKeys = 2
    }


    public partial class CIniEdit : UserControl
    {
        public CIniEdit(int c = -1, string p = null)
        {
            InitializeComponent();
            mycategory = c;
            if (!string.IsNullOrEmpty(p)) IniPath = p;
        }

        public string IniPath
        {
            set
            {
                if (!File.Exists(value)) throw new ArgumentException("File not found: " + value);
                path = value;
            }
        }


        private string[] _Ini_APP = {
            "CrawlerDefault",
            "QuoteSource",
            "FilteringScheduledAt",
            "FilteringPassiveCollection",
            "TaskRemoteFeedType",
            "FilterResultWrapper"
        };
        private bool[] _Ini_AB = {
            false,
            false,
            true,
            false,
            true,
            false
        };
        private string[] _Ini_URL = {
            "DatabaseDefault",
            "DatasetDefault",
            "Symbols",
            "SymbolsOfInterest",
            "Candles",
            "Metadata",
            "CrawlerDefault",
            "CrawlerTarget",
            "CrawlerTarget2",
            "WrapperHtml",
            "IndCandles"
        };

        private int mycategory;
        private string path;
        private Dictionary<string, string> dic;
        private bool[] b;

        private void SaveIni()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            for (int i = 0; i < dic.Count; i++)
            {
                XmlNode n = default(XmlNode);
                XmlNode pn = default(XmlNode);
                string v = string.Empty;

                if (b[i])
                {
                    v = ((TextBox)TableLayoutPanel1.GetControlFromPosition(2, i)).Text; if (string.IsNullOrEmpty(v)) continue;
                    n = doc.SelectSingleNode("//Key[text() ='" + dic.ElementAt(i).Key + "']"); if (n == null) continue;
                    pn = n.ParentNode;
                    n = pn.SelectSingleNode("Value"); if (n == null) n = doc.CreateElement("Value");
                    n.InnerText = v;
                }
            }
            doc.Save(path);
        }

        private void InitCategory()
        {
            ComboBoxCategory.Items.Clear();
            foreach (IniCategory k in Enum.GetValues(typeof(IniCategory)))
                ComboBoxCategory.Items.Add(k.ToString());

            if (mycategory != -1)
            {
                ComboBoxCategory.Text = ((IniCategory)mycategory).ToString();
                ComboBoxCategory.Enabled = false;
                LabelFnKeyHelp.Text = string.Format(LabelFnKeyHelp.Text, mycategory + 2);
            }
        }

        public static string ReadIniItemValue(XmlDocument xml, string k)
        {
            XmlNode n = default(XmlNode);
            string v = string.Empty;

            n = xml.SelectSingleNode("//IniItem[Key = '" + k + "']");
            if (n != null) v = n.SelectSingleNode("Value").InnerText;

            return v;
        }

        private int InitIniDictionary(IniCategory c)
        {
            List<bool> bools = default(List<bool>);
            XmlDocument xml = new XmlDocument();

            if (File.Exists(path)) xml.Load(path);
            dic = new Dictionary<string, string>();

            switch (c)
            {
                case IniCategory.Application:
                    foreach (string s in _Ini_APP)
                        dic[s] = ReadIniItemValue(xml, s);
                    b = _Ini_AB;
                    break;

                case IniCategory.URLs:
                    if (Properties.Settings.Default.qsource == QuandlAPI._PROVIDER)
                    {
                        bools = new List<bool>();
                        for (int i = 0; i <= _Ini_URL.Length - 2; i++)
                        {
                            dic[_Ini_URL[i]] = ReadIniItemValue(xml, _Ini_URL[i]);
                            bools.Add(i < 2);
                        }
                        b = bools.ToArray();
                    }
                    else
                    {
                        dic[_Ini_URL[2]] = ReadIniItemValue(xml, _Ini_URL[2]);
                        dic[_Ini_URL[3]] = ReadIniItemValue(xml, _Ini_URL[3]);
                        dic[_Ini_URL[4]] = ReadIniItemValue(xml, _Ini_URL[4]);
                        dic[_Ini_URL[_Ini_URL.Length - 1]] = ReadIniItemValue(xml, _Ini_URL[_Ini_URL.Length - 1]);
                        b = new bool[] { false, false, false, false };
                    }
                    break;

                case IniCategory.APIKeys:
                    foreach (CommodityType k in Enum.GetValues(typeof(CommodityType)))
                    {
                        if (k == CommodityType.None) continue;
                        dic[((int)k).ToString()] = ReadIniItemValue(xml, ((int)k).ToString());
                    }
                    b = new bool[] { true, true, true, true };
                    break;
            }

            return dic.Count;
        }

        private void InitTableLayout()
        {
            int cnt = InitIniDictionary((IniCategory)ComboBoxCategory.SelectedIndex);
            string txt = string.Empty;
            if ((IniCategory)mycategory == IniCategory.APIKeys) txt = "OpenAPI key for ";

            for (int r = 0; r < _Ini_URL.Length - 1; r++)
            {
                if (r < cnt)
                {
                    TableLayoutPanel1.GetControlFromPosition(1, r).Text = txt + dic.ElementAt(r).Key;
                    TableLayoutPanel1.GetControlFromPosition(2, r).Text = dic.ElementAt(r).Value;
                    TableLayoutPanel1.GetControlFromPosition(2, r).Tag = dic.ElementAt(r).Key;
                    TableLayoutPanel1.GetControlFromPosition(2, r).Enabled = b[r];
                    ButtonSave.Enabled = (ButtonSave.Enabled || b[r]);
                }
                else
                {
                    TableLayoutPanel1.Controls.Remove(TableLayoutPanel1.GetControlFromPosition(1, r));
                    TableLayoutPanel1.Controls.Remove(TableLayoutPanel1.GetControlFromPosition(2, r));
                }
            }
        }

        private void CIniEdit_Load(object sender, EventArgs e)
        {
            InitCategory();
        }

        private void ComboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitTableLayout();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            SaveIni();
            this.Parent.Dispose();
        }
    }
}

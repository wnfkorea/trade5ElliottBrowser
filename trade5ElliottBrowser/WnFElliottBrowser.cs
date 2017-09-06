using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlServerCe;
using Newtonsoft.Json;
using System.Net;

namespace trade5ElliottBrowser
{
    public class IniItem
    {
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }


        private string _key;
        private string _value;


        public void SaveConfig(string ipath)
	    {
			if (File.Exists(ipath))
            {
				XmlDocument doc = new XmlDocument();
				XPathNavigator nav = default(XPathNavigator);

				doc.Load(ipath);
				nav = doc.CreateNavigator();
				nav.MoveToChild("IniItemCollection", string.Empty);
				nav.MoveToChild("Items", string.Empty);

				XmlNode node = doc.SelectSingleNode("//IniItem[Key = '" + _key + "']");
				if (node != null) {
					node.SelectSingleNode("Value").InnerText = _value;
				} else {
					using (XmlWriter writer = nav.AppendChild()) {
						XmlSerializer serializer = new XmlSerializer(this.GetType());
						writer.WriteComment("");
						serializer.Serialize(writer, this);
					}
				}

				doc.Save(ipath);
			}
            else
            {
				StreamWriter objStreamWriter = new StreamWriter(ipath);
				IniItemCollection cls = new IniItemCollection();
				XmlSerializer x = new XmlSerializer(cls.GetType());

				cls.Items = new IniItem[]{this};
				x.Serialize(objStreamWriter, cls);
				objStreamWriter.Close();
			}
	    }
    }


    public class IniItemCollection
    {
        public IniItem[] Items
        {
            get { return itms; }
            set { itms = value; }
        }


        private IniItem[] itms;


        public void Add(string k, string v)
        {
            Dictionary<string, IniItem> dic = itms.ToList().ToDictionary(p => p.Key);

            if (dic.ContainsKey(k))
                throw new ConstraintException("Duplicate Key " + k);
            else
            {
                IniItem itm = new IniItem();
                itm.Key = k;
                itm.Value = v;

                Array.Resize(ref itms, itms.Length + 1);
                itms[itms.Length - 1] = itm;
            }
        }

        public void Replace(string k, string v)
        {
            Dictionary<string, IniItem> dic = itms.ToList().ToDictionary(p => p.Key);

            if (!dic.ContainsKey(k))
                throw new KeyNotFoundException("Key " + k + " not found");
            else
            {
                IniItem itm = dic[k];
                List<IniItem> lst = itms.ToList();

                lst.Remove(itm);
                itm = new IniItem();
                itm.Key = k;
                itm.Value = v;
                lst.Add(itm);
                itms = lst.ToArray();
            }
        }

        public void SaveConfig(string ipath)
        {
            if (File.Exists(ipath))
            {
                XmlDocument doc = new XmlDocument();
                XPathNavigator nav = default(XPathNavigator);
                XmlSerializer serializer = default(XmlSerializer);

                doc.Load(ipath);
                nav = doc.CreateNavigator();
                nav.MoveToChild("IniItemCollection", string.Empty);
                nav.MoveToChild("Items", string.Empty);

                foreach (IniItem itm in itms)
                {
                    XmlNode node = doc.SelectSingleNode("//IniItem[Key = '" + itm.Key + "']");
                    if (node != null)
                    {
                        node.SelectSingleNode("Value").InnerText = itm.Value;
                    }
                    else
                    {
                        using (XmlWriter writer = nav.AppendChild())
                        {
                            serializer = new XmlSerializer(itm.GetType());
                            writer.WriteComment("");
                            serializer.Serialize(writer, itm);
                        }
                    }
                }

                doc.Save(ipath);
            }
            else
            {
                StreamWriter objStreamWriter = new StreamWriter(ipath);
                XmlSerializer x = new XmlSerializer(this.GetType());

                x.Serialize(objStreamWriter, this);
                objStreamWriter.Close();
            }
        }

        public static Dictionary<string, string> ReadIni(string p)
        {
            if (!File.Exists(p)) throw new ArgumentException("File not exist: " + p);

            Dictionary<string, string> itms = new Dictionary<string, string>();
            XmlDocument doc = new XmlDocument();
            XPathNavigator nav;

            try
            {
                doc.Load(p);
                nav = doc.CreateNavigator();
                nav.MoveToChild("IniItemCollection", string.Empty);
                nav.MoveToChild("Items", string.Empty);

                if (nav.HasChildren)
                {
                    XmlSerializer ser = new XmlSerializer(typeof(IniItem));
                    XmlReader rdr = default(XmlReader);

                    nav.MoveToFirstChild();
                    do
                    {
                        IniItem itm = default(IniItem);
                        if (nav.Value == string.Empty) continue;

                        rdr = nav.ReadSubtree();
                        itm = (IniItem)ser.Deserialize(rdr);
                        itms[itm.Key] = itm.Value;
                    } while (nav.MoveToNext());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception at ApplicationSettings.LoadAPIKeys()\r\n" + ex.Message, Properties.Settings.Default.tm, MessageBoxButtons.OK);
            }

            return itms;
        }
    }


    public static class WnFElliottBrowser
    {
        public static string HomePath
        {
            get { return home_path; }
            set { home_path = value; }
        }

        public static IWnFOpenAPI Factory
        {
            get { return factory; }
            set { factory = value; }
        }

        public static int ThreadCount
        {
            get { return Math.Min(Environment.ProcessorCount, 8); }
        }

        static WnFElliottBrowser()
        {
            home_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            home_path = home_path.Replace("file:\\", string.Empty);
            HomePath = home_path;

            Properties.Settings.Default.logPath = HomePath + Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar;
            Properties.Settings.Default.imgPath = HomePath + Path.DirectorySeparatorChar + "chart_dumps" + Path.DirectorySeparatorChar;
            Properties.Settings.Default.ini = HomePath + Path.DirectorySeparatorChar 
                                                       + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "_ini.xml";

            if (!Directory.Exists(Properties.Settings.Default.logPath)) Directory.CreateDirectory(Properties.Settings.Default.logPath);
            if (!Directory.Exists(Properties.Settings.Default.imgPath)) Directory.CreateDirectory(Properties.Settings.Default.imgPath);
            if (!File.Exists(Properties.Settings.Default.ini)) CreateIni();
        }

        public static bool LoadIni()
        {
            string qsrc = string.Empty;
            Dictionary<string, string> itms = IniItemCollection.ReadIni(Properties.Settings.Default.ini);
            if (itms == null) throw new InvalidOperationException("Check ini file " + Properties.Settings.Default.ini);
            if (itms.Count == 0) throw new InvalidOperationException("Check ini file " + Properties.Settings.Default.ini);

            foreach (string k in itms.Keys)
            {
                switch (k)
                {
                    case "CrawlerDefault":
                        Properties.Settings.Default.crawler = itms[k];
                        break;
                    case "QuoteSource":
                        qsrc = itms[k];
                        break;
                    case "FilteringScheduledAt":
                        Properties.Settings.Default.fltr_schedule = itms[k];
                        break;
                    case "FilteringPassiveCollection":
                        Properties.Settings.Default.fltr_passive = bool.Parse(itms[k]);
                        break;
                    case "TaskRemoteFeedType":
                        Properties.Settings.Default.task_feed_type = int.Parse(itms[k]);
                        break;
                    case "FilterResultWrapper":
                        Properties.Settings.Default.fltr_wrap = itms[k];
                        break;
                }
            }

            if (qsrc != QuandlAPI._PROVIDER /* && more */) qsrc = string.Empty;
            if (qsrc == string.Empty && Properties.Settings.Default.qsource == string.Empty)
                Properties.Settings.Default.qsource = QuandlAPI._PROVIDER;

            if (Properties.Settings.Default.qsource == QuandlAPI._PROVIDER)
            {
                if (!CreateDefaultDbConfig())
                {
                    MessageBox.Show("Error creating default DB config.", Properties.Settings.Default.tm, MessageBoxButtons.OK);
                    return false;
                }
            }
            else
                throw new NotImplementedException();

            return true;
        }

        public static string APIIniPath(string provider, IniCategory ic)
        {
            string p;
            p = HomePath + Path.DirectorySeparatorChar + provider; if (!Directory.Exists(p)) Directory.CreateDirectory(p);
            if (ic == IniCategory.APIKeys)
                p += Path.DirectorySeparatorChar + Properties.Settings.Default.keys_ini;
            else if (ic == IniCategory.URLs)
                p += Path.DirectorySeparatorChar + Properties.Settings.Default.urls_ini;


            return p;
        }

        public static void CreateAPIKeysIni(string provider)
        {
            IniItem itm = default(IniItem);
            List<IniItem> items = new List<IniItem>();
            string p = APIIniPath(provider, IniCategory.APIKeys); if (File.Exists(p)) File.Delete(p);

            foreach (CommodityType t in Enum.GetValues(typeof(CommodityType)))
            {
                if ((int)t < 0) continue;
                itm = new IniItem(); itm.Key = ((int)t).ToString(); itm.Value = string.Empty;
                items.Add(itm);
            }

            CreateIni(items.ToArray(), p);
        }

        public static Dictionary<int, string> LoadAPIKeys(string p)
        {
            Dictionary<int, string> apikeys = new Dictionary<int, string>();
            Dictionary<string, string> dic;
            int key = -1;
            string msg = string.Empty;

            try
            {
                dic = IniItemCollection.ReadIni(p);
                foreach (string k in dic.Keys)
                {
                    if (int.TryParse(k, out key))
                    {
                        if (!string.IsNullOrEmpty(dic[k])) apikeys[key] = dic[k];
                    }
                    else
                        msg += (msg != string.Empty ? "\r\n" : string.Empty) + "Error reading an int " + k;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading file " + p + "\r\n" + ex.Message, Properties.Settings.Default.tm, MessageBoxButtons.OK);
            }
            
            if (msg != string.Empty)
                MessageBox.Show(msg, Properties.Settings.Default.tm, MessageBoxButtons.OK);
            return apikeys;
        }

        public static object GetJSONObjects(string url, Type t)
        {
            WebRequest request = default(WebRequest);
            WebResponse response = default(WebResponse);
            object result = null;
            JsonSerializer s = new JsonSerializer();

            request = WebRequest.Create(url);
            //request.Credentials = CredentialCache.DefaultCredentials
            response = request.GetResponse();

            if (((HttpWebResponse)response).StatusDescription != "OK")
            {
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            }

            using (Stream dataStream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    using (JsonReader jrdr = new JsonTextReader(reader))
                    {
                        try
                        {
                            result = s.Deserialize(jrdr, Type.GetType(t.FullName));
                        }
                        catch (OutOfMemoryException ex)
                        {
                            Console.WriteLine("OutOfMemoryException at OpenAPIQuoteFactory.GetJSONObjects() DeserializeObject failed.\r\n" + ex.Message);
                            result = null;
                        }
                    }
                }
            }

            response.Close();
            return result;
        }

        public static string ReadURLString(string p, string k)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode n = default(XmlNode);

            if (!File.Exists(p))
            {
                MessageBox.Show("Ini file for URLs does not exist. Copying the default.", Properties.Settings.Default.tm, MessageBoxButtons.OK);
                try
                {
                    File.WriteAllText(p, Properties.Resources.URLs_ini);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while copying the default: " + ex.Message, Properties.Settings.Default.tm, MessageBoxButtons.OK);
                }
                return string.Empty;
            }

            doc.Load(p);
            n = doc.SelectSingleNode("//IniItem[Key = '" + k + "']");
            if (n == null)
            {
                MessageBox.Show("Ini item for '" + k + "' missing.", Properties.Settings.Default.tm, MessageBoxButtons.OK);
                return string.Empty;
            }

            return n.SelectSingleNode("Value").InnerText;
        }

        public static Form GetForm(IWnFOpenAPI factory, ModeOfInfo m = ModeOfInfo.Product, int ic = -1)
        {
            Form f = new Form();
            UserControl c = default(UserControl);
            if (m == ModeOfInfo.Product)
            {
                f.Text = Properties.Settings.Default.tm + " Information";
                c = new CProductInfo();
            }
            else if (m == ModeOfInfo.IniEdit)
            {
                f.Text = Properties.Settings.Default.tm + " Settings for " + factory.ProviderName();
                c = new CIniEdit(ic, (ic != -1) ? WnFElliottBrowser.APIIniPath(QuandlAPI._PROVIDER, (IniCategory)ic) : null);
            }
            else
            {
                throw new NotImplementedException();

            }

            f.Size = new System.Drawing.Size(900, 500);
            f.Icon = Properties.Resources.tr5_10;
            c.Dock = DockStyle.Fill;
            f.Controls.Add(c);
            f.TopMost = true;
            return f;
        }


        private static string home_path;
        private static IWnFOpenAPI factory;


        private static bool TestConnection(string conn_str, ref string msg)
        {
            bool r = false;
            SqlCeConnection cnn = default(SqlCeConnection);
            try
            {
                cnn = new SqlCeConnection(conn_str);
                cnn.Open();
                msg = "Connection to Database has been opened.";
                cnn.Close();
                r = true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(Properties.Settings.Default.tm + " Exception at ControlConnSQLCE.TestConnection()\r\n" + ex.Message);
                msg = Properties.Settings.Default.tm + " Exception at ControlConnSQLCE.TestConnection()\r\n" + ex.Message;
            }
            catch (SqlCeException ex)
            {
                Console.WriteLine(Properties.Settings.Default.tm + " Exception at ControlConnSQLCE.TestConnection()\r\n" + ex.Message);
                msg = Properties.Settings.Default.tm + " Exception at ControlConnSQLCE.TestConnection()\r\n" + ex.Message;
            }
            finally
            {
                cnn.Dispose();
            }

            return r;
        }

        private static bool CreateDefaultDbConfig(string dbname = "trade5ElliottBrowser.sdf")
        {
            bool b = false;
            string strds = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\database\\";
            string strdbconn = null;

            strds = strds.Replace("file:\\", "");
            if (!Directory.Exists(strds)) Directory.CreateDirectory(strds);

            if (dbname == string.Empty)
            {
                strds += "{0}.sdf";
                strdbconn = "Data Source=" + strds + ";Max Database Size=3072;Max Buffer Size=2048";
                Properties.Settings.Default.dbConn = strdbconn;
                b = true;
            }
            else
            {
                strds += dbname;
                strdbconn = "Data Source=" + strds + ";Max Database Size=3072;Max Buffer Size=2048";
                Properties.Settings.Default.dbConn = strdbconn;

        create:
                if (!File.Exists(strds))
                {
                    SqlCeEngine engine = new SqlCeEngine(strdbconn);
                    engine.CreateDatabase();
                    b = true;
                }
                else
                {
                    string msg = string.Empty;
                    b = TestConnection(strdbconn, ref msg);
                    if (!b)
                    {
                        if (MessageBox.Show("Connection Error. Replace it?", Properties.Settings.Default.tm, MessageBoxButtons.YesNoCancel) == DialogResult.OK)
                        {
                            File.Delete(strds);
                            goto create;
                        }
                    }
                }
            }

            return b;
        }

        private static void CreateIni(IniItem[] items = null, string p = "")
        {
	        IniItemCollection cls = new IniItemCollection();

	        if (items == null)
            {
		        IniItem itm = new IniItem();
		        itm.Key = "CrawlerDefault";
		        itm.Value = "http://media.kisline.com/fininfo/mainFininfo.nice?paper_stock={0}&nav=4&header=N";
		        items = new IniItem[]{itm};

                p = Properties.Settings.Default.ini;
	        } else
            {
		        if (items.Length == 0)
			        throw new ArgumentException("Items length zero", "Length");
		        if (p == string.Empty)
			        throw new ArgumentException("File path null", "Path");
	        }
	        cls.Items = items;

	        StreamWriter objStreamWriter = new StreamWriter(p);
	        XmlSerializer x = new XmlSerializer(cls.GetType());

	        x.Serialize(objStreamWriter, cls);
	        objStreamWriter.Close();
        }
    }
}

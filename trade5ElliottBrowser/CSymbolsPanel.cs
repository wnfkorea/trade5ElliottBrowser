using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace trade5ElliottBrowser
{
    public delegate void ehSymbolSelect(CommodityType type, string symbol);


    public partial class CSymbolsPanel : UserControl
    {
        public ehSymbolSelect SymbolSelected;

        public CSymbolsPanel()
        {
            InitializeComponent();
            srch_list = new Dictionary<int, string>();
        }

        public CSymbolsPanel(IWnFOpenAPI agent, bool sector = false) : this()
        {
            int err;
            _factory = agent;
            _sector = sector;
            if (_sector)
                _factory.GetSectors(out _symbols, out err);
            else
                _factory.RefreshSymbols(out _symbols, out err);

            if (err < 0)
                MessageBox.Show("Error while getting symbols: " + ((APIError)err).ToString(), Properties.Settings.Default.tm, MessageBoxButtons.OK);
        }

        public Dictionary<string, string> Symbols
        {
            set { _symbols = value; }
        }


        private IWnFOpenAPI _factory;
        private bool _sector;
        private Dictionary<string, string> _symbols;

        private Dictionary<int, string> srch_list;
        private string search = "";
        private int s_index = -1;


        private void InitSearchBox()
        {
            ComboBoxSearch.Items.Clear();
            ComboBoxSearch.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            ComboBoxSearch.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        private void GetSymbols()
        {
            foreach (string str in _symbols.Keys)
            {
                if (str == "") continue;
                listViewCode.Items.Add(new ListViewItem(new string[] { str, _symbols[str] }));
            }

            listViewCode.EndUpdate();
        }

        private void InitListCode()
        {
            ColumnHeader[] colHeader;
            colHeader = new ColumnHeader[2];

            colHeader[0] = new ColumnHeader();
            colHeader[0].Text = "Ticker";
            colHeader[0].Width = 70;

            colHeader[1] = new ColumnHeader();
            colHeader[1].Text = "Title";
            colHeader[1].Width = listViewCode.Width - colHeader[0].Width;

            listViewCode.BeginUpdate();
            foreach (ColumnHeader c in colHeader) listViewCode.Columns.Add(c);
            listViewCode.Items.Clear();
            GetSymbols();
        }

        private void TitleSearch(string str)
        {
            ListViewItem itm;
            if (str == "") return;

            if (str == search && s_index != -1)
                itm = listViewCode.FindItemWithText(str, true, s_index + 1);
            else
                itm = listViewCode.FindItemWithText(str, true, 0);

            if (itm != null)
            {
                itm.EnsureVisible();
                itm.Selected = true;
                s_index = itm.Index;
                search = str;
                if (!ComboBoxSearch.AutoCompleteCustomSource.Contains(str))
                {
                    ComboBoxSearch.AutoCompleteCustomSource.Add(str);
                    ComboBoxSearch.Items.Add(str);
                }
            }
            else
                s_index = -1;
        }

        private void CSymbolsPanel_Load(object sender, EventArgs e)
        {
            InitSearchBox();
            if (_symbols != null) InitListCode();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (ComboBoxSearch.SelectedIndex == -1 && ComboBoxSearch.Text != "")
                if (!srch_list.ContainsValue(ComboBoxSearch.Text)) srch_list.Add(srch_list.Count, ComboBoxSearch.Text);
            TitleSearch(ComboBoxSearch.Text);
        }

        private void ComboBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (ComboBoxSearch.SelectedIndex == -1 && ComboBoxSearch.Text != "")
                    if (!srch_list.ContainsValue(ComboBoxSearch.Text)) srch_list.Add(srch_list.Count, ComboBoxSearch.Text);
                TitleSearch(ComboBoxSearch.Text);
            }
        }

        private void ComboBoxSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            TitleSearch(ComboBoxSearch.Text);
        }

        private void listViewCode_ItemActivate(object sender, EventArgs e)
        {
            SymbolSelected((_sector ? CommodityType.Index : CommodityType.Stocks), listViewCode.SelectedItems[0].Text);
        }

    }
}

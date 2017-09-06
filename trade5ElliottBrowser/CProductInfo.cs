using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace trade5ElliottBrowser
{
    public partial class CProductInfo : UserControl
    {
        public CProductInfo()
        {
            InitializeComponent();
        }

        private void CProductInfo_Load(object sender, EventArgs e)
        {
            LabelProdVer.Text = Properties.Settings.Default.tm + " (ver: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + ")";
            LabelContact.Text = Properties.Settings.Default.contact;
        }
    }
}

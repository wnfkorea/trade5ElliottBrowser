namespace trade5ElliottBrowser
{
    partial class CSymbolsPanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.listViewCode = new System.Windows.Forms.ListView();
            this.ComboBoxSearch = new System.Windows.Forms.ComboBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.Controls.Add(this.listViewCode, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.ComboBoxSearch, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonApply, 2, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(360, 500);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // listViewCode
            // 
            this.listViewCode.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.tableLayoutPanel1.SetColumnSpan(this.listViewCode, 2);
            this.listViewCode.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.listViewCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewCode.FullRowSelect = true;
            this.listViewCode.GridLines = true;
            this.listViewCode.HideSelection = false;
            this.listViewCode.Location = new System.Drawing.Point(13, 41);
            this.listViewCode.Name = "listViewCode";
            this.listViewCode.Size = new System.Drawing.Size(334, 448);
            this.listViewCode.TabIndex = 4;
            this.listViewCode.UseCompatibleStateImageBehavior = false;
            this.listViewCode.View = System.Windows.Forms.View.Details;
            this.listViewCode.ItemActivate += new System.EventHandler(this.listViewCode_ItemActivate);
            // 
            // ComboBoxSearch
            // 
            this.ComboBoxSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ComboBoxSearch.FormattingEnabled = true;
            this.ComboBoxSearch.Location = new System.Drawing.Point(13, 13);
            this.ComboBoxSearch.Name = "ComboBoxSearch";
            this.ComboBoxSearch.Size = new System.Drawing.Size(274, 20);
            this.ComboBoxSearch.TabIndex = 5;
            this.ComboBoxSearch.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSearch_SelectedIndexChanged);
            this.ComboBoxSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ComboBoxSearch_KeyDown);
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(293, 11);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(54, 23);
            this.buttonApply.TabIndex = 6;
            this.buttonApply.Text = "Search";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // CSymbolsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "CSymbolsPanel";
            this.Size = new System.Drawing.Size(360, 500);
            this.Load += new System.EventHandler(this.CSymbolsPanel_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListView listViewCode;
        internal System.Windows.Forms.ComboBox ComboBoxSearch;
        private System.Windows.Forms.Button buttonApply;
    }
}

namespace trade5ElliottBrowser
{
    partial class CProductInfo
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.LabelProdVer = new System.Windows.Forms.Label();
            this.LabelContact = new System.Windows.Forms.Label();
            this.Label5 = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.LabelProd = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LabelProdVer
            // 
            this.LabelProdVer.AutoSize = true;
            this.LabelProdVer.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LabelProdVer.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.LabelProdVer.Location = new System.Drawing.Point(285, 220);
            this.LabelProdVer.Name = "LabelProdVer";
            this.LabelProdVer.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelProdVer.Size = new System.Drawing.Size(50, 13);
            this.LabelProdVer.TabIndex = 58;
            this.LabelProdVer.Text = "Status:";
            // 
            // LabelContact
            // 
            this.LabelContact.AutoSize = true;
            this.LabelContact.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LabelContact.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.LabelContact.Location = new System.Drawing.Point(285, 245);
            this.LabelContact.Name = "LabelContact";
            this.LabelContact.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelContact.Size = new System.Drawing.Size(50, 13);
            this.LabelContact.TabIndex = 55;
            this.LabelContact.Text = "Status:";
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Label5.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Label5.Location = new System.Drawing.Point(217, 245);
            this.Label5.Name = "Label5";
            this.Label5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label5.Size = new System.Drawing.Size(58, 13);
            this.Label5.TabIndex = 57;
            this.Label5.Text = "Contact:";
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Label4.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Label4.Location = new System.Drawing.Point(217, 220);
            this.Label4.Name = "Label4";
            this.Label4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label4.Size = new System.Drawing.Size(59, 13);
            this.Label4.TabIndex = 56;
            this.Label4.Text = "Version:";
            // 
            // LabelProd
            // 
            this.LabelProd.AutoSize = true;
            this.LabelProd.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LabelProd.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.LabelProd.Location = new System.Drawing.Point(197, 192);
            this.LabelProd.Name = "LabelProd";
            this.LabelProd.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelProd.Size = new System.Drawing.Size(82, 13);
            this.LabelProd.TabIndex = 54;
            this.LabelProd.Text = "▶ Product";
            // 
            // CProductInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.BackgroundImage = global::trade5ElliottBrowser.Properties.Resources.wnf25;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.LabelProdVer);
            this.Controls.Add(this.LabelContact);
            this.Controls.Add(this.Label5);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.LabelProd);
            this.MaximumSize = new System.Drawing.Size(890, 470);
            this.MinimumSize = new System.Drawing.Size(890, 470);
            this.Name = "CProductInfo";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.Size = new System.Drawing.Size(890, 470);
            this.Load += new System.EventHandler(this.CProductInfo_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label LabelProdVer;
        internal System.Windows.Forms.Label LabelContact;
        internal System.Windows.Forms.Label Label5;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.Label LabelProd;
    }
}

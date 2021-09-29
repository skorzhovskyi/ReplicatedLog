namespace IPv4EndPoint
{
    partial class IPv4EndPointTextBox
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
            this.ipDiv4 = new System.Windows.Forms.TextBox();
            this.ipDiv3 = new System.Windows.Forms.TextBox();
            this.ipDiv2 = new System.Windows.Forms.TextBox();
            this.ipDiv1 = new System.Windows.Forms.TextBox();
            this.dotSeperator2 = new System.Windows.Forms.Label();
            this.dotSeperator3 = new System.Windows.Forms.Label();
            this.dotSeperator1 = new System.Windows.Forms.Label();
            this.ipDiv0 = new System.Windows.Forms.TextBox();
            this.dotSeperator0 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel1.ColumnCount = 9;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
            this.tableLayoutPanel1.Controls.Add(this.ipDiv4, 8, 0);
            this.tableLayoutPanel1.Controls.Add(this.ipDiv3, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.ipDiv2, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.ipDiv1, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.dotSeperator2, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.dotSeperator3, 7, 0);
            this.tableLayoutPanel1.Controls.Add(this.dotSeperator1, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.ipDiv0, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dotSeperator0, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(179, 13);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // ipDiv4
            // 
            this.ipDiv4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ipDiv4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ipDiv4.Location = new System.Drawing.Point(140, 0);
            this.ipDiv4.Margin = new System.Windows.Forms.Padding(0);
            this.ipDiv4.MaxLength = 5;
            this.ipDiv4.Name = "ipDiv4";
            this.ipDiv4.Size = new System.Drawing.Size(39, 13);
            this.ipDiv4.TabIndex = 4;
            this.ipDiv4.Text = "0";
            this.ipDiv4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ipDiv4.WordWrap = false;
            this.ipDiv4.TextChanged += new System.EventHandler(this.IpDiv_TextChanged);
            this.ipDiv4.Enter += new System.EventHandler(this.ipDiv_Enter);
            this.ipDiv4.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ipDiv_KeyDown);
            // 
            // ipDiv3
            // 
            this.ipDiv3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ipDiv3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ipDiv3.Location = new System.Drawing.Point(105, 0);
            this.ipDiv3.Margin = new System.Windows.Forms.Padding(0);
            this.ipDiv3.MaxLength = 3;
            this.ipDiv3.Name = "ipDiv3";
            this.ipDiv3.Size = new System.Drawing.Size(25, 13);
            this.ipDiv3.TabIndex = 3;
            this.ipDiv3.Text = "0";
            this.ipDiv3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ipDiv3.WordWrap = false;
            this.ipDiv3.TextChanged += new System.EventHandler(this.IpDiv_TextChanged);
            this.ipDiv3.Enter += new System.EventHandler(this.ipDiv_Enter);
            this.ipDiv3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ipDiv_KeyDown);
            // 
            // ipDiv2
            // 
            this.ipDiv2.BackColor = System.Drawing.SystemColors.Window;
            this.ipDiv2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ipDiv2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ipDiv2.Location = new System.Drawing.Point(70, 0);
            this.ipDiv2.Margin = new System.Windows.Forms.Padding(0);
            this.ipDiv2.MaxLength = 3;
            this.ipDiv2.Name = "ipDiv2";
            this.ipDiv2.Size = new System.Drawing.Size(25, 13);
            this.ipDiv2.TabIndex = 2;
            this.ipDiv2.Text = "0";
            this.ipDiv2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ipDiv2.WordWrap = false;
            this.ipDiv2.TextChanged += new System.EventHandler(this.IpDiv_TextChanged);
            this.ipDiv2.Enter += new System.EventHandler(this.ipDiv_Enter);
            this.ipDiv2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ipDiv_KeyDown);
            // 
            // ipDiv1
            // 
            this.ipDiv1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ipDiv1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ipDiv1.Location = new System.Drawing.Point(35, 0);
            this.ipDiv1.Margin = new System.Windows.Forms.Padding(0);
            this.ipDiv1.MaxLength = 3;
            this.ipDiv1.Name = "ipDiv1";
            this.ipDiv1.Size = new System.Drawing.Size(25, 13);
            this.ipDiv1.TabIndex = 1;
            this.ipDiv1.Text = "0";
            this.ipDiv1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ipDiv1.WordWrap = false;
            this.ipDiv1.TextChanged += new System.EventHandler(this.IpDiv_TextChanged);
            this.ipDiv1.Enter += new System.EventHandler(this.ipDiv_Enter);
            this.ipDiv1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ipDiv_KeyDown);
            // 
            // dotSeperator2
            // 
            this.dotSeperator2.AutoSize = true;
            this.dotSeperator2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dotSeperator2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.dotSeperator2.Location = new System.Drawing.Point(95, 0);
            this.dotSeperator2.Margin = new System.Windows.Forms.Padding(0);
            this.dotSeperator2.Name = "dotSeperator2";
            this.dotSeperator2.Size = new System.Drawing.Size(10, 13);
            this.dotSeperator2.TabIndex = 5;
            this.dotSeperator2.Text = ".";
            this.dotSeperator2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dotSeperator3
            // 
            this.dotSeperator3.AutoSize = true;
            this.dotSeperator3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dotSeperator3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.dotSeperator3.Location = new System.Drawing.Point(130, 0);
            this.dotSeperator3.Margin = new System.Windows.Forms.Padding(0);
            this.dotSeperator3.Name = "dotSeperator3";
            this.dotSeperator3.Size = new System.Drawing.Size(10, 13);
            this.dotSeperator3.TabIndex = 7;
            this.dotSeperator3.Text = ":";
            this.dotSeperator3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dotSeperator1
            // 
            this.dotSeperator1.AutoSize = true;
            this.dotSeperator1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dotSeperator1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.dotSeperator1.Location = new System.Drawing.Point(60, 0);
            this.dotSeperator1.Margin = new System.Windows.Forms.Padding(0);
            this.dotSeperator1.Name = "dotSeperator1";
            this.dotSeperator1.Size = new System.Drawing.Size(10, 13);
            this.dotSeperator1.TabIndex = 3;
            this.dotSeperator1.Text = ".";
            this.dotSeperator1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ipDiv0
            // 
            this.ipDiv0.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ipDiv0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ipDiv0.Location = new System.Drawing.Point(0, 0);
            this.ipDiv0.Margin = new System.Windows.Forms.Padding(0);
            this.ipDiv0.MaxLength = 3;
            this.ipDiv0.Name = "ipDiv0";
            this.ipDiv0.Size = new System.Drawing.Size(25, 13);
            this.ipDiv0.TabIndex = 0;
            this.ipDiv0.Text = "0";
            this.ipDiv0.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ipDiv0.WordWrap = false;
            this.ipDiv0.TextChanged += new System.EventHandler(this.IpDiv_TextChanged);
            this.ipDiv0.Enter += new System.EventHandler(this.ipDiv_Enter);
            this.ipDiv0.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ipDiv_KeyDown);
            // 
            // dotSeperator0
            // 
            this.dotSeperator0.AutoSize = true;
            this.dotSeperator0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dotSeperator0.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.dotSeperator0.Location = new System.Drawing.Point(25, 0);
            this.dotSeperator0.Margin = new System.Windows.Forms.Padding(0);
            this.dotSeperator0.Name = "dotSeperator0";
            this.dotSeperator0.Size = new System.Drawing.Size(10, 13);
            this.dotSeperator0.TabIndex = 1;
            this.dotSeperator0.Text = ".";
            this.dotSeperator0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // IPv4EndPointTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "IPv4EndPointTextBox";
            this.Size = new System.Drawing.Size(179, 13);
            this.FontChanged += new System.EventHandler(this.IPv4AddressTextBox_FontChanged);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox ipDiv3;
        private System.Windows.Forms.TextBox ipDiv4;
        private System.Windows.Forms.TextBox ipDiv2;
        private System.Windows.Forms.TextBox ipDiv1;
        private System.Windows.Forms.Label dotSeperator2;
        private System.Windows.Forms.Label dotSeperator3;
        private System.Windows.Forms.Label dotSeperator1;
        private System.Windows.Forms.TextBox ipDiv0;
        private System.Windows.Forms.Label dotSeperator0;
    }
}

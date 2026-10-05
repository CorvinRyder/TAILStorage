namespace ClipboardChecker
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pic_ClipImage = new System.Windows.Forms.PictureBox();
            this.menu_rc = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cleanClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideWhenEmptyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.watchRecordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txt_ClipText = new System.Windows.Forms.TextBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pic_ClipImage)).BeginInit();
            this.menu_rc.SuspendLayout();
            this.SuspendLayout();
            // 
            // pic_ClipImage
            // 
            this.pic_ClipImage.ContextMenuStrip = this.menu_rc;
            this.pic_ClipImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pic_ClipImage.Location = new System.Drawing.Point(0, 0);
            this.pic_ClipImage.Name = "pic_ClipImage";
            this.pic_ClipImage.Size = new System.Drawing.Size(222, 129);
            this.pic_ClipImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pic_ClipImage.TabIndex = 1;
            this.pic_ClipImage.TabStop = false;
            this.toolTip1.SetToolTip(this.pic_ClipImage, "Double-Click to enlarge");
            this.pic_ClipImage.DoubleClick += new System.EventHandler(this.pic_ClipImage_DoubleClick);
            // 
            // menu_rc
            // 
            this.menu_rc.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menu_rc.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cleanClipboardToolStripMenuItem,
            this.hideWhenEmptyToolStripMenuItem,
            this.watchRecordToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.menu_rc.Name = "menu_rc";
            this.menu_rc.Size = new System.Drawing.Size(181, 114);
            // 
            // cleanClipboardToolStripMenuItem
            // 
            this.cleanClipboardToolStripMenuItem.Name = "cleanClipboardToolStripMenuItem";
            this.cleanClipboardToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.cleanClipboardToolStripMenuItem.Text = "Clean Clipboard";
            this.cleanClipboardToolStripMenuItem.Click += new System.EventHandler(this.cleanClipboardToolStripMenuItem_Click);
            // 
            // hideWhenEmptyToolStripMenuItem
            // 
            this.hideWhenEmptyToolStripMenuItem.Checked = true;
            this.hideWhenEmptyToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.hideWhenEmptyToolStripMenuItem.Name = "hideWhenEmptyToolStripMenuItem";
            this.hideWhenEmptyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.hideWhenEmptyToolStripMenuItem.Text = "Hide when empty";
            this.hideWhenEmptyToolStripMenuItem.Click += new System.EventHandler(this.hideWhenEmptyToolStripMenuItem_Click);
            // 
            // watchRecordToolStripMenuItem
            // 
            this.watchRecordToolStripMenuItem.Name = "watchRecordToolStripMenuItem";
            this.watchRecordToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.watchRecordToolStripMenuItem.Text = "Watch Record";
            this.watchRecordToolStripMenuItem.Click += new System.EventHandler(this.watchRecordToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // txt_ClipText
            // 
            this.txt_ClipText.ContextMenuStrip = this.menu_rc;
            this.txt_ClipText.Location = new System.Drawing.Point(0, 0);
            this.txt_ClipText.Multiline = true;
            this.txt_ClipText.Name = "txt_ClipText";
            this.txt_ClipText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_ClipText.Size = new System.Drawing.Size(222, 129);
            this.txt_ClipText.TabIndex = 2;
            this.toolTip1.SetToolTip(this.txt_ClipText, "Double-Click to enlarge");
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.ContextMenuStrip = this.menu_rc;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Clipboard Monitor is running!";
            this.notifyIcon1.Visible = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(222, 129);
            this.richTextBox1.TabIndex = 3;
            this.richTextBox1.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(222, 129);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.pic_ClipImage);
            this.Controls.Add(this.txt_ClipText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Clipboard Monitor";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.Deactivate += new System.EventHandler(this.Form1_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this.pic_ClipImage)).EndInit();
            this.menu_rc.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pic_ClipImage;
        private System.Windows.Forms.TextBox txt_ClipText;
        private System.Windows.Forms.ContextMenuStrip menu_rc;
        private System.Windows.Forms.ToolStripMenuItem cleanClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hideWhenEmptyToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem watchRecordToolStripMenuItem;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}


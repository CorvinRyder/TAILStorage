namespace ClipboardChecker
{
    partial class ClipBoardRecorder
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
            this.txt_Text = new System.Windows.Forms.TextBox();
            this.pxb_image = new System.Windows.Forms.PictureBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.rtf = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pxb_image)).BeginInit();
            this.SuspendLayout();
            // 
            // txt_Text
            // 
            this.txt_Text.Location = new System.Drawing.Point(0, 0);
            this.txt_Text.Multiline = true;
            this.txt_Text.Name = "txt_Text";
            this.txt_Text.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_Text.Size = new System.Drawing.Size(526, 350);
            this.txt_Text.TabIndex = 5;
            this.txt_Text.MouseMove += new System.Windows.Forms.MouseEventHandler(this.txt_Text_MouseMove);
            // 
            // pxb_image
            // 
            this.pxb_image.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pxb_image.Location = new System.Drawing.Point(0, 0);
            this.pxb_image.Name = "pxb_image";
            this.pxb_image.Size = new System.Drawing.Size(724, 441);
            this.pxb_image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pxb_image.TabIndex = 4;
            this.pxb_image.TabStop = false;
            this.pxb_image.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pxb_image_MouseDown);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "JPEG Image|*.jpg";
            // 
            // rtf
            // 
            this.rtf.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtf.Location = new System.Drawing.Point(0, 0);
            this.rtf.Name = "rtf";
            this.rtf.Size = new System.Drawing.Size(724, 441);
            this.rtf.TabIndex = 6;
            this.rtf.Text = "";
            this.rtf.MouseMove += new System.Windows.Forms.MouseEventHandler(this.rtf_MouseMove);
            // 
            // ClipBoardRecorder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(724, 441);
            this.Controls.Add(this.txt_Text);
            this.Controls.Add(this.rtf);
            this.Controls.Add(this.pxb_image);
            this.Name = "ClipBoardRecorder";
            this.Text = "Clipboard Record";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClipBoardRecorder_FormClosing);
            this.Resize += new System.EventHandler(this.ClipBoardRecorder_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pxb_image)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_Text;
        private System.Windows.Forms.PictureBox pxb_image;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.RichTextBox rtf;



    }
}
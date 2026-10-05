using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ClipboardChecker
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        String lastText = String.Empty;
        private void ReadClipboard()
        {
            if (Clipboard.ContainsText())
            {
                String txt = null;
                txt = Clipboard.GetText(TextDataFormat.Rtf);
                bool isRtf = (txt.Length != 0);
                if (isRtf)
                {
                    richTextBox1.Rtf = txt;
                    richTextBox1.BringToFront();
                }
                else
                {
                    txt = Clipboard.GetText();
                    txt_ClipText.BringToFront();
                    txt_ClipText.Text = txt;
                }
                if (txt.GetHashCode() != lastText.GetHashCode())
                {
                    NewDataFormat.WriteRecord(txt, isRtf);
                    lastText = txt;
                }
            }
            else if (Clipboard.ContainsImage())
            {
                Image image = Clipboard.GetImage();
                pic_ClipImage.Image = image;
                pic_ClipImage.BringToFront();
                NewDataFormat.WriteRecord(image);
            }
            else if (Clipboard.ContainsFileDropList())
            {
                StringCollection sc = Clipboard.GetFileDropList();
                StringCollection fdl = sc;
                string textlist = "File List:\r\n";
                foreach (String s in fdl)
                {
                    textlist += s + "\r\n";
                }
                txt_ClipText.Text = textlist;
                NewDataFormat.WriteRecord(txt_ClipText.Text, false);
            }
            else
            {
                this.Hide();
                return;
            }
            this.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            NewDataFormat.init();
            HandleResize();
            ReadClipboard();
            WinAPI.AddClipboardFormatListener(this.Handle);
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            HandleResize();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                //WM_CLIPBOARDUPDATE
                case 0x031D:
                    ReadClipboard();
                    m.Result = IntPtr.Zero;
                    return;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void HandleResize()
        {
            Screen s = System.Windows.Forms.Screen.PrimaryScreen;
            int toppos = (Screen.GetWorkingArea(s.Bounds).Bottom);
            int rightpos = (s.Bounds.Right);
            this.Left = rightpos - this.Size.Width;
            this.Top = toppos - this.Size.Height;
            txt_ClipText.Size = this.ClientSize;
        }

        private void cleanClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            txt_ClipText.Text = "";
            txt_ClipText.BringToFront();
            this.Hide();
        }

        private void hideWhenEmptyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hideWhenEmptyToolStripMenuItem.Checked = !hideWhenEmptyToolStripMenuItem.Checked;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WinAPI.RemoveClipboardFormatListener(this.Handle);
            Process.GetCurrentProcess().Kill();
        }

        private void pic_ClipImage_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
                this.WindowState = FormWindowState.Normal;
            else
                this.WindowState = FormWindowState.Maximized;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cleanClipboardToolStripMenuItem.PerformClick();
            e.Cancel = true;
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            this.Opacity = 1;
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            this.Opacity = 0.5;
        }

        private void watchRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ClipBoardRecorder().Show();
        }
    }
}

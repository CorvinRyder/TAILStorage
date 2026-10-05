using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using TAILStorage;
using static ClipboardChecker.NewDataFormat;

namespace ClipboardChecker
{
    public partial class ClipBoardRecorder : Form
    {
        private Form listform;

        public ClipBoardRecorder()
        {
            InitializeComponent();
            listform = new RecordList(new Action<Record>((r) => showRecord(r)));
            listform.Show();
        }

        private void showRecord(Record record)
        {
            if (pxb_image.Image != null)
            {
                pxb_image.Image.Dispose();
                pxb_image.Image = null;
            }
            byte[] data = NewDataFormat.GetData(record);
            if (data == null)
            {
                MessageBox.Show("The data is currupted and cannot be displayed!");
                return;
            }
            FramedLogHeader header = record.GetCustomHeader<FramedLogHeader>();
            switch (header.ContentType)
            {
                case 0:
                    try
                    {
                        pxb_image.BringToFront();
                        pxb_image.Image = Image.FromStream(new MemoryStream(data));
                        if (pxb_image.Image.Size.Width < pxb_image.Parent.Size.Width
                                && pxb_image.Image.Size.Height < pxb_image.Parent.Size.Height)
                            pxb_image.SizeMode = PictureBoxSizeMode.CenterImage;
                        else
                            pxb_image.SizeMode = PictureBoxSizeMode.Zoom;
                    } catch(Exception e){
                        MessageBox.Show(e.ToString());
                    }
                    break;
                case 1:
                    txt_Text.BringToFront();
                    txt_Text.Text = Encoding.UTF8.GetString(data);
                    break;
                case 2:
                    rtf.BringToFront();
                    rtf.Rtf = Encoding.UTF8.GetString(data);
                    break;
            }
        }

        private void ClipBoardRecorder_FormClosing(object sender, FormClosingEventArgs e)
        {
            listform.Close();
        }

        private void txt_Text_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                txt_Text.DoDragDrop(txt_Text.SelectedText, DragDropEffects.Copy);
            }
        }

        private void pxb_image_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    pxb_image.Image.Save(saveFileDialog1.FileName);
                }
            }
        }

        private void ClipBoardRecorder_Resize(object sender, EventArgs e)
        {
            txt_Text.Size = this.Size;
        }

        private void rtf_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                rtf.DoDragDrop(rtf.Text, DragDropEffects.Copy);
        }
    }
}

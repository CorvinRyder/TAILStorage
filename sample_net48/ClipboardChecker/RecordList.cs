using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using TAILStorage;
using static ClipboardChecker.NewDataFormat;

namespace ClipboardChecker
{
    public partial class RecordList : Form
    {
        private List<Record> list;
        private Action<Record> callback;

        public RecordList(Action<Record> SelectCallBack)
        {
            InitializeComponent();
            splitContainer1.Panel1Collapsed = true;
            callback = SelectCallBack;
        }

        private enum ClipboardDataType
        {
            Image, Text, RTF
        }

        private void FilterResult(Func<Record, bool> condition)
        {
            lv_list.Items.Clear();
            var list = NewDataFormat.GetList();
            IEnumerable<Record> result;
            if (condition != null)
                result = list.Where(condition);
            else
                result = list;

            result = result.Reverse<Record>();

            int i = result.Count();
            foreach (Record r in result)
            {
                FramedLogHeader header = r.GetCustomHeader<FramedLogHeader>();
                string sizeText = "";
                if (header.ContentType == 0)
                    sizeText = (r.DataLength / 1024).ToString() + " KB";
                else
                    sizeText = r.DataLength.ToString() + " char(s)";
                ListViewItem li = new ListViewItem();
                li.Tag = r;
                li.Text = (i--).ToString();
                li.SubItems.Add(((ClipboardDataType)header.ContentType).ToString());
                li.SubItems.Add(sizeText);
                li.SubItems.Add(r.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                li.SubItems.Add(r.MetaDataString);
                lv_list.Items.Add(li);
            } 
        }

        private void lst_recordlst_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv_list.SelectedIndices.Count == 0)
                return;
            var result = NewDataFormat.GetList();
            callback.Invoke(result.ElementAt(lv_list.SelectedIndices[0]));
        }

        private void dataStatisicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(String.Format("Image: {0}\nText: {1}",
            //    list.Count((r) => r.DataType == 0), 
            //    list.Count((r) => r.DataType == 1)));
        }

        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = !splitContainer1.Panel1Collapsed;
        }

        private void btn_filter_Click(object sender, EventArgs e)
        {
            int length = 0;
            int.TryParse(txt_length.Text, out length);
            FilterResult((r) =>
            {
                var customHeader = r.GetCustomHeader<FramedLogHeader>();
                return r.MetaDataString.Contains(txt_title.Text) &&
                      (cbx_type.SelectedIndex == 3 || customHeader.ContentType == cbx_type.SelectedIndex) &&
                      r.Date < dtp.Value &&
                      r.DataLength > length;
            }); 
        }

        private void btn_reset_Click(object sender, EventArgs e)
        {
            txt_title.Text = "";
            cbx_type.SelectedIndex = 3;
            FilterResult(null);
        }

        private void lv_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv_list.SelectedItems.Count > 0)
            {
                var result = NewDataFormat.GetList();
                callback.Invoke((Record)lv_list.SelectedItems[0].Tag);
            }
        }

        private void dere_Click(object sender, EventArgs e)
        {
            if (lv_list.SelectedItems.Count > 0)
            {
                var result = NewDataFormat.GetList();
                NewDataFormat.DeleteRecord((Record)lv_list.SelectedItems[0].Tag);
                FilterResult(null);
            }
        }

        private void lv_list_ColumnClick(object sender, ColumnClickEventArgs e)
        {

        }

        private void RecordList_Load(object sender, EventArgs e)
        {
            this.Enabled = false;
            try
            {
                list = NewDataFormat.GetList();
                FilterResult((r) => true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            this.Enabled = true;
        }
    }
}

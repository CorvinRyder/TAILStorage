using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TAILStorage;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ListItem.xaml
    /// </summary>
    public partial class ListItem : UserControl
    {

        public ListItem(Record record)
        {
            InitializeComponent();

            string displayText = "";

            FramedLogHeader header = record.GetCustomHeader<FramedLogHeader>();

            if (header.ContentType != (int)DataType.Image && record.Compression == 0)
                displayText = record.DataLength.ToString() + " characters";
            else
                displayText += (record.DataLength > 1024 ? record.DataLength / 1024 : record.DataLength).ToString() + " KB";

            lbl_title.Text = record.MetaDataString;
            lbl_type.Text = Enum.GetName(typeof(DataType), header.ContentType);
            lbl_typebg.Background = header.ContentType == 
                (int)DataType.Image ? System.Windows.Media.Brushes.LightBlue : System.Windows.Media.Brushes.LightGreen;
            lbl_length.Text = displayText;
            lbl_date.Text = record.Date.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}

using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using TAILStorage;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Record> cachedList;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NewDataFormat.init();
            var list = NewDataFormat.GetList();
            cachedList = list.Reverse<Record>().ToList();
            cachedList.ForEach(item =>
            {
                RecordList1.Items.Add(new ListItem(item));
            });
        }
        private ImageSource ConvertByteArrayToImageSource(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                // OnLoad caches the image bytes in memory immediately so the stream can be disposed
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = mem;
                image.EndInit();
            }

            // Freeze makes the ImageSource thread-safe and improves rendering performance
            image.Freeze();
            return image;
        }

        private void RecordList1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Record selected = cachedList[RecordList1.SelectedIndex];
            FramedLogHeader framedLogHeader = selected.GetCustomHeader<FramedLogHeader>();
            byte[] data = NewDataFormat.GetData(selected);
            if (framedLogHeader.ContentType == 0)
                view_scroll.Content = new Image { Source = ConvertByteArrayToImageSource(data) };
            else if (framedLogHeader.ContentType == 1)
            {
                view_scroll.Content = new TextBlock
                {
                    Foreground = Brushes.White,
                    Text = Encoding.UTF8.GetString(data),
                    TextWrapping = TextWrapping.Wrap
                };
            }
            else if (framedLogHeader.ContentType == 2)
            {
                var richTextBox = new RichTextBox();
                view_scroll.Content = richTextBox;

                richTextBox.Selection.Load(new MemoryStream(data), DataFormats.Rtf);
            }
            lbl_srcwindow.Text = selected.MetaDataString;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TAILStorage;

namespace WpfApp1
{
    public enum DataType : byte
    {
        Image = 0,
        Text = 1,
        RTF = 2,
        Deleted = 0xFF
    }
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    internal struct FramedLogHeader
    {
        public byte Version;
        public byte Reserved;
        public short ContentType;
        public int Reserved2;
        long Reserved3;
    }
    internal class NewDataFormat
    {


        private static FramedLog<FramedLogHeader> instance = new FramedLog<FramedLogHeader>(new FramedLogConfiguration()
        {
            MagicNumber1 = "CLIP",
            MagicNumber2 = "RECD",
            FilePath = @"Record/history.bin"
        });
        public static void init()
        {
            Console.WriteLine(instance.ReadList().ToString());
        }
        public static List<Record> GetList()
        {
            return instance.Records;
        }


        private static String GetWinTitle()
        {
            IntPtr textpt = Marshal.AllocHGlobal(255);
            WinAPI.GetWindowTextW(WinAPI.GetForegroundWindow(), textpt, 255);
            string winTitle = Marshal.PtrToStringUni(textpt);
            Marshal.FreeHGlobal(textpt);
            return winTitle;
        }

        public static List<Record> ReadList()
        {
            return instance.Records;
        }


        public static void WriteRecord(Image Record)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Record.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                instance.WriteRecord(ms.ToArray(), false, GetWinTitle());
            }
        }

        public static void WriteRecord(String Record, bool isRtf)
        {
            byte[] chars = Encoding.UTF8.GetBytes(Record);
            instance.WriteRecord(chars, chars.Length > 1024, GetWinTitle(), new FramedLogHeader() { ContentType = (short)(isRtf ? 2 : 1) });
        }

        public static byte[] GetData(Record record)
        {
            return instance.GetData(record);
        }

        public static void DeleteRecord(Record record)
        {
            instance.DeleteRecord(record);
        }
    }
}

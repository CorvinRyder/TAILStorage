using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TAILStorage
{
    public class FramedLogConfiguration
    {
        public string FilePath { get; set; } = @"Record\records.tail";
        public int BufferSize { get; set; } = 16384;
        public string MagicNumber1
        {
            get
            {
                return Encoding.UTF8.GetString(BitConverter.GetBytes(magic1));
            }
            set
            {
                magic1 = BitConverter.ToInt32(Encoding.UTF8.GetBytes(value), 0);
            }
        }
        public string MagicNumber2
        {
            get
            {
                return Encoding.UTF8.GetString(BitConverter.GetBytes(magic2));
            }
            set
            {
                magic2 = BitConverter.ToInt32(Encoding.UTF8.GetBytes(value), 0);
            }
        }
        public bool LockFile { get; set; } = false;
        internal int magic1 = 0x4C494154;
        internal int magic2 = 0x44434552;
    }

    public struct Record
    {
        internal object CachedCustomHeader;
        internal uint timestamp;
        internal ReadOnlyMemory<byte> metaDataStringRaw;
        public int MetaDataStringLen;
        public int DataOffset;
        public int DataLength;
        public uint DataChecksum;
        public int Compression;
        internal bool IsDeleted;

        public DateTime Date
        {
            get
            {
                return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }
        }

        public T GetCustomHeader<T>() where T : struct
        {
            return (T)CachedCustomHeader;
        }

        public string MetaDataString
        {
            get
            {
                return Encoding.UTF8.GetString(metaDataStringRaw.ToArray());
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileHeader
    {
        public int magic1;
        public short version;
        public short reserved1;
        public long reserved2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DataPacket
    {
        // 24 bytes
        public int magic2;
        public uint headerChecksum;
        public uint checksum;
        public uint timestamp;
        public byte compression;
        public byte dataMarker;
        public short metaDataLen;
        public int dataLen;
        public long reserved;

        public static DataPacket FillStructure(byte[] data, int offset)
        {
            DataPacket dp = MemoryMarshal.Read<DataPacket>(data.AsSpan(offset, Marshal.SizeOf<DataPacket>()));
            return dp;
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 0)]
    public struct DefaultHeader { }

    public enum ReadResult
    {
        Normal,
        IncorrectFile,
        RecordsCorrected
    }

    /// <summary>
    /// FixedRecord structure, containing information about a fixed record, including its start position and length.
    /// </summary>
    public struct FixedRecord
    {
        public int StartPosition;
        public int Length;

        public override string ToString()
        {
            return $"StartPosition: {StartPosition.ToString("X")}, Length: {Length.ToString("X")}";
        }
    }

    /// <summary>
    /// ReadReport structure, containing information about the read operation, including the number of valid records, deleted records, session fixed records, tombstoned records, and the result of the read operation.
    /// </summary>
    public struct ReadReport
    {
        public int ValidRecords;
        public int DeletedRecords;
        public int SessionFixed;
        public int TombstonedRecords;
        public ReadResult Result;
        public FixedRecord[] FixedRecords;

        public override string ToString()
        {
            return $"ValidRecords: {ValidRecords}, DeletedRecords: {DeletedRecords}, SessionFixed: {SessionFixed}, TombstonedRecords: {TombstonedRecords}, Result: {Result}";
        }
    }
}

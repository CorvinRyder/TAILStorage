using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.IO.Hashing;

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
            get {
                return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }
        }

        public T GetCustomHeader<T>() where T : struct
        {
            return (T) CachedCustomHeader;
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
    public struct DefaultHeader {}

    /// <summary>
    /// This class is a wrapper for FramedLog with DefaultHeader as the custom header type.  <para/>
    /// The DefaultHeader struct is used as a placeholder for the custom header <para/>
    /// This class provides a convenient way to use FramedLog without specifying a custom header type.
    /// </summary>
    public class FramedLog : FramedLog<DefaultHeader> { 
        public FramedLog(FramedLogConfiguration config) : base(config) { }
    }

    /// <summary>
    /// This class is a generic wrapper for FramedLog that allows you to specify a custom header type.  <para/>
    /// Please make sure the custom header type is a struct and has a fixed size.  <para/>
    /// And the size of the structure should be multiples of 16.  <para/>
    /// Failed to meet these requirements will throw an exception.  <para/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FramedLog<T> where T : struct
    {
        private readonly string HISTORY_FILE_PATH;
        private readonly int MAGIC_NUMBER_1;
        private readonly int MAGIC_NUMBER_2;
        private readonly int BUFFER_SIZE;
        private readonly bool LOCK_FILE;

        private const int DATAPACKET_HEADER_SIZE = 32;
        private readonly int CUSTOM_HEADER_SIZE = Marshal.SizeOf<T>();
        private readonly int HEADER_SIZE;
        private readonly bool isCustomHeader = typeof(T) != typeof(DefaultHeader);
        private FileStream _file;
        private FileStream file
        {
            get
            {
                if (!LOCK_FILE || (LOCK_FILE && _file == null))
                {
                    _file = File.Open(HISTORY_FILE_PATH, FileMode.Open);
                }
                return _file;
            }
            set
            {
                _file = value;
            }
        }

        private List<Record> _records;
        private Dictionary<int, int> recordIdxLut;
        private bool isEndOfFile = false;

        private readonly ReaderWriterLockSlim _streamLock = new ReaderWriterLockSlim();
        private readonly object _lock = new object();

        public List<Record> Records
        {
            get
            {
                List<Record> recordsCopy;
                lock (_lock)
                {
                    recordsCopy = new List<Record>(_records);
                }
                recordsCopy.RemoveAll(r => r.IsDeleted);
                // recordsCopy.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));
                return recordsCopy;

            }
        }

        public FramedLog() : this(new FramedLogConfiguration()) { }

        public FramedLog(FramedLogConfiguration config)
        {
            MAGIC_NUMBER_1 = config.magic1;
            MAGIC_NUMBER_2 = config.magic2;
            HISTORY_FILE_PATH = config.FilePath;
            BUFFER_SIZE = config.BufferSize;
            LOCK_FILE = config.LockFile;

            if(!isCustomHeader)
                CUSTOM_HEADER_SIZE = 0;
            HEADER_SIZE = DATAPACKET_HEADER_SIZE + CUSTOM_HEADER_SIZE;

            if(HEADER_SIZE % 16 != 0)
                throw new ArgumentException("Custom header size must be a multiple of 16 bytes.");

            if (!File.Exists(HISTORY_FILE_PATH))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(HISTORY_FILE_PATH));
                FileStream fs = File.Create(HISTORY_FILE_PATH);
                FileHeader fh = new FileHeader()
                {
                    magic1 = MAGIC_NUMBER_1,
                    version = 2
                };
                byte[] result = new byte[16];
                var span = new Span<byte>(result);
                MemoryMarshal.Write(span, ref fh);
                fs.Write(result, 0, result.Length);
                fs.Close();
            }
            _records = new List<Record>();
            recordIdxLut = new Dictionary<int, int>();
        }

        ~FramedLog()
        {
            Console.WriteLine("FramedLog destructor called.");
            if (LOCK_FILE && _file != null)
            {
                _file.Close();
            }
        }

        public List<Record> ReadList()
        {
            recordIdxLut.Clear();
            _records.Clear();
            var file = this.file;
            file.Position = 0;
            byte[] buffer = new byte[BUFFER_SIZE];
            file.Read(buffer, 0, 16);
            Span<byte> span = new Span<byte>(buffer, 0, 16);
            FileHeader fh = MemoryMarshal.Read<FileHeader>(span);

            if (fh.magic1 != MAGIC_NUMBER_1 || file.Position == file.Length)
            {
                if(!LOCK_FILE)
                    file.Close();
                return _records;
            }

            int curPos = 0;
            long fileSize = file.Length;
            int readByteCnt = file.Read(buffer, 0, BUFFER_SIZE);
            while (true)
            {
                if (readByteCnt!=BUFFER_SIZE && curPos==readByteCnt || readByteCnt==0)
                    break;
                
                // 1. The current position exceeds the buffer size,
                // 2. The current position + header size exceeds the buffer size (Mean part of the next frame header is in the next chunk of data)
                if (curPos > BUFFER_SIZE || curPos + HEADER_SIZE > BUFFER_SIZE)
                {
                    readByteCnt = ReadNextChunk(file, buffer, ref curPos);
                }

                ReadOnlySpan<byte> headerSpan = new Span<byte>(buffer, curPos, HEADER_SIZE);
                DataPacket dp = MemoryMarshal.Read<DataPacket>(headerSpan);
                if (dp.magic2 != MAGIC_NUMBER_2)
                {
                    Console.WriteLine("Not frame header! Magic number not match");
                    int nextFrame = ScanForNextFrame(file, buffer, curPos + 1, ref readByteCnt);
                    if (nextFrame == -1)
                    {
                        Console.WriteLine("Reached the end of the file!");
                        break;
                    }
                    curPos = nextFrame;
                    continue;
                }

                if (dp.headerChecksum != 0)
                {
                    // 8 = sizeof(magic2) + sizeof(headerChecksum)
                    var headerByte = new ReadOnlySpan<byte>(buffer, curPos + 8, HEADER_SIZE - 8);
                    uint headerChecksum = Crc32.HashToUInt32(headerByte);
                    if (dp.headerChecksum != headerChecksum)
                    {
                        Console.WriteLine("Bad header checksum!");
                        int nextFrame = ScanForNextFrame(file, buffer, curPos + 4, ref readByteCnt);
                        if (nextFrame == -1)
                        {
                            Console.WriteLine("Reached the end of the file!");
                            break;
                        }
                        curPos = nextFrame;
                        continue;
                        // Scan for next valid frame start.
                    }
                }

                curPos += DATAPACKET_HEADER_SIZE;

                object cachedCustomHeader = null;
                if (isCustomHeader)
                {
                    var customHeaderSpan = new ReadOnlySpan<byte>(buffer, curPos, HEADER_SIZE - DATAPACKET_HEADER_SIZE);
                    cachedCustomHeader = MemoryMarshal.Read<T>(customHeaderSpan);
                }

                if (dp.dataMarker == 0xFF) // Deleted
                {
                    curPos += CUSTOM_HEADER_SIZE + dp.metaDataLen + dp.dataLen;
                    continue;
                }

                curPos += CUSTOM_HEADER_SIZE;

                // Scenario 1: The title is in the next chunk of data
                if (curPos + dp.metaDataLen > BUFFER_SIZE)
                {
                    int fistHalfTitle = BUFFER_SIZE - curPos;
                    Span<byte> span1 = new Span<byte>(buffer, curPos, fistHalfTitle);
                    span1.CopyTo(buffer.AsSpan(0));
                    readByteCnt = file.Read(buffer, fistHalfTitle, BUFFER_SIZE - fistHalfTitle) + fistHalfTitle;
                    curPos = 0;
                }

                ReadOnlySpan<byte> titleSpan = new ReadOnlySpan<byte>(buffer, curPos, dp.metaDataLen);
                ReadOnlyMemory<byte> metaDataMemory = new ReadOnlyMemory<byte>(titleSpan.ToArray());

                curPos += dp.metaDataLen;

                int address = (int)file.Position - readByteCnt + curPos;

                _records.Add(new Record()
                {
                    timestamp = dp.timestamp,
                    metaDataStringRaw = metaDataMemory,
                    CachedCustomHeader = cachedCustomHeader,
                    DataOffset = address,
                    DataLength = dp.dataLen,
                    DataChecksum = dp.checksum,
                    MetaDataStringLen = dp.metaDataLen,
                    Compression = dp.compression
                });

                recordIdxLut[address] = _records.Count - 1;

                // Scenario 2: The data stored across multiple chunks (which is common for images)
                if (dp.dataLen > BUFFER_SIZE)
                {
                    int remaining = dp.dataLen - (BUFFER_SIZE - curPos);
                    file.Position += remaining;
                    curPos = 0;
                    readByteCnt = file.Read(buffer, 0, BUFFER_SIZE);
                }
                else
                    curPos += dp.dataLen;
            }
            if(!LOCK_FILE)
                file.Close();

            isEndOfFile = true;

            return _records;
        }

        private int ReadNextChunk(FileStream file, byte[] buffer, ref int curPos)
        {
            int diff = curPos - BUFFER_SIZE;
            if (diff > 0 && diff < BUFFER_SIZE)
            {
                curPos = curPos - BUFFER_SIZE;
            }
            else
            {
                file.Position += diff;
                curPos = 0;
            }
            return file.Read(buffer, 0, BUFFER_SIZE);
        }

        private int ScanForNextFrame(FileStream fs, byte[] buffer, int curPos, ref int readByte)
        {
            byte[] magicCheck = BitConverter.GetBytes(MAGIC_NUMBER_2);
            var span = new ReadOnlySpan<byte>(buffer);
            span = span.Slice(curPos);

            int fileSize = (int)fs.Length;

            while (fs.Position <= fileSize)
            {
                int result = span.IndexOf(magicCheck);
                if (result != -1)
                {
                    return curPos + result;
                }
                curPos = 0;
                readByte = fs.Read(buffer, 0, BUFFER_SIZE);
                if (readByte == 0) break;
                span = new ReadOnlySpan<byte>(buffer);
            }
            return -1;
        }

        public byte[] GetData(Record record)
        {
           var file = this.file;
            _streamLock.EnterReadLock();
            try
            {
                file.Position = record.DataOffset;
                var data = new byte[record.DataLength];
                file.Read(data, 0, record.DataLength);
                isEndOfFile = false;
                if (record.DataChecksum != 0)
                {
                    var span = new ReadOnlySpan<byte>(data);
                    uint checksum = Crc32.HashToUInt32(span);
                    if (record.DataChecksum != checksum)
                    {
                        Console.WriteLine($"Incorrect Checksum!, address: {record.DataOffset.ToString("X")}");
                        return null;
                    }

                }

                if (record.Compression == 1)
                {

                    MemoryStream msResult = new MemoryStream();
                    try
                    {
                        GZipStream gz = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
                        gz.CopyTo(msResult);
                        gz.Close();
                    }
                    catch { return null; }

                    data = msResult.ToArray();
                    msResult.Close();
                }
                return data;
            }
            finally
            {
                if (!LOCK_FILE)
                    file.Close();
                _streamLock.ExitReadLock();
            }
        }

        public void WriteRecord(byte[] data, bool isCompressed, string metaData)
        {
            WriteRecord(data, isCompressed, metaData, new T());
        }
        public void WriteRecord(byte[] data, bool isCompressed, string metaData, T customHeader)
        {
            _streamLock.EnterWriteLock();
            FileStream file;
            if (!LOCK_FILE)
                file = File.Open(HISTORY_FILE_PATH, FileMode.Append);
            else
            {
                file = this.file;
                if (!isEndOfFile)
                    file.Seek(0, SeekOrigin.End);
            }

            byte[] metaDataBytes = Encoding.UTF8.GetBytes(metaData);

            var timestamp = Math.Round((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);

            DataPacket dp = new DataPacket()
            {
                magic2 = MAGIC_NUMBER_2,
                timestamp = (uint)timestamp,
                // dataMarker = (byte)type,
                compression = 0,
                metaDataLen = (short)metaDataBytes.Length,
                dataLen = data.Length,
                reserved = 0x4F4F4F4F4F4F5741,
            };

            if (isCompressed)
            {
                dp.compression = 1; //GZip
                MemoryStream ms = new MemoryStream();
                using (var gz = new GZipStream(ms, CompressionLevel.Fastest, false))
                {
                    gz.Write(data, 0, data.Length);
                }
                data = ms.ToArray();
                ms.Close();
                dp.dataLen = data.Length;
            }

            var dataSpan = new ReadOnlySpan<byte>(data);
            dp.checksum = Crc32.HashToUInt32(dataSpan);

            byte[] result = new byte[HEADER_SIZE];
            var span = new Span<byte>(result);
            MemoryMarshal.Write(span, ref dp);
            if(isCustomHeader)
                MemoryMarshal.Write(span.Slice(DATAPACKET_HEADER_SIZE), ref customHeader);

            // 8 = Magic number + header checksum
            var headerSpan = new ReadOnlySpan<byte>(result, 8, HEADER_SIZE - 8);
            var headerChecksum = Crc32.HashToUInt32(headerSpan);
            byte[] headerChecksumByte = BitConverter.GetBytes(headerChecksum);
            Array.Copy(headerChecksumByte, 0, result, 4, 4);

            file.Write(result, 0, HEADER_SIZE);
            file.Write(metaDataBytes, 0, metaDataBytes.Length);

            var newRecord = new Record()
            {
                timestamp = dp.timestamp,
                metaDataStringRaw = new ReadOnlyMemory<byte>(metaDataBytes),
                DataOffset = (int) file.Position,
                DataLength = dp.dataLen,
                DataChecksum = dp.checksum,
                MetaDataStringLen = dp.metaDataLen,
                Compression = dp.compression
            };
            if (isCustomHeader)
                newRecord.CachedCustomHeader = customHeader;
            lock (_lock)
            {
                _records.Add(newRecord);
            }

            recordIdxLut[newRecord.DataOffset] = _records.Count - 1;

            file.Write(data, 0, data.Length);

            isEndOfFile = true;
            if (!LOCK_FILE)
                file.Close();
            _streamLock.ExitWriteLock();
        }

        public void DeleteRecord(Record record, bool wipeData = false)
        {
            _streamLock.EnterWriteLock();
            var file = this.file;
            try
            {
                file.Position = record.DataOffset - record.MetaDataStringLen - HEADER_SIZE;
                isEndOfFile = false;
                int frameSize = HEADER_SIZE + record.MetaDataStringLen + record.DataLength;
                byte[] buffer = new byte[HEADER_SIZE];
                file.Read(buffer, 0, HEADER_SIZE);
                var dp = DataPacket.FillStructure(buffer, 0);
                if (dp.magic2 == MAGIC_NUMBER_2)
                {
                    var headerForChecksum = new ReadOnlySpan<byte>(buffer, 8, HEADER_SIZE - 8);
                    uint headerCheckSum = Crc32.HashToUInt32(headerForChecksum);
                    if (headerCheckSum != dp.headerChecksum)
                        return;

                    dp.dataMarker = 0xFF;
                    var span2 = new Span<byte>(buffer, 0, HEADER_SIZE);
                    MemoryMarshal.Write(span2, ref dp);

                    uint newChecksum = Crc32.HashToUInt32(headerForChecksum);
                    BitConverter.GetBytes(newChecksum).AsSpan<byte>().CopyTo(span2.Slice(4));

                    file.Position -= HEADER_SIZE;
                    file.Write(buffer, 0, buffer.Length);

                    if (wipeData)
                    {
                        byte[] wipeBuffer = new byte[dp.dataLen + dp.metaDataLen];
                        file.Write(wipeBuffer, 0, wipeBuffer.Length);
                    }
                    int idx = recordIdxLut[record.DataOffset];
                    var newRecord = record;
                    newRecord.IsDeleted = true;
                    _records[idx] = newRecord;
                }
            }
            finally
            {
                if (!LOCK_FILE)
                    file.Close();
                _streamLock.ExitWriteLock();
            }
        }
    }
}

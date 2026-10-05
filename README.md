# Traced Append Index Log (TAIL) Storage Engine

![Language](<https://img.shields.io/badge/Language-C%23%20(net%206.0+%20and%20net%204.8)-7355dd>)
![License](https://img.shields.io/badge/License-MIT-green)

A high-performance, self-healing frame based binary log and storage engine built in C# for modern .NET and .NET Framework. Designed for high-throughput, concurrent multi-threaded workloads.

This project was an upgrade of a 12 years old (2014) project, Clipboard Monitor. I tried to upgrade the original storage engine for better performance, and later, I decided to make the engine a standalone library for general-purpose binary logging and storage. The engine now has a better design and is more efficient than the original project.

Please see sample_net48 for the original project.

## Key Features

- **High Throughput & Low Latency:** Engineered for $10,000+$ ops/sec sequential binary logging and rapid offset lookups.
- **Zero-GC Allocation Core:** Utilizes value-type structs (`Record`), in-place mutation, and pooled memory buffers to minimize Garbage Collection pressure.
- **Fast Offset Indexing:** $O(1)$ lookups using an in-memory hash-map lookup table (`Dictionary<int, int>`) paired with flat memory access.
- **Soft Tombstoning & Zero-Wiping:** Non-destructive record deletions (`IsDeleted`) with optional physical payload zero-wiping (`wipeData: true`) without shifting disk arrays.
- **Optimized File I/O:** Eliminates redundant OS syscalls by using manual tail tracking instead of repeated file end seeking.
- **Thread-Safe Architecture:** Handles concurrent reads and appends safely using synchronized reader/writer locks without data tearing or race conditions.
- **Auto-Recovery & Corruption Repair:** Detects broken headers or bad checksums during scans, isolated corrupt frames, and returns a structured `ReadReport` detailing repaired offsets (`FixedRecords`).

## Binary Frame Structure (`.tail` Format)

### File Header Breakdown

Every valid TAIL storage file starts with magic number `TAIL`. Note that the engine currently has no way to check the validity of a 16-byte file header, therefore any file beginning with `TAIL` will be considered a valid TAIL storage file.

| Offset | Field            | Size (Bytes) | Data Type | Description                                  |
| :----- | :--------------- | :----------- | :-------- | :------------------------------------------- |
| 0      | **Magic Header** | 4            | `int`     | File magic number (`"TAIL"` / `0x4C494154`). |
| 4      | **Version**      | 2            | `short`   | The version of the TAIL file.                |
| 6      | **Reserved**     | 2            | `short`   | Byte-padding reserved for future expansion.  |
| 8      | **Reserved 2**   | 8            | `long`    | Byte-padding reserved for future expansion.  |

### Frame Layout Breakdown

Every record in a `.tail` log file is stored as an independent, self-contained **Frame** consisting of a fixed-size header, optional custom header struct, UTF-8 metadata string, data checksums, and payload bytes.

| Offset     | Field                | Size (Bytes) | Data Type | Description                                                                      |
| :--------- | :------------------- | :----------- | :-------- | :------------------------------------------------------------------------------- |
| 0          | **Magic Header**     | 4            | `int`     | Frame sync marker (`"RECD"` / `0x44434552`). Validates frame start during scans. |
| 4          | **Header Checksum**  | 4            | `uint`    | CRC32 checksum of the header bytes.                                              |
| 8          | **Data Checksum**    | 4            | `uint`    | CRC32 checksum of the payload bytes to detect disk corruption.                   |
| 12         | **Timestamp**        | 4            | `uint`    | Unix timestamp (seconds) when the frame was written.                             |
| 16         | **Compression Mode** | 1            | `byte`    | `0`: Raw / Uncompressed, `1`: GZip compression.                                  |
| 17         | **Data Marker**      | 1            | `byte`    | `0x00`: Active Record, `0xFE`: Tombstoned (Corrupt), `0xFF`: Soft Deleted.       |
| 18         | **Metadata Length**  | 2            | `short`   | Byte length ($N_1$) of the UTF-8 metadata label.                                 |
| 20         | **Payload Length**   | 4            | `int`     | Byte length ($N_2$) of the stored payload segment.                               |
| 24         | **Reserved**         | 8            | `long`    | Byte-padding reserved for future expansion.                                      |
| 32         | _Custom Header_      | $M$          | `struct`  | Optional custom fixed-size header struct (must be multiple of 16 bytes).         |
| $32+M$     | **Metadata**         | $N_1$        | `string`  | UTF-8 metadata string payload.                                                   |
| $32+M+N_1$ | **Payload Bytes**    | $N_2$        | `byte[]`  | Raw or compressed record payload. Overwritten with `0x00` when wiped.            |

## Framework Compatibility (.NET Framework 4.8)

While **FramedLog** targets modern .NET (.NET 6.0 / 8.0+), it fully supports legacy **.NET Framework 4.8** environments, with performance trade off.

To build or consume TAIL Storage in a .NET Framework 4.8 client project, ensure your project references **`TAILStorage_net48.csproj`**.

### Required NuGet Packages for .NET Framework 4.8

The following NuGet dependencies are required in a .NET 4.8 project:

1. **`System.Memory`** (Provides `Span<T>`, `ReadOnlySpan<T>`, and `Memory<T>`)
2. **`System.IO.Hashing`** (Provides `Crc32` for frame checksum calculation)

## Basic Usage

```csharp
using TAILStorage;
using System.Text;

// 1. Initialize Configuration & Engine
var config = new FramedLogConfiguration
{
    FilePath = @"Record\records.tail",
    LockFile = true // Recommended for high-throughput multi-threaded operations
};

var log = new FramedLog(config);

// 2. Write Records
byte[] payload = Encoding.UTF8.GetBytes("Hello, TAIL Storage Engine!");
log.WriteRecord(payload, isCompressed: payload.Length > 1024, metaData: "Record#1");

// 3. Scan & Read Records
ReadReport report = log.ReadList(); // Returns scan execution summary and auto-fix report
Console.WriteLine($"Loaded {report.ValidRecords} valid records. (Auto-fixed: {report.RecordsFixed})");

foreach (var record in log.Records)
{
    byte[] data = log.GetData(record);
    if (data != null)
    {
        string text = Encoding.UTF8.GetString(data);
        Console.WriteLine($"[{record.Date}] {record.MetaDataString}: {text}");
    }
}

// 4. Delete & Wipe a Record
if (log.Records.Count > 0)
{
    var target = log.Records[0];
    log.DeleteRecord(target, wipeData: true); // Soft-deletes and zero-wipes physical file bytes
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

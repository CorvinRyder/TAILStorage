# Traced Append Index Log (TAIL) Storage Engine

![Language](<https://img.shields.io/badge/Language-C%23%20(net%206.0+%20and%20net%204.8)-7355dd>)
![License](https://img.shields.io/badge/License-MIT-green)

**Traced Append Index Log** is a high-performance, thread-safe and self-healing frame based binary log and storage engine built in C# for modern .NET and .NET Framework.

## Performance & Reliability Benchmarks

Benchmarks were performed on a Windows 10 PC equipped with **AMD 8-core 3.4Ghz CPU** with **32 RAM of memory** and a **1TB mechanical hard drive**.

Please run the included sample projects to validate performance on your own hardware.

### Sequential Write & Random Read Throughput

50,000+ sequential writes and random reads, data sized from **123 bytes** to **24 KB**.

- **Write Throughput:** 68,870+ ops/sec
- **Read Throughput:** 71,000+ ops/sec

### Multithreaded Stress Test

- **Topology:** 3 concurrent Reader threads and 3 concurrent Writer threads.
- **Duration:** 30 continuous minutes of continuous parallel execution.
- **Result:** **Zero race conditions, zero deadlocks, and zero data corruption.**

## Features

- **High Performance:** Sub-millisecond sequential index tracking optimized for modern hardware.
- **Thread-Safe Architecture:** Native concurrent protections allowing parallel multi-reader/multi-writer interactions without sacrificing integrity.
- **Frame-Based Storage:** Structured frame layouts ensure data integrity and self-healing capabilities during partial writes or unexpected shutdowns.
- **Append-Only Logging:** Reduces random disk write overhead, making it highly resilient against partial writes or sudden failures.

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

## Framework Compatibility (.NET Framework 4.8)

While **TAILStorage** targets modern .NET (.NET 6.0 / 8.0+), it fully supports legacy **.NET Framework 4.8** environments, with performance trade off.

To build or consume TAIL Storage in a .NET Framework 4.8 client project, ensure your project references **`TAILStorage_net48.csproj`**.

### Required NuGet Packages for .NET Framework 4.8

The following NuGet dependencies are required in a .NET 4.8 project:

1. **`System.Memory`** (Provides `Span<T>`, `ReadOnlySpan<T>`, and `Memory<T>`)
2. **`System.IO.Hashing`** (Provides `Crc32` for frame checksum calculation)

## Included Sample Projects

This repository includes three practical sample applications demonstrating real-world usage, high-frequency throughput, and multi-threaded engine resilience:

### 1. Clipboard Monitor (ClipboardMonitor)

A practical desktop application that demonstrates real-time clipboard monitoring and logging with TAIL Storage written in .NET Framework 4.8.

### 2. Performance Benchmark (PerformanceBenchmark)

A dedicated stress-testing tool that measures sequential write and random read throughput of the TAIL Storage engine.

### 3. Multi-Threaded Stability Test (MultipleThreadsTest)

A dedicated multi-threaded stress test application that validates the thread-safe architecture of TAIL Storage with 3 concurrent Reader threads and 3 concurrent Writer threads.

## Binary Frame Structure (`.tail` Format)

Please refer to the [TAIL File Format](TAIL_Format.md) documentation for a detailed breakdown of the `.tail` file format, including frame headers, payloads, and checksum validation.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

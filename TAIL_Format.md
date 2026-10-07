# Binary Frame Structure (`.tail` Format)

A valid TAIL storage is a binary file starts with a **16-byte file header** with magic number `TAIL`, followed by a sequence of **Frames**.

Each frame starts with a **32-byte frame header**, with magic number `RECD`, containing the information of the record, followed by the metadata string and payload bytes.

Developer can also define a **custom header struct** (the size must be multiple of 16 bytes) to store additional information in the frame header.

Therefore, the minimum size of a frame is `32 (frame header) + 0 (custom header) + 0 (metadata) + 0 (payload) = 32 bytes`.

## File Header Breakdown

Every valid TAIL storage file starts with magic number `TAIL`. Note that the engine currently has no way to check the validity of a 16-byte file header, therefore any file beginning with `TAIL` will be considered a valid TAIL storage file.

If the file is not recognized as a valid TAIL storage file, the engine will throw an `ArgumentException` when attempting to open it, and any operation afterwards will throw an `InvalidOperationException`.

| Offset | Field            | Size (Bytes) | Data Type | Description                                  |
| :----- | :--------------- | :----------- | :-------- | :------------------------------------------- |
| 0      | **Magic Header** | 4            | `int`     | File magic number (`"TAIL"` / `0x4C494154`). |
| 4      | **Version**      | 2            | `short`   | The version of the TAIL file.                |
| 6      | **Reserved**     | 2            | `short`   | Byte-padding reserved for future expansion.  |
| 8      | **Reserved 2**   | 8            | `long`    | Byte-padding reserved for future expansion.  |

## Frame Layout Breakdown

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

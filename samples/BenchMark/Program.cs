using TAILStorage;
using System.Text;

string ipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

DateTime time = DateTime.Now;
Console.WriteLine("Preparing Test Data...50000 records in totoal");
List<byte[]> testData = new List<byte[]>();
StringBuilder sb = new StringBuilder();
for (int i = 0; i < 50000; i++)
{
    int repeatTime = Random.Shared.Next(200);
    for (int j = 0; j < repeatTime; j++)
    {
        sb.Append(ipsum);
    }
    testData.Add(Encoding.UTF8.GetBytes(sb.ToString()));
    sb.Clear();
}
Console.WriteLine($"Time Used: {(DateTime.Now - time).TotalMilliseconds} ms");

var log = new FramedLog(new FramedLogConfiguration() { LockFile = true });

time = DateTime.Now;
Console.WriteLine("Start Writing Data to the File");
for(int i = 0; i < 50000; i++)
{
    log.WriteRecord(testData[i], testData[i].Length > 1024, $"Record#0{i}-{ipsum}");
}
Console.WriteLine($"Time Used: {(DateTime.Now - time).TotalMilliseconds} ms");

time = DateTime.Now;
Console.WriteLine("Start Reading Index from the File");
var readReport = log.ReadList();
var recordList = log.Records;
Console.WriteLine($"Time Used: {(DateTime.Now - time).TotalMilliseconds} ms");
Console.WriteLine($"{log.Records.Count()} records read from the file.");
Console.WriteLine(readReport.ToString());

time = DateTime.Now;
Console.WriteLine("Start Reading 50000 records payload from the File");
for(int i = 0; i < 50000; i++)
{
    log.GetData(recordList[Random.Shared.Next(recordList.Count)]);
}
Console.WriteLine($"Time Used: {(DateTime.Now - time).TotalMilliseconds} ms");

time = DateTime.Now;
Console.WriteLine("Start Randomly Delete record from the file, 30000 records in totoal");
time = DateTime.Now;
for (int i = 0; i < 30000; i++)
{
    log.DeleteRecord(recordList[Random.Shared.Next(recordList.Count)]);
}
Console.WriteLine($"Time Used: {(DateTime.Now - time).TotalMilliseconds} ms");

time = DateTime.Now;
Console.WriteLine("Start Randomly performing 50000 activities (Read, Write, Delete)");
for (int i = 0; i < 50000; i++)
{
    int action = Random.Shared.Next(3);
    int idx = Random.Shared.Next(recordList.Count);
    switch (action)
    {
        case 0:
            log.DeleteRecord(recordList[idx]);
            break;
         case 1:
            int idx2 = Random.Shared.Next(50000);
            log.WriteRecord(testData[idx2], testData[idx2].Length > 1024, DateTime.Now.ToString());
            break;
        case 2:
            log.GetData(recordList[idx]);
            break;
    }
}
Console.WriteLine($"Time Used: {(DateTime.Now - time).TotalMilliseconds} ms");

Console.WriteLine($"Time Used: {(DateTime.Now - time).TotalMilliseconds} ms");

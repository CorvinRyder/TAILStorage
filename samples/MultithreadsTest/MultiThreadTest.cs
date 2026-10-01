using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using TAILStorage;

namespace MultipleThreadsTest
{
    internal class MultiThreadTest
    {

        private volatile int readIndex = 0;
        private volatile int writeIndex = 0;

        private object _lock = new object();
        private object _writerlock = new object();

        string ipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

        public void Main()
        {
            var config = new FramedLogConfiguration();

            Console.WriteLine("Validating the previous result...");
            var log = new FramedLog(config);
            log.ReadList();
            Console.WriteLine("Validation completed. Ctrl + C to exit, any key to start the 3 writers 3 readers multithread stress test");

            log = null;
            File.Delete(config.FilePath);
            log = new FramedLog(new FramedLogConfiguration()
            {
                LockFile = true
            });
            
            Console.Read();

            var readThread = new ThreadStart(() =>
            {
                string threadName = Thread.CurrentThread.Name;
                while (true)
                {
                    int tReadIndex;
                    byte[] data;
                    lock (_lock)
                    {
                        tReadIndex = readIndex;
                        var record = log.Records;
                        if (!(tReadIndex < writeIndex && tReadIndex < record.Count))
                        {
                            continue;
                        }
                        data = log.GetData(record.ElementAt(tReadIndex));
                        Interlocked.Increment(ref readIndex);
                    }
                    string str = Encoding.UTF8.GetString(data);
                    Console.WriteLine($"{threadName} - Write: {writeIndex}, Read: {tReadIndex} {str.Count(new Func<char, bool>(c => c == 'a'))}");
                    Thread.Sleep(75);
                }
            });

            StringBuilder sb = new StringBuilder();
            byte[][] testData = new byte[400][];
            for (int i = 0; i < 400; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    sb.Append(ipsum);
                }
                testData[i] = Encoding.UTF8.GetBytes(sb.ToString());
                sb.Clear();
            }

            var writeThread = new ThreadStart(() =>
            {
                while (true)
                {
                    int repeatTime = Random.Shared.Next(0, 400);
                    byte[] data = testData[repeatTime];
                    log.WriteRecord(data, data.Length > 1024, "Record#" + writeIndex);
                    lock (_writerlock)
                    {
                        Interlocked.Increment(ref writeIndex);
                    }
                    Thread.Sleep(repeatTime / 2);
                }
            });

            new Thread(writeThread).Start();
            new Thread(writeThread).Start();
            new Thread(writeThread).Start();

            var t1 = new Thread(readThread);
            t1.Name = "ReadThread1";
            t1.Start();
            var t2 = new Thread(readThread);
            t2.Name = "ReadThread2";
            t2.Start();
            var t3 = new Thread(readThread);
            t3.Name = "ReadThread3";
            t3.Start();
        }

    }
}

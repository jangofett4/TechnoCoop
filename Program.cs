// #define LINUX
using System;
#if LINUX
    using Newtonsoft.Json;
#else
    using System.Text.Json;
    using System.Text.Json.Serialization;
#endif
using System.IO.Ports;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;

namespace TechnoCoop
{
    public class Program
    {
        public const int Seconds = 1000;
        public const string SerialDevice = "/dev/ttyS0";

        public static readonly int SleepBetweenData = 60 * Seconds;
        public static readonly int DataNeededForAMinute = (60 * Seconds) / SleepBetweenData;
        public static readonly int DataNeededForAHour = 60 * DataNeededForAMinute;
        public static readonly int DataNeededForADay = 24 * DataNeededForAHour;
        public static readonly int DataNeededForAWeek = 7 * DataNeededForADay;

        public static CoopController Controller;
        public static FirestoreDb Client;
        public static Logger Log;

        public static void Main(string[] args)
        {
            Log = new Logger(new StreamWriter("techno-coop.log", false, Encoding.UTF8));
            Controller = new CoopController();
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "key.json");

            Client = FirestoreDb.Create("technocoop-f0398");
            
            AsyncEntry();
            for(;;) Thread.Sleep(100);
        }

        public static Task ThreadChickenStatus()
        {
            var rnd = new Random();
            return new Task(async () => {
                for (;;)
                {
                    #if !LINUX
                    {
                        Controller.Data.LoadCell1 = rnd.Next(400, 700);
                    }
                    #endif
                    
                    var prev = Controller.LastStatus;
                    var status = Controller.GetChickenStatus();
                    Console.WriteLine(status.OnNest);
                    if (prev.OnNest != status.OnNest || prev.Day != status.Day)
                    {
                        var json = JsonSerializer.Serialize(status);
                        var col = Client.Collection("ChickenStatus");
                        var doc = col.Document();
                        await doc.SetAsync(status);
                        Log.Info("Data sent @ {0} ({1})", doc.Id, json);
                    }
                    Thread.Sleep(10 * Seconds);
                }
            });
        }

        public static async void AsyncEntry()
        {
            /*{
                var today = 7 ;
                var col = Client.Collection("ChickenStatus");
                for (int i = today; i > today - 4; i--)
                {
                    var snap = await col.WhereEqualTo("Day", i).OrderBy("Creation").GetSnapshotAsync();
                    if (snap.Documents.Count == 0) // no data left
                        break; // TODO: handle this
                    foreach (var d in snap.Documents)
                    {
                        var status = d.ConvertTo<ChickenStatus>();
                        Console.WriteLine($"{ status.Day }, { status.Weight }, { status.OnNest }");
                    }
                }
            }*/
            
            {
                var today = DateTime.Now.Day;
                var col = Client.Collection("ChickenStatus");
                var snap = await col.OrderBy("Day").OrderBy("Creation").GetSnapshotAsync();
                foreach (var d in snap.Documents)
                {
                    var status = d.ConvertTo<ChickenStatus>();
                    Console.WriteLine($"{ status.Day }, { status.Weight }, { status.OnNest }");
                }
            }
            /*
            var g = new TCMockDataGenerator(Client);

            for (int i = 0; i < 7; i++)
                g.GenerateChickenStatus(5, 400, 700, i + 1).RunSynchronously();
            */
            /* START OF TASKS */
            // ThreadChickenStatus().Start();
            /* END OF TASKS */

            for(;;)
            {
                Log.Flush();
                Thread.Sleep(1000);
            }

            #if LINUX
            {
                var serial = new SerialPort(SerialDevice);
            }
            #endif

            for (;;)
            {
                Log.Flush();

                // for testing purposes, disabled in windows
                #if LINUX
                {
                    Log.Info("Retrieving data from {0}", SerialDevice);
                    string json = serial.ReadLine();
                    var data = JsonSerializer.Deserialize<SensorData>(json);
                    Controller.Data = data;
                    Log.Info("Data read ({0} bytes)", json.Length);
                }
                #endif

                Log.Info("Sending data to cloud...");
                var json = JsonSerializer.Serialize(Controller.Data);
                var col = Client.Collection("SensorData");
                var doc = col.Document();
                await doc.SetAsync(Controller.Data);
                Log.Info("Data sent @ {0} (Timestamp: {1})", doc.Id, Controller.Data.Creation);

                Log.Info("Waiting for {0} seconds...", SleepBetweenData / Seconds);
                Thread.Sleep(SleepBetweenData);
            }
        }
    }
}

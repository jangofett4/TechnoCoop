using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace TechnoCoop
{
    public class TCMockDataGenerator
    {
        public FirestoreDb Client;
        public Random Rng;

        public TCMockDataGenerator(FirestoreDb c)
        {
            Client = c;
            Rng = new Random();
        }

        public Task GenerateChickenStatus(int tries, int min, int max, int day)
        {
            return new Task(async () => {
                ChickenStatus recent = new ChickenStatus();
                for (int i = 0; i < tries; i++)
                {
                    var w = Rng.Next(min, max);
                    var status = new ChickenStatus()
                    {
                        Weight = w,
                        OnNest = w >= 500,
                        Day = day
                    };
                    if (recent.OnNest != status.OnNest || recent.Day != status.Day)
                        await Client.Collection("ChickenStatus").Document().SetAsync(status);
                    recent = status;
                }
            });
        }
    }
}
using System;
using System.Text.Json.Serialization;

using Google.Cloud.Firestore;

namespace TechnoCoop
{
    [FirestoreData]
    public abstract class HasTimestamp
    {
        [FirestoreProperty]
        [JsonPropertyName("Creation")]
        public long Creation { get; set; }

        public DateTime CreationDate { get { return new DateTime(Creation); } }

        public HasTimestamp()
        {
            Creation = DateTime.Now.Ticks;
        }
    }
    
    [FirestoreData]
    public class SensorData : HasTimestamp
    {
        [FirestoreProperty]
        [JsonPropertyName("T")]
        public float Temperature { get; set; }

        [FirestoreProperty]
        [JsonPropertyName("H")]
        public float Humidity { get; set; }

        [FirestoreProperty]
        [JsonPropertyName("L")]
        public int Light { get; set; }

        [FirestoreProperty]
        [JsonPropertyName("W")]
        public int Water { get; set; }

        [FirestoreProperty]
        [JsonPropertyName("LC1")]
        public int LoadCell1 { get; set; }

        [FirestoreProperty]
        [JsonPropertyName("LC2")]
        public int LoadCell2 { get; set; }
    }


    [FirestoreData]
    public class ChickenStatus : HasTimestamp
    {
        [FirestoreProperty]
        [JsonPropertyName("Weight")]
        public int Weight { get; set; }

        [FirestoreProperty]
        [JsonPropertyName("OnNest")]
        public bool OnNest { get; set; }

        [FirestoreProperty]
        [JsonPropertyName("Day")]
        public int Day { get; set; }
    }

    public class CoopController
    {
        public SensorData Data;
        public ChickenStatus LastStatus;

        public CoopController()
        {
            LastStatus = new ChickenStatus();
            Data = new SensorData();
        }

        public bool IsChickenPresent()
        {
            return Data.LoadCell1 >= 500;
        }

        public ChickenStatus GetChickenStatus()
        {
            LastStatus = new ChickenStatus() {
                Weight = Data.LoadCell1,
                OnNest = IsChickenPresent(),
                Day    = DateTime.Now.Day,
            };
            return LastStatus;
        }
    }
}
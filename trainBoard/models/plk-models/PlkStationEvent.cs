using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace trainBoard.models.plkModels
{
    internal class PlkStationEvent
    {
        [JsonPropertyName("arrivalTime")]
        public string ArrivalTime { get; set; }

        [JsonPropertyName("departureTime")]
        public string DepartureTime { get; set; }

        [JsonPropertyName("departurePlatform")]
        public string Platform { get; set; }

        [JsonPropertyName("departureTrack")]
        public string Track { get; set; }
    }
}

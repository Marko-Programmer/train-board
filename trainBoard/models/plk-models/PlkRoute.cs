using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace trainBoard.models.plkModels
{
    internal class PlkRoute
    {
        [JsonPropertyName("nationalNumber")]
        public string NationalNumber { get; set; }

        [JsonPropertyName("carrierCode")]
        public string CarrierCode { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("stations")]
        public List<PlkStationEvent> Stations { get; set; }
        public string DisplayName => string.IsNullOrEmpty(Name) ? NationalNumber : $"{Name} ({NationalNumber})";
    }
}

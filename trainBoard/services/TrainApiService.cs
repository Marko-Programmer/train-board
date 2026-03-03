using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using trainBoard.models;

namespace trainBoard.services
{
    public class TrainApiService
    {
        private readonly HttpClient _client;

        public TrainApiService(string apiKey)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://pdp-api.plk-sa.pl/");
            _client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        }


        public async Task<List<Train>> GetRouteAsync(int fromId, int toId, string fromName, string toName)
        {
            try
            {
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                var response = await _client.GetAsync($"api/v1/schedules?stations={fromId},{toId}&dateFrom={today}&dateTo={today}");
                if (!response.IsSuccessStatusCode) return new List<Train>();

                string json = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    List<Train> resultList = new List<Train>();
                    if (!doc.RootElement.TryGetProperty("routes", out JsonElement routes)) return resultList;

                    foreach (JsonElement route in routes.EnumerateArray())
                    {
                        var stations = route.GetProperty("stations").EnumerateArray().ToList();
                        var start = stations.FirstOrDefault(s => s.GetProperty("stationId").GetInt32() == fromId);
                        var end = stations.FirstOrDefault(s => s.GetProperty("stationId").GetInt32() == toId);

                        if (start.ValueKind != JsonValueKind.Undefined && end.ValueKind != JsonValueKind.Undefined)
                        {
                            string depStr = start.TryGetProperty("departureTime", out var dt) ? dt.GetString() : null;
                            string arrStr = end.TryGetProperty("arrivalTime", out var at) ? at.GetString() : null;
                             
                            if (depStr != null && arrStr != null && string.Compare(depStr, arrStr) < 0)
                            {
                                resultList.Add(new Train
                                { 
                                    ScheduleId = route.GetProperty("scheduleId").ToString(),
                                    OrderId = route.GetProperty("orderId").ToString(),

                                    Number = route.GetProperty("nationalNumber").GetString(),
                                    Type = route.GetProperty("carrierCode").GetString(),
                                    From = fromName,
                                    To = toName,

                                    DepartureTime = DateTime.Today.Add(TimeSpan.Parse(depStr.Substring(0, 5))),
                                    ArrivalTime = DateTime.Today.Add(TimeSpan.Parse(arrStr.Substring(0, 5))),
                                    Platform = start.TryGetProperty("departurePlatform", out var p) ? p.GetString() : "-",
                                    Track = start.TryGetProperty("departureTrack", out var t) ? t.GetString() : "-"
                                });
                            }
                        }
                    }
                    return resultList.OrderBy(t => t.DepartureTime).ToList();
                }
            }
            catch { return new List<Train>(); }
        }


        public async Task<List<StopDetail>> GetTrainDetailsAsync(string sid, string oid, DictionaryService dictService, string carrier)
        {
            try
            {
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                var response = await _client.GetAsync($"api/v1/operations/train/{sid}/{oid}/{today}");
                if (!response.IsSuccessStatusCode) return null;

                string json = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var stops = new List<StopDetail>();
                    if (!doc.RootElement.TryGetProperty("stations", out JsonElement stationsElement)) return null;

                    foreach (var st in stationsElement.EnumerateArray())
                    {
                        int sId = st.GetProperty("stationId").GetInt32();
                        var stationObj = dictService.Stations.FirstOrDefault(s => s.Id == sId);

                        stops.Add(new StopDetail
                        {
                            StationName = stationObj?.Name ?? $"ID: {sId}",
                            Arrival = ExtractTime(st, "actualArrival"),
                            Departure = ExtractTime(st, "actualDeparture"),
                            StopDuration = CalculateWait(st),
                            Carrier = carrier
                        });
                    }
                    return stops;
                }
            }
            catch { return null; }
        }
         
        private string ExtractTime(JsonElement element, string prop)
        {
            if (element.TryGetProperty(prop, out JsonElement val))
            {
                string s = val.GetString(); // 2024-05-20T14:30:00
                return s?.Contains("T") == true ? s.Split('T')[1].Substring(0, 5) : "--:--";
            }
            return "--:--";
        }


        private string CalculateWait(JsonElement st)
        {
            string a = ExtractTime(st, "actualArrival");
            string d = ExtractTime(st, "actualDeparture");

            if (a != "--:--" && d != "--:--")
            {
                try
                {
                    var diff = TimeSpan.Parse(d) - TimeSpan.Parse(a);
                    return diff.TotalMinutes > 0 ? $"{(int)diff.TotalMinutes} min" : "1 min";
                }
                catch { return "1 min"; }
            }
            return "--";
        }
    }
}
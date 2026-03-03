using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using trainBoard.models;

namespace trainBoard.services
{
    public class DictionaryService
    {
        private const string FilePath = "stations.json";
        private const string MetaFilePath = "stations_meta.json";

        private readonly HttpClient _client;
        private List<Station> _stations = new List<Station>();
        public IReadOnlyList<Station> Stations => _stations;

        public DictionaryService(string apiKey)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://pdp-api.plk-sa.pl/");
            _client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        }

        public async Task InitializeAsync()
        {
            bool shouldDownload = true;

            if (File.Exists(FilePath) && File.Exists(MetaFilePath))
            {
                try
                {
                    string metaJson = File.ReadAllText(MetaFilePath);
                    if (!string.IsNullOrWhiteSpace(metaJson))
                    {
                        DateTime lastUpdate = JsonSerializer.Deserialize<DateTime>(metaJson); 

                        if ((DateTime.Now - lastUpdate).TotalHours < 24)
                        {
                            LoadFromFile();
                            if (_stations != null && _stations.Any())
                                shouldDownload = false;
                        }
                    }
                }
                catch
                {
                    shouldDownload = true;
                }
            }

            if (shouldDownload)
                await DownloadAndSaveAsync();
        }

        private void LoadFromFile()
        {
            if (!File.Exists(FilePath)) return;

            string json = File.ReadAllText(FilePath);
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                _stations = JsonSerializer.Deserialize<List<Station>>(json, options) ?? new List<Station>();
            }
            catch
            {
                _stations = new List<Station>();
            }
        }

        private async Task DownloadAndSaveAsync()
        {
            try
            {
                var response = await _client.GetAsync("api/v1/dictionaries/stations?pageSize=5000");
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(json))
                { 
                    if (doc.RootElement.TryGetProperty("stations", out JsonElement stationsElement))
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        _stations = JsonSerializer.Deserialize<List<Station>>(stationsElement.GetRawText(), options) ?? new List<Station>();
                    }
                    else if (doc.RootElement.TryGetProperty("data", out JsonElement dataElement))
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        _stations = JsonSerializer.Deserialize<List<Station>>(dataElement.GetRawText(), options) ?? new List<Station>();
                    }
                }

                if (_stations.Any())
                {
                    File.WriteAllText(FilePath, JsonSerializer.Serialize(_stations));
                    File.WriteAllText(MetaFilePath, JsonSerializer.Serialize(DateTime.Now));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error downloading stations:\n" + ex.Message);
                LoadFromFile();
            }
        }

        public List<Station> Search(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<Station>();

            text = text.ToLower();
            return _stations
                .Where(s => !string.IsNullOrEmpty(s.Name) && s.Name.ToLower().StartsWith(text))
                .OrderBy(s => s.Name)
                .Take(15)
                .ToList();
        }
    }
}

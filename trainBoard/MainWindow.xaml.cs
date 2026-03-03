using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using trainBoard.models;
using trainBoard.services;
using Microsoft.Extensions.Configuration;

namespace trainBoard
{
    public partial class MainWindow : Window
    {
        private readonly DictionaryService _dictionaryService;
        private readonly TrainApiService _trainService;
        private readonly string _apiKey;

        public MainWindow()
        {
            InitializeComponent();

            var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .       Build();

            _apiKey = configuration["ApiSettings:ApiKey"];

            if (string.IsNullOrEmpty(_apiKey))
            {
                MessageBox.Show("API key not found in appsettings.json");
                Application.Current.Shutdown();
                return;
            }

            _dictionaryService = new DictionaryService(_apiKey);
            _trainService = new TrainApiService(_apiKey);
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _dictionaryService.InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading station database: " + ex.Message);
            }
        }

        private void ComboBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter) return;

            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            string searchText = comboBox.Text;
            if (string.IsNullOrWhiteSpace(searchText)) return;

            var results = _dictionaryService.Search(searchText);
            if (results != null && results.Any())
            {
                comboBox.ItemsSource = results;
                comboBox.IsDropDownOpen = true;

                var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
                if (textBox != null)
                {
                    textBox.SelectionStart = searchText.Length;
                    textBox.SelectionLength = 0;
                }
            }
        }

        private async void ShowTrains_Click(object sender, RoutedEventArgs e)
        {
            var fromStation = FromComboBox.SelectedItem as Station;
            var toStation = ToComboBox.SelectedItem as Station;

            if (fromStation == null || toStation == null)
            {
                MessageBox.Show("Please select both 'From' and 'To' stations.");
                return;
            }

            TrainsDataGrid.Visibility = Visibility.Collapsed;

            try
            {
                var trains = await _trainService.GetRouteAsync(fromStation.Id, toStation.Id, fromStation.Name, toStation.Name);

                if (trains == null || trains.Count == 0)
                {
                    MessageBox.Show("No direct routes found for the selected stations today.");
                }
                else
                {
                    TrainsDataGrid.ItemsSource = trains;
                    TrainsDataGrid.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Request failed: " + ex.Message);
            }
        }


        private async void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Train selectedTrain)
            {
                this.Cursor = Cursors.Wait;
                try
                {
                    var details = await _trainService.GetTrainDetailsAsync(selectedTrain.ScheduleId, selectedTrain.OrderId, _dictionaryService, selectedTrain.Type);

                    if (details != null && details.Any())
                    {
                        var dw = new TrainDetailsWindow(selectedTrain.Number, selectedTrain.Type, details);
                        dw.Owner = this;
                         
                        Brush theme = Brushes.SteelBlue;
                        if (selectedTrain.Type == "IC") theme = new SolidColorBrush(Color.FromRgb(0, 122, 204));
                        else if (selectedTrain.Type == "PR" || selectedTrain.Type == "R") theme = Brushes.DarkOrange;

                        dw.SetHeaderColor(theme);
                        dw.ShowDialog();
                    }
                }
                finally
                {
                    this.Cursor = Cursors.Arrow;
                }
            }
        }


        private void BtnReverse_Click(object sender, RoutedEventArgs e)
        { 
            var fromStation = FromComboBox.SelectedItem as Station;
            var toStation = ToComboBox.SelectedItem as Station;
             
            var fromText = FromComboBox.Text;
            var toText = ToComboBox.Text;
             
            var fromList = FromComboBox.ItemsSource;
            var toList = ToComboBox.ItemsSource;

            FromComboBox.ItemsSource = toList;
            ToComboBox.ItemsSource = fromList;
             
            FromComboBox.SelectedItem = toStation;
            ToComboBox.SelectedItem = fromStation;
             
            if (FromComboBox.SelectedItem == null) FromComboBox.Text = toText;
            if (ToComboBox.SelectedItem == null) ToComboBox.Text = fromText;
             
            UpdateComboTextPosition(FromComboBox);
            UpdateComboTextPosition(ToComboBox);
        }
         
        private void UpdateComboTextPosition(ComboBox comboBox)
        {
            var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
            if (textBox != null)
            {
                textBox.SelectionStart = textBox.Text.Length;
            }
        }
         
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            { 
                ShowTrains_Click(this, new RoutedEventArgs());
            }
        }
    }
}
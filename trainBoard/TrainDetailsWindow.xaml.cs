using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using trainBoard.models;

namespace trainBoard
{
    public partial class TrainDetailsWindow : Window
    {
        public TrainDetailsWindow(string trainNo, string carrier, List<StopDetail> stops)
        {
            InitializeComponent();
            txtHeader.Text = "Train: " + trainNo;
            txtCarrier.Text = carrier;
            DetailsGrid.ItemsSource = stops;
        }

        public void SetHeaderColor(Brush brush)
        {
            if (HeaderBorder != null) HeaderBorder.Background = brush;
            if (txtCarrier != null) txtCarrier.Foreground = brush;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
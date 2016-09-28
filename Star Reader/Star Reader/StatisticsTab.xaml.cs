using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Dragablz;
using LiveCharts;
using LiveCharts.Wpf;
using Star_Reader.Model;

namespace Star_Reader
{
    public partial class StatisticsTab : TabItem, INotifyPropertyChanged
    {
        private Recording chartRecording;

        public StatisticsTab()
        {
            InitializeComponent();
            InitialiseGraphs();
            if (App.RecordingData.Count == 0) return;
            CalculateDataForGougeCharts();
            CalculateDataForCharts();
        }

        public int NrOfErrors { get; set; }
        public int NrOfPackets { get; set; }
        public int NrOfCharacters { get; set; }
        public int NrOfPacketsTo { get; set; }
        public int NrOfCharactersTo { get; set; }
        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> Formatter { get; set; }
        public string[] Labels { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public void CalculateDataForGougeCharts()
        {
            NrOfErrors = 0;
            NrOfPackets = 0;
            NrOfCharacters = 0;
            foreach (var recording in App.RecordingData.Values)
            {
                NrOfErrors += recording.ErrorsPresent;
                NrOfPackets += recording.ListOfPackets.Count;
                NrOfCharacters += recording.GetNumberOfCharacters();
            }
            NrOfCharactersTo = NrOfCharacters;
            NrOfPacketsTo = NrOfPackets;
            if (NrOfCharacters == 0)
                NrOfCharactersTo = 1;
            if (NrOfPackets == 0)
                NrOfPacketsTo = 1;
            DataContext = this;
            NotifyPropertyChanged("NrOfErrors");
            NotifyPropertyChanged("NrOfPackets");
            NotifyPropertyChanged("NrOfCharacters");
            NotifyPropertyChanged("NrOfPacketsTo");
            NotifyPropertyChanged("NrOfCharactersTo");

        }

        public void CalculateDataForCharts()
        {
            DataContext = this;
            chartRecording = null;
            chartRecording = new Recording();
            ClearSeriesCollection();
            foreach (var recording in App.RecordingData.Values)
                chartRecording.ListOfPackets.AddRange(recording.ListOfPackets);
            if ((bool)!DataRate.IsChecked)
                DataRate.IsChecked = true;
            else
                GenerateDataRate();
        }

        public void ShowLoadedPorts()
        {
            OpenPortPanel.Children.Clear();
            foreach (var recording in App.RecordingData.Values)
            {
                var button = new Button
                {
                    Content = recording.Port,
                    Background = Brushes.DarkBlue,
                    Foreground = Brushes.White
                };
                button.Click += btn_click;
                OpenPortPanel.Children.Add(button);
            }
        }

        protected void btn_click(object sender, EventArgs e)
        {
            var b = (Button)sender;
            var x = b.Content.ToString();

            var controlsList = TabablzControl.GetLoadedInstances();

            foreach (var control in controlsList)
            {
                for (var i = control.Items.Count; i > 0; i--)
                {
                    TabItem item = (TabItem)control.Items[i - 1];
                    if (item.Name.Equals("PortTab"+x))
                    {
                        control.SelectedItem = item;
                        control.Focus();
                        item.Focus();
                    }
                }
            }

        }

        //InitialiseGraphs on right of screen
        private void InitialiseGraphs()
        {
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Data rate B/m",
                    Values = new ChartValues<double>(),
                },
                new RowSeries
                {
                    Title = "Errors",
                    Values = new ChartValues<double>(),
                    DataLabels = true,
                    LabelPoint = point => point.X + ""
                },
                new RowSeries
                {
                    Title = "Parity",
                    Values = new ChartValues<double>(),
                    DataLabels = true,
                    LabelPoint = point => point.X + ""
                },
                new RowSeries
                {
                    Title = "EEP",
                    Values = new ChartValues<double>(),
                    DataLabels = true,
                    LabelPoint = point => point.X + ""
                },
                new RowSeries
                {
                    Title = "Total Errors",
                    Values = new ChartValues<double>(),
                    DataLabels = true,
                    LabelPoint = point => point.X + ""
                }
            };
        } //End of InitializeGraphs

        private void DataRate_Checked(object sender, RoutedEventArgs e)
        {
            GenerateDataRate();
        }

        private void GenerateDataRate()
        {
            var getPlots = new Graphing();

            var plots = new List<double>();
            foreach (var recording in App.RecordingData.Values)
                plots.AddRange(getPlots.GetPlots(recording));
            foreach (double plot in plots)
            {
                SeriesCollection[0].Values.Add(plot);
            }
        }

        private void DataRate_Unchecked(object sender, RoutedEventArgs e)
        {
            SeriesCollection[0].Values.Clear();
        }

        private void Errors_Checked(object sender, RoutedEventArgs e)
        {
            var getBars = new Graphing();
            if (chartRecording == null) return;
            var bars = getBars.GetBars(chartRecording);
            SeriesCollection[1].Values.Add(bars[0]);
            SeriesCollection[2].Values.Add(bars[1]);
            SeriesCollection[3].Values.Add(bars[2]);
        }

        private void Errors_Unchecked(object sender, RoutedEventArgs e)
        {
            SeriesCollection[1].Values.Clear();
            SeriesCollection[2].Values.Clear();
            SeriesCollection[3].Values.Clear();
            SeriesCollection[4].Values.Clear();
        }

        private void ClearSeriesCollection()
        {
            foreach (var collection in SeriesCollection)
            {
                collection.Values.Clear();
            }
        }
    }
}
using System.ComponentModel;
using System.Windows.Controls;
using Dragablz;
using LiveCharts;
using System;
using Star_Reader.Model;
using System.Collections.Generic;
using System.Windows;
using LiveCharts.Wpf;

namespace Star_Reader
{
    public partial class StatisticsTab : TabItem, INotifyPropertyChanged
    {
        public double NrOfErrors { get; set; }

        public double NrOfPackets { get; set; }

        public double NrOfCharacters { get; set; }

        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> Formatter { get; set; }
        public Func<double, string> Formatter1 { get; set; }
        public Func<double, string> Formatter2 { get; set; }
        private Recording gData;
        public string[] Labels { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private bool chartInitialized = false;

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        public StatisticsTab()
        {
            InitializeComponent();
            
            if (App.RecordingData.Count != 0)
            {
                CalculateDataForGougeCharts();
                CalculateDataForCharts();
            }
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
            DataContext = this;
            NotifyPropertyChanged("NrOfErrors");
            NotifyPropertyChanged("NrOfPackets");
            NotifyPropertyChanged("NrOfCharacters");
        }

        public void CalculateDataForCharts()
        {
            DataContext = this;
            gData = null;
            gData = new Recording();

            foreach (var recording in App.RecordingData.Values)
            {
                gData.ListOfPackets.AddRange(recording.ListOfPackets);
            }
            if (!chartInitialized)
            {
                InitialiseGraphs();
                chartInitialized = true;
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
                    Values = new ChartValues<double>()
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

            DataRate.IsChecked = true;
        }//End of InitialiseGraphs

        private void DataRate_Checked(object sender, RoutedEventArgs e)
        {
            Graphing getPlots = new Graphing();

            List<double> plots = getPlots.getPlots(gData);
            for (int x = 0; x < plots.Count; x++)
            {
                SeriesCollection[0].Values.Add(plots[x]);
                DataContext = this;
            }
        }

        private void DataRate_Unchecked(object sender, RoutedEventArgs e)
        {
            SeriesCollection[0].Values.Clear();
        }

        private void Errors_Checked(object sender, RoutedEventArgs e)
        {
            Graphing getBars = new Graphing();
            List<double> bars = getBars.getBars(gData);
            SeriesCollection[1].Values.Add(bars[0]);
            SeriesCollection[2].Values.Add(bars[1]);
            SeriesCollection[3].Values.Add(bars[2]);
            SeriesCollection[4].Values.Add(bars[3]);
            DataContext = this;
        }

        private void Errors_Unchecked(object sender, RoutedEventArgs e)
        {
            SeriesCollection[1].Values.Clear();
            SeriesCollection[2].Values.Clear();
            SeriesCollection[3].Values.Clear();
            SeriesCollection[4].Values.Clear();
            DataContext = this;
        }
    }
}

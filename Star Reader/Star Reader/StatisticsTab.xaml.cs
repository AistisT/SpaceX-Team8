﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dragablz;
using LiveCharts;
using LiveCharts.Wpf;
using Star_Reader.Model;

namespace Star_Reader
{
    public partial class StatisticsTab : TabItem, INotifyPropertyChanged
    {
        private Recording _chartRecording;

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

        //Raise notify event
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        //Get data for gouge chart
        public void CalculateDataForGougeCharts()
        {
            NrOfErrors = 0;
            NrOfPackets = 0;
            NrOfCharacters = 0;
            NrOfCharactersTo = 0;
            foreach (var recording in App.RecordingData.Values)
            {
                NrOfErrors += recording.ErrorsPresent;
                NrOfPackets += recording.ListOfPackets.Count;
                NrOfCharacters += recording.GetNumberOfCharacters();
                NrOfCharactersTo += recording.GetExpectedNumberOfCharacters();
            }
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

        //Initiate Date Rate chart
        public void CalculateDataForCharts()
        {
            DataContext = this;
            _chartRecording = null;
            _chartRecording = new Recording();
            ClearSeriesCollection();
            foreach (var recording in App.RecordingData.Values)
                _chartRecording.ListOfPackets.AddRange(recording.ListOfPackets);
            if ((bool) !DataRate.IsChecked)
                DataRate.IsChecked = true;
            else
                GenerateDataRate();
        }

        // Show currently open ports
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

        //Open port buton on click
        protected void btn_click(object sender, EventArgs e)
        {
            var b = (Button) sender;
            var x = b.Content.ToString();

            var controlsList = TabablzControl.GetLoadedInstances();

            foreach (var control in controlsList)
                for (var i = control.Items.Count; i > 0; i--)
                {
                    var item = (TabItem) control.Items[i - 1];
                    if (!item.Name.Equals("PortTab" + x)) continue;
                    control.SelectedItem = item;
                    control.Focus();
                    item.Focus();
                }
        }

        // Chart data container
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
                    Title = "Disconnect",
                    Values = new ChartValues<double>(),

                },
                new RowSeries
                {
                    Title = "Parity",
                    Values = new ChartValues<double>(),

                },
                new RowSeries
                {
                    Title = "Out of Sequence",
                    Values = new ChartValues<double>(),

                },
                new RowSeries
                {
                    Title = "Repeated Packets",
                    Values = new ChartValues<double>(),

                },
                 new RowSeries
                {
                    Title = "Data CRC Error",
                    Values = new ChartValues<double>(),

                },
                new RowSeries
                {
                    Title = "Header CRC Error",
                    Values = new ChartValues<double>(),

                },
                new RowSeries
                {
                    Title = "Incorrect Data Length",
                    Values = new ChartValues<double>(),

                },
                new RowSeries
                {
                    Title = "Total Errors",
                    Values = new ChartValues<double>(),

                }
            };
        } 

        //Checkbox
        private void DataRate_Checked(object sender, RoutedEventArgs e)
        {
            GenerateDataRate();
        }

        //Generate data rate plot
        private void GenerateDataRate()
        {
            var getPlots = new Graphing();

            var plots = new List<double>();
            foreach (var recording in App.RecordingData.Values)
                plots.AddRange(getPlots.GetPlots(recording));
            foreach (var plot in plots)
                SeriesCollection[0].Values.Add(plot);
        }

        //Checkbox
        private void DataRate_Unchecked(object sender, RoutedEventArgs e)
        {
            SeriesCollection[0].Values.Clear();
        }

        //Checkbox
        private void Errors_Checked(object sender, RoutedEventArgs e)
        {
            var getBars = new Graphing();
            if (_chartRecording == null) return;
            var bars = getBars.GetBars(_chartRecording);
            SeriesCollection[1].Values.Add(bars[0]);
            SeriesCollection[2].Values.Add(bars[1]);
            SeriesCollection[3].Values.Add(bars[2]);
            SeriesCollection[4].Values.Add(bars[3]);
            SeriesCollection[5].Values.Add(bars[4]);
            SeriesCollection[6].Values.Add(bars[5]);
            SeriesCollection[7].Values.Add(bars[6]);
            SeriesCollection[8].Values.Add(bars[7]);
        }

        //Checkbox
        private void Errors_Unchecked(object sender, RoutedEventArgs e)
        {
            SeriesCollection[1].Values.Clear();
            SeriesCollection[2].Values.Clear();
            SeriesCollection[3].Values.Clear();
            SeriesCollection[4].Values.Clear();
            SeriesCollection[5].Values.Clear();
            SeriesCollection[6].Values.Clear();
            SeriesCollection[7].Values.Clear();
            SeriesCollection[8].Values.Clear();
        }

        //Clean chart data
        private void ClearSeriesCollection()
        {
            foreach (var collection in SeriesCollection)
                collection.Values.Clear();
        }
    }
}
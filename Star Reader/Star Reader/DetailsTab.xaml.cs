using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using Star_Reader.Model;

namespace Star_Reader
{
    /// <summary>
    ///     Interaction logic for RecordedData.xaml
    /// </summary>
    public partial class DetailsTab : TabItem, INotifyPropertyChanged
    {
        private readonly DispatcherTimer _resizeTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 200),
            IsEnabled = false
        };

        private ICollectionView _dataGridCollection;
        private string _filterString;
        private Recording _gData;

        //Constructor
        public DetailsTab(int portNr)
        {
            InitializeComponent();
            PopulateOverview(portNr);
            InitialiseGauge();
            DataGridCollection = CollectionViewSource.GetDefaultView(App.RecordingData[portNr].ListOfPackets);
            DataGridCollection.Filter = Filter;
            DataContext = this;
            _resizeTimer.Tick += ResizeStopped;
        }

        public string[] Labels { get; set; }

        public ICollectionView DataGridCollection
        {
            get { return _dataGridCollection; }
            set
            {
                _dataGridCollection = value;
                NotifyPropertyChanged("DataGridCollection");
            }
        }

        public string FilterString
        {
            get { return _filterString; }
            set
            {
                _filterString = value;
                NotifyPropertyChanged("FilterString");
                FilterCollection();
            }
        }

        public SeriesCollection SeriesCollection { get; set; }
        public int NrOfErrors { get; set; }
        public int NrOfPackets { get; set; }
        public int NrOfCharacters { get; set; }
        public int NrOfPacketsTo { get; set; }
        public int NrOfCharactersTo { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private void FilterCollection()
        {
            if (_dataGridCollection != null)
                _dataGridCollection.Refresh();
        }

        public bool Filter(object obj)
        {
            var packet = obj as Packet;
            if (packet == null) return false;
            if (string.IsNullOrEmpty(_filterString)) return true;
            return ((packet.ErrorType != null)
                    &&
                    (CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.ErrorType, _filterString,
                         CompareOptions.IgnoreCase) >= 0))
                   ||
                   ((packet.Payload != null)
                    &&
                    (CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.Payload, _filterString,
                         CompareOptions.IgnoreCase) >= 0))
                   ||
                   (CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.PacketType.ToString(), _filterString,
                        CompareOptions.IgnoreCase) >= 0)
                   ||
                   ((packet.PacketEnd != null)
                    &&
                    (CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.PacketEnd, _filterString,
                         CompareOptions.IgnoreCase) >= 0))
                   ||
                   (CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.Time.ToString("MM/dd/yyyy HH:mm:ss.fff"),
                        _filterString, CompareOptions.IgnoreCase) >= 0);
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var packet = DetailedViewerA.SelectedItem;
            var selectedRow =
                (DataGridRow)DetailedViewerA.ItemContainerGenerator.ContainerFromIndex(DetailedViewerA.SelectedIndex);
            FilterString = "";
            FilterCollection();
            if (selectedRow == null) return;
            DetailedViewerA.ScrollIntoView(packet);
            DetailedViewerA.SelectedItem = packet;
            FocusManager.SetIsFocusScope(selectedRow, true);
            FocusManager.SetFocusedElement(selectedRow, selectedRow);
        }

        //generating the button in the overview
        public void PopulateOverview(int portNr)
        {
            const int size = 20;
            var r = App.RecordingData[portNr];
            if (r == null) return;
            var length = r.ListOfPackets.Count;

            for (var i = 0; i < length; i++)
            {
                var p = r.ListOfPackets[i];
                if (i > 0)
                {
                    var nextP = r.ListOfPackets[i - 1];
                    var td = p.Time.Subtract(nextP.Time);
                    if (td.TotalMilliseconds > 100)
                    {
                        var btn1s = new Button
                        {
                            Width = size,
                            Height = size
                        };

                        switch (td.Seconds)
                        {
                            case 0:
                                btn1s.ToolTip = "Empty Space of 0." + td.TotalMilliseconds + " seconds.";
                                btn1s.Background = Brushes.White;
                                break;
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                btn1s.ToolTip = "Empty Space of " + td.Seconds + "." + td.TotalMilliseconds.ToString().Substring(1) + " seconds.";
                                btn1s.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffe699");
                                // Beige
                                break;
                            default:
                                btn1s.ToolTip = "Empty Space of " + td.Seconds + "." + td.TotalMilliseconds.ToString().Substring(1) + " seconds.";
                                btn1s.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#994d00");
                                break;
                        }
                        PacketViewerA.Children.Add(btn1s);
                    }
                }
                var btn1 = new Button
                {
                    Width = size,
                    Height = size
                };
                if (p.PacketType == 'E')
                {
                    switch (p.ErrorType)
                    {
                        case "Disconnect":
                            btn1.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ff3333");
                            btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.ErrorType;
                            btn1.Content = p.ErrorType[0];
                            break;
                        case "Parity":
                            btn1.Background = Brushes.Yellow;
                            btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.ErrorType;
                            btn1.Content = p.ErrorType[0];
                            break;
                    }
                }
                else
                {
                    if (p.PacketEnd.Equals("EOP"))
                    {
                        if (p.ErrorType != null && p.ErrorType.Contains("Out of Sequence"))
                        {
                            btn1.Background = Brushes.Goldenrod;
                            btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.ErrorType;
                            btn1.Content = p.ErrorType[0];
                        }
                        else if (p.ErrorType != null && p.ErrorType.Contains("Babbling Idiot"))
                        {
                            btn1.Background = Brushes.AntiqueWhite;
                            btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.ErrorType;
                            btn1.Content = p.ErrorType[0];
                        }
                        else if(p.ErrorType != null && p.ErrorType.Contains("CRC"))
                        {
                            btn1.Background = Brushes.Fuchsia;
                            btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.ErrorType;
                            btn1.Content = p.ErrorType[0];
                        }
                        else
                        {
                            btn1.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#00dddd"); // Blue
                            btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.Payload + "\n" + p.PacketEnd;
                        }
                    }
                    else if (p.PacketEnd.Equals("EEP"))
                    {
                        btn1.Background = Brushes.Orange;
                        btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.Payload + "\n" + p.PacketEnd;
                        btn1.Content = p.PacketEnd[0];
                    }
                    else
                    {
                        btn1.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffaacc"); // Pink
                        btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.Payload + "\n" + p.PacketEnd;
                    }
                }
                btn1.Click += btn_click;
                btn1.Tag = portNr + "" + i;
                PacketViewerA.Children.Add(btn1);
            }
            _gData = r;
            InitialiseGraphs();
        } //End of PopulateOverview

        //on click for buttons in overview
        protected void btn_click(object sender, EventArgs e)
        {
            var b = (Button)sender;
            var x = b.Tag.ToString();
            var portc = x[0];
            var port = int.Parse(portc + "");
            var item = int.Parse(x.Substring(1));
            DetailedViewerA.ScrollIntoView(App.RecordingData[port].ListOfPackets[item]);
            DetailedViewerA.SelectedItem = App.RecordingData[port].ListOfPackets[item];
            var selectedRow =
                (DataGridRow)DetailedViewerA.ItemContainerGenerator.ContainerFromIndex(DetailedViewerA.SelectedIndex);
            FocusManager.SetIsFocusScope(selectedRow, true);
            FocusManager.SetFocusedElement(selectedRow, selectedRow);
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

            DataRate.IsChecked = true;
        } //End of InitialiseGraphs

        private void DataRate_Checked(object sender, RoutedEventArgs e)
        {
            var getPlots = new Graphing();

            var plots = getPlots.GetPlots(_gData);
            foreach (var plot in plots)
                SeriesCollection[0].Values.Add(plot);
        }

        private void DataRate_Unchecked(object sender, RoutedEventArgs e)
        {
            SeriesCollection[0].Values.Clear();
        }

        private void Errors_Checked(object sender, RoutedEventArgs e)
        {
            var getBars = new Graphing();
            var bars = getBars.GetBars(_gData);
            SeriesCollection[1].Values.Add(bars[0]);
            SeriesCollection[2].Values.Add(bars[1]);
            SeriesCollection[3].Values.Add(bars[2]);
            SeriesCollection[4].Values.Add(bars[3]);
            SeriesCollection[5].Values.Add(bars[4]);
            SeriesCollection[6].Values.Add(bars[5]);
            SeriesCollection[7].Values.Add(bars[6]);
            SeriesCollection[8].Values.Add(bars[7]);
        }

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

        private void InitialiseGauge()
        {
            NrOfErrors = _gData.ErrorsPresent;
            NrOfPackets = _gData.ListOfPackets.Count;
            NrOfCharacters = _gData.GetNumberOfCharacters();

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

        public void SetHeader(UIElement header)
        {
            var closeButton = new CloseButton();
            closeButton.Click +=
                (sender, e) =>
                {
                    var tabControl = Parent as ItemsControl;
                    tabControl?.Items.Remove(this);
                    App.RecordingData.Remove(_gData.Port);
                    var mainWindow = (MainWindow)Application.Current.MainWindow;
                    mainWindow.UpdateStatistics();
                };
            closeButton.TabHeaderGrid.Children.Add(header);
            Header = closeButton;
        }

        public void GenerateTimeStamps()
        {
            var panelWidth = PacketViewerA.ActualWidth;
            var button = VisualTreeHelper.GetChild(PacketViewerA, 0) as Button;
            if (button == null) return;
            var buttonWidth = button.ActualWidth;
            var numberOfButtonsPerRow = (int)panelWidth / (int)buttonWidth;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(PacketViewerA); i += numberOfButtonsPerRow)
            {
                var child = VisualTreeHelper.GetChild(PacketViewerA, i) as Button;

                if (child == null) continue;
                var timestamp = child.ToolTip as string;

                if (timestamp == null) continue;
                var counter = 1;
                while ((timestamp != null) && timestamp.Contains("Empty Space of "))
                {
                    child = VisualTreeHelper.GetChild(PacketViewerA, i + counter) as Button;
                    counter++;
                    if (child != null) timestamp = child.ToolTip as string;
                }
                if (timestamp == null) continue;
                var label = new Label
                {
                    Height = 20,
                    FontSize = 9,
                    Content = timestamp.Substring(11, 12)
                };
                TimeStamps.Children.Add(label);
            }
        } //End of InitialiseTimeStamps

        private void ClearTimeStamps()
        {
            var timestamps = from UIElement timestamp in TimeStamps.Children where timestamp is Label select timestamp;
            var uiElements = timestamps as IList<UIElement> ?? timestamps.ToList();
            for (var i = uiElements.Count; i > 0; i--)
            {
                var element = (Label)uiElements[i - 1];
                TimeStamps.Children.Remove(element);
            }
        }

        private void PacketViewerA_OnLoaded(object sender, RoutedEventArgs e)
        {
            GenerateTimeStamps();
        }

        private void ResizeStopped(object sender, EventArgs e)
        {
            _resizeTimer.IsEnabled = false;
            ClearTimeStamps();
            GenerateTimeStamps();
        }

        private void PacketViewerA_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _resizeTimer.IsEnabled = true;
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }
    }
}
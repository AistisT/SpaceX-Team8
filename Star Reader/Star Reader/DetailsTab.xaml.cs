using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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
        private ICollectionView dataGridCollection;
        private string filterString;
        private Recording gData;

        //Constructor
        public DetailsTab(int portNr)
        {
            InitializeComponent();

            PopulateOverview(portNr);
            initialiseGauge();
            DataGridCollection = CollectionViewSource.GetDefaultView(App.RecordingData[portNr].ListOfPackets);
            DataGridCollection.Filter = Filter;
            this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            this.Arrange(new Rect(0, 0, this.DesiredSize.Width, this.DesiredSize.Height));
            InitialiseTimeStamps();

            DataContext = this;
        }


        public string[] Labels { get; set; }


        public ICollectionView DataGridCollection
        {
            get { return dataGridCollection; }
            set
            {
                dataGridCollection = value;
                NotifyPropertyChanged("DataGridCollection");
            }
        }

        public string FilterString
        {
            get { return filterString; }
            set
            {
                filterString = value;
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
            // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private void FilterCollection()
        {
            if (dataGridCollection != null)
                dataGridCollection.Refresh();
        }

        public bool Filter(object obj)
        {
            var packet = obj as Packet;
            if (packet == null) return false;
            if (string.IsNullOrEmpty(filterString)) return true;
            return ((packet.ErrorType != null) &&
                    (CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.ErrorType, filterString,
                         CompareOptions.IgnoreCase) >= 0))
                   ||
                   ((packet.Payload != null) &&
                    (CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.Payload, filterString,
                         CompareOptions.IgnoreCase) >= 0))
                   ||
                   (CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.PacketType.ToString(), filterString,
                        CompareOptions.IgnoreCase) >= 0)
                   ||
                   ((packet.PacketEnd != null) &&
                    (CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.PacketEnd, filterString,
                         CompareOptions.IgnoreCase) >= 0))
                   ||
                   (CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.Time.ToString("MM/dd/yyyy HH:mm:ss.fff"),
                        filterString, CompareOptions.IgnoreCase) >= 0);
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var packet = DetailedViewerA.SelectedItem;
            var selectedRow = (DataGridRow)DetailedViewerA.ItemContainerGenerator.ContainerFromIndex(DetailedViewerA.SelectedIndex);
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
                    var NextP = r.ListOfPackets[i - 1];
                    var td = p.Time.Subtract(NextP.Time);
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
                                btn1s.ToolTip = "Empty Space of " + td.Seconds + "." +
                                                td.TotalMilliseconds.ToString().Substring(1) + " seconds.";
                                btn1s.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffe699");
                                // Beige
                                break;
                            default:
                                btn1s.ToolTip = "Empty Space of " + td.Seconds + "." +
                                                td.TotalMilliseconds.ToString().Substring(1) + " seconds.";
                                btn1s.Background = (SolidColorBrush) new BrushConverter().ConvertFrom("#994d00");
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
                            btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" +
                                           p.ErrorType;
                            btn1.Content = p.ErrorType[0];
                            break;
                        case "Parity":
                            btn1.Background = Brushes.Yellow;
                            btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" +
                                           p.ErrorType;
                            btn1.Content = p.ErrorType[0];
                            break;
                    }
                }
                else
                {
                    if (p.PacketEnd.Equals("EOP"))
                    {
                        btn1.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#00dddd"); // Blue
                        btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.Payload +
                                       "\n" + p.PacketEnd;
                    }
                    else if (p.PacketEnd.Equals("EEP"))
                    {
                        btn1.Background = Brushes.Orange;
                        btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.Payload +
                                       "\n" + p.PacketEnd;
                        btn1.Content = p.PacketEnd[0];
                    }
                    else
                    {
                        btn1.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffaacc"); // Pink
                        btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.Payload +
                                       "\n" + p.PacketEnd;
                    }
                }
                btn1.Click += btn_click;
                btn1.Tag = portNr + "" + i;
                PacketViewerA.Children.Add(btn1);
            }
            gData = r;
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
            var selectedRow = (DataGridRow)DetailedViewerA.ItemContainerGenerator.ContainerFromIndex(DetailedViewerA.SelectedIndex);
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
        } //End of InitialiseGraphs

        private void DataRate_Checked(object sender, RoutedEventArgs e)
        {
            var getPlots = new Graphing();

            var plots = getPlots.GetPlots(gData);
            foreach (double plot in plots)
                SeriesCollection[0].Values.Add(plot);
        }

        private void DataRate_Unchecked(object sender, RoutedEventArgs e)
        {
            SeriesCollection[0].Values.Clear();
        }

        private void Errors_Checked(object sender, RoutedEventArgs e)
        {
            var getBars = new Graphing();
            var bars = getBars.GetBars(gData);
            SeriesCollection[1].Values.Add(bars[0]);
            SeriesCollection[2].Values.Add(bars[1]);
            SeriesCollection[3].Values.Add(bars[2]);
            SeriesCollection[4].Values.Add(bars[3]);
        }

        private void Errors_Unchecked(object sender, RoutedEventArgs e)
        {
            SeriesCollection[1].Values.Clear();
            SeriesCollection[2].Values.Clear();
            SeriesCollection[3].Values.Clear();
            SeriesCollection[4].Values.Clear();
        }

        private void initialiseGauge()
        {
            NrOfErrors = gData.ErrorsPresent;
            NrOfPackets = gData.ListOfPackets.Count;
            NrOfCharacters = gData.GetNumberOfCharacters();

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
                    tabControl.Items.Remove(this);
                    App.RecordingData.Remove(this.gData.Port);
                    MainWindow mainWindow= (MainWindow)Application.Current.MainWindow;
                    mainWindow.updateStatistics();
                };
            closeButton.tabHeaderGrid.Children.Add(header);
            Header = closeButton;
        }


        /*
         * Initialise the time stamps for the left side of the overview.
         * Coordinates for the buttons are all 0.
         * Width of the parent is also 0.
         * So couldn't impliment it as we need to know what packet is the left most packet.
         * Cannot do this without width or coordinates.
         * Still researching is there is another way.
         * For the moment it just displays the time stamp of the first packet.
         */
        public void InitialiseTimeStamps()
        {

            double panelWidth = PacketViewerA.ActualWidth;
            Button button = VisualTreeHelper.GetChild(PacketViewerA, 0) as Button;
            double buttonWidth = button.ActualWidth;
            //int numberOfButtonsPerRow = (int)panelWidth / (int)buttonWidth;
            //InitialLabel.Margin = new Thickness(0, 0, 0, 0); //Left, top, right, bottom

            //int childrenCount = VisualTreeHelper.GetChildrenCount(TimeStamps);
            //UIElement contain = VisualTreeHelper.GetChild(TimeStamps, childrenCount - 1) as UIElement;
            //UIElement container = VisualTreeHelper.GetParent(contain) as UIElement;
            //Point relativeLocation = contain.TranslatePoint(new Point(0, yPlus), container);

            //int childrenCount2 = VisualTreeHelper.GetChildrenCount(TimeStamps);


            Button contain3 = VisualTreeHelper.GetChild(PacketViewerA, numberOfButtonsPerRow+1) as Button;
            Point currentPoint = contain3.TransformToAncestor(PacketViewerA).Transform(new Point(0, 0));
            //Button contain3 = VisualTreeHelper.GetChild(PacketViewerA, 1) as Button;
            //UIElement container2 = VisualTreeHelper.GetParent(contain2) as UIElement;
            //Point relativeLocation = contain2.TranslatePoint(new Point(0, yPlus), container2);
            //var relativeLocation2 = contain2.TransformToAncestor(this);

            //string str = null;
            //str = contain2.ToolTip as string;
            //if (str.Contains("P") == true) //ignore the button if it is an error or an "empty space" button
            //{
            // Return the offset vector for the TextBlock object.
            //Vector vector = VisualTreeHelper.GetOffset(contain2);
            // Convert the vector to a point value.
            //Point currentPoint = new Point(vector.X, vector.Y);

            /*UIElement firstItem = ((PacketViewerA.Children)[0] as UIElement);
            double y = firstItem.TranslatePoint(new Point(0, 0), PacketViewerA).Y;

            int counter = 0;
            foreach (UIElement item in PacketViewerA.Children)
            {
                if ((item.TranslatePoint(new Point(0, 0), PacketViewerA).Y != y))
                {
                    break;
                }
                counter++;
            }*/

            //double width = PacketViewerA.ActualWidth;
            //double width = blah.ActualWidth;

            //Point relativePoint = contain3.TransformToVisual(contain2).Transform(new Point(0, 0));

            //Point position = contain2.PointToScreen(new Point(0d, 0d));

            string str2 = null;
            str2 = button.ToolTip as string;
            string str3 = contain3.ToolTip as string;

            var Lbl1 = new Label
            {
                Height = 20,
                FontSize = 9,
                //Content = contain2.ToolTip
                Content = str2.Substring(11, 12)
                //Content = position
            };
            var Lbl2 = new Label
            {
                Height = 20,
                FontSize = 9,
                //Content = contain2.ToolTip
                Content = str3.Substring(11, 12)
                //Content = position
            };

            TimeStamps.Children.Add(Lbl1);
            TimeStamps.Children.Add(Lbl2);

            //}
            //else
            //{
            //do nothing
            //}
        } //End of InitialiseTimeStamps

        private static int GetNumberOfItemsInFirstRow(ItemsControl itemsControl)
        {
            double previousX = -1;
            int itemIndex;

            for (itemIndex = 0; itemIndex < itemsControl.Items.Count; itemIndex++)
            {
                var container = (UIElement)itemsControl.ItemContainerGenerator.ContainerFromIndex(itemIndex);
                var x = container.TranslatePoint(new Point(), itemsControl).X;
                if (x <= previousX)
                {
                    break;
                }
                previousX = x;
            }
            return itemIndex;
        }
    }
}
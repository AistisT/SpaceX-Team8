using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Star_Reader.Model;
using LiveCharts;
using System.ComponentModel;
using System.Globalization;
using LiveCharts.Wpf;
using System.Collections.Generic;
using System.Windows.Input;

namespace Star_Reader
{
    /// <summary>
    /// Interaction logic for RecordedData.xaml
    /// </summary>
    public partial class DetailsTab : TabItem, INotifyPropertyChanged
    {
        private Recording gData;

        private ICollectionView dataGridCollection;
        private string filterString;
        public string[] Labels { get; set; }
        



        public ICollectionView DataGridCollection
        {
            get { return dataGridCollection; }
            set { dataGridCollection = value; NotifyPropertyChanged("DataGridCollection"); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
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

        private void FilterCollection()
        {
            if (dataGridCollection != null)
            {
                dataGridCollection.Refresh();
            }
        }

        public bool Filter(object obj)
        {
            var packet = obj as Packet;
            if (packet == null) return false;
            if (string.IsNullOrEmpty(filterString)) return true;
            return packet.ErrorType != null && CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.ErrorType, filterString, CompareOptions.IgnoreCase) >= 0
                || packet.Payload != null && CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.Payload, filterString, CompareOptions.IgnoreCase) >= 0
                || packet.PacketType != null && CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.PacketType.ToString(), filterString, CompareOptions.IgnoreCase) >= 0
                || packet.PacketEnd != null && CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.PacketEnd, filterString, CompareOptions.IgnoreCase) >= 0
                || packet.Time != null && CultureInfo.CurrentCulture.CompareInfo.IndexOf(packet.Time.ToString("MM/dd/yyyy HH:mm:ss.fff"), filterString, CompareOptions.IgnoreCase) >= 0;
        }

        //Constructor
        public DetailsTab(int portNr)
        {
            InitializeComponent();
            PopulateOverview(portNr);
            DataGridCollection = CollectionViewSource.GetDefaultView(App.RecordingData[portNr].ListOfPackets);
            DataGridCollection.Filter = Filter;
            InitialiseTimeStamps();
            initialiseGauge();


        }

        //generating the button in the overview
        public void PopulateOverview(int portNr)
        { 
            const int size = 20;
            var r = App.RecordingData[portNr];
            if (r == null) return;
            int length = r.ListOfPackets.Count;

            for (int i = 0; i < length; i++)
            {
                Packet p = r.ListOfPackets[i];
                if (i > 0)
                {
                    Packet NextP = r.ListOfPackets[i - 1];
                    TimeSpan td = p.Time.Subtract(NextP.Time);
                    if (td.TotalMilliseconds > 100)
                    {
                        Button btn1s = new Button
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
                                btn1s.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffe699");  // Beige
                                break;
                            default:
                                btn1s.ToolTip = "Empty Space of " + td.Seconds + "." + td.TotalMilliseconds.ToString().Substring(1) + " seconds.";
                                btn1s.Background = Brushes.Crimson;
                                break;
                        }
                        PacketViewerA.Children.Add(btn1s);

                    }
                }
                Button btn1 = new Button
                {
                    Width = size,
                    Height = size
                };
                if (p.PacketType == 'E')
                {
                    switch (p.ErrorType)
                    {
                        case "Disconnect":
                            btn1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff3333"));
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
                        btn1.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#00dddd");   // Blue
                        btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.Payload + "\n" + p.PacketEnd;
                    }
                    else
                        if (p.PacketEnd.Equals("EEP"))
                    {
                        btn1.Background = Brushes.Red;
                        btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.Payload + "\n" + p.PacketEnd;
                        btn1.Content = p.PacketEnd[0];
                    }
                    else
                    {
                        btn1.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffaacc");   // Pink
                        btn1.ToolTip = p.Time + "." + p.Time.ToString("fff") + "\n" + p.PacketType + "\n" + p.Payload + "\n" + p.PacketEnd;
                    }
                }
                btn1.Click += btn_click;
                btn1.Tag = portNr + "" + i;
                PacketViewerA.Children.Add(btn1);
            }
            gData = r;
            InitialiseGraphs();
        }//End of PopulateOverview

        //on click for buttons in overview
        protected void btn_click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            string x = b.Tag.ToString();
            char portc = x[0];
            int port = int.Parse(portc + "");
            int item = int.Parse(x.Substring(1));
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
        }//End of InitialiseGraphs

        private void DataRate_Checked(object sender, RoutedEventArgs e)
        {
            Graphing getPlots = new Graphing();

            List<double> plots = gData.getDataRates();//getPlots.getPlots(gData);
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
        }

        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> Formatter { get; set; }
        public Func<double, string> Formatter1 { get; set; }
        public Func<double, string> Formatter2 { get; set; }


        private void initialiseGauge()
        {
            
          
            errValue = gData.ErrorsPresent;
            packetValue = gData.ListOfPackets.Count;
            charValue = gData.getTotalPackets();

            Formatter = x => x + " ";

            Formatter1 = x => x + " ";

            Formatter2 = x => x + " ";

            DataContext = this;

        }
        public double errValue { get; set; }

        public double packetValue { get; set; }
        
        public double charValue { get; set; }
       
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
            //InitialLabel.Margin = new Thickness(0, 0, 0, 0); //Left, top, right, bottom



            //int childrenCount = VisualTreeHelper.GetChildrenCount(TimeStamps);
            //UIElement contain = VisualTreeHelper.GetChild(TimeStamps, childrenCount - 1) as UIElement;
            //UIElement container = VisualTreeHelper.GetParent(contain) as UIElement;
            //Point relativeLocation = contain.TranslatePoint(new Point(0, yPlus), container);




            //int childrenCount2 = VisualTreeHelper.GetChildrenCount(TimeStamps);

            Button contain2 = VisualTreeHelper.GetChild(PacketViewerA, 0) as Button;
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

            string str2 = null;
            str2 = contain2.ToolTip as string;

            Label Lbl1 = new Label
            {
                Height = 20,
                FontSize = 9,
                //Content = contain2.ToolTip
                Content = str2.Substring(11, 12)
                //Content = width
            };

           // TimeStamps.Children.Add(Lbl1);
            //}
            //else
            //{
            //do nothing
            //}



        }//End of InitialiseTimeStamps

    }
}
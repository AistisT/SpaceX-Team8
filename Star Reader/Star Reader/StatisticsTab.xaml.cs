using System.ComponentModel;
using System.Windows.Controls;
using Dragablz;

namespace Star_Reader
{
    public partial class StatisticsTab : TabItem, INotifyPropertyChanged
    {
        public double NrOfErrors { get; set; }

        public double NrOfPackets { get; set; }

        public double NrOfCharacters { get; set; }
 
        public event PropertyChangedEventHandler PropertyChanged;

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
    }
}

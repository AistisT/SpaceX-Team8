using System.Collections.Generic;
using System.Windows;
using Star_Reader.Model;

namespace Star_Reader
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Dictionary<int, Recording> RecordingData = new Dictionary<int, Recording>();
    }
}
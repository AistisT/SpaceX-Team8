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
        // Dictionary to hold open ports data
        public static Dictionary<int, Recording> RecordingData = new Dictionary<int, Recording>();

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }
    }
}
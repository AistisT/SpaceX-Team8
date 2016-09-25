using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Dragablz;
using Microsoft.Win32;
using Star_Reader.Model;

namespace Star_Reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StatisticsTab statisticsTab;
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();
            InitializeComponent();
            statisticsTab = new StatisticsTab
            {
               Name = "Statistics"
            };
            TabControl.AddToSource(statisticsTab);
        }

        //On click method for upload file button
        private void UploadFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Rec files (*.rec)|*.rec|Text files (*.txt)|*.txt",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)
            };
            if (openFileDialog.ShowDialog() != true) return;
            FileReader fr = new FileReader();
            foreach (var file in openFileDialog.FileNames)
            {
                Recording r = fr.StoreRecording(file);
                var name = "PortTab" + r.Port;

                for (var i = TabControl.Items.Count; i > 0; i--)
                {
                    TabItem item = (TabItem)TabControl.Items[i - 1];
                    if (item.Name.Equals(name))
                    {
                        App.RecordingData.Remove(r.Port);
                        TabControl.Items.Remove(item);
                    }
                }
                App.RecordingData.Add(r.Port, r);
                DetailsTab tab = new DetailsTab(r.Port)
                {
                    Header = "Port " + r.Port,
                    Name = name
                };
                TabControl.AddToSource(tab);
                statisticsTab.CalculateDataForGougeCharts();
            }
        }

        //On click method for exit button
        private void btnExitProgram_Click(object sender, EventArgs e)
        {
            Close();
        }

        //On click method for about button
        private void aboutButton_OnClick(object sender, EventArgs e)
        {
            //http://stackoverflow.com/questions/5851833/c-sharp-wpf-child-window-about-window
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
    }
}
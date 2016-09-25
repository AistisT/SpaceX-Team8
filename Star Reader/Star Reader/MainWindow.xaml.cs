using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Star_Reader.Model;
using LiveCharts;

namespace Star_Reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //constructor
        public MainWindow()
        {
            InitializeComponent();
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

            if (openFileDialog.ShowDialog() == true)
            {
                FileReader fr = new FileReader();
                foreach (var file in openFileDialog.FileNames)
                {
                    Recording r = fr.StoreRecording(file);
                    var name = "PortTab" + r.Port;

                    for (var i = TabControl.Items.Count; i >0 ; i--)
                    {
                        TabItem item = (TabItem)TabControl.Items[i-1];
                        if (!item.Name.Equals(name)) continue;
                        App.RecordingData.Remove(r.Port);
                        TabControl.Items.Remove(item);
                    }
                    App.RecordingData.Add(r.Port, r);
                    DetailsTab tab = new DetailsTab(r.Port)
                    {
                        Header = "Port " + r.Port,
                        Name = name
                    };
                    TabControl.AddToSource(tab);
                }
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


using System;
using System.Windows;
using System.Windows.Controls;

namespace Star_Reader
{
    /// <summary>
    /// Interaction logic for CloseButton.xaml
    /// </summary>
    
    public partial class CloseButton : UserControl
    {
        public CloseButton()
        {
            InitializeComponent();
        }

        public event EventHandler Click;

        // Tab close button
        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (Click != null)
                Click(sender, e);
        }
    }
}
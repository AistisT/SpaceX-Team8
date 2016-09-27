using System;
using System.Collections.Generic;
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

namespace Star_Reader
{
    /// <summary>
    /// Interaction logic for CloseButton.xaml
    /// </summary>
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public partial class CloseButton : UserControl
    {
        public event EventHandler Click;

        public CloseButton()
        {
            InitializeComponent();
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (Click != null)
            {
                Click(sender, e);
            }
        }
    }
}

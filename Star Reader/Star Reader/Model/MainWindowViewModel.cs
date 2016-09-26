using Dragablz;

namespace Star_Reader.Model
{
    internal class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            InterTabClient = new TabHostInterTabClient();
        }

        public IInterTabClient InterTabClient { get; set; }
    }
}
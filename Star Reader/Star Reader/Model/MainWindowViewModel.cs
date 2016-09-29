using Dragablz;

namespace Star_Reader.Model
{
    internal class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            //Host window for dragable tabs
            InterTabClient = new TabHostInterTabClient();
        }

        public IInterTabClient InterTabClient { get; set; }
    }
}
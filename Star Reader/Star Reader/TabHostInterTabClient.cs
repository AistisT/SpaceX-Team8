using System.Windows;
using Dragablz;

namespace Star_Reader
{
    public class TabHostInterTabClient : IInterTabClient
    {
        //Create new tab host window
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            var view = new TabHostWindow();
            return new NewTabHost<TabHostWindow>(view, view.TabControl);
        }

        //what happens when host window gets closed
        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }
    }
}
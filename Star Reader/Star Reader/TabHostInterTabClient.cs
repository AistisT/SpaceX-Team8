using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dragablz;
using Star_Reader.Model;

namespace Star_Reader
{
    public class TabHostInterTabClient : IInterTabClient
    {
        public INewTabHost<System.Windows.Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            var view = new TabHostWindow();
            return new NewTabHost<TabHostWindow>(view, view.TabControl);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }
    }
}

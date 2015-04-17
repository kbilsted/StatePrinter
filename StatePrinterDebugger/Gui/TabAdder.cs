using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms.Integration;

using FastColoredTextBoxNS;

namespace StatePrinterDebugger.Gui
{
    class TabAdder
    {
        readonly MainWindow mainWindow;

        public TabAdder(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public void AddTab(string outerTabName, string innerTabName, string contentHeader, string content)
        {
            if (String.IsNullOrEmpty(contentHeader))
                contentHeader = "";
            else
                contentHeader += Environment.NewLine;

            TabItem tabItem = CreateEditor(innerTabName, contentHeader + content);

            TabControl panelToAddTo = FindOrCreatePanelToAddTo(outerTabName);
            panelToAddTo.Items.Add(tabItem);
        }

        TabItem CreateEditor(string tabName, string content)
        {
            var editor = new FastColoredTextBox { Text = content, WideCaret = true };
            var host = new WindowsFormsHost { Child = editor };

            var tabItem = new TabItem()
            {
                Header = tabName,
                Content = host
            };

            mainWindow.HackToEnsureFocusForWinFormsControl(host, editor);

            return tabItem;
        }

        TabControl FindOrCreatePanelToAddTo(string groupName)
        {
            var tabControl = TryFindPanelToAdd(groupName);
            return tabControl ?? CreateInnerTabControl(groupName);
        }

        TabControl CreateInnerTabControl(string groupName)
        {
            var innerTabControl = new TabControl();
            var item = new TabItem() { Header = groupName, Content = innerTabControl, };
            mainWindow.GroupTabs.Items.Add(item);

            return innerTabControl;
        }

        TabControl TryFindPanelToAdd(string groupName)
        {
            var groupTab = mainWindow.GroupTabs.Items
                .Cast<TabItem>()
                .FirstOrDefault(x => (string)x.Header == groupName);

            if (groupTab != null)
                return (TabControl)groupTab.Content;
            
            return null;
        }

        /// <summary>
        /// Setup the window for the initial state
        /// </summary>
        public void InitialSetup()
        {
            AddTab("boo", "my cool", null, "dsfdsf æj sdf \n new line\n...");
            AddTab("boo", "my cool2", null, "aaaaaaaaaaaaaaaaaaaa..");
            AddTab("boo", "my cool3", null, "bbbb bbb bbb..");
        }
    }
}

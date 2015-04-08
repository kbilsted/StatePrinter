using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Threading;

using FastColoredTextBoxNS;

namespace StatePrinterDebugger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AddTab("boo", "my cool", null, "dsfdsf æj sdf \n new line\n...");
            AddTab("boo", "my cool2", null, "aaaaaaaaaaaaaaaaaaaa..");
            AddTab("boo", "my cool3", null, "bbbb bbb bbb..");
        }

        public void AddTab(string groupName, string tabName, string contentHeader, string content)
        {
            if (groupName == null) throw new ArgumentNullException("groupName");
            if (tabName == null) throw new ArgumentNullException("tabName");
            if (content == null) throw new ArgumentNullException("content");

            if (String.IsNullOrEmpty(contentHeader)) 
                contentHeader = "";
            else 
                contentHeader += Environment.NewLine;

            TabItem tabItem = CreateEditor(tabName, contentHeader + content);

            TabControl panelToAddTo = FindOrCreatePanelToAddTo(groupName);
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
 
            HackToEnsureFocusForWinFormsControl(host, editor);

            return tabItem;
        }

        void HackToEnsureFocusForWinFormsControl(WindowsFormsHost host, FastColoredTextBox editor)
        {
            host.IsVisibleChanged += (sender, evtArg) =>
                {
                    if ((bool)evtArg.NewValue)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() => editor.Focus()));
                    }
                };
        }

        TabControl FindOrCreatePanelToAddTo(string groupName)
        {
            var groupTab = GroupTabs.Items
                .Cast<TabItem>()
                .FirstOrDefault(x => (string)x.Header == groupName);

            if (groupTab != null)
                return (TabControl)groupTab.Content;

            var innerTabControl = new TabControl();
            var item = new TabItem()
            {
                Header = groupName,
                Content = innerTabControl,
            };
            GroupTabs.Items.Add(item);

            return innerTabControl;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
        }

        void OnClick_ButtonBold(object sender, RoutedEventArgs e)
        {
        }

        void MenuItem_Find(object sender, RoutedEventArgs e)
        {
        }
    }
}

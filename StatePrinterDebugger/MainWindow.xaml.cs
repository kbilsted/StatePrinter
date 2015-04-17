using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Threading;

using FastColoredTextBoxNS;

using StatePrinterDebugger.Gui;

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
        }


        internal void HackToEnsureFocusForWinFormsControl(WindowsFormsHost host, FastColoredTextBox editor)
        {
            host.IsVisibleChanged += (sender, evtArg) =>
            {
                if ((bool)evtArg.NewValue)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() => editor.Focus()));
                }
            };
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

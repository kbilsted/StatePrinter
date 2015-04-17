using System;
using System.Windows;

using StatePrinterDebugger.Gui;

namespace StatePrinterDebugger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            InitializeComponent();
        }

        [STAThread]
        static void Main()
        {
            App app = new App();
            MainWindow window = new MainWindow();
            TabAdder adder = new TabAdder(window);
            adder.InitialSetup();
            var debugger = new Debugger(adder);
            app.Run(window);
        }
    }
}

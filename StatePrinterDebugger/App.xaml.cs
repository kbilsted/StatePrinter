using System;
using System.Windows;

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
            app.Run(window);
        }
    }
}

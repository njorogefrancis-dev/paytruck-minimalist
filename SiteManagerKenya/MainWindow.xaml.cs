using System.Windows;

namespace SiteManagerKenya
{
    /// <summary>
    /// Main application window. DataContext (MainWindowViewModel) is assigned
    /// by App.xaml.cs after the DI container is built.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}

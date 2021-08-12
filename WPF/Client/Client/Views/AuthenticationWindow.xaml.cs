using System.Security.Authentication;
using System.Windows;
using Client.ViewModels;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AuthenticationWindow : Window
    {
        public AuthenticationWindow()
        {
            InitializeComponent();
            DataContext = new AuthenticationWindowViewModel();
        }
    }
}
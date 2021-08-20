using System.Windows;
using Client.Models;
using Client.ViewModels;

namespace Client
{
    public partial class MainWindow : Window
    {
        public MainWindow(UserData UserData)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(UserData,this);
        }
    }
}
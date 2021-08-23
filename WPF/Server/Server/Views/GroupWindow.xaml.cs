using System.ComponentModel;
using System.Windows;
using Server.ViewModels;

namespace Server.Views
{
    public partial class GroupWindow : Window
    {
        public GroupWindow()
        {
            InitializeComponent();
            DataContext = new GroupWindowViewModel(this);
        }
        
        void Window_Closing(object sender, CancelEventArgs e)
        {
            this.Owner.IsEnabled = true;
        }
    }
}
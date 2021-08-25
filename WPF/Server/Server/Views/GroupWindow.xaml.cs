using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Server.Models;
using Server.ViewModels;

namespace Server.Views
{
    public partial class GroupWindow : Window
    {

        public Group Group { get; set; }

        public GroupWindow(MainWindowViewModel mainWindowViewModel, bool isEdit=false)
        {
            InitializeComponent();
            DataContext = new GroupWindowViewModel(this,mainWindowViewModel,isEdit);
        }
        
        
    }
}
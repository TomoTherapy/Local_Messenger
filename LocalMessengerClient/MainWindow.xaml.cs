using LocalMessengerClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LocalMessengerClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindow_ViewModel view;

        public MainWindow()
        {
            InitializeComponent();
            view = new MainWindow_ViewModel();
            DataContext = view;
        }

        private void Connect_button_Click(object sender, RoutedEventArgs e)
        {
            view.Connect_button_Click();
        }

        private void SignIn_button_Click(object sender, RoutedEventArgs e)
        {
            view.SignIn_button_Click();
        }

        private void SignOut_button_Click(object sender, RoutedEventArgs e)
        {
            view.SignOut_button_Click();
        }
    }
}

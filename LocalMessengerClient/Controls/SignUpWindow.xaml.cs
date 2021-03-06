using LocalMessengerClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LocalMessengerClient.Controls
{
    /// <summary>
    /// Interaction logic for SignUpWindow.xaml
    /// </summary>
    public partial class SignUpWindow : Window
    {
        SignUpWindow_ViewModel view;
        public SignUpWindow(MainWindow_ViewModel mainView)
        {
            InitializeComponent();
            view = new SignUpWindow_ViewModel(mainView);
            DataContext = view;
        }

        private void SignUp_button_Click(object sender, RoutedEventArgs e)
        {
            view.SignUp_button_Click();
        }
    }
}

using LocalMessengerServer.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LocalMessengerServer
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

        private void ServerStart_button_Click(object sender, RoutedEventArgs e)
        {
            view.ServerStart_button_Click();
        }

        private void Warning_button_Click(object sender, RoutedEventArgs e)
        {
            view.Warning_button_Click();
        }

        private void Block_button_Click(object sender, RoutedEventArgs e)
        {
            view.Block_button_Click();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            view.Window_Closing();
        }

        private void DBWorkbench_button_Click(object sender, RoutedEventArgs e)
        {
            view.DBWorkbench_button_Click();
        }

        bool AutoScroll = true;
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if ((sender as ScrollViewer).VerticalOffset == (sender as ScrollViewer).ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set auto-scroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset auto-scroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : auto-scroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                (sender as ScrollViewer).ScrollToVerticalOffset((sender as ScrollViewer).ExtentHeight);
            }
        }
    }
}

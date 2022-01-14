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
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        ChatWindow_ViewModel view;
        public ChatWindow(string targetId, MainWindow_ViewModel main)
        {
            InitializeComponent();
            view = new ChatWindow_ViewModel(this, main, targetId);
            DataContext = view;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            view.TextBox_KeyDown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            view.Window_Closing();
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

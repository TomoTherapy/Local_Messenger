using LocalMessengerClient.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using ViewModels;

namespace LocalMessengerClient.ViewModels
{

    public class ChatWindow_ViewModel : ViewModelBase
    {
        private ChatWindow chatWindow;
        private MainWindow_ViewModel main;
        private string message;
        private string targetID;

        public string Message { get => message; set { message = value; RaisePropertyChanged(); } }
        public string TargetID { get => targetID; set => targetID = value; }

        public ChatWindow_ViewModel(ChatWindow window, MainWindow_ViewModel main, string targetId)
        {
            this.chatWindow = window;
            this.main = main;
            TargetID = targetId;
        }

        internal void TextBox_KeyDown()
        {
            main.ChatWindowSendMSG(targetID, Message);

            chatWindow.Chat_stackPanel.Children.Add(new TextBlock() { TextAlignment = System.Windows.TextAlignment.Right, Text = Message, Margin = new System.Windows.Thickness(0, 10, 10, 0), Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xA0, 0xE7, 0xFB)) });//FFA0E7FB
            Message = "";
        }

        internal void Window_Closing()
        {
            main.ChatWindowClose(TargetID);
        }

        public void ReceiveMessage(string fromId, string msg)
        {
            chatWindow.Chat_stackPanel.Children.Add(new TextBlock() { TextAlignment = System.Windows.TextAlignment.Left, Text = fromId, Margin = new System.Windows.Thickness(15, 10, 0, 0) });
            chatWindow.Chat_stackPanel.Children.Add(new TextBlock() { TextAlignment = System.Windows.TextAlignment.Left, Text = msg, Margin = new System.Windows.Thickness(10, 5, 0, 0), Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xD3, 0xD3, 0xD3)) });//#FFD3D3D3
        }
    }
}

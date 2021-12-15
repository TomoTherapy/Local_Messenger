using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ViewModels;

namespace LocalMessengerClient.ViewModels
{
    public class SignUpWindow_ViewModel : ViewModelBase
    {
        private MainWindow_ViewModel mainView;
        private string iD;
        private string password;
        private string confirm;

        public string ID { get => iD; set => iD = value; }
        public string Password { get => password; set => password = value; }
        public string Confirm { get => confirm; set => confirm = value; }

        public SignUpWindow_ViewModel(MainWindow_ViewModel mainView)
        {
            this.mainView = mainView;
        }

        internal void SignUp_button_Click()
        {
            if (Password != Confirm)
            {
                MessageBox.Show("Confirmation code does not match with the original password");
                return;
            }
            if (mainView.SignUpRequest(ID, Password) == 1)
            {
                MessageBox.Show("Sign Up success!\nSign In in the main window");
            }
            else
            {
                MessageBox.Show("Sign Up failed!");
            }
        }
    }
}

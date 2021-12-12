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
using System.Windows.Shapes;

namespace LocalMessengerServer.Controls
{
    /// <summary>
    /// Interaction logic for DBWorkbenchWindow.xaml
    /// </summary>
    public partial class DBWorkbenchWindow : Window
    {
        DBWorkbenchWindow_ViewModel view;

        public DBWorkbenchWindow()
        {
            InitializeComponent();
            view = new DBWorkbenchWindow_ViewModel();
            DataContext = view;
        }

        private void AllTable_button_Click(object sender, RoutedEventArgs e)
        {
            view.AllTable_button_Click();
        }

        private void SelectAll_button_Click(object sender, RoutedEventArgs e)
        {
            view.SelectAll_button_Click();
        }

        private void InsertInto_button_Click(object sender, RoutedEventArgs e)
        {
            view.InsertInto_button_Click();
        }

        private void DropTable_button_Click(object sender, RoutedEventArgs e)
        {
            view.DropTable_button_Click();
        }

        private void CreateTable_button_Click(object sender, RoutedEventArgs e)
        {
            view.CreateTable_button_Click();
        }

        private void Update_button_Click(object sender, RoutedEventArgs e)
        {
            view.Update_button_Click();
        }

        private void QueryExecute_btn_Click(object sender, RoutedEventArgs e)
        {
            view.QueryExecute_btn_Click();
        }
    }
}

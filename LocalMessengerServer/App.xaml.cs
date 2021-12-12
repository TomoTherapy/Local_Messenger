using LocalMessengerServer.Devices;
using LocalMessengerServer.Xml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LocalMessengerServer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public XmlParser m_XmlParser;
        public Database m_Database;

        public App()
        {
            m_XmlParser = new XmlParser();
            m_Database = new Database();
        }
    }
}

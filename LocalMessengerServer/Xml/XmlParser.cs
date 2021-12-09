using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LocalMessengerServer.Xml
{
    public class XmlParser
    {
        public SavedData SavedData { get; set; }
        private string path = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\NetworkTCP");
        private XmlSerializer SavedDataXmlSerializer = new XmlSerializer(typeof(SavedData));

        public XmlParser()
        {
            SavedDataLoad();
        }

        public void SavedDataSave()
        {
            try
            {
                Directory.CreateDirectory(path);

                using (TextWriter tw = new StreamWriter(path + @"\SavedData.xml"))
                {
                    SavedDataXmlSerializer.Serialize(tw, SavedData);
                }
            }
            catch
            {

            }
        }

        public void SavedDataLoad()
        {
            try
            {
                if (File.Exists(path + @"\SavedData.xml"))
                {
                    using (TextReader tr = new StreamReader(path + @"\SavedData.xml"))
                    {
                        SavedData = SavedDataXmlSerializer.Deserialize(tr) as SavedData;
                    }
                }
                else
                {
                    SavedData = new SavedData();
                    SetDefault();
                }
            }
            catch
            {

            }
        }

        public void SetDefault()
        {
            SavedData.ShowTimeStamp = true;
            SavedData.ServerPort = 0;
        }


    }

    public class SavedData
    {
        public bool ShowTimeStamp { get; set; }
        public int ServerPort { get; set; }
    }
}

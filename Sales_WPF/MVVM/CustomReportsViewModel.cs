using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Sales_WPF.MVVM;

namespace Sales_WPF
{
    internal class CustomReportsViewModel : INotifyPropertyChanged
    {
        public List<Report> ListReports { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public List<Report> GetReports()
        {
            var list = new List<Report>();

            try
            {
                var serializer = new XmlSerializer(typeof(ReportConfig));
                var reader = new StreamReader("reports.xml");

                var obj = (ReportConfig) serializer.Deserialize(reader);
                list = obj.ReportList;

                return list;
            }
            catch (XmlException)
            {
                return list;
            }

            catch (FileNotFoundException)
            {
                return list;
            }
        }

        public CustomReportsViewModel()
        {
            ListReports = Common.GetReports();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using SQLite.Net.Platform.Generic;
using SQLiteNetExtensions.Extensions;
using SQLitePCL;
using SQLiteConnection = SQLite.Net.SQLiteConnection;

namespace Sales_WPF.MVVM
{
    public static class Common
    {
       

        public static ObservableCollection<Products> GetAllProducts()
        {
            List<Products> list;
            ObservableCollection<Products> list2;
            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                list = db.GetAllWithChildren<Products>(recursive: true).ToList().Where(x => x.ProductStatus == 1)
                    .OrderBy(x => x.Producer.ProducerName).ToList();
            }

            list2 = new ObservableCollection<Products>(list);
            return list2;
        }

        public static DataTable ExecuteQuery(Report rep, string parameter = "")
        {
            var dt = new DataTable();
            var qr_chk = rep.Query.ToLower();

            if (
                qr_chk.Contains("create") || qr_chk.Contains("delete") || qr_chk.Contains("alter") ||
                qr_chk.Contains("update") ||
                qr_chk.Contains("drop") || qr_chk.Contains("trunc") || qr_chk.Contains("insert")
            )

            {
            }
            else
            {
                var qr = rep.Query;

                foreach (var c in rep.ParamsList) qr = qr.Replace('{' + c.Name + '}', c.Value);
                // rep.Query = qr;

                qr = qr.Replace("{parameter}", parameter);

                var ConString = "sale.db";
                using (var con = new SQLitePCL.SQLiteConnection(ConString))
                {
                    using (var statement = con.Prepare(qr))
                    {
                        try
                        {
                            for (var i = 0; i < statement.ColumnCount; i++)
                            {
                                var column = new DataColumn(statement.ColumnName(i));

                                foreach (var l in rep.ColumnList)
                                    if (column.ColumnName.ToLower() == l.Name.ToLower())
                                        if (l.Format == "decimal")
                                            column.DataType = Type.GetType("System.Int32");
                                dt.Columns.Add(column);
                            }

                            while (statement.Step() == SQLiteResult.ROW)
                            {
                                DataRow dr;
                                dr = dt.NewRow();

                                for (var i = 0; i < statement.ColumnCount; i++) dr[i] = statement[i];
                                dt.Rows.Add(dr);
                            }
                        }
                        catch (DuplicateNameException)
                        {
                            // MessageBox.Show(duplcol);
                            statement.Dispose();
                        }
                    }
                }
            }

            return dt;
        }

        public static List<Report> GetReports(int? reportid = null)
        {
            var list = new List<Report>();

            try
            {
                var serializer = new XmlSerializer(typeof(ReportConfig));
                var reader = new StreamReader("reports.xml");

                var obj = (ReportConfig) serializer.Deserialize(reader);

                if (reportid != null)
                    list = obj.ReportList.FindAll(x => x.ReportID == reportid);

                else list = obj.ReportList.FindAll(x => x.Type == "");

                return list;
            }
            catch (XmlException)
            {
                //  MessageBox.Show(xmlfile);
                return list;
            }

            catch (FileNotFoundException)
            {
                //MessageBox.Show(reportfile);
                return list;
            }
        }
    }
}
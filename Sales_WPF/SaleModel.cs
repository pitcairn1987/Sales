using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Sales_WPF;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.Generic;
using SQLiteNetExtensions.Attributes;

namespace Sales_WPF
{
    [XmlRoot("config")]
    public class ReportConfig
    {
        [XmlArray("reports")]
        [XmlArrayItem("report")]
        public List<Report> ReportList { get; set; }
    }

    public class Report
    {
        [XmlElement("id")] public int ReportID { get; set; }
        [XmlElement("title")] public string Name { get; set; }
        [XmlElement("type")] public string Type { get; set; }
        [XmlElement("query")] public string Query { get; set; }

        [XmlArray("columns")]
        [XmlArrayItem("column")]
        public List<Column> ColumnList { get; set; }

        [XmlArray("params")]
        [XmlArrayItem("param")]
        public List<Param> ParamsList { get; set; }
    }

    public class Column
    {
        [XmlAttribute("name")] public string Name { get; set; }
        [XmlAttribute("caption")] public string Caption { get; set; }
        [XmlAttribute("format")] public string Format { get; set; }
        [XmlAttribute("background")] public string Backgorund { get; set; }
        [XmlAttribute("foreground")] public string Foreground { get; set; }
        [XmlAttribute("align")] public string Align { get; set; }
        [XmlAttribute("passtoreport")] public string PassToReport { get; set; }
    }

    public class Param
    {
        [XmlAttribute("name")] public string Name { get; set; }
        [XmlAttribute("caption")] public string Caption { get; set; }
        [XmlAttribute("value")] public string Value { get; set; }
    }

    public class Events
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        [PrimaryKey] [AutoIncrement] public int EventID { get; set; }
        public string EventName { get; set; }
        public string EventDate { get; set; }
        public int EventTypeID { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<SaleDetails> ListSales { get; set; }

        public Events()
        {
            ListSales = new ObservableCollection<SaleDetails>();
        }
    }
}

public class Products
{
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        var handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }

    [PrimaryKey] [AutoIncrement] public int ProductID { get; set; }
    private string _productname;

    public string ProductName
    {
        get => _productname;
        set
        {
            if (value == _productname) return;
            _productname = value;
            OnPropertyChanged("ProductName");
        }
    }

    private int _price;

    public int Price
    {
        get => _price;
        set
        {
            if (value == _price) return;
            _price = value;
            OnPropertyChanged("Price");
        }
    }

    private string _productcode;

    public string ProductCode
    {
        get => _productcode;
        set
        {
            if (value == _productcode) return;
            _productcode = value;
            OnPropertyChanged("ProductCode");
        }
    }

    public double Prod { get; set; }
    public double Tax { get; set; }
    public int ProductStatus { get; set; }

    [ForeignKey(typeof(Producers))] public int ProducerID { get; set; }
    [OneToOne] public Producers Producer { get; set; }
    public string FullName => Producer.ProducerName + " " + ProductName;
}

public class SaleDetails
{
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        var handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }

    [PrimaryKey] [AutoIncrement] public int SaleDetailID { get; set; }
    [ForeignKey(typeof(Events))] public int EventID { get; set; }
    [ForeignKey(typeof(Products))] public int ProductID { get; set; }
    public int Price { get; set; }
    public int Qty { get; set; }
    public double Prod { get; set; }
    public int SumUp { get; set; }
    public bool IsRegister { get; set; }

    private string _product;

    [Ignore]
    public string Product
    {
        get
        {
            string pname;

            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                pname = db.Table<Products>().FirstOrDefault(x => x.ProductID == ProductID)?.ProductName;
            }

            return pname;
        }
        set
        {
            if (value == _product) return;
            _product = value;
            OnPropertyChanged("Product");
        }
    }
}

public class CostM
{
    [PrimaryKey] [AutoIncrement] public int CostID { get; set; }
    public int Cost { get; set; }
    public string Description { get; set; }
    public string CostDate { get; set; }

    public string DateSubstr
    {
        get
        {
            var date = new DateTime();
            date = DateTime.Parse(CostDate);

            return " " + date.ToString("dd-MM-yy");
        }
    }
}

public class Producers
{
    [PrimaryKey] [AutoIncrement] public int ProducerID { get; set; }
    public string ProducerName { get; set; }
    public string Info { get; set; }
    public int ProducerStatus { get; set; }
}



public class repProducers
{
    public string ProducerName { get; set; }
    public int suma { get; set; }
    public string zysk { get; set; }
    public string cnt { get; set; }
}

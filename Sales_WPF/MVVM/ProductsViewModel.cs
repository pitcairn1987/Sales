using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Sales_WPF.MVVM;
using SQLite.Net;
using SQLite.Net.Platform.Generic;

namespace Sales_WPF
{
    internal class ProductsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Products> _listproducts;

        public ObservableCollection<Products> ListProducts
        {
            get => _listproducts ?? (_listproducts = new ObservableCollection<Products>());
            set
            {
                _listproducts = value ?? throw new ArgumentNullException(nameof(value));
                OnPropertyChanged("ListProducts");
            }
        }

        public List<Producers> ListProducers { get; set; }
        public Products NewProduct { get; set; }
        public Producers SelectedProducer { get; set; }
        public Products SelectedProduct { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<Producers> GetAllProducers()
        {
            List<Producers> list;
            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                list = db.Table<Producers>().ToList();
            }

            return list;
        }

        private RelayCommand _addProductCommand;

        public ICommand AddProductCommand
        {
            get
            {
                if (_addProductCommand == null) _addProductCommand = new RelayCommand(param => AddProduct());
                return _addProductCommand;
            }
        }

        private RelayCommand _delProductCommand;

        public ICommand DelProductCommand
        {
            get
            {
                if (_delProductCommand == null) _delProductCommand = new RelayCommand(param => DeleteProduct());
                return _delProductCommand;
            }
        }

        public ProductsViewModel()
        {
            ListProducts = Common.GetAllProducts();
            ListProducers = GetAllProducers();
            NewProduct = new Products();
            SelectedProducer = new Producers();
            SelectedProduct = new Products();
        }

        private void AddProduct()
        {
            var p = NewProduct;
            p.ProductStatus = 1;
            p.ProducerID = SelectedProducer.ProducerID;
            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                db.Insert(p);
                ListProducts = Common.GetAllProducts();
            }
        }

        private void DeleteProduct()
        {
            var vp = SelectedProduct;

            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                var prod = db.Table<Products>().FirstOrDefault(x => x.ProductID == vp.ProductID);
                if (prod != null)
                {
                    prod.ProductStatus = 0;
                    db.Update(prod);
                }
            }

            ListProducts = Common.GetAllProducts();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Sales_WPF.MVVM;
using SQLite.Net;
using SQLite.Net.Platform.Generic;
using SQLiteNetExtensions.Extensions;

namespace Sales_WPF
{
    internal class ShopViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Events> _listShopDays;
        public Events SelectedEvent { get; set; }
        public ObservableCollection<Products> ListProducts { get; set; }

        public ObservableCollection<Events> ListShopDays
        {
            get => _listShopDays;
            set
            {
                _listShopDays = value;
                OnPropertyChanged("ListShopDays");
            }
        }

        public SaleDetails NewSale { get; set; }
        public Products SelectedProduct { get; set; }
        public SaleDetails SelectedSale { get; set; }
        public Events NewEvent { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<Events> GetAllShopDays()
        {
            List<Events> list;
            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                list = db.GetAllWithChildren<Events>(recursive: true)
                    .OrderByDescending(x => DateTime.Parse(x.EventDate)).ToList();
            }

            var list2 = new ObservableCollection<Events>(list);
            return list2;
        }

        private RelayCommand _addSaleCommand;

        public ICommand AddSaleCommand
        {
            get
            {
                if (_addSaleCommand == null) _addSaleCommand = new RelayCommand(param => AddSale());
                return _addSaleCommand;
            }
        }

        private RelayCommand _delSaleCommand;

        public ICommand DeleteSaleCommand
        {
            get
            {
                if (_delSaleCommand == null) _delSaleCommand = new RelayCommand(param => DeleteSale());
                return _delSaleCommand;
            }
        }

        private RelayCommand _addEventCommand;

        public ICommand AddEventCommand
        {
            get
            {
                if (_addEventCommand == null) _addEventCommand = new RelayCommand(param => AddEvent());
                return _addEventCommand;
            }
        }

        private RelayCommand _deleteEventCommand;

        public ICommand DeleteEventCommand
        {
            get
            {
                if (_deleteEventCommand == null) _deleteEventCommand = new RelayCommand(param => DeleteEvent());
                return _deleteEventCommand;
            }
        }

        private RelayCommand _editingCommand;

        public ICommand EditCommand
        {
            get
            {
                if (_editingCommand == null) _editingCommand = new RelayCommand(param => EditSale());
                return _editingCommand;
            }
        }

        private void EditSale()
        {
            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                db.Update(SelectedSale);
                SaleSum = SelectedEvent.ListSales.Sum(x => x.Price * x.Qty);
            }
        }

        private void AddEvent()
        {
            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                var c = EventDay;
                NewEvent.EventTypeID = 4;
                var date = DateTime.Parse(EventDay.ToShortDateString());

                var polish = new CultureInfo("pl-PL");
                var day = polish.DateTimeFormat.DayNames[(int) date.DayOfWeek];

                NewEvent.EventName = day + " " + date.ToString("yyyy-MM-dd");
                NewEvent.EventDate = date.ToString("yyyy-MM-dd");

                db.Insert(NewEvent);

                ListShopDays.Add(NewEvent);
                ListShopDays = GetAllShopDays();

                NewEvent = new Events();
            }
        }

        public int SaleQty { get; set; }
        public int SalePrice { get; set; }
        public bool SaleRegister { get; set; }
        public DateTime EventDay { get; set; }
        private int _salesum { get; set; }

        public int SaleSum
        {
            get => _salesum;
            set
            {
                _salesum = value;
                OnPropertyChanged("SaleSum");
            }
        }

        private void AddSale()
        {
            if (SelectedEvent == null)
            {
            }

            else
            {
                using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
                {
                    NewSale.EventID = SelectedEvent.EventID;
                    NewSale.ProductID = SelectedProduct.ProductID;
                    NewSale.Prod = SelectedProduct.Prod;
                    NewSale.Qty = SaleQty;
                    NewSale.Price = SalePrice;
                    NewSale.IsRegister = SaleRegister;

                    db.Insert(NewSale);
                    SelectedEvent.ListSales.Add(NewSale);
                    SelectedEvent.ListSales.OrderByDescending(x => x.SaleDetailID);
                    SaleSum = SelectedEvent.ListSales.Sum(x => x.Price * x.Qty);

                    NewSale = new SaleDetails();
                }
            }
        }

        private void DeleteSale()
        {
            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                db.Delete(SelectedSale);
                SelectedEvent.ListSales.Remove(SelectedSale);
                SaleSum = SelectedEvent.ListSales.Sum(x => x.Price * x.Qty);
            }
        }

        private void DeleteEvent()
        {
            var result = MessageBox.Show("Usunąć cały dzień", "Ostrzeżenie", MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
                {
                    db.DeleteAll(SelectedEvent.ListSales);
                    db.Delete(SelectedEvent);
                    ListShopDays.Remove(SelectedEvent);
                }
        }

        public ShopViewModel()
        {
            SaleQty = 1;
            ListShopDays = GetAllShopDays();
            ListProducts = Common.GetAllProducts();
            SelectedEvent = new Events();
            SelectedProduct = new Products();

            NewSale = new SaleDetails();
            NewEvent = new Events();
            SelectedSale = new SaleDetails();
        }
    }
}
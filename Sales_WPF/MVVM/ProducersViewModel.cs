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
    internal class ProducersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Producers> _listProducers;

        public ObservableCollection<Producers> ListProducers
        {
            get => _listProducers ?? (_listProducers = new ObservableCollection<Producers>());
            set
            {
                _listProducers = value ?? throw new ArgumentNullException(nameof(value));
                OnPropertyChanged("ListProducers");
            }
        }

        public Producers NewProducer { get; set; }
        public Producers SelectedProducer { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<Producers> GetAllProducers()
        {
            List<Producers> list;
            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                list = db.Table<Producers>().Where(x => x.ProducerStatus == 1).ToList();
            }

            var list2 = new ObservableCollection<Producers>(list);
            return list2;
        }

        private RelayCommand _addProducerCommand;

        public ICommand AddProducerCommand
        {
            get
            {
                if (_addProducerCommand == null) _addProducerCommand = new RelayCommand(param => AddProducer());
                return _addProducerCommand;
            }
        }

        private RelayCommand _delProducerCommand;

        public ICommand DelProducerCommand
        {
            get
            {
                if (_delProducerCommand == null) _delProducerCommand = new RelayCommand(param => DeleteProducer());
                return _delProducerCommand;
            }
        }

        public ProducersViewModel()
        {
            ListProducers = GetAllProducers();
            NewProducer = new Producers();
            SelectedProducer = new Producers();
        }

        private void AddProducer()
        {
            var p = NewProducer;
            p.ProducerStatus = 1;
            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                db.Insert(p);
                ListProducers = GetAllProducers();
            }
        }

        private void DeleteProducer()
        {
            var vp = SelectedProducer;

            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                var prod = db.Table<Producers>().FirstOrDefault(x => x.ProducerID == vp.ProducerID);
                if (prod != null)
                {
                    prod.ProducerStatus = 0;
                    db.Update(prod);
                }
            }

            ListProducers = GetAllProducers();
        }
    }
}
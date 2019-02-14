using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Sales_WPF.MVVM;
using SQLite.Net;
using SQLite.Net.Platform.Generic;

namespace Sales_WPF
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Report SelectedReport;
        public Events SelectedEvent;

        private DataTable dt;

        public MainWindow()
        {
            InitializeComponent();

            var ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "dd-MM-yy";
            Thread.CurrentThread.CurrentCulture = ci;

            datePicker.SelectedDate = DateTime.Now.Date;
        }

        private void dtgProducts_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var p = e.Row.DataContext as Products;
            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                db.Update(p);
            }
        }

        private void lstReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedReport = (Report) lstCustomReports.SelectedItem;
            btnGenerateReport.Visibility = SelectedReport.ParamsList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            if (stkParams.Children.Count > 0) stkParams.Children.RemoveRange(0, stkParams.Children.Count);

            dtgCustomReport.Visibility = Visibility.Visible;

            dt = new DataTable();
            dt = Common.ExecuteQuery(SelectedReport);

            foreach (var c in SelectedReport.ParamsList)
            {
                var lbl = new Label
                {
                    Content = c.Caption
                };
                stkParams.Children.Add(lbl);

                var txb = new TextBox
                {
                    Name = c.Name,
                    Text = c.Value
                };

                stkParams.Children.Add(txb);
            }

            dtgCustomReport.DataContext = dt.DefaultView;

            UpdateLayout();

        }

        private void cmbProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pr = (Products) cmbProducts.SelectedItem;

            txtSalePrice.Text = pr.Price.ToString();
        }

        private void dtgCustomReport_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var item = e.Row.Item as DataRowView;

            try
            {
                var color = item.Row["RowColor"].ToString();

                if (color != "")
                    e.Row.Background = new SolidColorBrush((Color) ColorConverter.ConvertFromString(color));
            }
            catch
            {
            }
        }

        private void dtgCustomReport_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dtgCustomReport.CurrentCell.Column != null)
            {
                var index = dtgCustomReport.CurrentCell.Column.DisplayIndex;

                var dataRow = (DataRowView) dtgCustomReport.SelectedItem;
                var extReport = new Report();
                var cellValue = dataRow.Row.ItemArray[index].ToString();
                var c = dtgCustomReport.CurrentCell.Column;

                if (SelectedReport.ColumnList[c.DisplayIndex].PassToReport != null)
                {
                    var ExtendReports = Common.GetReports(short.Parse(SelectedReport.ColumnList[c.DisplayIndex].PassToReport));
                    extReport = ExtendReports.FirstOrDefault();
                    if (extReport != null)
                    {
                        extReport.ParamsList = SelectedReport.ParamsList;

                        var win = new CustomReportsExtendWindow(extReport, cellValue);
                        win.Show();
                    }
                }
            }
        }

        private void btnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            foreach (var c in stkParams.Children)
                if (c is TextBox txb)
                {
                    foreach (var p in SelectedReport.ParamsList)
                        if (txb.Name == p.Name)
                            p.Value = txb.Text;
                }

            dt = Common.ExecuteQuery(SelectedReport);
            dtgCustomReport.DataContext = dt.DefaultView;
        
        }

        private void lstShopDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedEvent = (Events) lstShopDays.SelectedItem;
            if (SelectedEvent != null) txbSalesSum.Text = SelectedEvent.ListSales.Sum(x => x.Price * x.Qty).ToString();
        }

        private void dtgCustomReport_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            for (var i = 0; i < SelectedReport.ColumnList.Count; i++) e.Column.Visibility = Visibility.Collapsed;

            foreach (var t in SelectedReport.ColumnList)
                if (e.Column.Header.ToString().ToLower() == t.Name.ToLower())
                {
                    e.Column.Header = t.Caption;
                    e.Column.Visibility = Visibility.Visible;

                    var style = new Style(typeof(DataGridColumnHeader));

                    var value = Enum.Parse(typeof(HorizontalAlignment), t.Align);

                    var AlignSetter = new Setter(HorizontalContentAlignmentProperty, value);
                    var BackgroundSeter = new Setter(BackgroundProperty,
                        new SolidColorBrush(
                            (Color) ColorConverter.ConvertFromString(t.Backgorund)));
                    var ForegroundSeter = new Setter(ForegroundProperty,
                        new SolidColorBrush(
                            (Color) ColorConverter.ConvertFromString(t.Foreground)));
                    style.Setters.Add(AlignSetter);
                    style.Setters.Add(BackgroundSeter);
                    style.Setters.Add(ForegroundSeter);
                    e.Column.HeaderStyle = style;
                }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {


            var sl = e.Source as Producers;
           // var ff = dtgProducers.
            using (var db = new SQLiteConnection(new SQLitePlatformGeneric(), "sale.db"))
            {
                db.Update(sl);
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var sl = e.Source as Producers;

        }
    }
}
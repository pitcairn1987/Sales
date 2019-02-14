using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Sales_WPF.MVVM;

namespace Sales_WPF
{
    /// <summary>
    ///     Interaction logic for CustomReportsExtendWindow.xaml
    /// </summary>
    public partial class CustomReportsExtendWindow : Window
    {
        public Report Report;

        public CustomReportsExtendWindow(Report rep, string parameter)
        {
            Report = rep;
            InitializeComponent();

            var dt = Common.ExecuteQuery(Report, parameter);

            dtgExtendReport.DataContext = dt.DefaultView;

        }

        private void dtgExtendReport_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dtgExtendReport.CurrentCell.Column != null)
            {
                var index = dtgExtendReport.CurrentCell.Column.DisplayIndex;
                var dataRow = (DataRowView) dtgExtendReport.SelectedItem;
                var cellValue = dataRow.Row.ItemArray[index].ToString();
                var c = dtgExtendReport.CurrentCell.Column;

                if (Report.ColumnList[c.DisplayIndex].PassToReport != null)
                {
                  var  ExtendReports = Common.GetReports(short.Parse(Report.ColumnList[c.DisplayIndex].PassToReport));
                   var extReport = ExtendReports.FirstOrDefault();
                    if (extReport != null)
                    {
                        extReport.ParamsList = Report.ParamsList;

                        var win = new CustomReportsExtendWindow(extReport, cellValue);
                        win.Show();
                    }
                }
            }
        }

        private void dtgExtendReport_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var item = e.Row.Item as DataRowView;

            try
            {
                if (item != null)
                {
                    var color = item.Row["RowColor"].ToString();

                    if (color != "")
                        e.Row.Background = new SolidColorBrush((Color) ColorConverter.ConvertFromString(color));
                }
            }
            catch
            {
                // ignored
            }
        }

        private void dtgExtendReport_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            for (var i = 0; i < Report.ColumnList.Count; i++) e.Column.Visibility = Visibility.Collapsed;

            foreach (var t in Report.ColumnList)
                if (e.Column.Header.ToString().ToLower() == t.Name.ToLower())
                {
                    e.Column.Header = t.Caption;
                    e.Column.Visibility = Visibility.Visible;

                    var style = new Style(typeof(DataGridColumnHeader));

                    var value = Enum.Parse(typeof(HorizontalAlignment), t.Align);

                    var AlignSetter = new Setter(HorizontalContentAlignmentProperty, value);
                    var BackgroundSeter = new Setter(BackgroundProperty,
                        new SolidColorBrush((Color) ColorConverter.ConvertFromString(t.Backgorund)));
                    var ForegroundSeter = new Setter(ForegroundProperty,
                        new SolidColorBrush((Color) ColorConverter.ConvertFromString(t.Foreground)));
                    style.Setters.Add(AlignSetter);
                    style.Setters.Add(BackgroundSeter);
                    style.Setters.Add(ForegroundSeter);
                    e.Column.HeaderStyle = style;
                }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace HttpServerConfig
{
    //A Separate MyDataGrid class has to be derived as the Official DataGrid Class Enter Key behavior had to be overridden
    //The default enter key causes the grid to change lines
    //This is overridden so that instead of changing lines, the enter key would close the window or etc.
    //THe method is executed by the 'EnterKeyAction' action delegate.
    public class OPDataGrid : DataGrid
    {
        public OPDataGrid()
        {
            this.SelectionChanged += OPDataGrid_SelectionChanged;
        }

        void OPDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectedItemsList != null)
            {
                this.SelectedItemsList.Clear();
                foreach (var item in this.SelectedItems) { this.SelectedItemsList.Add(item); }
            }
        }

        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsListProperty = DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(OPDataGrid), new PropertyMetadata(null));

        public Action EnterKeyAction;
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EnterKeyAction();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        public delegate void RowClickedHandler(object sender, DataGridRowClickedArgs e);
        private double _DoubleClickTime = 1500;
        public double DoubleClickTime
        {
            get
            {
                return _DoubleClickTime;
            }
            set
            {
                _DoubleClickTime = value;
            }
        }

        /*
        protected void OnRowClicked()
        {
            if (RowClicked != null)
            {
                RowClicked(this, new DataGridRowClickedArgs
          (_LastDataGridRow, _LastDataGridColumn, _LastDataGridCell, _LastObject));
            }
        }
         */
    }

    public class DataGridRowClickedArgs
    {
        public DataGridRow DataGridRow { get; set; }
        public DataGridColumn DataGridColumn { get; set; }
        public DataGridCell DataGridCell { get; set; }
        public object DataGridRowItem { get; set; }

        public DataGridRowClickedArgs(DataGridRow dataGridRow,
    DataGridColumn dataGridColumn, DataGridCell dataGridCell, object dataGridRowItem)
        {
            DataGridRow = dataGridRow;
            DataGridColumn = dataGridColumn;
            DataGridCell = dataGridCell;
            DataGridRowItem = dataGridRowItem;
        }
    }
}

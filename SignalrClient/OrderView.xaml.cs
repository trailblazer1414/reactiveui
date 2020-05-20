using MahApps.Metro.Controls;
using SignalrClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SignalrClient
{
    /// <summary>
    /// Interaction logic for OrderView.xaml
    /// </summary>
    public partial class OrderView : MetroWindow
    {
        public OrderView(OrderViewModel ordVm)
        {
            InitializeComponent();

            this.DataContext = ordVm;
        }
    }
}

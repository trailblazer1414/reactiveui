using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SignalrClient.Container;
using SignalrClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SignalrClient
{
    /// <summary>
    /// Interaction logic for StartUpWindow.xaml
    /// </summary>
    public partial class StartUpWindow : Window
    {
        HubConnection _connection;
        private IServiceProvider _serviceColl;
        public StartUpWindow(IServiceProvider Svc)
        {
            InitializeComponent();
            _serviceColl = Svc;
            _connection = new HubConnectionBuilder()
                .WithUrl("http://127.0.0.1:52776/relayhub", options =>
                {
                    options.UseDefaultCredentials = true;
                })
                .AddMessagePackProtocol()
                .Build();

            _connection.Closed += async (error) =>
            {
                await _connection.StartAsync();
            };
        }
        private async void connectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CacheContainer _cache = _serviceColl.GetService<CacheContainer>();
            _connection.On<Order>("SendOrder", (ord) =>
            {
                _cache.orderRecvBlock.Post(ord);
            });


            _connection.On<List<Order>>("SendInitOrders", (orders) =>
            {
                _cache.initOrdersBlock.Post(orders);
            });

            _connection.On<OrderUpd>("SendOrderUpd", (exec) =>
            {

                _cache.orderExecBufBlock.SendAsync(exec);

            });

            _connection.On<OrderOtherUpd>("SendOrderOtherUpd", (strat) =>
            {
                _cache.stratMsgBlock.Post(strat);
            });

            try
            {
                await _connection.StartAsync();
                messagesList.Items.Add("Connection started");
                connectButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            OrderView cvm = _serviceColl.GetService<OrderView>();
            cvm.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

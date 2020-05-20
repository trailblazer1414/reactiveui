using GalaSoft.MvvmLight.Messaging;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalrClient.Container;
using SignalrClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SignalrClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;
        private IConfiguration _config;
        private HubConnection _connection;
        //protected override async void OnStartup(StartupEventArgs e)
        //{

        //    InitializeComponent();
        //    base.OnStartup(e);
        //    AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

        //}

        public App()
        {
            _host = new HostBuilder()
                           .ConfigureServices((context, services) =>
                           {
                               services.AddSingleton<CacheContainer>();
                               services.AddTransient<OrderViewModel>();
                               services.AddTransient<OrderView>();
                               services.AddSingleton<StartUpWindow>();
                           })
                           .Build();
        }
        private async void Application_Startup(object sender, StartupEventArgs e)
        {

            await _host.StartAsync();

            var mainWindow = _host.Services.GetService<StartUpWindow>();
            mainWindow.Show();
            

            
        }
        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                //Handle exception
            }
            catch (Exception ex)
            {

            }
        }

    }
}

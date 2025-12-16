using System.Windows;
using Locomotiv.Utils;
using Locomotiv.ViewModel;
using Locomotiv.Model;
using Locomotiv.View;
using Microsoft.Extensions.DependencyInjection;
using Locomotiv.Model.Interfaces;
using Locomotiv.Model.DAL;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.Utils.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using Locomotiv.Utils.Services.Map;

namespace Locomotiv
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        public App()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            IConfiguration configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });

            services.AddTransient<MainViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<ConnectUserViewModel>();
            services.AddTransient<MapViewModel>();
            services.AddTransient<TrainManagementViewModel>();
            services.AddTransient<CreateTrainForStationViewModel>();
            services.AddTransient<ReserveTicketViewModel>();

            services.AddSingleton<IUserDAL, UserDAL>();
            services.AddSingleton<IStationDAL, StationDAL>();
            services.AddSingleton<IBlockDAL, BlockDAL>();
            services.AddSingleton<IBlockPointDAL, BlockPointDAL>();
            services.AddSingleton<ILocomotiveDAL, LocomotiveDAL>();
            services.AddSingleton<IWagonDAL, WagonDAL>();
            services.AddSingleton<ITrainDAL, TrainDAL>();
            services.AddSingleton<IPredefinedRouteDAL, PredefinedRouteDal>();

            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IUserSessionService, Service>();
            services.AddSingleton<IStationContextService, StationContextService>();

            services.AddSingleton<TrainMovementService>();
            services.AddSingleton<MapMarkerFactory>();
            services.AddSingleton<MapInfoService>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<Func<Type, BaseViewModel>>(serviceProvider =>
            {
                BaseViewModel ViewModelFactory(Type viewModelType)
                {
                    return (BaseViewModel)serviceProvider.GetRequiredService(viewModelType);
                }
                return ViewModelFactory;
            });

            services.AddDbContext<ApplicationDbContext>();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (dbContext.Database.EnsureCreated())
                {
                    dbContext.SeedData();
                }
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }
    }
}

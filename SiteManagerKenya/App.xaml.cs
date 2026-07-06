/*
 * APP.XAML.CS - COMPLETE DEPENDENCY INJECTION SETUP
 * Configures all services and initializes the application
 */

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;
using SiteManagerKenya.Application;
using SiteManagerKenya.Data;
using SiteManagerKenya.Infrastructure;
using SiteManagerKenya.Presentation;
using ILogger = SiteManagerKenya.Infrastructure.ILogger;

namespace SiteManagerKenya
{
    public partial class App : System.Windows.Application
    {
        private IServiceProvider? _serviceProvider;

        /// <summary>
        /// Application startup - configure all services
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Build service container
                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                // Initialize database
                InitializeDatabase();

                // Show main window
                MainWindow = new MainWindow();
                MainWindow.DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                MainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Application startup failed:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        /// <summary>
        /// Configure all dependency injection services
        /// </summary>
        private void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            var config = new AppConfiguration();
            services.AddSingleton<IAppConfiguration>(config);

            // Infrastructure
            services.AddSingleton<ILogger, FileLogger>();
            services.AddSingleton<IConnectionPool, SqliteConnectionPool>();
            services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();

            // Data Access
            services.AddTransient<ITimeRecordRepository, TimeRecordRepository>();
            services.AddTransient<IWorkerRepository, WorkerRepository>();
            // Add other repositories as needed...

            // Application Services
            services.AddTransient<IPayrollService, PayrollService>();
            services.AddTransient<ITimeRecordService, TimeRecordService>();
            services.AddTransient<IWorkerService, WorkerService>();
            // Add other services as needed...

            // ViewModels
            services.AddTransient<ClockViewModel>();
            services.AddTransient<PayrollViewModel>();
            services.AddTransient<WorkersViewModel>();
            services.AddTransient<MainWindowViewModel>();
        }

        /// <summary>
        /// Initialize database on startup
        /// </summary>
        private void InitializeDatabase()
        {
            var initializer = _serviceProvider!.GetRequiredService<IDatabaseInitializer>();
            initializer.InitializeAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Cleanup on shutdown
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            // Dispose connection pool
            var connectionPool = _serviceProvider?.GetService<IConnectionPool>();
            connectionPool?.Dispose();

            base.OnExit(e);
        }

        /// <summary>
        /// Get service from container
        /// </summary>
        public T GetService<T>() where T : notnull
        {
            return _serviceProvider!.GetRequiredService<T>();
        }
    }

    /// <summary>
    /// Main window view model - exposes the tab view models
    /// </summary>
    public class MainWindowViewModel
    {
        private readonly ILogger _logger;

        public ClockViewModel Clock { get; }
        public PayrollViewModel Payroll { get; }
        public WorkersViewModel Workers { get; }

        public MainWindowViewModel(
            ILogger logger,
            ClockViewModel clockViewModel,
            PayrollViewModel payrollViewModel,
            WorkersViewModel workersViewModel)
        {
            _logger = logger;
            Clock = clockViewModel;
            Payroll = payrollViewModel;
            Workers = workersViewModel;

            _logger.LogInfo("Application started");

            // Fire-and-forget initial data load for each tab
            _ = Clock.InitializeAsync();
            _ = Workers.InitializeAsync();
        }
    }

    /// <summary>
    /// Converts a bool (IsLoading) to Visibility for progress indicators.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}


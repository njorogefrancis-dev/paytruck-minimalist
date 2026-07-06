/*
 * LAYER 4: VIEW MODELS & APP STARTUP
 * MVVM implementation with proper async/await, INotifyPropertyChanged,
 * and complete DI setup
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SiteManagerKenya.Application;
using SiteManagerKenya.Data;
using SiteManagerKenya.Infrastructure;

namespace SiteManagerKenya.Presentation
{
    #region Base ViewModel

    /// <summary>
    /// Base view model with INotifyPropertyChanged implementation
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }
    }

    #endregion

    #region Relay Command

    /// <summary>
    /// Generic relay command for MVVM
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
    }

    /// <summary>
    /// Async relay command
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    #endregion

    #region Clock ViewModel

    /// <summary>
    /// ViewModel for Clock In/Out functionality
    /// </summary>
    public class ClockViewModel : ViewModelBase
    {
        private readonly ITimeRecordService _timeRecordService;
        private readonly IWorkerService _workerService;
        private readonly ILogger _logger;

        private ObservableCollection<Worker> _workers = new();
        private Worker? _selectedWorker;
        private TimeRecord? _todayStatus;
        private bool _isLoading;
        private string _statusMessage = "";
        private decimal _hoursWorkedThisMonth;

        public ObservableCollection<Worker> Workers
        {
            get => _workers;
            set => SetProperty(ref _workers, value, nameof(Workers));
        }

        public Worker? SelectedWorker
        {
            get => _selectedWorker;
            set => SetProperty(ref _selectedWorker, value, nameof(SelectedWorker));
        }

        public TimeRecord? TodayStatus
        {
            get => _todayStatus;
            set => SetProperty(ref _todayStatus, value, nameof(TodayStatus));
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value, nameof(IsLoading));
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value, nameof(StatusMessage));
        }

        public decimal HoursWorkedThisMonth
        {
            get => _hoursWorkedThisMonth;
            set => SetProperty(ref _hoursWorkedThisMonth, value, nameof(HoursWorkedThisMonth));
        }

        public ICommand ClockInCommand { get; }
        public ICommand ClockOutCommand { get; }
        public ICommand RefreshCommand { get; }

        public ClockViewModel(
            ITimeRecordService timeRecordService,
            IWorkerService workerService,
            ILogger logger)
        {
            _timeRecordService = timeRecordService;
            _workerService = workerService;
            _logger = logger;

            ClockInCommand = new AsyncRelayCommand(ClockInExecuteAsync, () => SelectedWorker != null && TodayStatus == null);
            ClockOutCommand = new AsyncRelayCommand(ClockOutExecuteAsync, () => SelectedWorker != null && TodayStatus != null);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
        }

        public async Task InitializeAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                var workers = await _workerService.GetAllAsync();
                Workers = new ObservableCollection<Worker>(workers);

                if (SelectedWorker != null)
                {
                    TodayStatus = await _timeRecordService.GetTodayStatusAsync(SelectedWorker.WorkerId);
                    HoursWorkedThisMonth = await _timeRecordService.GetHoursWorkedThisMonthAsync(
                        SelectedWorker.WorkerId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load data", ex);
                StatusMessage = $"❌ Error loading data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ClockInExecuteAsync()
        {
            if (SelectedWorker == null) return;

            try
            {
                IsLoading = true;
                var result = await _timeRecordService.ClockInAsync(
                    SelectedWorker.WorkerId,
                    SelectedWorker.FullName);

                StatusMessage = result.Message;

                if (result.Success)
                {
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Clock in failed", ex);
                StatusMessage = $"❌ Clock in failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ClockOutExecuteAsync()
        {
            if (SelectedWorker == null) return;

            try
            {
                IsLoading = true;
                var result = await _timeRecordService.ClockOutAsync(SelectedWorker.WorkerId);

                StatusMessage = result.Message;

                if (result.Success)
                {
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Clock out failed", ex);
                StatusMessage = $"❌ Clock out failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    #endregion

    #region Payroll ViewModel

    /// <summary>
    /// ViewModel for Payroll calculation
    /// </summary>
    public class PayrollViewModel : ViewModelBase
    {
        private readonly IPayrollService _payrollService;
        private readonly ILogger _logger;

        private ObservableCollection<PayrollEntry> _payrollEntries = new();
        private int _selectedYear = DateTime.Now.Year;
        private int _selectedMonth = DateTime.Now.Month;
        private bool _isLoading;
        private string _statusMessage = "";
        private string _periodLabel = "";

        public ObservableCollection<PayrollEntry> PayrollEntries
        {
            get => _payrollEntries;
            set => SetProperty(ref _payrollEntries, value, nameof(PayrollEntries));
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set => SetProperty(ref _selectedYear, value, nameof(SelectedYear));
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            set => SetProperty(ref _selectedMonth, value, nameof(SelectedMonth));
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value, nameof(IsLoading));
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value, nameof(StatusMessage));
        }

        public string PeriodLabel
        {
            get => _periodLabel;
            set => SetProperty(ref _periodLabel, value, nameof(PeriodLabel));
        }

        public ICommand CalculateCommand { get; }

        public PayrollViewModel(IPayrollService payrollService, ILogger logger)
        {
            _payrollService = payrollService;
            _logger = logger;

            CalculateCommand = new AsyncRelayCommand(CalculateExecuteAsync);
        }

        private async Task CalculateExecuteAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Calculating payroll...";

                var (from, to, label) = await _payrollService.GetMonthRangeAsync(_selectedYear, _selectedMonth);
                var entries = await _payrollService.CalculateAsync(from, to);

                PayrollEntries = new ObservableCollection<PayrollEntry>(entries);
                PeriodLabel = label;

                var totalGross = entries.Sum(e => e.GrossEarnings);
                var totalPaid = entries.Sum(e => e.TotalPaid);

                StatusMessage = $"✅ Calculated for {entries.Count} workers | " +
                    $"Total Gross: {totalGross:C} | Total Paid: {totalPaid:C}";

                _logger.LogInfo($"Payroll calculated: {entries.Count} workers, Period: {label}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Payroll calculation failed", ex);
                StatusMessage = $"❌ Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    #endregion

    #region Workers ViewModel

    /// <summary>
    /// ViewModel for Worker Management
    /// </summary>
    public class WorkersViewModel : ViewModelBase
    {
        private readonly IWorkerService _workerService;
        private readonly ILogger _logger;

        private ObservableCollection<Worker> _workers = new();
        private Worker? _selectedWorker;
        private string _searchTerm = "";
        private bool _isLoading;
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _totalPages = 1;
        private string _statusMessage = "";

        // Bound fields for the "Add Worker" form
        private string _newWorkerFullName = "";
        private string _newWorkerNationalId = "";
        private string _newWorkerPhoneNumber = "";
        private string _newWorkerCategory = "Casual";
        private string _newWorkerJobRole = "General Laborer";
        private string _newWorkerDailyWageRate = "";

        public ObservableCollection<Worker> Workers
        {
            get => _workers;
            set => SetProperty(ref _workers, value, nameof(Workers));
        }

        public Worker? SelectedWorker
        {
            get => _selectedWorker;
            set => SetProperty(ref _selectedWorker, value, nameof(SelectedWorker));
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value, nameof(SearchTerm));
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value, nameof(IsLoading));
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value, nameof(CurrentPage));
        }

        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value, nameof(TotalPages));
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value, nameof(StatusMessage));
        }

        public string NewWorkerFullName
        {
            get => _newWorkerFullName;
            set => SetProperty(ref _newWorkerFullName, value, nameof(NewWorkerFullName));
        }

        public string NewWorkerNationalId
        {
            get => _newWorkerNationalId;
            set => SetProperty(ref _newWorkerNationalId, value, nameof(NewWorkerNationalId));
        }

        public string NewWorkerPhoneNumber
        {
            get => _newWorkerPhoneNumber;
            set => SetProperty(ref _newWorkerPhoneNumber, value, nameof(NewWorkerPhoneNumber));
        }

        public string NewWorkerCategory
        {
            get => _newWorkerCategory;
            set => SetProperty(ref _newWorkerCategory, value, nameof(NewWorkerCategory));
        }

        public string NewWorkerJobRole
        {
            get => _newWorkerJobRole;
            set => SetProperty(ref _newWorkerJobRole, value, nameof(NewWorkerJobRole));
        }

        public string NewWorkerDailyWageRate
        {
            get => _newWorkerDailyWageRate;
            set => SetProperty(ref _newWorkerDailyWageRate, value, nameof(NewWorkerDailyWageRate));
        }

        public ICommand LoadWorkersCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand AddWorkerCommand { get; }

        public WorkersViewModel(IWorkerService workerService, ILogger logger)
        {
            _workerService = workerService;
            _logger = logger;

            LoadWorkersCommand = new AsyncRelayCommand(LoadWorkersAsync);
            SearchCommand = new AsyncRelayCommand(SearchAsync);
            NextPageCommand = new RelayCommand(_ => NextPage(), _ => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(_ => PreviousPage(), _ => CurrentPage > 1);
            AddWorkerCommand = new AsyncRelayCommand(AddWorkerAsync);
        }

        public async Task InitializeAsync()
        {
            await LoadWorkersAsync();
        }

        private async Task LoadWorkersAsync()
        {
            try
            {
                IsLoading = true;
                var workers = await _workerService.GetPagedAsync(CurrentPage, _pageSize);
                Workers = new ObservableCollection<Worker>(workers);

                var count = await _workerService.CountAsync();
                TotalPages = Math.Max(1, (count + _pageSize - 1) / _pageSize);

                _logger.LogInfo($"Loaded {workers.Count()} workers");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load workers", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchAsync()
        {
            try
            {
                IsLoading = true;
                var results = await _workerService.SearchAsync(SearchTerm);
                Workers = new ObservableCollection<Worker>(results);
            }
            catch (Exception ex)
            {
                _logger.LogError("Search failed", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                _ = LoadWorkersAsync();
            }
        }

        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                _ = LoadWorkersAsync();
            }
        }

        private async Task AddWorkerAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewWorkerFullName))
                {
                    StatusMessage = "Full name is required.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(NewWorkerNationalId))
                {
                    StatusMessage = "National ID is required.";
                    return;
                }
                if (!decimal.TryParse(NewWorkerDailyWageRate, out var dailyRate) || dailyRate <= 0)
                {
                    StatusMessage = "Daily wage rate must be a number greater than 0.";
                    return;
                }

                IsLoading = true;

                var worker = new Worker
                {
                    WorkerId = "W" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
                    FullName = NewWorkerFullName.Trim(),
                    NationalId = NewWorkerNationalId.Trim(),
                    PhoneNumber = NewWorkerPhoneNumber.Trim(),
                    DateJoined = DateTime.Now,
                    WorkerCategory = string.IsNullOrWhiteSpace(NewWorkerCategory) ? "Casual" : NewWorkerCategory,
                    JobRole = string.IsNullOrWhiteSpace(NewWorkerJobRole) ? "General Laborer" : NewWorkerJobRole,
                    DailyWageRate = dailyRate,
                    Status = "Active"
                };

                await _workerService.CreateAsync(worker, "system");

                StatusMessage = $"Worker '{worker.FullName}' added.";

                // Clear the form
                NewWorkerFullName = "";
                NewWorkerNationalId = "";
                NewWorkerPhoneNumber = "";
                NewWorkerCategory = "Casual";
                NewWorkerJobRole = "General Laborer";
                NewWorkerDailyWageRate = "";

                await LoadWorkersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to add worker", ex);
                StatusMessage = $"Failed to add worker: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    #endregion
}


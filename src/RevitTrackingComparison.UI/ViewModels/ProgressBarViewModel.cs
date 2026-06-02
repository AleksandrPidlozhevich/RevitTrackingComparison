using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RevitTrackingComparison.UI.ViewModels;

public partial class ProgressBarViewModel : ObservableObject, IDisposable
{
    private readonly string[] _statuses;
    private readonly System.Timers.Timer _timer = new(1000);
    private readonly DateTime _startTime = DateTime.Now;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Percents))]
    [NotifyPropertyChangedFor(nameof(OperationStatus))]
    private int _index;

    [ObservableProperty] private string _operationName = string.Empty;

    [ObservableProperty] private string _elapsed = "00:00:00";

    public int Length { get; }

    public double Percents => Length == 0 ? 0 : Math.Round((double)Index * 100 / Length, 2);

    public string OperationStatus =>
        Index >= _statuses.Length ? "Operation completed" : _statuses[Index];

    public Action? CloseAction { get; set; }

    public ProgressBarViewModel(string operationName, string[] statuses)
    {
        OperationName = operationName;
        _statuses = statuses ?? Array.Empty<string>();
        Length = _statuses.Length;

        _timer.Elapsed += OnTick;
        _timer.Start();
    }

    public void NextStep()
    {
        if (Index < Length)
            Index++;
    }

    private void OnTick(object? sender, ElapsedEventArgs e)
    {
        Elapsed = (DateTime.Now - _startTime).ToString(@"hh\:mm\:ss");
    }

    [RelayCommand]
    private void Close()
    {
        if (Index >= Length)
            CloseAction?.Invoke();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Elapsed -= OnTick;
        _timer.Dispose();
    }
}
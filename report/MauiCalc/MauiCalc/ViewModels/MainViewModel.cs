using System.ComponentModel;
using System.Runtime.CompilerServices;
using MauiCalc.Models;

namespace MauiCalc.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly ClickCounterModel _counter = new();
    private string _result = "Результат: —";
    private int _clickCount;

    public string Result
    {
        get => _result;
        set { _result = value; OnPropertyChanged(); } //При изменении значения (set) вызывается OnPropertyChanged() → UI обновляется.
    }

    public int ClickCount
    {
        get => _clickCount;
        set { _clickCount = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public System.Windows.Input.ICommand OnButtonClickCommand { get; }

    public MainViewModel()
    {
        OnButtonClickCommand = new Command(OnButtonClick);
    }

    private void OnButtonClick()
    {
        _counter.Increment();
        ClickCount = _counter.Count;
        Result = $"Кнопка нажата {ClickCount} раз(а).";
        System.Diagnostics.Debug.WriteLine($">>> Нажатий: {ClickCount}");
    }
}
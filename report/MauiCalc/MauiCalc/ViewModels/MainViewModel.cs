// <copyright file="MainViewModel.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using MauiCalc.Models;

namespace MauiCalc.ViewModels;

public partial class MainViewModel : ObservableObject // ObservableObject предоставляет базовую функциональность для наблюдения за изменениями свойств
{
    private readonly ClickCounterModel _counterModel = new();
    
    [ObservableProperty] // Атрибут [ObservableProperty] автоматически создает публичное свойство Result, которое будет ссылаться на это поле.
    private string _result = "Результат: —";
    
    [ObservableProperty] // Благодаря атрибуту [ObservableProperty], свойства Result и ClickCount автоматически генерируются, и при их изменении в UI произойдёт обновление
    private int _clickCount;

    [RelayCommand] // Атрибут [RelayCommand] автоматически создает команду
    private void OnButtonClick()
    {
        _counterModel.Increment();
        ClickCount = _counterModel.Count; // синхронизируем с обсервабельным свойством
        Result = $"Кнопка нажата {ClickCount} раз(а).";
        System.Diagnostics.Debug.WriteLine($">>> ClickCount = {ClickCount}, Result = {Result}");
    }
    
}
namespace MauiCalc;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        
        if (BindingContext == null)
        {
            BindingContext = new ViewModels.MainViewModel();
        }
    }
}
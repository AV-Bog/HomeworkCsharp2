namespace MauiCalc;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {
        ResultLabel.Text = "Результат: Работает!";
    }
}
namespace Hospital.Client.Views;

public partial class PageBackBar : ContentView
{
    public static readonly BindableProperty CaptionProperty = BindableProperty.Create(
        nameof(Caption),
        typeof(string),
        typeof(PageBackBar),
        "Geri",
        propertyChanged: (b, _, v) => ((PageBackBar)b).CaptionLabel.Text = (string)v);

    public event EventHandler? BackClicked;

    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public PageBackBar()
    {
        InitializeComponent();
    }

    private void OnBackTapped(object? sender, TappedEventArgs e) => BackClicked?.Invoke(this, EventArgs.Empty);
}

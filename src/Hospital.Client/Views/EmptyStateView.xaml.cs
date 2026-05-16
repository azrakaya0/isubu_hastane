namespace Hospital.Client.Views;

public partial class EmptyStateView : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(EmptyStateView),
        "Kayıt bulunamadı");

    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message),
        typeof(string),
        typeof(EmptyStateView),
        "Liste boş veya arama sonucu yok.");

    public static readonly BindableProperty HintProperty = BindableProperty.Create(
        nameof(Hint),
        typeof(string),
        typeof(EmptyStateView),
        string.Empty);

    public static readonly BindableProperty IconGlyphProperty = BindableProperty.Create(
        nameof(IconGlyph),
        typeof(string),
        typeof(EmptyStateView),
        "\u2205");

    public EmptyStateView()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string Hint
    {
        get => (string)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }
}

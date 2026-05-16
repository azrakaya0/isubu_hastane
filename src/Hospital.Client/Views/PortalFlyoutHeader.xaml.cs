namespace Hospital.Client.Views;

public partial class PortalFlyoutHeader : ContentView
{
    public PortalFlyoutHeader()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty BadgeTextProperty = BindableProperty.Create(
        nameof(BadgeText),
        typeof(string),
        typeof(PortalFlyoutHeader),
        string.Empty,
        propertyChanged: OnBadgeChanged);

    public static readonly BindableProperty HeadlineProperty = BindableProperty.Create(
        nameof(Headline),
        typeof(string),
        typeof(PortalFlyoutHeader),
        string.Empty,
        propertyChanged: OnHeadlineChanged);

    public static readonly BindableProperty TaglineProperty = BindableProperty.Create(
        nameof(Tagline),
        typeof(string),
        typeof(PortalFlyoutHeader),
        string.Empty,
        propertyChanged: OnTaglineChanged);

    public string BadgeText
    {
        get => (string)GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }

    public string Headline
    {
        get => (string)GetValue(HeadlineProperty);
        set => SetValue(HeadlineProperty, value);
    }

    public string Tagline
    {
        get => (string)GetValue(TaglineProperty);
        set => SetValue(TaglineProperty, value);
    }

    private static void OnBadgeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PortalFlyoutHeader h)
        {
            h.BadgeLabel.Text = newValue as string ?? string.Empty;
        }
    }

    private static void OnHeadlineChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PortalFlyoutHeader h)
        {
            h.HeadlineLabel.Text = newValue as string ?? string.Empty;
        }
    }

    private static void OnTaglineChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PortalFlyoutHeader h)
        {
            h.TaglineLabel.Text = newValue as string ?? string.Empty;
        }
    }
}

namespace Hospital.Client.Portal;

internal sealed record PortalMenuItem(string Title, string Route, Func<Page> CreatePage);

namespace Hospital.Client;

internal static class SearchFilterWiring
{
    internal static void WireEntry(Entry entry, DebouncedReload debouncer)
    {
        entry.TextChanged += (_, _) => debouncer.Schedule();
        entry.Completed += async (_, _) => await debouncer.RunNowAsync();
    }

    internal static void WireSearchBar(SearchBar bar, DebouncedReload debouncer)
    {
        bar.TextChanged += (_, _) => debouncer.Schedule();
        bar.SearchButtonPressed += async (_, _) => await debouncer.RunNowAsync();
    }
}

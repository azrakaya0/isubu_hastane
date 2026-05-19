namespace Hospital.Client;

/// <summary>
/// WinUI dahil platformlarda Picker seçiminin Enter beklemeden filtreyi uygulaması için
/// SelectedIndexChanged ve Unfocused birlikte izlenir.
/// </summary>
internal sealed class PickerFilterWatcher : IDisposable
{
    private readonly Picker _picker;
    private readonly Func<Task> _reload;
    private bool _suppress;
    private int _lastIndex;

    public PickerFilterWatcher(Picker picker, Func<Task> reload)
    {
        _picker = picker;
        _reload = reload;
        _lastIndex = picker.SelectedIndex;
        picker.SelectedIndexChanged += OnPickerSelectionCommitted;
        picker.Unfocused += OnPickerSelectionCommitted;
    }

    public IDisposable SuppressChanges()
    {
        _suppress = true;
        return new SuppressScope(this);
    }

    public void SyncIndex() => _lastIndex = _picker.SelectedIndex;

    private async void OnPickerSelectionCommitted(object? sender, EventArgs e)
    {
        if (_suppress)
        {
            return;
        }

        if (_picker.SelectedIndex == _lastIndex)
        {
            return;
        }

        _lastIndex = _picker.SelectedIndex;
        await _reload();
    }

    public void Dispose()
    {
        _picker.SelectedIndexChanged -= OnPickerSelectionCommitted;
        _picker.Unfocused -= OnPickerSelectionCommitted;
    }

    private sealed class SuppressScope : IDisposable
    {
        private readonly PickerFilterWatcher _owner;

        public SuppressScope(PickerFilterWatcher owner) => _owner = owner;

        public void Dispose()
        {
            _owner._suppress = false;
            _owner.SyncIndex();
        }
    }
}

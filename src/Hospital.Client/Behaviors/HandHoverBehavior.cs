using System.Reflection;
using Microsoft.Maui.Controls;

#if WINDOWS
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
#endif

namespace Hospital.Client.Behaviors;

/// <summary>
/// Windows’ta tıklanabilir kart üzerinde el imleci; diğer platformlarda no-op.
/// WinUI <see cref="UIElement.ProtectedCursor"/> korumalı olduğu için reflection kullanılır.
/// </summary>
public sealed class HandHoverBehavior : Behavior<Border>
{
#if WINDOWS
    private static readonly PropertyInfo? ProtectedCursorProperty =
        typeof(UIElement).GetProperty("ProtectedCursor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private UIElement? _platform;
    private PointerEventHandler? _entered;
    private PointerEventHandler? _exited;

    private static void SetProtectedCursor(UIElement el, InputCursor? cursor)
    {
        ProtectedCursorProperty?.SetValue(el, cursor);
    }
#endif

    protected override void OnAttachedTo(Border bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.HandlerChanged += OnHandlerChanged;
        TryAttach(bindable);
    }

    protected override void OnDetachingFrom(Border bindable)
    {
        bindable.HandlerChanged -= OnHandlerChanged;
        TryDetach();
        base.OnDetachingFrom(bindable);
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        if (sender is Border b)
        {
            TryDetach();
            TryAttach(b);
        }
    }

    private void TryAttach(Border border)
    {
#if WINDOWS
        if (ProtectedCursorProperty is null)
        {
            return;
        }

        if (border.Handler?.PlatformView is not UIElement el)
        {
            return;
        }

        _platform = el;
        _entered = (_, _) => SetProtectedCursor(el, InputSystemCursor.Create(InputSystemCursorShape.Hand));
        _exited = (_, _) => SetProtectedCursor(el, null);
        el.PointerEntered += _entered;
        el.PointerExited += _exited;
#endif
    }

    private void TryDetach()
    {
#if WINDOWS
        if (_platform is null || _entered is null || _exited is null)
        {
            return;
        }

        _platform.PointerEntered -= _entered;
        _platform.PointerExited -= _exited;
        SetProtectedCursor(_platform, null);
        _platform = null;
        _entered = null;
        _exited = null;
#endif
    }
}

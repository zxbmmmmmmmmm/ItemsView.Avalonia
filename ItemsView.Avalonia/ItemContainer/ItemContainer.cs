using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Metadata;
using PropertyGenerator.Avalonia;
using System.Xml.Linq;

namespace ItemsView.Avalonia;

[PseudoClasses(":pressed", ":selected")]
public partial class ItemContainer : TemplatedControl, ISelectable
{
    private static readonly Point s_invalidPoint = new Point(double.NaN, double.NaN);
    private Point _pointerDownPoint = s_invalidPoint;

    private Panel? _rootPanel;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var rootPanel = e.NameScope.Find<Panel>("PART_ContainerRoot");
        _rootPanel = rootPanel ?? throw new Exception();
        if(Child is { } child)
        {
            _rootPanel.Children.Insert(0, child);
        }
        base.OnApplyTemplate(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        _pointerDownPoint = s_invalidPoint;

        if (e.Handled)
            return;

        var p = e.GetCurrentPoint(this);

        if (p.Properties.PointerUpdateKind is PointerUpdateKind.LeftButtonPressed or
            PointerUpdateKind.RightButtonPressed)
        {
            if (p.Pointer.Type == PointerType.Mouse
                || (p.Pointer.Type == PointerType.Pen && p.Properties.IsRightButtonPressed))
            {
                // If the pressed point comes from a mouse or right-click pen, perform the selection immediately.
                // In case of pen, only right-click is accepted, as left click (a tip touch) is used for scrolling. 
                if (CanRaiseItemInvoked())
                {
                    e.Handled = RaiseItemInvoked(ItemContainerInteractionTrigger.PointerReleased, e.Source);
                }
            }
            else
            {
                // Otherwise perform the selection when the pointer is released as to not
                // interfere with gestures.
                _pointerDownPoint = p.Position;

                // Ideally we'd set handled here, but that would prevent the scroll gesture
                // recognizer from working.
                ////e.Handled = true;
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!e.Handled &&
            !double.IsNaN(_pointerDownPoint.X) &&
            e.InitialPressMouseButton is MouseButton.Left or MouseButton.Right)
        {
            var point = e.GetCurrentPoint(this);
            var settings = TopLevel.GetTopLevel(e.Source as Visual)?.PlatformSettings;
            var tapSize = settings?.GetTapSize(point.Pointer.Type) ?? new Size(4, 4);
            var tapRect = new Rect(_pointerDownPoint, new Size())
                .Inflate(new Thickness(tapSize.Width, tapSize.Height));

            if (new Rect(Bounds.Size).ContainsExclusive(point.Position) &&
                tapRect.ContainsExclusive(point.Position))
            {
                if (CanRaiseItemInvoked())
                {
                    e.Handled = RaiseItemInvoked(ItemContainerInteractionTrigger.PointerReleased, e.Source);
                }
            }
        }

        _pointerDownPoint = s_invalidPoint;
    }

    protected override void OnKeyDown(KeyEventArgs args)
    {
        base.OnKeyDown(args);

        if (!args.Handled && CanRaiseItemInvoked())
        {
            if (args.Key == Key.Enter)
            {
                args.Handled = RaiseItemInvoked(ItemContainerInteractionTrigger.EnterKey, args.Source);
            }
            else if (args.Key == Key.Space)
            {
                args.Handled = RaiseItemInvoked(ItemContainerInteractionTrigger.SpaceKey, args.Source);
            }
        }
    }

    internal bool RaiseItemInvoked(ItemContainerInteractionTrigger interactionTrigger, object? originalSource)
    {

        if (ItemInvoked is not null)
        {
            var itemInvokedEventArgs = new ItemContainerInvokedEventArgs(interactionTrigger, originalSource);
            ItemInvoked.Invoke(this, itemInvokedEventArgs);

            return itemInvokedEventArgs.Handled;
        }

        return false;
    }

    private bool CanRaiseItemInvoked()
    {
        return (CanUserInvoke & ItemContainerUserInvokeMode.UserCanInvoke) != 0 ||
               (CanUserSelect & (ItemContainerUserSelectMode.Auto | ItemContainerUserSelectMode.UserCanSelect)) != 0;
    }

    internal event EventHandler<ItemContainer, ItemContainerInvokedEventArgs>? ItemInvoked;
}
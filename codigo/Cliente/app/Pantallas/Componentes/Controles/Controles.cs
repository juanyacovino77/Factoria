using System;
using MauiReactor;
using MauiReactor.Internals;
using Microsoft.Maui.Controls;

namespace app.Pantallas.Componentes.Controles;

#region ANIMACIONES
[Scaffold(typeof(SkiaSharp.Extended.UI.Controls.SKSurfaceView))]
partial class SKSurfaceView { }

[Scaffold(typeof(SkiaSharp.Extended.UI.Controls.SKAnimatedSurfaceView))]
partial class SKAnimatedSurfaceView { }

[Scaffold(typeof(SkiaSharp.Extended.UI.Controls.SKLottieView))]
partial class Animacion { }
#endregion

#region POPUP
[Scaffold(typeof(Syncfusion.Maui.Popup.SfPopup))]
public partial class Popup
{
    public Popup Content(Func<VisualNode> render)
    {
        this.Set(Syncfusion.Maui.Popup.SfPopup.ContentTemplateProperty,
            new MauiControls.DataTemplate(() => TemplateHost.Create(render()).NativeElement));

        return this;
    }
}

[Scaffold(typeof(Syncfusion.Maui.Core.SfView))]
public abstract partial class SfView
{
}


[Scaffold(typeof(Syncfusion.Maui.Core.SfContentView))]
public abstract partial class SfContentView
{
}

public abstract partial class SfContentView<T>
{
    protected override void OnAddChild(VisualNode widget, BindableObject childControl)
    {
        NativeControl.EnsureNotNull();
        if (childControl is View content)
        {
            NativeControl.Content = content;
        }

        base.OnAddChild(widget, childControl);
    }

    protected override void OnRemoveChild(VisualNode widget, BindableObject childControl)
    {
        NativeControl.EnsureNotNull();
        if (childControl is View content &&
            NativeControl.Content == content)
        {
            NativeControl.Content = null;
        }


        base.OnRemoveChild(widget, childControl);
    }
}
#endregion

#region EXPANDER
[Scaffold(typeof(epj.Expander.Maui.Expander))]
public partial class Expander 
{
    public Expander Header(Func<VisualNode> render)
    {
        this.Set(epj.Expander.Maui.Expander.HeaderContentProperty,
            new MauiControls.DataTemplate(() => TemplateHost.Create(render()).NativeElement));

        return this;
    }

}

#endregion

#region BARRA DE PROGRESO
[Scaffold(typeof(Syncfusion.Maui.ProgressBar.SfCircularProgressBar))]
public partial class BarraProgreso
{
}
[Scaffold(typeof(Syncfusion.Maui.ProgressBar.ProgressBarBase))]
public abstract class ProgressBarBase { }

[Scaffold(typeof(Syncfusion.Maui.ProgressBar.SfLinearProgressBar))]
public partial class LineaProgreso
{
}
#endregion

#region SONIDO
[Scaffold(typeof(CommunityToolkit.Maui.Views.MediaElement))]    
public partial class SonidoMensaje { }
#endregion
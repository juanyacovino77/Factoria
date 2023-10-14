using System;
using MauiReactor;

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
public abstract class SfView { }
#endregion

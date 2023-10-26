using app.Componentes;
using app.Pantallas.Componentes;
using CommunityToolkit.Maui;
using epj.Expander.Maui;
using MauiReactor;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Syncfusion.Maui.Core.Hosting;

namespace app
{
    public static class MauiProgram
    {
        
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjc4MDE3NEAzMjMzMmUzMDJlMzBNdGtDSllZZ1FxT2lOSjFKYXhXUmtPVFpneFR2bCtzUS9FbFBHQlBPdFVVPQ==");

            builder
                .UseMauiReactorApp<PantallaInicio>(app =>
                {
                    app.AddResource("Resources/Styles/Colors.xaml");
                    app.AddResource("Resources/Styles/Styles.xaml");

                    app.SetWindowsSpecificAssetsDirectory("Assets");
                })
                .UseSkiaSharp()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureSyncfusionCore()
                .UseExpander()
                
                #if DEBUG
                .EnableMauiReactorHotReload()
                #endif

                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
                });

            builder.Services.AddSingleton<Servicios.IServicios, Servicios.Servidor>(); // usa conexión al servidor real
            //builder.Services.AddSingleton<Servicios.IServicios, Servicios.Mock>(); // usa datos de ensayo


            return builder.Build();

        }
    }
}
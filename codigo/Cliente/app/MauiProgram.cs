using app.Componentes;
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

            builder
                .UseMauiReactorApp<PantallaInicio>(app =>
                {
                    app.AddResource("Resources/Styles/Colors.xaml");
                    app.AddResource("Resources/Styles/Styles.xaml");

                    app.SetWindowsSpecificAssetsDirectory("Assets");
                })
                .UseSkiaSharp()
                
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

            builder.ConfigureSyncfusionCore();

            return builder.Build();

        }
    }
}
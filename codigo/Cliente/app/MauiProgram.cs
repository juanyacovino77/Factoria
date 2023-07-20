using app.Componentes;
using MauiReactor;
using Microsoft.Extensions.DependencyInjection;

namespace app
{
    public static class MauiProgram
    {

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiReactorApp<Inicio>(app =>
                {
                    app.AddResource("Resources/Styles/Colors.xaml");
                    app.AddResource("Resources/Styles/Styles.xaml");

                    app.SetWindowsSpecificAssetsDirectory("Assets");
                })
                
                #if DEBUG
                .EnableMauiReactorHotReload()
                #endif

                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
                });

            builder.Services.AddSingleton<Servicios.Servicios>();

            return builder.Build();

        }
    }
}
using MauiReactor;
using app.Pantallas.Componentes.Controles;
using Microsoft.Extensions.DependencyInjection;
using app.Servicios;
using System;

namespace app.Componentes;

internal class estado_del_inicio
{
    public string Clave { get; set; }
    public bool Respuesta { get; set; }
}
internal class PantallaInicio : Component<estado_del_inicio>
{
    public override VisualNode Render()
    {
        return new NavigationPage
        {
                new ContentPage()
                {
                    new StackLayout()
                    {
                        new Animacion()
                            .Source(new SkiaSharp.Extended.UI.Controls.SKFileLottieImageSource()
                            {
                                File = "dotnetbot.json"
                            })
                            .IsAnimationEnabled(true)
                            .RepeatCount(-1)
                            .HeightRequest(200)
                            .WidthRequest(200)
                            .HCenter()

                            ,

                        new Entry()
                            .Placeholder("Clave")
                            .OnTextChanged((s,e)=> SetState(s => s.Clave = e.NewTextValue, invalidateComponent:false))
                            .IsPassword(true)
                            
                            ,

                        new Button("Iniciar sesión")
                            .OnClicked(OnLogin)
                            
                            ,

                        ! State.Respuesta
                        ? new Label("Denegado")
                        : new Label("Aprobado"),

                    }
                    .VCenter()
                    .HCenter()
                }
        };
    }
    private async void OnLogin()
    {

        var servicio = Services.GetService<IServicios>();
        try
        {
            await servicio.PrenderConexion();
        }
        catch(Exception ex) 
        {
            await ContainerPage.DisplayAlert("Error", "El servidor está apagado", "Ok");
            return;
        }

        var respuesta = await servicio.IniciarSesion(new Contratos.SolicitudIniciarSesion { idEmpleado = State.Clave });


        SetState(s => s.Respuesta = respuesta.exito);

        if (!respuesta.exito) return;

        await Navigation.PushAsync<PantallaTablero, parametros_tablero>(p =>
        {
            p.operario = respuesta.operario;
            p.conectados = respuesta.conectados;
        });
    }
}


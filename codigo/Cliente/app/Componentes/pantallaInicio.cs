using MauiReactor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.Componentes;

public class estado_del_inicio
{
    public string Clave { get; set; }
    public bool Respuesta { get; set; }
}
public class pantallaInicio : Component<estado_del_inicio>
{
    public override VisualNode Render()
    {
        return new NavigationPage
            {
                new ContentPage()
                {
                    new StackLayout()
                    {
                        new Entry()
                            .Placeholder("Clave")
                            .OnTextChanged((s,e)=> SetState(_ => _.Clave = e.NewTextValue)),

                        new Button("Iniciar sesión")
                            .IsEnabled(!string.IsNullOrWhiteSpace(State.Clave) && !string.IsNullOrWhiteSpace(State.Clave))
                            .OnClicked(OnLogin),

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
        //use State.Username and State.Password to login...
        var servicio = Services.GetRequiredService<Servicios.Servidor>();

        var respuesta = await servicio.IniciarSesion(State.Clave);

        if (respuesta.exito)
        {
            SetState(s => s.Respuesta = respuesta.exito);
            await Navigation.PushAsync<pantallaTablero>();
        }

    }
}

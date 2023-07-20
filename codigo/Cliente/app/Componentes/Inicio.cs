using Contratos;
using MauiReactor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.Componentes
{
    public class inicioEstado
    {
        public string Clave { get; set; }
        public bool Respuesta { get; set; }
    }
    public class Inicio : Component<inicioEstado>
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
            var servicio = Services.GetRequiredService<Servicios.Servicios>();

            var respuesta = await servicio.IniciarSesion(State.Clave);

            if (respuesta.exito)
            {
                SetState(s => s.Respuesta = respuesta.exito);
            }
            else
            {
                await Navigation.PushAsync<Tablero>();
            }

        }
    }
    

    
    public class estadoTablero
    {
        public Empleado empleadoLogueado  { get; set; }
        public Empleado[] empleadosActivos { get; set; }
    }
    public class parametrosTablero
    {
        //public Jornada jornadaActual  { get; set; }
    }
    public class Tablero : Component<estadoTablero, parametrosTablero>
    {
        protected override void OnMounted()
        {
            //State.empleadoLogueado = Props.jornadaActual.empleadoActivo;
            //State.empleadosActivos = Props.jornadaActual.empleadosEnTurno;

            base.OnMounted();
        }
        public override VisualNode Render()
        {
            return new ContentPage()
            {
                new Button("Back")
                    .VCenter()
                    .HCenter()
                    .OnClicked(GoBack)
            }
            .Title("Child Page");
        }
        private async void GoBack()
        {
            await Navigation.PopAsync();
        }
    }



}

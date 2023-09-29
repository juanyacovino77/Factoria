using Contratos;
using MauiReactor;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace app.Componentes
{
    public class estado_del_despacho
    {
        public Mensaje nuevoMensaje { get; set; } = new Mensaje(); // el nuevo mensaje creado
        public int tipoMensaje { get; set; }
    }
    public class parametros_despacho
    {
        public Empleado empleadoOperativo { get; set; } // quien está enviando el nuevo mensaje
        public Empleado empleadoConectadoSeleccionado { get; set; } // a quien está dirijido el nuevo mensaje
        public Action<Mensaje> mensajeEnviado { get; set; }
    }
    public class pantallaDespacho : Component<estado_del_despacho, parametros_despacho>
    {
        protected override void OnMounted()
        {
            State.nuevoMensaje.emisor = Props.empleadoOperativo;
            State.nuevoMensaje.receptor = Props.empleadoConectadoSeleccionado;
        }
        public override VisualNode Render()
        {
            return
                new ContentPage()
                {
                    new Grid("*", "500, *"){

                        new VStack()
                        {
                            new Label($"msj para:{State.nuevoMensaje.receptor.nombreEmpleado} de parte de:{State.nuevoMensaje.emisor.nombreEmpleado}")
                            .FontSize(10)
                            .TextColor(Colors.Black),

                            new Label("Elija un tipo de mensaje a enviar:")
                            .FontSize(20)
                            .Padding(20)
                            .TextColor(Colors.Black)
                            ,

                            new Label("Notificacion").TextColor(Colors.Black),
                            new RadioButton().OnCheckedChanged((s,a) => SetState(s => s.tipoMensaje=1)),
                            new Label("Tareas").TextColor(Colors.Black),
                            new RadioButton().OnCheckedChanged((s,a) => SetState(s => s.tipoMensaje=2)),
                            new Label("Receta").TextColor(Colors.Black),
                            new RadioButton().OnCheckedChanged((s,a) => SetState(s => s.tipoMensaje=3)),


                            new ImageButton("icono_mensaje3.png")
                                .HeightRequest(150)
                                .WidthRequest(150)
                                .OnClicked(() => EnviarMensaje(State.nuevoMensaje))


                        }
                        .HCenter()
                        .BackgroundColor(Colors.AliceBlue) .Padding(20)
                        ,

                        new VStack()
                        {
                            State.tipoMensaje is 1 ?
                                new Entry().Placeholder("ingrese texto a enviar").OnTextChanged((t) => SetState(s=> s.nuevoMensaje.notaMensaje=t))
                                :
                                State.tipoMensaje is 2 ?
                                new Entry().Placeholder("ingrese una tarea")
                                :
                                new Entry().Placeholder("ingrese una receta")
                        }
                        .GridColumn(1)
                        .BackgroundColor(Colors.Blue)
                    }
                };
        
        }

        private async void EnviarMensaje(Mensaje msj)
        {
            // obtener el servicio
            var servicio = Services.GetRequiredService<Servicios.IServicios>();
            await servicio.EnviarMensaje(new SolicitudEnviarMensaje() { mensaje=msj });
        }


    }
}

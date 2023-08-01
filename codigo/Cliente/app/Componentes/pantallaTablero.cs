using Contratos;
using MauiReactor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.Componentes;

public class estado_del_tablero
{
    public Empleado empleadoOperativo { get; set; } = new Empleado()
    {
        idEmpleado = 1,
        idSector = 2,
        mensajes = new ObservableCollection<Mensaje>()
        {
            new Mensaje()
            {
                notaMensaje = "Este mensaje viene deL HIPER",
                emisor = new Empleado(){ nombreEmpleado = "MAXIMO"},
                notificacion=
                    new Notificacion()
                    {
                        estadoActual = Notificacion.Estado.Recibido,
                        texto = "Nueva lista de tareas requeridas:",
                        urlImagen = "/imagen/limpia/pisos.png"
                    },
                idMensaje = 1,
            },

            new Mensaje()
            {
                notaMensaje = "Este mensaje viene de FANTI",
                emisor = new Empleado(){ nombreEmpleado = "erni"},
                receta = new Receta(){ paso1="Mezclar harina, agua, y azuar", paso2="Prepara la levadura y el horno" },
                idMensaje = 2,
            },

            new Mensaje()
            {
                notaMensaje = "Este mensaje viene de SUPER",
                emisor = new Empleado(){ nombreEmpleado = "lula"},
                idMensaje = 3,
            },
        },
        nombreEmpleado = "juamanuel",
        nombreSector = "panaderia.ynsd"
    };
    public Mensaje mensajeSeleccionado { get; set; }

    public void InsertarActualizarMensaje(Mensaje msjNuevo)
    {
        var mensajes = this.empleadoOperativo.mensajes;
        var msjActual = mensajes.SingleOrDefault(m => m.idMensaje == msjNuevo.idMensaje);

        if (msjActual is null) mensajes.Insert(0, msjNuevo); // Es un mensaje nuevo
        else // Es la actualizacion de un mensaje existente
        {
            var i = mensajes.IndexOf(msjActual);
            mensajes[i] = msjNuevo;
        }
    }
}
public class pantallaTablero : Component<estado_del_tablero>
{
    protected override void OnMounted()
    {
        // suscribirse a Nuevos Mensajes
        // suscribirse a Señales Empleado en linea
        var servicio = Services.GetRequiredService<Servicios.Servidor>();

        servicio.MensajeRecibido += RecibirMensaje; // escucha nuevos msj
        servicio.EmpleadoConectado += RecibirNuevoConectado;

        base.OnMounted();
    }

    public override VisualNode Render()
    {
        return new ContentPage()
        {
            new Grid("80 , *", "* , *")
            {
                GraficarEncabezado(),
                GraficarListaDeMensajes(),
                GraficarCuerpoDelMensaje()
            }
        };

    }

    private VisualNode GraficarEncabezado()
    {
        return new Grid("*,*", "*,*")
        {
           new Label($"Tablero de {State.empleadoOperativo.nombreEmpleado} del sector {State.empleadoOperativo.nombreSector} ")
           .FontSize(20)
           .Padding(25,0,0,0)

           ,

           new Label($"id del empleado {State.empleadoOperativo.idEmpleado} y del sector {State.empleadoOperativo.idSector}")
           .Padding(25, 0, 0, 0)
           .GridRow(1)
        }
            .BackgroundColor(Colors.Black)
            .GridColumn(0)
            .GridRow(0)
            .GridColumnSpan(2);

    }

    private VisualNode GraficarListaDeMensajes()
    {
        return new VStack()
        {
            new Label("Mensajes").FontSize(25),

            new Button("Eliminar mensajes")

                .Padding(10,10,10,10)
                .WidthRequest(150)
                .OnClicked( () => SetState(s => s.empleadoOperativo.mensajes.Clear()) )

            ,

            new Grid("*", "*")
            {
                new CollectionView()
                    .ItemsSource(State.empleadoOperativo.mensajes, GraficarMensajes)
                    .SelectionMode(MauiControls.SelectionMode.Single)
                    .OnSelectionChanged((s,e) => SeleccionarMensaje(e))
            }
            .HeightRequest(500)

            ,


            new Button("Agregar un mensaje")
                .OnClicked(() => SetState(s => s.InsertarActualizarMensaje(
                    new Mensaje()
                    {
                        notaMensaje = "Este mensaje viene de ALGUN LADO DEL MUNDO",
                        emisor = new Empleado(){ nombreEmpleado = "lula"},
                        notificacion= new Notificacion(),
                        idMensaje = 1,
                    }))),

        }
            .BackgroundColor(Colors.LightSkyBlue)
            .GridRow(1);

        void SeleccionarMensaje(MauiControls.SelectionChangedEventArgs msjSeleccionado)
        {
            SetState(s => s.mensajeSeleccionado = msjSeleccionado.CurrentSelection.FirstOrDefault() as Mensaje);
        }

        VisualNode GraficarMensajes(Mensaje msj)
        {
            return new cajaMensaje()
            {
            }
                .Descripcion(msj.notaMensaje)
                .NombreEmisor(msj.emisor.nombreEmpleado);
        }

    }

    private VisualNode GraficarCuerpoDelMensaje()
    {
        return State.mensajeSeleccionado switch
        {
            { notificacion: not null } => GraficarNotificacion(State.mensajeSeleccionado.notificacion),
            { tareas: not null } => GraficarTareas(State.mensajeSeleccionado.tareas),
            { receta: not null } => GraficarReceta(State.mensajeSeleccionado.receta),
            _ => GraficarCuerpoVacio(),
        };

        VisualNode GraficarNotificacion(Notificacion notificacion)
        {
            return new VStack()
            {
                new Label($"detalle de msj - id {State.mensajeSeleccionado.idMensaje}")
                    .FontFamily("bold")
                    .FontSize(25)
                    ,

                new Label($"el mensaje tiene estado: {notificacion.estadoActual}")
                    .FontSize(25)
                    ,

                new Entry()
                    .Placeholder("editar msj")
                    .OnTextChanged( (t) => { SetState( s => s.mensajeSeleccionado.notaMensaje = t); })
                    .HeightRequest(55)
                    ,

                new Button("MARCAR COMO VISTO")
                    .OnClicked(ActualizarEstadoCuerpoMensaje)

                    ,

                new Button("Responder mensaje")
                    .OnClicked(ResponderMensaje)
            }
                .BackgroundColor(Colors.CornflowerBlue)
                .GridRow(1)
                .GridColumn(1);
        }

        VisualNode GraficarReceta(Receta receta)
        {
            return new VStack()
            {
                new Label($"paso 1: {receta.paso1} despues hacer paso 2: {receta.paso2} ")
                    .FontAttributes(Microsoft.Maui.Controls.FontAttributes.Bold)
                    .TextColor(Colors.Black)
            }
            .BackgroundColor(Colors.LightPink)
            .GridRow(1)
            .GridColumn(1);
        }

        VisualNode GraficarTareas(Tareas tarea)
        {
            return new VStack();
        }

        VisualNode GraficarCuerpoVacio()
        {
            return new VStack()
                    .BackgroundColor(Colors.CornflowerBlue)
                    .GridRow(1)
                    .GridColumn(1);
        }

        async void ResponderMensaje()
        {
            // obtener el mensaje 
            var msj = State.mensajeSeleccionado;

            // obtener el servicio
            var servicio = Services.GetRequiredService<Servicios.Servidor>();

            msj.notificacion = null;

            msj.receta = new Receta()
            {
                paso1 = "Preparar milanesas, cortar bifes de lomo, y atiernizarlos",
                paso2 = "Hacer mezcla de huevo y prepara pan rallado"
            };

            // actualizar la descripcion del mensaje para hacer eso
            // hay que reenviarselo al emisor
            var respuesta = await servicio.EnviarMensaje(new SolicitudEnviarMensaje()
            {
                mensaje = msj
            }
            );

        }

        async void ActualizarEstadoCuerpoMensaje()
        {
            // mensaje seleccionado
            var msj = State.mensajeSeleccionado;

            // servicio del servidor
            var servicio = Services.GetRequiredService<Servicios.Servidor>();

            // actualizar el estado
            msj.notificacion.estadoActual = Notificacion.Estado.Visto;

            // envio el msj con el estado nuevo
            await servicio.EnviarMensaje(new SolicitudEnviarMensaje { mensaje = msj});
        }
    }

    private void RecibirMensaje(object? sender, Mensaje msjRecibido)
    {
        SetState(s => s.InsertarActualizarMensaje(msjRecibido));
    }

    private void RecibirNuevoConectado(object? sender, string msj)
    {
    }
}
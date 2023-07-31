using Contratos;
using MauiReactor;
using MauiReactor.Shapes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;

namespace app.Componentes;

public class estado_del_tablero
{
    public Contratos.Empleado empleadoOperativo { get; set; } = new Contratos.Empleado()
    {
        idEmpleado = 1,
        idSector = 2,
        mensajes = new ObservableCollection<Contratos.Mensaje>() 
        {
            new Contratos.Mensaje()
            {
                descripcionMensaje = "Este mensaje viene deL HIPER",
                emisor = new Empleado(){ nombreEmpleado = "MAXIMO"},
                cuerpo=
                    new Notificacion()
                    {
                        estadoActual = Notificacion.Estado.Recibido,
                        texto = "Nueva lista de tareas requeridas:",
                        urlImagen = "/imagen/limpia/pisos.png"
                    },
                idAsunto = 1,
                idEstado = 1,
                idMensaje = 1,
            },

            new Contratos.Mensaje()
            {
                descripcionMensaje = "Este mensaje viene de FANTI",
                emisor = new Empleado(){ nombreEmpleado = "erni"},
                cuerpo = new Receta(){ paso1="Mezclar harina, agua, y azuar", paso2="Prepara la levadura y el horno" },
                idAsunto = 2,
                idEstado = 2,
                idMensaje = 2,
            },

            new Contratos.Mensaje()
            {
                descripcionMensaje = "Este mensaje viene de SUPER",
                emisor = new Empleado(){ nombreEmpleado = "lula"},
                idAsunto = 1,
                idEstado = 1,
                idMensaje = 1,
            },
        },
        nombreEmpleado = "juamanuel",
        nombreSector = "panaderia.ynsd"
    };
    public Contratos.Mensaje mensajeSeleccionado { get; set; }
}
public class Tablero : Component<estado_del_tablero>
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
                    .SelectionMode(Microsoft.Maui.Controls.SelectionMode.Single)
                    .OnSelectionChanged((s,e) => SeleccionarMensaje(e))
            }
            .HeightRequest(500)
            
            ,
            

            new Button("Agregar un mensaje")
                .OnClicked(() => SetState(s => s.empleadoOperativo.mensajes.Add(
                    new Contratos.Mensaje()
                    {
                        descripcionMensaje = "Este mensaje viene de ALGUN LADO DEL MUNDO",
                        emisor = new Empleado(){ nombreEmpleado = "lula"},
                        idAsunto = 1,
                        idEstado = 1,
                        idMensaje = 1,
                    }))),

        }
        .BackgroundColor(Colors.LightSkyBlue)
        .GridRow(1);

        void SeleccionarMensaje(Microsoft.Maui.Controls.SelectionChangedEventArgs msjSeleccionado)
        {
            SetState(s => s.mensajeSeleccionado = msjSeleccionado.CurrentSelection.FirstOrDefault() as Contratos.Mensaje);
        }

        VisualNode GraficarMensajes(Contratos.Mensaje msj)
        {
            return new Mensaje()
            {
            }
            .Descripcion(msj.descripcionMensaje)
            .Titulo("importante")
            .NombreEmisor(msj.emisor.nombreEmpleado);
        }

    }

    private VisualNode GraficarCuerpoDelMensaje()
    {
        var cuerpo = State.mensajeSeleccionado?.cuerpo;

        return cuerpo switch
        {
            Notificacion => GraficarNotificacion(cuerpo as Notificacion),

            Tareas => GraficarTareas(cuerpo as Tareas),

            Receta => GraficarReceta(cuerpo as Receta),

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

                new Label($"el mensaje dice: {notificacion.texto + notificacion.estadoActual + notificacion.urlImagen}")
                    .FontSize(25)
                    ,

                new Entry()
                    .Placeholder("editar msj")
                    .OnTextChanged( (t) => { SetState( s => s.mensajeSeleccionado.descripcionMensaje = t); })
                    .HeightRequest(55)
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

            msj.cuerpo = new Receta()
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
    }

    private void RecibirMensaje(object? sender, Contratos.Mensaje msjRecibido)
    {
        SetState(s => s.empleadoOperativo.mensajes.Add(msjRecibido));
    }

    private void RecibirNuevoConectado(object? sender, string msj)
    {
    }
}

public class Mensaje : Component
{
    private string _titulo;
    private string _nombreEmisor;
    private string _descripcion;
    public Mensaje Titulo(string titulo) { _titulo = titulo; return this; }
    public Mensaje NombreEmisor(string nombre) { _nombreEmisor = nombre; return this; }
    public Mensaje Descripcion(string descrp) { _descripcion = descrp; return this; }


    public override VisualNode Render()
    {
        return

            new Border()
            {
                new Grid("*,*,*","*")
                {
                    new Label($"NUEVO MENSAJE RECIBIDO")
                        .TextColor(Colors.Black)
                        .Padding(5)
                        ,

                    new Label($"De: {_nombreEmisor}")
                        .GridRow(1)
                        .Padding(5)
                        ,

                    new Label($"Descripcion: {_descripcion}")
                        .GridRow(2)
                        .Padding(5)
                }
            }
            .Padding(10)
            .Stroke(Microsoft.Maui.Controls.Brush.Chocolate)
            .StrokeThickness(2)
            .BackgroundColor(Colors.Orange)
            .StrokeShape(new RoundRectangle().CornerRadius(15, 15, 15, 15));
        
    }
}





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
        var servicio = Services.GetRequiredService<Servicios.Servidor>();

        var respuesta = await servicio.IniciarSesion(State.Clave);

        if (respuesta.exito)
        {
            SetState(s => s.Respuesta = respuesta.exito);
            await Navigation.PushAsync<Tablero>();
        }

    }
}

/* una posible implementación, diviendo por componentes el tablero
 * es codigo mas limpio, pero implica mas logica de estado...
 * en este caso no es necesario crear componentes porque en principio
 * por ahora, el ListaDeMensajes ni el DetalleDelMensaje se reutilizarian
 * en otras pantallas.
 * 
class parametrosTablero
{
    public Contratos.Mensaje mensajeSeleccionado { get; set; }
}
public class Tablero : Component
{
    private readonly IParameter<parametrosTablero> _mensajeSeleccionado;

    public Tablero()
    {
        _mensajeSeleccionado = CreateParameter<parametrosTablero>();
    }

    public override VisualNode Render()
    {
        return new ContentPage()
        {
            new Grid("80 , *", "* , *")
            {
                new Encabezado()
                    .GridColumnSpan(2),
               
                new ListaDeMensajes()
                    .GridRow(1),

                new DetalleDelMensaje()
                    .GridRow(1)
                    .GridColumn(1)
            }
        };
    }
}



public class Encabezado : Component
{
    public override VisualNode Render()
    {
        return new HStack()
        {
           new Label($"Tablero del empleado: olaa")
             .FontSize(50)
             

        }
        .BackgroundColor(Color.FromRgb(1, 255, 1));


    }
}

public class estadoListaDeMensajes
{
    public ObservableCollection<Contratos.Mensaje> mensajes { get; set; } = new ObservableCollection<Contratos.Mensaje>()
    {
        new Contratos.Mensaje()
        {
            descripcionMensaje = "Este mensaje viene de ysnd",
            emisor = new Empleado(){ nombreEmpleado = "juan"},
            idAsunto = 1,
            idEstado = 1,
            idMensaje = 1,
        },

        new Contratos.Mensaje()
        {
            descripcionMensaje = "Este mensaje viene de FANTI",
            emisor = new Empleado(){ nombreEmpleado = "erni"},
            idAsunto = 2,
            idEstado = 2,
            idMensaje = 2,
        },

        new Contratos.Mensaje()
        {
            descripcionMensaje = "Este mensaje viene de SUPER",
            emisor = new Empleado(){ nombreEmpleado = "lula"},
            idAsunto = 1,
            idEstado = 1,
            idMensaje = 1,
        },

    };
}
public class ListaDeMensajes : Component<estadoListaDeMensajes>
{
    protected override void OnMounted()
    {
        // esuchcar eventos de nuevos mensaje del servidor
        base.OnMounted();
    }

    public override VisualNode Render()
    {
        return new VStack()
        {
            new Button("Eliminar mensajes").OnClicked( () => SetState(s => s.mensajes.Clear())),

            new Label("Seccion de la lista mensajesS").FontSize(25),

            new Grid("*", "*")
            {
                new CollectionView()
                    .ItemsSource(State.mensajes, GraficarMensajes)
                    .SelectionMode(Microsoft.Maui.Controls.SelectionMode.Single)
                    .OnSelectionChanged((s,e) => MostrarDetalleMensaje(e))
            }
            .HeightRequest(500),

            

            new Button("Agregar un mensaje")
                .OnClicked(() => SetState(s => s.mensajes.Add(
                    new Contratos.Mensaje()
                    {
                        descripcionMensaje = "Este mensaje viene de ALGUN LADO DEL MUNDO",
                        emisor = new Empleado(){ nombreEmpleado = "lula"},
                        idAsunto = 1,
                        idEstado = 1,
                        idMensaje = 1,
                    }))),
            
        }
        
        .BackgroundColor(Color.FromRgb(1, 1, 255));
    }

    private void MostrarDetalleMensaje(Microsoft.Maui.Controls.SelectionChangedEventArgs mensajeSeleccionado)
    {
        var paramMsjSeleccionado = GetParameter<parametrosTablero>();
        var msj = mensajeSeleccionado.CurrentSelection?[0] as Contratos.Mensaje;

        if (msj is null) return;

        paramMsjSeleccionado.Set(p => p.mensajeSeleccionado = msj);
    }

    private VisualNode GraficarMensajes(Contratos.Mensaje msj)
    {
        return new Mensaje()
        {
        }
        .Titulo("importante")
        .NombreEmisor(msj.emisor.nombreEmpleado);
    }
}

public class Mensaje : Component
{
    private string _titulo;
    private string _nombreEmisor;
    private string _descripcion;
    public Mensaje Titulo(string titulo) { _titulo = titulo; return this; }
    public Mensaje NombreEmisor(string nombre) { _nombreEmisor = nombre; return this; }
    public Mensaje Descripcion(string descrp) { _descripcion= descrp; return this; }


    public override VisualNode Render()
    {
        return new Border()
        {
            new Label($"Nota: {_descripcion}").Padding(15),

        }
        .Padding(10)
        .WidthRequest(500)
        .HeightRequest(100)
        .StrokeThickness(2)
        .BackgroundColor(Color.FromRgb(140,150,1))
        .StrokeShape(new RoundRectangle().CornerRadius(40, 0, 0, 40))
        ;
    }
}
public class DetalleDelMensaje : Component
{
    public override VisualNode Render()
    {
        var msj = GetParameter<parametrosTablero>().Value.mensajeSeleccionado;


        return new VStack()
        {
            msj is not null 
            ? new Label($"detalle de msj{msj.idMensaje} , dice: {msj.descripcionMensaje}")
            : new Label(String.Empty)
        }
        .BackgroundColor(Color.FromRgb(1, 255, 255));

    }
}


*/

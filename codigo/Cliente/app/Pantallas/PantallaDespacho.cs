using Contratos;
using MauiReactor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using static app.Componentes.estado_del_despacho;

namespace app.Componentes;


public class parametros_despacho
{
    public Empleado empleadoOperativo { get; set; } // quien está enviando el nuevo mensaje
    public Empleado empleadoConectadoSeleccionado { get; set; } // a quien está dirijido el nuevo mensaje
    public Action<Mensaje> mensajeEnviado { get; set; }
}
public class estado_del_despacho
{

    public enum Cuerpo
    {
        Notificacion = 1,
        Tareas = 2,
        Receta = 3
    }
    public Mensaje nuevoMensaje { get; set; } = new Mensaje();
    public ObservableCollection<Tarea> tareas { get; set; } = new ObservableCollection<Tarea>();
    public Cuerpo tipoMensaje { get; set; }
}
public class PantallaDespacho : Component<estado_del_despacho, parametros_despacho>
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
                new Grid("*", "*, *"){

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

                        Enum.GetValues<Cuerpo>().Select(e => 
                                 new RadioButton(e.ToString())
                                 .CornerRadius(10)
                                 .Margin(1)
                                 .HCenter()
                                 .BorderWidth(1.5)
                                 .BorderColor(Colors.BlueViolet)
                                 .BackgroundColor(Colors.Black)
                                 .TextColor(Colors.Black)
                                 .OnCheckedChanged((s,a) => SetState(s => s.tipoMensaje= e)))
                        
                        ,
                        
                        new ImageButton("icono_mensaje3.png")
                            .HeightRequest(150)
                            .WidthRequest(150)
                            .OnClicked(() => EnviarMensaje(State.nuevoMensaje))


                    }
                        .HCenter()
                        .BackgroundColor(Colors.AliceBlue)
                        .Padding(20)
                    ,

                    State.tipoMensaje switch
                    {
                        Cuerpo.Notificacion => GraficarFormularioNotificacion(),
                        Cuerpo.Tareas => GraficarFormularioTareas(),
                        Cuerpo.Receta => GraficarFormularioReceta(),
                        _ => GraficarFormularioNotificacion()
                    }
                    
                    ,

                }
            };


        VisualNode GraficarFormularioNotificacion() 
        {
            return new VStack()
            {
                new Label("Nueva notificacion")
                    .FontSize(25)
                    .HCenter()
                    
                    ,

                new Entry()
                    .HCenter()
                    .Margin(20)
                    .Placeholder("Ingrese el texto de la notificacion")
                    .OnTextChanged((t) => SetState(s => s.nuevoMensaje.notaMensaje = t))

                    ,

                new Label("Adjunte una imagen")
                    .HCenter()
            }
            .GridColumn(1)
            .BackgroundColor(Colors.LightBlue);  
        }
        VisualNode GraficarFormularioTareas() 
        {
            return new VStack()
            {
                new Label("Nuevas tareas")
                    .HCenter()
                    .TextColor(Colors.Black)
                    .FontSize(25)
                    
                    ,

                new Entry()
                    .Margin(20)
                    .HCenter()
                    .TextColor(Colors.Black)
                    .Placeholder("Ingrese tareas individuales a enviar")
                    .OnCompleted((s, e) => AgregarTarea(s,e))

                    ,

                new CollectionView()
                    .ItemsSource(State.tareas, GraficarCajaTarea)
            }
            .GridColumn(1)
            .BackgroundColor(Colors.LightCyan);

            void AgregarTarea(object sender, EventArgs e) 
            {
                var texto = ((MauiControls.Entry)sender).Text;
                SetState(s => s.tareas.Add(new Tarea() { instrucciones = texto, estadoTarea = Tarea.Estado.NoRealizada }));
                ((MauiControls.Entry)sender).Text = "";
            }
            VisualNode GraficarCajaTarea(Tarea tarea) 
            {
                return new Border()
                {
                    new HStack()
                    {

                        new Label(tarea.instrucciones)
                            .TextColor(Colors.Black)
                            .TextDecorations(TextDecorations.Underline)

                            ,

                        new Button("X")
                            .TextColor(Colors.Black)
                            .FontSize(30)
                            .OnClicked( () => SetState( s=> s.tareas.Remove(tarea)))
                    }

                }
                .BackgroundColor(Colors.Yellow)
                ;
                    
            }

        }
        VisualNode GraficarFormularioReceta() 
        {
            return new VStack()
            {
                new Label("Ingrese RECETA a enviar"),

                new Entry()
                .Placeholder("Ingrese tareas individuales a enviar")
                .OnTextChanged((t) => SetState(s => s.nuevoMensaje.notaMensaje = t))
            }
            .GridColumn(1)
            .BackgroundColor(Colors.Blue);
        }

    }

    private async void EnviarMensaje(Mensaje msj)
    {
        msj.estado = Mensaje.Estado.Despachado;
        msj.idMensaje = new Random().Next(0, 100);

        switch (State.tipoMensaje)
        {
            case Cuerpo.Notificacion:
                msj.notificacion = new Notificacion();
                break;
            case Cuerpo.Tareas:
                msj.tareas = new Tareas() { tareas = State.tareas.ToArray() };
                break;
            case Cuerpo.Receta:
                msj.receta = new Receta();
                break;
            default:
                break;
        }

        // evento para guardar mensaje enviado
        Props.mensajeEnviado.Invoke(msj);

        // obtener el servicio
        var servicio = Services.GetRequiredService<Servicios.IServicios>();
        var respuesta = await servicio.EnviarMensaje(new SolicitudEnviarMensaje() { mensaje=msj });

        if (respuesta.exito) 
        {
            await ContainerPage.DisplayAlert("OK!", "Mensaje enviado", "ok");
        }
    }


}

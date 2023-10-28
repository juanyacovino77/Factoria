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
    public Empleado[] empleadoConectadoSeleccionado { get; set; } // a quien está dirijido el nuevo mensaje
    public Action<Mensaje> mensajeEnviado { get; set; }
}
public class estado_del_despacho
{

    public enum Cuerpo
    {
        Notificacion = 1,
        Tareas = 2,
        Receta = 3,
        Conversa = 4
    }
    public Mensaje nuevoMensaje { get; set; } = new Mensaje();
    public ObservableCollection<Tarea> tareas { get; set; } = new ObservableCollection<Tarea>();
    public Cuerpo tipoMensaje { get; set; }

    public void LimpiarCuerpoMensaje()
    {
        nuevoMensaje.actualizacion = null;
        nuevoMensaje.notificacion = null;
        nuevoMensaje.receta = null;
        nuevoMensaje.tareas = null;
        nuevoMensaje.conversa = null;
    }
}
public class PantallaDespacho : Component<estado_del_despacho, parametros_despacho>
{
    protected override void OnMounted()
    {
        State.nuevoMensaje.emisor = Props.empleadoOperativo;
    }
    public override VisualNode Render()
    {
        string nombres = "";
        Props.empleadoConectadoSeleccionado.ToList().ForEach(e => nombres += $"{e.nombreEmpleado}, ");
        return
            new ContentPage()
            {
                new Grid("*", "*"){

                    new VStack()
                    {
                        new Label("Despacho de mensajes")
                                .FontSize(40)
                                .FontAttributes(MauiControls.FontAttributes.Bold)
                                .TextDecorations(TextDecorations.Underline)
                                ,

                        new Label($"Estas redactando un nuevo mensaje para: {nombres}")
                            .Margin(10)
                            .FontAttributes(MauiControls.FontAttributes.Bold)
                            .TextColor(Colors.Black)
                        ,

                        new HStack()
                        {
                            new Picker()
                                .Title("Tipo de mensaje")
                                .ItemsSource(Enum.GetValues<Cuerpo>().Select(c => c.ToString()).ToList())
                                .OnSelectedIndexChanged(i => SetState(s=>s.tipoMensaje=(Cuerpo)i+1))
                                .TextColor(Colors.Black)
                                .TitleColor(Colors.Black)
                                .Margin(10)
                                .BackgroundColor(Colors.Grey)
                                .HStart()
                                ,

                            new ImageButton("icono_mensaje3.png")
                                .HeightRequest(80)
                                .WidthRequest(80)
                                .OnClicked(() => EnviarMensaje(State.nuevoMensaje))
                                .Margin(50,0,0,0)
                                .HEnd()

                        }.HFill()

                        ,

                        State.tipoMensaje switch
                        {
                            Cuerpo.Notificacion => GraficarFormularioNotificacion(),
                            Cuerpo.Tareas => GraficarFormularioTareas(),
                            Cuerpo.Receta => GraficarFormularioReceta(),
                            Cuerpo.Conversa => GraficarFormularioConversacion(),
                            _ => GraficarFormularioNotificacion()
                        }

                    }
                    .HCenter()
                }
            }
            .BackgroundColor(Colors.CornflowerBlue)
            ;


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
            };
        }
        VisualNode GraficarFormularioTareas() 
        {
            return new VStack()
            {
                new Label("Nuevas tareas")
                    .HCenter()
                    .TextColor(Colors.White)
                    .FontSize(25)

                    ,

                new Entry()
                    .Margin(20)
                    .HCenter()
                    .TextColor(Colors.White)
                    .Placeholder("Ingrese tareas individuales a enviar")
                    .OnCompleted((s, e) => AgregarTarea(s,e))

                    ,



                new CollectionView()
                    .ItemsSource(State.tareas, GraficarCajaTarea)
            };

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
                            .FontSize(20)
                            .Margin(5)

                            ,

                        new Button()
                            .Text("X").HCenter().FontSize(30)
                            .TextColor(Colors.Black)
                            .HeightRequest(35)
                            .BackgroundColor(Colors.Red)
                            .HEnd()
                            .OnClicked( () => SetState( s=> s.tareas.Remove(tarea)))

                    }
                    .HFill()
                    .VFill()

                }
                .StrokeCornerRadius(15,0,0,15)
                .BackgroundColor(Colors.White)
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
            };
        }
        VisualNode GraficarFormularioConversacion()
        {
            return new Label("Inicie una nueva conversacion");
        }
    }

    private async void EnviarMensaje(Mensaje msj)
    {
        msj.estado = Mensaje.Estado.Despachado;

        switch (State.tipoMensaje)
        {
            case Cuerpo.Notificacion:
                msj.notificacion = new Notificacion();
                break;
            case Cuerpo.Tareas:
                msj.tareas = new Tareas() { tareas = State.tareas.ToArray() };
                break;
            case Cuerpo.Receta:
                msj.receta = new Receta() { };
                break;
            case Cuerpo.Conversa:
                msj.conversa = new Conversacion() { mensajesDeTexto = new ObservableCollection<Mensaje>() };
                break;
            default:
                break;
        }


        // obtener el servicio
        var servicio = Services.GetRequiredService<Servicios.IServicios>();
        foreach (var seleccionado in Props.empleadoConectadoSeleccionado)
        {
            var nuevaConversa = new Conversacion() { mensajesDeTexto = new ObservableCollection<Mensaje>()};
            
            var nuevo = new Mensaje()
            {
                 idMensaje = new Random().Next(0,1000),
                 receptor = seleccionado,
                 notaMensaje = msj.notaMensaje,
                 emisor = msj.emisor,
                 notificacion = msj.notificacion,
                 receta = msj.receta,
                 tareas = msj.tareas,
                 actualizacion = msj.actualizacion,
                 conversa = nuevaConversa,
                 estado = Mensaje.Estado.Despachado
            };


            // evento para guardar mensaje enviado
            Props.mensajeEnviado.Invoke(nuevo);

            // enviamos el mensaje
            await PantallaTablero.EnviarMensaje(nuevo);
        }

        await ContainerPage.DisplayAlert("OK!", "Mensaje enviado", "ok");
        await Navigation.PopAsync();

        // limpiar el cuerpo de mensaje
        SetState(s => s.nuevoMensaje=new Mensaje() { emisor = Props.empleadoOperativo });
    }


}
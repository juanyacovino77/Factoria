using Contratos;
using MauiReactor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
        return
            new ContentPage()
            {
                new Grid("*", "auto"){

                    new VStack()
                    {
                       
                            new Label($"{Props.empleadoOperativo.nombreEmpleado} le estas enviando un mensaje a:" +
                            $" {Props.empleadoConectadoSeleccionado.SelectMany(s => {return s.nombreEmpleado; })}")
                            .FontSize(10)
                            .TextColor(Colors.White).Margin(20)
                        ,

                        new HStack()
                        {
                            new Picker()
                                .Title("Tipo de mensaje")
                                .ItemsSource(Enum.GetValues<Cuerpo>().Select(c => c.ToString()).ToList())
                                .OnSelectedIndexChanged(i => SetState(s=>s.tipoMensaje=(Cuerpo)i+1))
                                .BackgroundColor(Colors.Grey)

                                ,

                            new ImageButton("icono_mensaje3.png")
                                .HeightRequest(50)
                                .WidthRequest(50)
                                .OnClicked(() => EnviarMensaje(State.nuevoMensaje))
                                .Margin(10)
                                .HCenter()
                        }

                        ,

                        State.tipoMensaje switch
                        {
                            Cuerpo.Notificacion => GraficarFormularioNotificacion(),
                            Cuerpo.Tareas => GraficarFormularioTareas(),
                            Cuerpo.Receta => GraficarFormularioReceta(),
                            _ => GraficarFormularioNotificacion()
                        }

                    }
                    .HCenter()
                    .Padding(20)
                    .Margin(30)
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

                

                //new CollectionView()
                    //.ItemsSource(State.tareas, GraficarCajaTarea)
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

                            ,

                        new Button("eliminar")
                            .TextColor(Colors.Black)
                            .FontSize(20)
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
            };
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
                 conversa = msj.conversa,
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




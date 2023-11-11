using Contratos;
using MauiReactor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;
using System.IO;
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
    ObservableCollection<Tarea> tareas = new();
    ObservableCollection<PasoReceta> pasos = new();

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
            MauiControls.Image refNotiImg = new();
            SetState(s => { s.LimpiarCuerpoMensaje(); s.nuevoMensaje.notificacion = new Notificacion(); });

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
                    .OnTextChanged((t) => State.nuevoMensaje.notificacion.texto=t)

                    ,

                new Label("Adjunte una imagen")
                    .HCenter()
                    .OnTapped(async () =>
                        {
                            var r = await FilePicker.PickAsync(new PickOptions(){ FileTypes= FilePickerFileType.Images });
                            if(r is null) return;

                            var stream = await r.OpenReadAsync();

                            byte[] bytes;

                            using var lector = new BinaryReader(stream);
                            bytes= lector.ReadBytes((int)stream.Length);
                            await stream.DisposeAsync();

                            State.nuevoMensaje.notificacion.imagen = bytes;

                            refNotiImg.Source = MauiControls.ImageSource.FromStream(() => new MemoryStream(bytes));
                        })
                    ,

                new Image(r => refNotiImg=r)
                    .HeightRequest(300)
                    .WidthRequest(150)

            };
        }
        VisualNode GraficarFormularioTareas() 
        {

            SetState(s => { s.LimpiarCuerpoMensaje(); s.nuevoMensaje.tareas = new Tareas(); });
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
                    .ItemsSource(tareas, GraficarCajaTarea)
            };

            void AgregarTarea(object sender, EventArgs e) 
            {
                var texto = ((MauiControls.Entry)sender).Text;
                tareas.Add(new Tarea() { instrucciones = texto, estadoTarea = Tarea.Estado.NoRealizada });

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
                            .OnClicked( () => tareas.Remove(tarea))

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

            SetState(s => { s.LimpiarCuerpoMensaje(); s.nuevoMensaje.receta = new Receta(); });
            byte[] imagen = Array.Empty<byte>();

            return new VStack()
            {
                new Label("Nuevas Receta")
                    .HCenter()
                    .TextColor(Colors.White)
                    .FontSize(25)

                    ,

                new HStack()
                {
                    new Button("Adjunte una imagen")
                        .Margin(20)
                        .HCenter()
                        .TextColor(Colors.Black)
                        .OnClicked(async () =>
                        {
                                var r = await FilePicker.PickAsync(new PickOptions(){ FileTypes= FilePickerFileType.Images });
                                if(r is null) return;

                                var stream = await r.OpenReadAsync();

                                using var lector = new BinaryReader(stream);
                                imagen= lector.ReadBytes((int)stream.Length);
                                await stream.DisposeAsync();
                        })

                        ,

                     new Entry()
                        .Margin(20)
                        .HCenter()
                        .TextColor(Colors.White)
                        .Placeholder("Ingrese un paso de la receta")
                        .OnCompleted((s, e) => AgregarPaso(s,e))
                        .WidthRequest(200)
                    
                },



                new Grid("*", "*")
                {
                    new CollectionView()
                        .ItemsSource(pasos, GraficarCajaPasos)
                }

            };

            async void AgregarPaso(object sender, EventArgs e)
            {
                if (imagen.Length < 1)
                {
                    await ContainerPage.DisplayAlert("Error", "Debe ingresar una imagen", "ok!");
                    return;
                }

                var texto = ((MauiControls.Entry)sender).Text;
                pasos.Add(new PasoReceta() { paso = texto, imagen= imagen });

                ((MauiControls.Entry)sender).Text = "";
                imagen = Array.Empty<byte>();
            }
            VisualNode GraficarCajaPasos(PasoReceta paso)
            {
                return new Border()
                {
                    new VStack()
                    {
                        new HStack()
                        {
                            new Label($"Paso n°{pasos.IndexOf(paso)}")
                                .TextColor(Colors.Black)
                                .FontAttributes(MauiControls.FontAttributes.Bold)
                                .FontSize(30)
                                .TextDecorations(TextDecorations.Underline)
                                .HStart()

                                ,

                            new Button()
                                    .Text("X").HCenter().FontSize(30)
                                    .TextColor(Colors.Black)
                                    .HeightRequest(35)
                                    .BackgroundColor(Colors.Red)
                                    .HEnd()
                                    .OnClicked( () => pasos.Remove(paso))

                        }

                         ,
                            new VStack()
                            {
                                new Label(paso.paso)
                                .TextColor(Colors.Black)
                                .FontSize(15)
                                .Margin(5)
                                .HStart()
                            }

                                ,


                                new Image()
                                .Source(MauiControls.ImageSource.FromStream(() => new MemoryStream(paso.imagen)))
                                .HeightRequest(200)
                                .WidthRequest(200)
                                .HEnd()
                    }
                }
                .WidthRequest(400)
                .StrokeCornerRadius(15, 15, 15, 15)
                .Stroke(Colors.White)
                ;

            }

        }
        VisualNode GraficarFormularioConversacion()
        {
            SetState(s => { s.LimpiarCuerpoMensaje(); s.nuevoMensaje.conversa = new Conversacion() { mensajesDeTexto = new ObservableCollection<Mensaje>()}; });
            return new Label("Inicie una nueva conversacion");
        }
    }

    private async void EnviarMensaje(Mensaje msj)
    {
        msj.estado = Mensaje.Estado.Despachado;

        if (msj is { receta: not null }) msj.receta.pasos = pasos.ToArray();
        if (msj is { tareas: not null }) msj.tareas.tareas = tareas.ToArray();

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
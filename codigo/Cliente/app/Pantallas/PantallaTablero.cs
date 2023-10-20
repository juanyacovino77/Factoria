using app.Pantallas.Componentes;
using app.Pantallas.Componentes.Controles;
using app.Servicios;
using CommunityToolkit.Maui.Views;
using Contratos;
using MauiReactor;
using MauiReactor.Canvas;
using MauiReactor.Shapes;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.Popup;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace app.Componentes;

public class parametros_tablero
{
    public Operario operario { get; set; }
    public Empleado[] conectados { get; set; }
}
public class estado_del_tablero
{
    public Empleado empleadoOperativo { get; set; }
    public Mensaje? mensajeSeleccionado { get; set; }
    public ObservableCollection<Mensaje> mensajes { get; set; } = new ObservableCollection<Mensaje>();
    public ObservableCollection<Empleado> conectados { get; set; } = new ObservableCollection<Empleado>();
    public ObservableCollection<Mensaje> enviados { get; set; } = new ObservableCollection<Mensaje>();
    public bool IsPopupOpen { get; set; } = false;
    public bool checkBoxEnviados { get; set; } = false;

    public void AñadirOActualizarMensaje(Mensaje msjNuevo)
    {
        var lista = msjNuevo.emisor.idEmpleado == empleadoOperativo.idEmpleado ? enviados : mensajes; // Si es enviado o recibido el nuevo

        if (msjNuevo.actualizacion is not null) // Es actualizacion de un mensaje ya existente
        {
            lista = msjNuevo.actualizacion.emisor.idEmpleado == empleadoOperativo.idEmpleado ? enviados : mensajes;
            var msj = lista.FirstOrDefault(m => m.idMensaje == msjNuevo.actualizacion.idMensaje);
            var i = lista.IndexOf(msj);

            if (i == -1) return;

            lista[i].notificacion = msjNuevo.actualizacion.notificacion;
            lista[i].receta = msjNuevo.actualizacion.receta;
            lista[i].tareas = msjNuevo.actualizacion.tareas;
            lista[i].estado = msjNuevo.actualizacion.estado;
        }
        else // Es un nuevo mensaje
        {
            lista.Insert(0, msjNuevo);
        }
    }
}
public class PantallaTablero : Component<estado_del_tablero, parametros_tablero>
{
    CommunityToolkit.Maui.Views.MediaElement mediaRef = new();

    protected override void OnMounted()
    {
        State.empleadoOperativo = Props.operario.datos;
        Array.ForEach(Props.operario.mensajes, State.mensajes.Add);
        Array.ForEach(Props.conectados, State.conectados.Add);

        var servicio = Services.GetRequiredService<IServicios>();

        servicio.MensajeRecibido += RecibirMensaje; // escucha nuevos msj
        servicio.EmpleadoConectado += RecibirNuevoConectado; // escucha nuevos empleados "conectados"
        servicio.EmpleadoDesconectado += RecibirNuevoDesconectado; // escucha nuevos empleados "desconectados"

        base.OnMounted();
    }
    protected override void OnWillUnmount()
    {
        var servicio = Services.GetRequiredService<IServicios>();

        servicio.MensajeRecibido -= RecibirMensaje; 
        servicio.EmpleadoConectado -= RecibirNuevoConectado; 
        servicio.EmpleadoDesconectado -= RecibirNuevoDesconectado; 

        base.OnWillUnmount();
    }


    public override VisualNode Render()
    {
        Debug.WriteLine("tablero renderizado");
        return 
            new ContentPage()
            {
                new Grid("auto , *", "* , *")
                {
                    GraficarEncabezado()
                    ,

                    GraficarListaDeMensajes()
                    
                    #if ANDROID
                                        ,
                    new Popup()
                        .IsOpen(State.mensajeSeleccionado is not null)
                        .IsFullScreen(true)
                        .Content(GraficarCuerpoDelMensaje)
                        .ShowHeader(false)
                        .ShowCloseButton(true)
                        .ShowHeader(true)
                        .HeaderTitle("Detalle del mensaje")
                    #else
                    ,
                    GraficarCuerpoDelMensaje()
                    #endif
                }
            };
    }
    private VisualNode GraficarEncabezado()
    {

        return new Grid("*","*")
        {

                new VStack()
                {
                   new Label($"Bienvenido,")
                       .FontAttributes(MauiControls.FontAttributes.Bold)
                       .FontSize(20)
                       .Padding(2)
                       .VCenter()
                  
                       ,

                  new Label($"{State.empleadoOperativo.nombreEmpleado} del sector {State.empleadoOperativo.nombreSector}")
                       .FontAttributes(MauiControls.FontAttributes.Bold)
                       .VCenter()
                       .Padding(2)

                       ,

                   new ImageButton("icono_de_usuario2.png")
                       .HeightRequest(80)
                       .HStart()
                       .Margin(10)
                }
                .HStart()
                ,

                new HStack()
                {

                   new Popup()
                            .AutoSizeMode(PopupAutoSizeMode.Height)
                            .Content(GraficarTablaEmpleadosConectados)
                            .HeaderTitle("Envie un mensaje a")
                            .IsOpen(() => State.IsPopupOpen)
                            .OnClosed(()=>SetState(s => s.IsPopupOpen = false, false))
                            .AnimationMode(PopupAnimationMode.SlideOnRight)
                            .AnimationDuration(130)

                            ,

                   new ImageButton("icono_conectados4.png")
                           .OnClicked(() => SetState(s => s.IsPopupOpen = true, false))

                           ,

                   new SonidoMensaje(r => mediaRef=r)
                            .Source(MediaSource.FromResource("sonido_mensaje.wav"))
                            .IsVisible(false)
                            .HeightRequest(200)
                            .WidthRequest(200)


                           ,

                   new ImageButton("icono_cerrar_sesion1.png")
                            .Margin(5)
                            .HeightRequest(50)
                            .OnClicked(CerrarSesion)

                }
                .HEnd()
                .VEnd()
        }
            .BackgroundColor(Colors.Black)
            .GridColumn(0)
            .GridRow(0)
            .GridColumnSpan(2);

        VisualNode GraficarTablaEmpleadosConectados()
        {
            object seleccionados = new();
            return new VStack()
            {
                 new CollectionView { }
                    .ItemsSource(State.conectados, GraficarItemEmpleadoConectado)
                    .SelectionMode(MauiControls.SelectionMode.Multiple)
                    .OnSelectedMany<CollectionView, Empleado>((a) => seleccionados = a)
                    ,

                 new Button("Enviar")
                    .OnClicked( () => IrPantallaDespacho(seleccionados as Empleado[]))
            }
            ;
       
            VisualNode GraficarItemEmpleadoConectado(Empleado p)
                {
                    return
                            //new Border()
                            //{
                            new HStack(spacing: 5)
                            {
                                new MauiReactor.Shapes.Ellipse(){}
                                    .Fill(Colors.Green)
                                    .HeightRequest(10)
                                    .WidthRequest(10)
                                    .HorizontalOptions(MauiControls.LayoutOptions.Start)

                                ,

                                new Label($"{p.nombreEmpleado} - {p.nombreSector} ")
                                    .TextColor(Colors.Black)
                                    .FontAttributes(MauiControls.FontAttributes.Bold)

                            }
                            .HCenter()
                            .VCenter();
                         //};
                    
                    
                } 

            async void IrPantallaDespacho(Empleado[] seleccionados)
            {
                var empleadoOperativo = State.empleadoOperativo;
                var empleadosSeleccionados = seleccionados;

                if (empleadosSeleccionados.Any(e => e.Equals(empleadoOperativo)))
                {
                    await ContainerPage.DisplayAlert("Imposible", "Dale viejo, como te vas a mandar un mensaje a vos mismo?", "Tenés razón");
                    return;
                }

                SetState(s => s.IsPopupOpen = false);


                await Navigation.PushAsync<PantallaDespacho, parametros_despacho>(p =>
                {
                    p.empleadoConectadoSeleccionado = empleadosSeleccionados;
                    p.empleadoOperativo = empleadoOperativo;
                    p.mensajeEnviado += GuardarMensajeEnviado; // callback para guardar mensaje enviado
                });

            }
            void GuardarMensajeEnviado(Mensaje msj)
            {
                SetState(s => s.AñadirOActualizarMensaje(msj));
            }
        }
        async void CerrarSesion()
        {
            bool no = await ContainerPage.DisplayAlert("Cerrar sesión", "¿Desea cerrar su sesión?", "No", "Si");
            if (no) return;

            var respuesta = await Services.GetRequiredService<IServicios>()
                .CerrarSesion(new SolicitudCerrarSesion(State.empleadoOperativo.idEmpleado.ToString()));

            if (!respuesta.exito) return;
            await Navigation.PopAsync();
            
        }
    }
    private VisualNode GraficarListaDeMensajes()
    {
        return new VStack()
        {
            new HStack()
            {


               new Button("+")
                .Padding(10)
                .OnClicked(() => SetState(s => s.AñadirOActualizarMensaje(
                    new Mensaje()
                    {
                        notaMensaje = "Este mensaje viene de ALGUN LADO DEL MUNDO",
                        emisor = new Empleado(){ nombreEmpleado = "carmen", nombreSector= "CAJA - FANTI"},
                        notificacion= new Notificacion(),
                        idMensaje = new Random().Next(0, 100),
                    })))

                ,

                new Button("-")
                    .Padding(10)
                    .OnClicked(() => SetState(s => {s.mensajes.Clear(); s.enviados.Clear(); }))

                ,

                new Border()
                {
                    new HStack
                    {
                        new Label("ENVIADOS")
                            .VCenter()
                            .HCenter()
                            .FontSize(12)
                            .TextColor(Colors.DimGrey)
                            ,

                        new CheckBox()
                            .Color(Colors.BlueViolet)
                            .OnCheckedChanged((s,a) => SetState(s => s.checkBoxEnviados=a.Value))

                            ,
                    }
                    .Padding(3,0,0,0)
                    .HCenter()
                    .VCenter()
                }
                    .BackgroundColor(Colors.WhiteSmoke)
                    .Stroke(Colors.BlueViolet)
                    .StrokeThickness(1)
                    .StrokeCornerRadius(10)
                    
            }
            .HEnd()
            ,

            new Grid("*", "*")
            {
                    new CollectionView()
                        .ItemsSource(State.checkBoxEnviados ? State.enviados : State.mensajes, GraficarMensajes)
                        .SelectionMode(MauiControls.SelectionMode.Single)
                        .OnSelectionChanged((s,e) => SeleccionarMensaje(e))
            }
            .HeightRequest(400)
            ,



        }
        #if WINDOWS
        .BackgroundColor(Colors.LightSteelBlue)
        .GridRow(1);
        #else
        .BackgroundColor(Colors.LightSteelBlue)
        .GridColumnSpan(2)
        .GridRow(1);
        #endif

        VisualNode GraficarMensajes(Mensaje msj)
        {
            return new CajaMensaje()
            {
            }
            .Mensaje(msj)
            ;
        }

        void SeleccionarMensaje(MauiControls.SelectionChangedEventArgs msjSeleccionado)
        {
            var msj = (Mensaje)msjSeleccionado.CurrentSelection.FirstOrDefault();

            SetState(s => s.mensajeSeleccionado = msj);

            if (msj is null || msj is { estado: Mensaje.Estado.Visto } || State.checkBoxEnviados) return;

            MarcarMensajeVisto(msj);

            async void MarcarMensajeVisto(Mensaje msj)
            {
                msj.estado = Mensaje.Estado.Visto;

                // Responder que se vió el mensaje:
                var emisor = msj.emisor;
                var receptor = msj.receptor;

                // Mensaje a enviar
                var msjAEnviar = new Mensaje()
                {
                    emisor = receptor,
                    receptor = emisor,
                    actualizacion = msj
                };

                await EnviarMensaje(msjAEnviar);
            }
        }
    }
    private VisualNode GraficarCuerpoDelMensaje()
    {
        var msj = State.mensajeSeleccionado;
        return new VStack()
        {
            msj switch
            {
                { notificacion: not null } => GraficarNotificacion(msj.notificacion),
                { tareas: not null } => GraficarTareas(msj.tareas),
                { receta: not null } => GraficarReceta(msj.receta),
                _ => new VStack()
            }
        }
            .BackgroundColor(Colors.CornflowerBlue)
            .GridRow(1)
            .GridColumn(1);


        VisualNode GraficarNotificacion(Notificacion notificacion)
        {
            return new VStack()
            {
                new HStack()
                {
                    new SKLottieView()
                        .Source(new SkiaSharp.Extended.UI.Controls.SKFileLottieImageSource()
                        {
                             File = "notificacion1.json"
                        })
                        .IsAnimationEnabled(true)
                        .RepeatCount(-1)
                        .HeightRequest(150)
                        .WidthRequest(150)

                        ,

                    new Label("Notificación")
                        .FontAttributes(MauiControls.FontAttributes.Bold)
                        .TextDecorations(TextDecorations.Underline)
                        .Padding(10)
                        .FontSize(30)
                        .FontFamily("Italic")
                        .TextColor(Colors.White)
                        .VCenter()
                }

                    ,


                new Label($"{notificacion.texto}")
                    .FontFamily("bold")
                    .FontSize(25)
                    .HCenter()


                    ,

                notificacion.estadoActual is Notificacion.Estado.NoConfirmado
                    ?
                    new Label("Confirmá la notificación")
                        .TextColor(Colors.Red)
                        .FontSize(25)
                        .Padding(10,30)
                    :
                    new Label($"Confirmado! con respuesta: {notificacion.respuesta}")
                        .TextColor(Colors.Green)
                        .FontSize(25)
                        .Padding(10,30)


                    ,

                new Button("Confirmar")
                    .OnClicked(async () => 
                    {
                        notificacion.estadoActual = Notificacion.Estado.Confirmado;
                        notificacion.respuesta = await ContainerPage.DisplayPromptAsync("Confirmacion", "Responda la notificacion para confirmarla","Ok", "Cancelar","Afirmativo!", 15);
                        ActualizarEstadoCuerpoMensaje(notificacion); })
                    .HEnd()

                    ,

            };
        }
        VisualNode GraficarReceta(Receta receta)
        {
            return new VStack()
            {
                new HStack()
                {
                    new SKLottieView()
                        .Source(new SkiaSharp.Extended.UI.Controls.SKFileLottieImageSource()
                        {
                             File = "recetas.json"
                        })
                        .IsAnimationEnabled(true)
                        .RepeatCount(-1)
                        .HeightRequest(200)
                        .WidthRequest(200)
                        .HStart()
                    ,

                    new Label("Receta")
                        .FontAttributes(MauiControls.FontAttributes.Bold)
                        .TextDecorations(TextDecorations.Underline)
                        .Padding(10)
                        .FontSize(30)
                        .FontFamily("Italic")
                        .TextColor(Colors.Black)
                        .HEnd()
                        .VCenter()
                }

                ,

                receta.pasos.Select((p,i) => new Label($"Paso n°{i}: {p.paso}")
                    .FontAttributes(MauiControls.FontAttributes.Bold)
                    .TextColor(Colors.Black))
 
            }
            .BackgroundColor(Colors.LightPink);

        }
        VisualNode GraficarTareas(Tareas tareas)
        {
            return new VStack()
            {
                new HStack()
                {
                    new SKLottieView()
                        .Source(new SkiaSharp.Extended.UI.Controls.SKFileLottieImageSource()
                        {
                             File = "tareas.json"
                        })
                        .IsAnimationEnabled(true)
                        .RepeatCount(-1)
                        .HeightRequest(200)
                        .WidthRequest(200)
                        .HStart()
                    ,

                    new Label("Tareas")
                        .FontAttributes(MauiControls.FontAttributes.Bold)
                        .TextDecorations(TextDecorations.Underline)
                        .Padding(10)
                        .FontSize(30)
                        .FontFamily("Italic")
                        .TextColor(Colors.Black)
                        .HEnd()
                        .VCenter()

                    ,

                }
                    ,

                  new CollectionView()
                    .ItemsSource(tareas.tareas, (t) => GraficarCajaTarea(t, TareaCambiada))
            };
            
            void TareaCambiada()
            {
                ActualizarEstadoCuerpoMensaje(tareas);
            }
            VisualNode GraficarCajaTarea(Tarea tarea, Action tareaCambiada)
            {
                return

                    new HStack()
                    {
                        new CheckBox()
                            .IsChecked(tarea.estadoTarea == Tarea.Estado.Realizada)
                            .HeightRequest(50)
                            .Color(Colors.Green)
                            .OnCheckedChanged((a,b) => ActualizarTarea(tarea, b.Value))
                            
                        ,

                        new Label(tarea.instrucciones)
                            .TextColor(Colors.Black)
                            .FontSize(20)

                        ,

                        new Label(tarea.estadoTarea)
                            .TextColor(Colors.Black)
                        
                    }
                    ;
                
                void ActualizarTarea(Tarea tarea, bool confirmado)
                {
                    tarea.estadoTarea = confirmado ? Tarea.Estado.Realizada : Tarea.Estado.NoRealizada;
                    tareaCambiada.Invoke();
                    
                }

            }
}

        async void ActualizarEstadoCuerpoMensaje(object cuerpo)
        {
            // actualizar el estado
            switch (cuerpo)
            {
                case Notificacion n:
                    SetState(s => s.mensajeSeleccionado.notificacion = n);
                    break;
                case Tareas t:
                    SetState(s => s.mensajeSeleccionado.tareas = t);
                    break;
                case Receta r:
                    SetState(s => s.mensajeSeleccionado.receta = r);
                    break;
                default:
                    break;
            }

            // mensaje seleccionado
            var msj = State.mensajeSeleccionado;

            // servicio del servidor
            var mensaje = new Mensaje()
            {
                emisor = msj.receptor,
                receptor = msj.emisor,
                actualizacion = msj
            };

            await PantallaTablero.EnviarMensaje(mensaje);
        }
    }

    private async void RecibirMensaje(object sender, Mensaje msjRecibido)
    {
        try
        {
            if (mediaRef.CurrentState is not CommunityToolkit.Maui.Core.Primitives.MediaElementState.Failed)
            {
                mediaRef.Dispatcher.Dispatch(mediaRef.Play);
            }
        }
        catch (Exception e)
        {
            throw;
        }


        // Guardar el mensaje como recibido
        SetState(s => s.AñadirOActualizarMensaje(msjRecibido));

        if (msjRecibido is { actualizacion: not null }) return;

        // Es un nuevo mensaje hay que responder qeu se recibió
        var emisor = msjRecibido.emisor;
        var receptor = msjRecibido.receptor;

        // Cambiarle el estado a recibido
        msjRecibido.estado = Mensaje.Estado.Recibido;

        // Mensaje final a enviar.
        var msj = new Mensaje()
        {
            actualizacion = msjRecibido,
            emisor = receptor,
            receptor = emisor
        };

        await PantallaTablero.EnviarMensaje(msj);
    }
    private void RecibirNuevoConectado(object sender, Empleado item)
        => SetState(s => s.conectados.Add(item));
    private void RecibirNuevoDesconectado(object sender, Empleado item)
        => SetState(s => s.conectados.Remove(item));


    public static async Task EnviarMensaje(Mensaje mensajeAEnviar)
    {
        await Services.GetRequiredService<Servicios.IServicios>()
            .EnviarMensaje(new SolicitudEnviarMensaje() { mensaje = mensajeAEnviar });
    }

}














/* posibles ramificaciones:
 * 
 * separacion-en-componentes: encabezado, mensajes, cuerpo
 * distintas implementaciones de los DTOS mensaje - cuerpo, genericos, herencia
 * distintas implementaciones del mapa de direcciones singalR - idEmpleado
 * logica de conectados del hub
 * 
 */
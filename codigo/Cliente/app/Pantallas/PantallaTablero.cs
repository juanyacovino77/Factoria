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
    public Mensaje mensajeSeleccionado { get; set; }
    public ObservableCollection<Mensaje> mensajes { get; set; } = new ObservableCollection<Mensaje>();
    public ObservableCollection<Mensaje> enviados { get; set; } = new ObservableCollection<Mensaje>();
    public ObservableCollection<Empleado> conectados { get; set; } = new ObservableCollection<Empleado>();
    public bool IsPopupOpen { get; set; } = false;
    public bool checkBoxEnviados { get; set; } = false;

    public void AñadirOActualizarMensaje(Mensaje msjNuevo)
    {
        bool esEnviado = msjNuevo.emisor.idEmpleado == empleadoOperativo.idEmpleado; // Si es enviado o recibido el nuevo
        var lista = esEnviado ? enviados : mensajes; 

        if (msjNuevo.actualizacion is null) // Es un nuevo mensaje
        { 
            lista.Insert(0, msjNuevo);
            return;
        }


        // Es una actualización de un mensaje ya existente
        esEnviado = msjNuevo.actualizacion.emisor.idEmpleado == empleadoOperativo.idEmpleado;
        lista = esEnviado ? enviados : mensajes;

        var msj = lista.FirstOrDefault(m => m.idMensaje == msjNuevo.actualizacion.idMensaje);
        var i = lista.IndexOf(msj); if (i == -1) return;

        lista[i].notificacion = msjNuevo.actualizacion.notificacion;
        lista[i].receta = msjNuevo.actualizacion.receta;
        lista[i].tareas = msjNuevo.actualizacion.tareas;
        lista[i].conversa?.mensajesDeTexto.Add(msjNuevo.actualizacion.conversa.mensajesDeTexto.LastOrDefault());
        lista[i].estado = msjNuevo.actualizacion.estado;

        // Pertenece a un Proceso Productivo?
        // Buscar pp en la lista de recibidos
        // quiero obtener el proceso que contiene a este msj nuevo.actualizacion
        //if (esEnviado) return;

        ProcesarMensajeDeProceso();

        void ProcesarMensajeDeProceso()
        {
            Proceso proceso = new();
            foreach (var m in mensajes)
            {
                if (m.proceso is null) 
                {
                    proceso = null;
                    continue;
                }
                var esDelProceso = m.proceso.cadena.Any(m => m.idMensaje == msjNuevo.actualizacion.idMensaje);
                if (esDelProceso)
                {
                    proceso = m.proceso;
                    break;
                }
                else
                {
                    proceso = null;
                }
            }

            if (proceso is not null && proceso?.cadena is not null)
            {
                var msjAEnviar = proceso.Procesar(msjNuevo.actualizacion);
                if (msjAEnviar is not null)
                {
                    SolicitarEnviarMensaje(msjAEnviar);
                }
            }

            async void SolicitarEnviarMensaje(Mensaje m)
            {
                await PantallaTablero.EnviarMensaje(m);
                AñadirOActualizarMensaje(m);
            }
        }
        
    }
}
public class PantallaTablero : Component<estado_del_tablero, parametros_tablero>
{
    protected override void OnMounted()
    {
        State.empleadoOperativo = Props.operario.datos;
        Array.ForEach(Props.conectados, State.conectados.Add);
        Array.ForEach(Props.operario.mensajes, State.mensajes.Add);

        var servicio = Services.GetRequiredService<IServicios>();

        servicio.MensajeRecibido += RecibirMensaje; // escucha nuevos msj
        servicio.EmpleadoConectado += RecibirNuevoConectado;
        servicio.EmpleadoDesconectado += RecibirNuevoDesconectado;


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
                new Grid("auto, 80, *", "*")
                {                        
                    GraficarEncabezado()
                    ,
                    GraficarListaDeMensajes()
                    ,
                    GraficarCuerpoDelMensaje()
                }
            }
            .Set(MauiControls.NavigationPage.HasNavigationBarProperty, false)
            ;

        VisualNode GraficarEncabezado()
        {
            var tareasTotales = State.mensajes.Count(m => m.tareas is not null);
            var notificacionesTotales = State.mensajes.Count(m => m.notificacion is not null);
            var tareasCompletas = State.mensajes.Count(m => m.tareas?.estado is Tareas.Estado.Finalizado);
            var notisConfirmadas = State.mensajes.Count(m => m.notificacion?.estadoActual is Notificacion.Estado.Confirmado);

            return new Grid("* *", "*")
            {

                        new HStack()
                        {
                            new Label($"Bienvenido! ")
                               .FontAttributes(MauiControls.FontAttributes.Bold)
                               .FontSize(20)
                               .Padding(2)
                               .VCenter()
                               .TextColor(Colors.Orange)

                               ,

                          new Label($"{State.empleadoOperativo.nombreEmpleado} del sector {State.empleadoOperativo.nombreSector}")
                               .FontAttributes(MauiControls.FontAttributes.Bold)
                               .VCenter()
                               .Padding(2)
                        }
                            .GridRow(0)
                        ,

                        new HStack()
                          {
                               new ImageButton("icono_de_usuario2.png")
                                   .HeightRequest(80)
                                   .HStart()
                                   .Margin(10)
                               ,

                               new VStack()
                               {
                                   new Label($"Hoy confirmaste notificaciones! {notisConfirmadas}/{notificacionesTotales}")
                                    .TextColor(Colors.White)
                                    .FontSize(12)
                                    ,
                                    new LineaProgreso()
                                    .HStart()
                                    .VStart()
                                    .TrackFill(Colors.Grey)
                                    .WidthRequest(150)
                                    .TrackHeight(10)
                                    .ProgressHeight(10)
                                    .Margin(10)
                                    .SegmentCount(notificacionesTotales)
                                    .Progress(notificacionesTotales > 0 ? notisConfirmadas*100/notificacionesTotales: 0)
                                    .ProgressFill(Colors.Yellow)
                                    .IsIndeterminate(notificacionesTotales==0)


                                        ,

                                   new Label($"Hoy completaste tareas! {tareasCompletas}/{tareasTotales}")
                                   .TextColor(Colors.White)
                                   .FontSize(12)
                                   ,
                                   new LineaProgreso()
                                    .HStart()
                                    .VCenter()
                                    .TrackFill(Colors.Grey)
                                    .WidthRequest(150)
                                    .TrackHeight(10)
                                    .ProgressHeight(10)
                                    .Margin(10)
                                    .SegmentCount(tareasTotales)
                                    .Progress(tareasTotales > 0 ? tareasCompletas*100/tareasTotales: 0)
                                    .ProgressFill(Colors.LightGreen)
                                    .IsIndeterminate(tareasTotales==0)

                               }
                                    .VCenter()

                               ,

                                       new Button("+")
                                       .VEnd()
                                       .HeightRequest(35)
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
                                            .VEnd()
                                            .HeightRequest(35)
                                            .OnClicked(() => SetState(s => {s.mensajes.Clear(); s.enviados.Clear(); }))

                                        ,

                                        new Border()
                                        {
                                            new HStack
                                            {
                                                new Label()
                                                    .Text("ENVIADOS")
                                                    .VCenter()
                                                    .HCenter()
                                                    .FontSize(12)
                                                    .TextColor(State.checkBoxEnviados ? Colors.White : Colors.Black)
                                                    .FontAttributes(MauiControls.FontAttributes.Bold)
                                                    ,

                                                new CheckBox()
                                                    .Color(Colors.BlueViolet)
                                                    .OnCheckedChanged((s,a) => SetState(s => s.checkBoxEnviados=a.Value))
                                            }
                                            .Padding(3,0,0,0)
                                            .HCenter()
                                            .VCenter()
                                        }
                                            .BackgroundColor(State.checkBoxEnviados ? Colors.BlueViolet : Colors.WhiteSmoke)
                                            .Stroke(Colors.BlueViolet)
                                            .StrokeThickness(1)
                                            .StrokeCornerRadius(10)
                                            .HeightRequest(35)
                                            .VEnd()

                                            ,

                                   new Popup()
                                            .AutoSizeMode(PopupAutoSizeMode.Height)
                                            .Content(GraficarTablaEmpleadosConectados)
                                            .HeaderTitle("Envie un mensaje a")
                                            .IsOpen(State.IsPopupOpen)
                                            .OnClosed(()=>SetState(s => s.IsPopupOpen = false, false))
                                            .AnimationMode(PopupAnimationMode.SlideOnRight)
                                            .AnimationDuration(130),

                                   new ImageButton("icono_conectados4.png")
                                           .OnClicked(() => SetState(s => s.IsPopupOpen = true))

                                           ,

                                   new SonidoMensaje()
                                            .Source(MediaSource.FromResource("sonido_mensaje.wav"))
                                            .IsVisible(false)
                                           ,

                                   new ImageButton("icono_cerrar_sesion1.png")
                                            .Margin(5)
                                            .HeightRequest(50)
                                            .OnClicked(CerrarSesion)
                          }
                             .GridRow(1)
            }
                .BackgroundColor(Colors.Black)
                ;

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
                    SetState(s =>
                    {
                        s.AñadirOActualizarMensaje(msj);
                        s.mensajeSeleccionado = msj;
                        s.checkBoxEnviados = true;
                    });
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
        VisualNode GraficarListaDeMensajes()
        {
            return
                new Grid("*", "*")
                {
                        new CollectionView()
                            .ItemsSource(State.checkBoxEnviados ? State.enviados : State.mensajes, GraficarMensajes)
                            .SelectionMode(MauiControls.SelectionMode.Single)
                            .OnSelectionChanged((s,e) => SeleccionarMensaje(e))
                            .ItemsLayout(new HorizontalLinearItemsLayout())
                }
            .BackgroundColor(Colors.LightSteelBlue)
            .GridRow(1)
            ;

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
        VisualNode GraficarCuerpoDelMensaje()
        {
            var msj = State.mensajeSeleccionado;

            return new Grid("*", "*")
            {
                msj switch
                {
                    { notificacion: not null } => GraficarNotificacion(msj.notificacion),
                    { tareas: not null } => GraficarTareas(msj.tareas),
                    { receta: not null } => GraficarReceta(msj.receta),
                    { proceso: not null } => GraficarProceso(msj.proceso),
                    { conversa: not null } => GraficarConversacion(msj.conversa),
                    _ => new VStack()
                }
            }
                .GridRow(2)
                .BackgroundColor(Colors.CornflowerBlue)
                .VFill()
                ;
                


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
                            .IsEnabled(!State.checkBoxEnviados)
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

                };

            }
            VisualNode GraficarTareas(Tareas tareas)
            {
                return 
                    new VStack()
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

                            new VStack()
                            {
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

                                new BarraProgreso()
                                    .SegmentCount(tareas.tareas.Length)
                                    .Progress(tareas.Progreso() * 100 / tareas.Cantidad())
                                    .IsIndeterminate(tareas.estado is Tareas.Estado.NoIniciado)
                                    .ProgressFill(Colors.Green)
                                    .ProgressThickness(7)
                                    .TrackThickness(7)
                                    .HeightRequest(50)
                                    .SegmentGapWidth(3)
                                    .HCenter()

                                ,

                                new Label($"{tareas.estado}")
                                    .HCenter()
                                    .FontAttributes(MauiControls.FontAttributes.Bold)

                            }
                            .VCenter()


                        }
                        .HStart()
                            ,

                        new Grid("*", "*")
                        {
                             new CollectionView()
                                .ItemsSource(tareas.tareas, (t) => GraficarCajaTarea(t, TareaCambiada))
                                .IsEnabled(tareas.EnPreparacion())
                                .HCenter()
                        }
                        .HeightRequest(200)
                        .HStart()

                            ,
                        new HStack()
                        {
                            new Button("TOMAR")
                                .TextColor(Colors.Black)
                                .BackgroundColor(Colors.Green)
                                .OnClicked(() => ActualizarEstadoCuerpoMensaje(tareas.PonerEnPreparacion()))
                                .IsEnabled(!State.checkBoxEnviados)

                            ,
                            new Button("RECHAZAR")
                                .TextColor(Colors.Black)
                                .BackgroundColor(Colors.Red)
                                .OnClicked(() => ActualizarEstadoCuerpoMensaje(tareas.PonerEnRechazado()))
                                .IsEnabled(!State.checkBoxEnviados)

                        }
                        .HStart()
                    };

                void TareaCambiada()
                {
                    ActualizarEstadoCuerpoMensaje(tareas.PonerEnFinalizado());
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
                            .IsEnabled(!State.checkBoxEnviados)


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
            VisualNode GraficarProceso(Proceso proceso)
            {
                return new VStack()
                {
                    new HStack()
                    {
                        new SKLottieView()
                            .Source(new SkiaSharp.Extended.UI.Controls.SKFileLottieImageSource()
                            {
                                 File = "proceso1.json"
                            })
                            .IsAnimationEnabled(true)
                            .RepeatCount(-1)
                            .HeightRequest(200)
                            .WidthRequest(200)
                            .HStart()
                        ,

                        new VStack()
                        {
                             new Label($"Proceso productivo  #{State.mensajeSeleccionado.idMensaje}")
                            .FontAttributes(MauiControls.FontAttributes.Bold)
                            .TextDecorations(TextDecorations.Underline)
                            .Padding(10)
                            .FontSize(30)
                            .FontFamily("Italic")
                            .TextColor(Colors.Black)
                            .VStart()

                            ,

                            new Label($"{proceso.estado}")
                            .VCenter()
                            .Margin(35,0,0,0)
                        }

                    }
                    .HCenter()

                        ,

                    new Grid("*", "*")
                    {
                         new CollectionView()
                            .ItemsLayout(new HorizontalLinearItemsLayout())
                            .ItemsSource(proceso.cadena, GraficarCajaMensajeProceso)
                    }
                        ,

                    new HStack()
                    {
                        new Button("DESENCADENAR PROCESO")
                            .TextColor(Colors.Black)
                            .BackgroundColor(Colors.Green)
                            .OnClicked(() => IniciarProceso(proceso))

                        ,
                        new Button("PAUSAR PROCESO")
                            .TextColor(Colors.Black)
                            .BackgroundColor(Colors.Red)

                    }
                    .HCenter()

                }
                .HCenter()
                ;

                VisualNode GraficarCajaMensajeProceso(Mensaje m)
                {
                    return new Border()
                    {
                        new VStack()
                        {
                            new Label($"msj #{m.idMensaje}"),
                            new Label($"estado {m.estado}"),
                            new Label($"recibe {m.receptor.nombreEmpleado}")
                        }
                    }
                    .Margin(5)
                    .StrokeCornerRadius(5,5,5,5)
                    .Stroke(Colors.White)
                    ;
                }
            }
            VisualNode GraficarConversacion(Conversacion conversa)
            {
                MauiControls.Entry entryRef = new();
                var con 
                    = State.mensajeSeleccionado.emisor.idEmpleado
                    == State.empleadoOperativo.idEmpleado
                    ?  State.mensajeSeleccionado.receptor.nombreEmpleado
                    :  State.mensajeSeleccionado.emisor.nombreEmpleado;

                return new VStack()
                {
                    new HStack()
                    {

                        new Label($"Conversación con {con}")
                            .FontAttributes(MauiControls.FontAttributes.Bold)
                            .TextDecorations(TextDecorations.Underline)
                            .Padding(10)
                            .FontSize(30)
                            .FontFamily("Italic")
                            .TextColor(Colors.Black)
                            .HCenter()
                            .VCenter()
                    }
                    ,

                    new Grid("*", "*")
                    {
                         new CollectionView()
                            .ItemsSource(conversa.mensajesDeTexto, GraficarMensajeTexto)
                            .ItemsUpdatingScrollMode(MauiControls.ItemsUpdatingScrollMode.KeepLastItemInView)
                            .VerticalScrollBarVisibility(ScrollBarVisibility.Always)
                            .OnLoaded((sender, args) =>
                            {
                                if (conversa.mensajesDeTexto.Count > 0)
                                {
                                    ContainerPage?.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
                                        ((MauiControls.CollectionView?)sender)?.ScrollTo(conversa.mensajesDeTexto[^1]));
                                }
                            })
                            

                    }
                        .HeightRequest(300)

                        ,

                        new Entry(r => entryRef = r)
                            .Placeholder("Escriba un mensaje...")
                            .TextColor(Colors.Black)
                            .PlaceholderColor(Colors.White)
                            .OnCompleted(
                            () => {
                                conversa.mensajesDeTexto.Add(new Mensaje()
                                {
                                    notaMensaje= entryRef.Text,
                                    emisor = State.empleadoOperativo,
                                    receptor = State.mensajeSeleccionado.receptor,
                                });
                                ActualizarEstadoCuerpoMensaje(conversa);
                                entryRef.Text = "";

                            })
                            .VEnd()
                            .MaxLength(50)

                }
                ;
                VisualNode GraficarMensajeTexto(Mensaje m)
                {
                    return

                        new HStack()
                        {
                        new Border()
                        {
                            new HStack()
                            {
                                 new Label($"{m.emisor.nombreEmpleado.ToUpper()}")
                                .FontSize(15)
                                .FontAttributes(MauiControls.FontAttributes.Bold)
                                .TextDecorations(TextDecorations.Underline)
                                .Margin(5)

                                ,

                                new Label($"{m.notaMensaje}")
                                .FontSize(15)
                                .FontAttributes(MauiControls.FontAttributes.Italic)
                                .TextColor(Colors.DarkGrey)
                                .Margin(5)
                            }.VCenter()
                        }
                        .BackgroundColor(Colors.Black)
                        .StrokeCornerRadius(10,0,0,10)
                        .Stroke(Colors.BlueViolet)
                        .StrokeThickness(2)
                        .HeightRequest(50)
                        .VCenter()
                        }
                        .Margin(2)
                        .HeightRequest(50)
                        .FlowDirection(m.emisor.idEmpleado == State.empleadoOperativo.idEmpleado ? FlowDirection.RightToLeft : FlowDirection.LeftToRight)
                        ;

                }

            }

            async void IniciarProceso(Proceso proceso)
            {
                var msjAEnviar = proceso.IniciarProceso(); // Saca el primer mensaje del proceso

                var receptorActivo = State.conectados.Any(e => e.idEmpleado == msjAEnviar.receptor.idEmpleado);

                if (!receptorActivo)
                {
                    await ContainerPage.DisplayAlert("Error", "No se puede iniciar el proceso, los receptores no estan activos", "Ok");
                    return;
                }

                ActualizarEstadoCuerpoMensaje(proceso.PonerEnProceso());
                msjAEnviar.estado = Mensaje.Estado.Despachado;

                await EnviarMensaje(msjAEnviar);

                SetState(s => s.AñadirOActualizarMensaje(msjAEnviar));
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
                    case Proceso p:
                        SetState(s => s.mensajeSeleccionado.proceso = p);
                        return;
                    case Conversacion c:
                        SetState(s => s.mensajeSeleccionado.conversa = c);
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

                if (State.mensajeSeleccionado.conversa is not null)
                {
                    if (State.mensajeSeleccionado.emisor.idEmpleado == State.empleadoOperativo.idEmpleado)
                    {
                        mensaje = new Mensaje()
                        {
                            emisor = State.mensajeSeleccionado.emisor,
                            receptor = State.mensajeSeleccionado.receptor,
                            actualizacion = msj
                        };
                    }
                }

                await PantallaTablero.EnviarMensaje(mensaje);
            }
        }
    }
    

    private async void RecibirMensaje(object sender, Mensaje msjRecibido)
    {
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
        await Services.GetRequiredService<IServicios>()
            .EnviarMensaje(new SolicitudEnviarMensaje() { mensaje = mensajeAEnviar });
    }
}













/* Alternativa de Pantalla Tablero dividida en componentenes 
public class parametros_tablero_alternativo
{
    public Operario operario { get; set; }
    public Empleado[] conectados { get; set; }
}
public class estado_tablero_alternativo 
{
    public Mensaje mensajeSeleccionado { get; set; }
}
public class PantallaTableroAlternativa : Component<estado_tablero_alternativo, parametros_tablero_alternativo>
{
    public override VisualNode Render()
    {
        Debug.WriteLine("tablero ALT renderizado");
        return new ContentPage() 
        {
            new FlyoutPage()
            {
                new ContentPage
                {
                                    new Label("hola").TextColor(Colors.White).FontSize(200)

                }

               //new Grid("auto *", "*")
               //     {
               //         new Encabezado()
               //             .EmpleadoOperativo(Props.operario.datos)
               //             .EmpleadosConectados(Props.conectados)
               //             ,
               //         new SectorDeMensajes()
               //             .GridRow(1)
               //             .Empleado(Props.operario.datos)
               //             .Mensajes(Props.operario.mensajes)
               //     }
            }
            //.Flyout(GraficarCuerpoDelMensaje())
         };

        VisualNode GraficarCuerpoDelMensaje()
        {
            var msj = State.mensajeSeleccionado;
            return new StackLayout()
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

            };

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

                    new Label($"{tareas.estado}")
                        .VCenter()
                        .Margin(35,0,0,0)

                }
                    ,

                new CollectionView()
                    .ItemsSource(tareas.tareas, (t) => GraficarCajaTarea(t, TareaCambiada))
                    .IsEnabled(tareas.estado is Tareas.Estado.EnPreparacion)
                    .HCenter()
                    ,

                new HStack()
                {
                    new Button("TOMAR")
                        .TextColor(Colors.Black)
                        .BackgroundColor(Colors.Green)
                        .OnClicked(() => {tareas.estado = Tareas.Estado.EnPreparacion; ActualizarEstadoCuerpoMensaje(tareas); })
                    ,
                    new Button("RECHAZAR")
                        .TextColor(Colors.Black)
                        .BackgroundColor(Colors.Red)
                        .OnClicked(() => {tareas.estado = Tareas.Estado.Rechazado; ActualizarEstadoCuerpoMensaje(tareas); })
                }
                .HCenter()

            }
                ;

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

        //new ContentPage()
        //{
        //    new Grid("auto *", "*")
        //    {
        //        new Encabezado()
        //            .EmpleadoOperativo(Props.operario.datos)
        //            .EmpleadosConectados(Props.conectados)

        //            ,

        //        new SectorDeMensajes()
        //            .GridRow(1)
        //            .Empleado(Props.operario.datos)
        //            .Mensajes(Props.operario.mensajes)
        //    }
        //};
    }
}

public class estado_sector_mensajes
{
    public Empleado empleadoOperativo { get; set; }
    public Mensaje mensajeSeleccionado { get; set; }
    public ObservableCollection<Mensaje> mensajes { get; set; } = new ObservableCollection<Mensaje>();
    public ObservableCollection<Mensaje> enviados { get; set; } = new ObservableCollection<Mensaje>();
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
public class SectorDeMensajes : Component<estado_sector_mensajes>
{
    public SectorDeMensajes Mensajes(Mensaje[] msj) { Array.ForEach(msj, State.mensajes.Add); return this; }
    public SectorDeMensajes Empleado(Empleado e) { State.empleadoOperativo = e; return this; }


    protected override void OnMounted()
    {
        var servicio = Services.GetRequiredService<IServicios>();

        servicio.MensajeRecibido += RecibirMensaje; // escucha nuevos msj

        base.OnMounted();
    }
    protected override void OnWillUnmount()
    {
        var servicio = Services.GetRequiredService<IServicios>();

        servicio.MensajeRecibido -= RecibirMensaje;

        base.OnWillUnmount();
    }


    public override VisualNode Render()
    {
        return new Grid("*", "* *")
        {
            GraficarListaDeMensajes(),
            //GraficarCuerpoDelMensaje()
        };

        VisualNode GraficarListaDeMensajes()
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
                            .OnCheckedChanged((s,a) => {
                                SetState(s => s.checkBoxEnviados=a.Value);
                            })

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




        }
            .BackgroundColor(Colors.LightSteelBlue)
            .GridColumn(0);


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
    }

    private async void RecibirMensaje(object sender, Mensaje msjRecibido)
    {
        //Mensaje nuevoMsjEnviado= new Mensaje();
        //GetParameter<EstadoInyectable>().Set(s => nuevoMsjEnviado = s.msjEnviado ?? null);
        //if (nuevoMsjEnviado is null) return;


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
    public static async Task EnviarMensaje(Mensaje mensajeAEnviar)
    {
        await Services.GetRequiredService<IServicios>()
            .EnviarMensaje(new SolicitudEnviarMensaje() { mensaje = mensajeAEnviar });
    }

}

public class estado_encabezado
{
    public bool IsPopupOpen { get; set; } = false;
    public ObservableCollection<Empleado> conectados { get; set; } = new ObservableCollection<Empleado>();
}
public class Encabezado : Component<estado_encabezado>
{
    private Empleado empleadoOperativo;
    public Encabezado EmpleadoOperativo(Empleado e) {empleadoOperativo = e; return this;}
    public Encabezado EmpleadosConectados(Empleado[] e) { SetState(s => Array.ForEach(e, s.conectados.Add)); return this;}


    protected override void OnMounted()
    {
        var servicio = Services.GetRequiredService<IServicios>();

        servicio.EmpleadoConectado += RecibirNuevoConectado;
        servicio.EmpleadoDesconectado += RecibirNuevoDesconectado;

        base.OnMounted();
    }
    protected override void OnWillUnmount()
    {
        var servicio = Services.GetRequiredService<IServicios>();

        servicio.EmpleadoConectado -= RecibirNuevoConectado;
        servicio.EmpleadoDesconectado -= RecibirNuevoDesconectado;

        base.OnWillUnmount();
    }

    public override VisualNode Render()
    {
        Debug.WriteLine("encabezado renderizado");
        return new Grid("*", "*")
        {

                new VStack()
                        {
                           new Label($"Bienvenido,")
                               .FontAttributes(MauiControls.FontAttributes.Bold)
                               .FontSize(20)
                               .Padding(2)
                               .VCenter()

                               ,

                          new Label($"{empleadoOperativo.nombreEmpleado} del sector {empleadoOperativo.nombreSector}")
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
                            .IsOpen(State.IsPopupOpen)
                            .OnClosed(()=>SetState(s => s.IsPopupOpen = false))
                            .AnimationMode(PopupAnimationMode.SlideOnRight)
                            .AnimationDuration(130),




                   new ImageButton("icono_conectados4.png")
                           .OnClicked(() => SetState(s => s.IsPopupOpen = true))

                           ,




                   new SonidoMensaje()
                            .Source(MediaSource.FromResource("sonido_mensaje.wav"))
                            .IsVisible(true)
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
        .BackgroundColor(Colors.Black);



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


            }

            async void IrPantallaDespacho(Empleado[] seleccionados)
            {
                var empleadoOperativoo = empleadoOperativo;
                var empleadosSeleccionados = seleccionados;

                if (empleadosSeleccionados.Any(e => e.Equals(empleadoOperativoo)))
                {
                    await ContainerPage.DisplayAlert("Imposible", "Dale viejo, como te vas a mandar un mensaje a vos mismo?", "Tenés razón");
                    return;
                }

                SetState(s => s.IsPopupOpen = false);


                await Navigation.PushAsync<PantallaDespacho, parametros_despacho>(p =>
                {
                    p.empleadoConectadoSeleccionado = empleadosSeleccionados;
                    p.empleadoOperativo = empleadoOperativoo;
                    p.mensajeEnviado += GuardarMensajeEnviado; // callback para guardar mensaje enviado
                });

            }
            void GuardarMensajeEnviado(Mensaje msj)
            {
                //GetParameter<EstadoInyectable>().Set(s => s.msjEnviado = msj);
            }
        }
        async void CerrarSesion()
        {
            bool no = await ContainerPage.DisplayAlert("Cerrar sesión", "¿Desea cerrar su sesión?", "No", "Si");
            if (no) return;

            var respuesta = await Services.GetRequiredService<IServicios>()
                .CerrarSesion(new SolicitudCerrarSesion(empleadoOperativo.idEmpleado.ToString()));

            if (!respuesta.exito) return;
            await Navigation.PopAsync();

        }
    }


    private void RecibirNuevoConectado(object sender, Empleado item)
    => SetState(s => s.conectados.Add(item));
    private void RecibirNuevoDesconectado(object sender, Empleado item)
        => SetState(s => s.conectados.Remove(item));
}
Alternativa de Pantalla Tablero dividida en componentenes */

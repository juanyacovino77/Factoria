using app.Pantallas.Componentes;
using app.Pantallas.Componentes.Controles;
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    protected override void OnMounted()
    {
        State.empleadoOperativo = Props.operario.datos;
        Array.ForEach(Props.operario.mensajes, State.mensajes.Add);
        Array.ForEach(Props.conectados, State.conectados.Add);

        var servicio = Services.GetRequiredService<Servicios.IServicios>();

        servicio.MensajeRecibido += RecibirMensaje; // escucha nuevos msj
        servicio.EmpleadoConectado += RecibirNuevoConectado; // escucha nuevos empleados "conectados"
        servicio.EmpleadoDesconectado += RecibirNuevoDesconectado; // escucha nuevos empleados "desconectados"

        base.OnMounted();
    }
    protected override void OnWillUnmount()
    {
        var servicio = Services.GetRequiredService<Servicios.IServicios>();

        servicio.MensajeRecibido -= RecibirMensaje; 
        servicio.EmpleadoConectado -= RecibirNuevoConectado; 
        servicio.EmpleadoDesconectado -= RecibirNuevoDesconectado; 

        base.OnWillUnmount();
    }


    public override VisualNode Render()
    {
        Debug.WriteLine("Render");
        return new NavigationPage() {

            new ContentPage()
            {
                new Grid("80 , *", "* , *")
                {
                    GraficarEncabezado(),
                    GraficarListaDeMensajes(),
                    GraficarCuerpoDelMensaje()
                }
            }
        };

    }
    private VisualNode GraficarEncabezado()
    {

        return new Grid("*,*", "*,*")
        {
           new Label($"Tablero de {State.empleadoOperativo.nombreEmpleado} " +
                     $"del sector {State.empleadoOperativo.nombreSector} ")
               .FontSize(20)
               .Padding(25,0,0,0)

           ,

           new Label($"id del empleado {State.empleadoOperativo.idEmpleado} " +
                     $"y del sector {State.empleadoOperativo.idSector}")
               .Padding(25, 0, 0, 0)
               .GridRow(1)

           ,

           new HStack()
           {
               new ImageButton("icono_conectados4.png")
                   .OnClicked(() => SetState(s => s.IsPopupOpen = true))
                   .HStart()

                ,

               new Button("icono_cerrar_sesion.svg")
                    .OnClicked(async () => await ContainerPage.DisplayAlert("Cerraste la sesión", "", "OK"))

           }
               .GridColumn(1)
               .GridRow(1)
           
           ,

            new Popup()
                    .AutoSizeMode(PopupAutoSizeMode.Height)
                    .Content(GraficarTablaEmpleadosConectados)
                    .HeaderTitle("Envie un mensaje a")
                    .IsOpen(State.IsPopupOpen)
                    .OnClosed(()=>SetState(s => s.IsPopupOpen = false, false))
                    .GridColumn(1)
                    .GridRow(0)

        }
            .BackgroundColor(Colors.Black)
            .GridColumn(0)
            .GridRow(0)
            .GridColumnSpan(2);

        VisualNode GraficarTablaEmpleadosConectados()
        {
            return
                new CollectionView { }
            .ItemsSource(State.conectados, GraficarItemEmpleadoConectado)
            .SelectionMode(MauiControls.SelectionMode.Single)
            .OnSelectionChanged((s, e) => SeleccionarEmpleadoConectado(e))

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
            
            async void SeleccionarEmpleadoConectado(MauiControls.SelectionChangedEventArgs e)
            {
                SetState(s => s.IsPopupOpen = false);

                var empleadoSeleccionado = e.CurrentSelection.FirstOrDefault() as Empleado;
                var empleadoOperativo = State.empleadoOperativo;

                if (empleadoOperativo.Equals(empleadoSeleccionado))
                {
                    await ContainerPage.DisplayAlert("Imposible", "Dale viejo, como te vas a mandar un mensaje a vos mismo?", "Tenés razón");
                    return;
                }

                await Navigation.PushAsync<PantallaDespacho, parametros_despacho>(p => 
                { 
                    p.empleadoConectadoSeleccionado = empleadoSeleccionado;
                    p.empleadoOperativo = empleadoOperativo;
                    p.mensajeEnviado += GuardarMensajeEnviado; // callback para guardar mensaje enviado
                }) ;
            }
            void GuardarMensajeEnviado(Mensaje msj)
            {
                SetState(s => s.AñadirOActualizarMensaje(msj));
            }
        }
        async void CerrarSesion()
        {

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
                    .OnClicked(() => SetState(s => s.mensajes.Clear()))

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

                        new CheckBox(){}
                            .Color(Colors.BlueViolet)
                            .OnCheckedChanged((e,b) => SetState(s=>s.checkBoxEnviados=b.Value))
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
        .BackgroundColor(Colors.LightSteelBlue)
        .GridRow(1);

        VisualNode GraficarMensajes(Mensaje msj)
        {
            return new CajaMensaje()
            {
            }
            .Mensaje(msj);
        }

        void SeleccionarMensaje(MauiControls.SelectionChangedEventArgs msjSeleccionado)
        {
            var msj = msjSeleccionado.CurrentSelection.FirstOrDefault() as Mensaje;

            SetState(s => s.mensajeSeleccionado = msj);

            if (msj is { estado: Mensaje.Estado.Visto } || State.checkBoxEnviados) return;

            MarcarMensajeVisto(msj);

            void MarcarMensajeVisto(Mensaje msj)
            {
                // Responder que se vió el mensaje:
                var emisor = msj.emisor;
                var receptor = msj.receptor;

                // Cambiarle el estado a recibido
                msj.estado = Mensaje.Estado.Visto;


                Services.GetRequiredService<Servicios.IServicios>().EnviarMensaje(
                    new SolicitudEnviarMensaje()
                    {
                        mensaje = new Mensaje()
                        {
                            emisor = receptor,
                            receptor = emisor,
                            actualizacion = msj
                        }
                    });
            }
        }
    }
    private VisualNode GraficarCuerpoDelMensaje()
    {
        var msj = State.mensajeSeleccionado;
        return msj switch
        {
            { notificacion: not null } => GraficarNotificacion(msj.notificacion),
            { tareas: not null } => GraficarTareas(msj.tareas),
            { receta: not null } => GraficarReceta(msj.receta),
            _ => GraficarCuerpoVacio(),
        };

        VisualNode GraficarNotificacion(Notificacion notificacion)
        {
            return new VStack()
            {
                new HStack(spacing: 10)
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
                        .HStart()
                    ,

                    new Label("NOTIFICACION")
                        .FontSize(30)
                        .VCenter()
                        .FontFamily("Italic")
                        .TextColor(Colors.LightGray)
                }

                    ,


                new Label($"{notificacion.texto}")
                    .FontFamily("bold")
                    .FontSize(25)

                    ,

                new Label($"{notificacion.estadoActual}")
                    .FontSize(25)

                    ,

                new Button("Confirmar")
                    .OnClicked(() => ActualizarEstadoCuerpoMensaje(notificacion.estadoActual = Notificacion.Estado.Confirmado))
                    .HEnd()

                    ,

            }
                .BackgroundColor(Colors.CornflowerBlue)
                .GridRow(1)
                .GridColumn(1);
        }
        VisualNode GraficarReceta(Receta receta)
        {
            return new VStack()
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

                new Label($"paso 1: {receta.paso1} despues hacer paso 2: {receta.paso2} ")
                    .FontAttributes(MauiControls.FontAttributes.Bold)
                    .TextColor(Colors.Black)
            }
            .BackgroundColor(Colors.LightPink)
            .GridRow(1)
            .GridColumn(1);
        }
        VisualNode GraficarTareas(Tareas tareas)
        {
            return new VStack()
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

                  new CollectionView()
                    .ItemsSource(tareas.tareas, (t) => GraficarCajaTarea(t, TareaCambiada))
            }
            .GridColumn(1)
            .GridRow(1)
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
                        new Label(tarea.instrucciones)
                            .TextColor(Colors.Black)
                            .FontSize(20)

                        ,

                        new Label(tarea.estadoTarea)
                            .TextColor(Colors.Black)

                        ,

                        new CheckBox()
                            .HeightRequest(50)
                            .Color(Colors.Green)
                            .OnCheckedChanged(() => ActualizarTarea(tarea))
                        

                    }
                    .BackgroundColor(Colors.Gainsboro)
                    ;
                
                void ActualizarTarea(Tarea tarea)
                {
                    tarea.estadoTarea = Tarea.Estado.Realizada;
                    tareaCambiada.Invoke();
                    
                }

            }
        }
        VisualNode GraficarCuerpoVacio()
        {
            return new VStack()
            {

            }
                    .BackgroundColor(Colors.CornflowerBlue)
                    .GridRow(1)
                    .GridColumn(1);
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
            var servicio = Services.GetRequiredService<Servicios.IServicios>();

            // envio el msj con el estado nuevo
            await servicio.EnviarMensaje(
                new SolicitudEnviarMensaje
                {

                    mensaje = new Mensaje()
                    {
                        emisor = msj.receptor,
                        receptor = msj.emisor,
                        actualizacion = msj
                    }
                });
        }
    }



    private void RecibirMensaje(object sender, Mensaje msjRecibido)
    { 
        // Guardar el mensaje como recibido
        SetState(s => s.AñadirOActualizarMensaje(msjRecibido));

        if (msjRecibido is { actualizacion: not null }) return;

        // Es un nuevo mensaje hay que responder qeu se recibió
        var emisor = msjRecibido.emisor;
        var receptor = msjRecibido.receptor;

        // Cambiarle el estado a recibido
        msjRecibido.estado = Mensaje.Estado.Recibido;

        Services.GetRequiredService<Servicios.IServicios>().EnviarMensaje(
            new SolicitudEnviarMensaje()
            {
                mensaje = new Mensaje()
                {
                    actualizacion = msjRecibido,
                    emisor = receptor,
                    receptor = emisor
                }
            });
    }
    private void RecibirNuevoConectado(object sender, Empleado item)
        => SetState(s => s.conectados.Add(item));
    private void RecibirNuevoDesconectado(object sender, Empleado item)
        => SetState(s => s.conectados.Remove(item));
}













/* posibles ramificaciones:
 * 
 * separacion-en-componentes: encabezado, mensajes, cuerpo
 * distintas implementaciones de los DTOS mensaje - cuerpo, genericos, herencia
 * distintas implementaciones del mapa de direcciones singalR - idEmpleado
 * logica de conectados del hub
 * 
 */
using Contratos;
using MauiReactor;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.Popup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace app.Componentes;




public class estado_del_tablero
{
    public Empleado empleadoOperativo { get; set; }
    public Mensaje mensajeSeleccionado { get; set; }
    public ObservableCollection<Mensaje> mensajes { get; set; } = new ObservableCollection<Mensaje>();
    public ObservableCollection<Empleado> conectados { get; set; } = new ObservableCollection<Empleado>();
    public bool IsPopupOpen { get; set; } = false;

    public void InsertarActualizarMensaje(Mensaje msjNuevo)
    {
        var msjActual = mensajes.SingleOrDefault(m => m.idMensaje == msjNuevo.idMensaje);

        if (msjActual is null) mensajes.Insert(0, msjNuevo); // Es un mensaje nuevo
        else // Es la actualizacion de un mensaje existente
        {
            var i = mensajes.IndexOf(msjActual);
            mensajes[i] = msjNuevo;
        }
    }
}
public class parametros_tablero
{
    public Operario operario { get; set; }
    public Empleado[] conectados { get; set; } 
}
public class pantallaTablero : Component<estado_del_tablero, parametros_tablero>
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
    public override VisualNode Render()
    {
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

           new Button("ver conectados")
           .GridRow(1).GridColumn(1)
           .OnClicked( () => SetState(s => s.IsPopupOpen = true))
           
           ,

            new SfPopup()
                    .Content(GraficarTablaEmpleadosConectados)
                    .HeaderTitle("empleados contectados:")
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
            return new CollectionView
            {

            }
            .ItemsSource(State.conectados, p => new Label($"{p.idEmpleado} - {p.nombreEmpleado} - {p.nombreSector} ").TextColor(Colors.Black))
            .SelectionMode(MauiControls.SelectionMode.Single)
            .OnSelectionChanged((s, e) => SeleccionarEmpleadoConectado(e))
            ;

            async void SeleccionarEmpleadoConectado(MauiControls.SelectionChangedEventArgs e)
            {
                SetState(s => s.IsPopupOpen = false);

                var empleadoSeleccionado = e.CurrentSelection.FirstOrDefault() as Empleado;
                var empleadoOperativo = State.empleadoOperativo;

                await Navigation.PushAsync<pantallaDespacho, parametros_despacho>(p => 
                { 
                    p.empleadoConectadoSeleccionado = empleadoSeleccionado;
                    p.empleadoOperativo = empleadoOperativo;
                    p.mensajeEnviado += GuardarMensajeEnviado; // callback
                }) ;
            }

            void GuardarMensajeEnviado(Mensaje msj)
            {
                SetState(s => s.InsertarActualizarMensaje(msj));
            }
        }

    }
    private VisualNode GraficarListaDeMensajes()
    {
        return new VStack()
        {
            new Label("Mensajes").FontSize(25),

            new Button("Eliminar mensajes")
                .Padding(10,10,10,10)
                .WidthRequest(150)
                .OnClicked( () => SetState(s => s.mensajes.Clear()) )
            ,

            new Grid("*", "*")
            {
                new CollectionView()
                    .ItemsSource(State.mensajes, GraficarMensajes)
                    .SelectionMode(MauiControls.SelectionMode.Single)
                    .OnSelectionChanged((s,e) => SeleccionarMensaje(e))
            }
            .HeightRequest(350)

            ,


            new Button("Agregar un mensaje")
                .OnClicked(() => SetState(s => s.InsertarActualizarMensaje(
                    new Mensaje()
                    {
                        notaMensaje = "Este mensaje viene de ALGUN LADO DEL MUNDO",
                        emisor = new Empleado(){ nombreEmpleado = "carmen", nombreSector= "CAJA - FANTI"},
                        notificacion= new Notificacion(),
                        idMensaje = new Random().Next(0, 100),
                    }))),

        }
        .BackgroundColor(Colors.LightSteelBlue)
        .GridRow(1);

        VisualNode GraficarMensajes(Mensaje msj)
        {
            return new cajaMensaje()
            {
            }
            .Mensaje(msj);
        }

        void SeleccionarMensaje(MauiControls.SelectionChangedEventArgs msjSeleccionado)
        {
            SetState(s => s.mensajeSeleccionado = msjSeleccionado.CurrentSelection.FirstOrDefault() as Mensaje);
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
            {

            }
                    .BackgroundColor(Colors.CornflowerBlue)
                    .GridRow(1)
                    .GridColumn(1);
        }


        async void ActualizarEstadoCuerpoMensaje()
        {
            // mensaje seleccionado
            var msj = State.mensajeSeleccionado;

            // servicio del servidor
            var servicio = Services.GetRequiredService<Servicios.IServicios>();

            // actualizar el estado
            msj.notificacion.estadoActual = Notificacion.Estado.Visto;

            // envio el msj con el estado nuevo
            await servicio.EnviarMensaje(new SolicitudEnviarMensaje { mensaje = msj});
        }
    }



    private void RecibirMensaje(object sender, Mensaje msjRecibido)
        => SetState(s => s.InsertarActualizarMensaje(msjRecibido));
    private void RecibirNuevoConectado(object sender, Empleado item)
        => SetState(s => s.conectados.Add(item));
    private void RecibirNuevoDesconectado(object sender, Empleado item)
        => SetState(s => s.conectados.Remove(item));
}






[Scaffold(typeof(Syncfusion.Maui.Popup.SfPopup))]
public partial class SfPopup
{
    public SfPopup Content(Func<VisualNode> render)
    {
        this.Set(Syncfusion.Maui.Popup.SfPopup.ContentTemplateProperty,
            new MauiControls.DataTemplate(() => TemplateHost.Create(render()).NativeElement));

        return this;
    }
}

[Scaffold(typeof(Syncfusion.Maui.Core.SfView))]
public abstract class SfView { }



/* posibles ramificaciones:
 * 
 * separacion-en-componentes: encabezado, mensajes, cuerpo
 * distintas implementaciones de los DTOS mensaje - cuerpo, genericos, herencia
 * distintas implementaciones del mapa de direcciones singalR - idEmpleado
 * logica de conectados del hub
 * 
 */
using Contratos;
using MauiReactor;
using MauiReactor.Shapes;
using static Contratos.Mensaje.Estado;

namespace app.Pantallas.Componentes;


public class CajaMensaje : Component
{
    private Mensaje _mensaje;
    private string _imagenEstado => _mensaje.estado switch
    {
        NoDespachado => "icono_sindespachar.png",
        Despachado => "icono_despachado.png",
        Recibido => "icono_recibido.png",
        Visto => "icono_visto.png",
        _ => "",
    };
    private string _tipoMensaje => _mensaje switch
    {
        { notificacion: not null } => "Notificacion",
        { tareas: not null } => "Tareas",
        { receta: not null } => "Receta",
        _ => "Mensaje"
    };

    public CajaMensaje Mensaje(Mensaje mensaje) { _mensaje = mensaje; return this; }

    
    public override VisualNode Render()
    {
        return
            new VStack()
            {
                new Border()
                {
                    new Grid("*,*,*","100,*,*")
                    {
                        new Image("icono_mensaje.png")
                            .HeightRequest(70)
                            .HCenter()
                            .VCenter()
                            .GridRowSpan(3)
                            .GridColumn(0)
                            .GridRow(0)

                            ,

                        new Label($"{_tipoMensaje} RECIBIDO")
                            .TextColor(Colors.Black)
                            .Padding(5)
                            .HStart()
                            .GridRow(0)
                            .GridColumn(1)
                            ,

                        new Label($"De: {_mensaje.emisor.nombreEmpleado?.ToUpper() } para {_mensaje.receptor?.nombreEmpleado ?? "sin receptor"} ")
                            .GridRow(1)
                            .Padding(5)
                            .GridColumn(1)
                            ,

                        new Label()
                            .GridRow(2)
                            .GridColumn(1)

                            ,


                        new VStack()
                        {
                            new Image(_imagenEstado)
                                .HeightRequest(30)
                                .HCenter()
                                .VCenter()
                                ,


                            new Label(_mensaje.estado)
                                .FontSize(12)
                                .Padding(5)
                                .HCenter()
                        }
                            .GridColumn(2)
                            .GridRowSpan(3)
                            .HEnd()


                    }
                    
                }
                    .Shadow(new Shadow())
                    .StrokeCornerRadius(30, 15, 15, 30)
                    .Padding(5)
                    .Margin(10, 10, 15, 3)
                    .Stroke(_mensaje.estado is Visto ? Colors.CadetBlue : Colors.Orange)
                    .StrokeThickness(3)
                    .BackgroundColor(Colors.LightSkyBlue)
            }
            ;

    }
}



// mensaje expander
/*
public class estado_caja_mensaje
{
    public double Height { get; set; } = 200;
}
public class CajaMensaje : Component<estado_caja_mensaje>
{
    private Mensaje _mensaje;
    private string _imagenEstado => _mensaje.estado switch
    {
        NoDespachado => "icono_sindespachar.png",
        Despachado => "icono_despachado.png",
        Recibido => "icono_recibido.png",
        Visto => "icono_visto.png",
        _ => "",
    };
    private string _tipoMensaje => _mensaje switch
    {
        { notificacion: not null } => "Notificacion",
        { tareas: not null } => "Tareas",
        { receta: not null } => "Receta",
        _ => "Mensaje"
    };

    public CajaMensaje Mensaje(Mensaje mensaje) { _mensaje = mensaje; return this; }

    protected override void OnMountedOrPropsChanged()
    {
        State.Height = 200;

        MauiControls.Application.Current?.Dispatcher.Dispatch(() =>
        SetState(s =>
        {
            s.Height = 0;
        }));
        base.OnMountedOrPropsChanged();
    }
    /*
    public override VisualNode Render()
    {
        return
            new VStack()
            {
                new Border()
                {
                    new Grid("*,*,*","100,*,*")
                    {
                        new Image("icono_mensaje.png")
                            .HeightRequest(70)
                            .HCenter()
                            .VCenter()
                            .GridRowSpan(3)
                            .GridColumn(0)
                            .GridRow(0)

                            ,

                        new Label($"{_tipoMensaje} RECIBIDO")
                            .TextColor(Colors.Black)
                            .Padding(5)
                            .HStart()
                            .GridRow(0)
                            .GridColumn(1)
                            ,

                        new Label($"De: {_mensaje.emisor.nombreEmpleado.ToUpper() } para {_mensaje.receptor?.nombreEmpleado ?? "sin receptor"} ")
                            .GridRow(1)
                            .Padding(5)
                            .GridColumn(1)
                            ,

                        new Label()
                            .GridRow(2)
                            .GridColumn(1)

                            ,


                        new VStack()
                        {
                            new Image(_imagenEstado)
                                .HeightRequest(30)
                                .HCenter()
                                .VCenter()
                                ,


                            new Label(_mensaje.estado)
                                .FontSize(12)
                                .Padding(5)
                                .HCenter()
                        }
                            .GridColumn(2)
                            .GridRowSpan(3)
                            .HEnd()


                    } // Header
                    
                }
                    .Shadow(new Shadow())
                    .StrokeCornerRadius(30, 15, 15, 30)
                    .Padding(5)
                    .Margin(10, 10, 15, 3)
                    .Stroke(_mensaje.estado is Visto ? Colors.CadetBlue : Colors.Orange)
                    .StrokeThickness(3)
                    .BackgroundColor(Colors.LightSkyBlue)
            }
            ;

    }
    */

    /*
    public override VisualNode Render()
    {
        return

                new Border()
                {
                    new Grid ("auto auto", "*")
                    {
                        // Header
                        new Grid("*,*,*","100,*,*")
                        {
                            new Image("icono_mensaje.png")
                                .HeightRequest(70)
                                .HCenter()
                                .VCenter()
                                .GridRowSpan(3)
                                .GridColumn(0)
                                .GridRow(0)

                                ,

                            new Label($"{_tipoMensaje} RECIBIDO")
                                .TextColor(Colors.Black)
                                .Padding(5)
                                .HStart()
                                .GridRow(0)
                                .GridColumn(1)
                                ,

                            new Label($"De: {_mensaje.emisor.nombreEmpleado } para {_mensaje.receptor?.nombreEmpleado ?? "sin receptor"} ")
                                .GridRow(1)
                                .Padding(5)
                                .GridColumn(1)
                                ,

                            new Label()
                                .GridRow(2)
                                .GridColumn(1)

                                ,


                            new VStack()
                            {
                                new Image(_imagenEstado)
                                    .HeightRequest(30)
                                    .HCenter()
                                    .VCenter()
                                    ,


                                new Label(_mensaje.estado)
                                    .FontSize(12)
                                    .Padding(5)
                                    .HCenter()
                            }
                                .GridColumn(2)
                                .GridRowSpan(3)
                                .HEnd()


                        }
                            .OnTapped(() => SetState(s => s.Height = s.Height is 200 ? 0 : 200))
                            .GridRow(0)
                            

                        ,

                        // Body
                        new Grid()
                        {
                            new ScrollView() 
                            {
                               Children()

                            }
                        }
                            .GridRow(1)
                            .HeightRequest(State.Height)
                            .BackgroundColor(Colors.CornflowerBlue)
                    }

                }
                    .StrokeCornerRadius(30, 15, 15, 30)
                    .Padding(5)
                    .Margin(10, 10, 15, 3)
                    .Stroke(_mensaje.estado is Visto ? Colors.CadetBlue : Colors.Orange)
                    .StrokeThickness(3)
                    .BackgroundColor(Colors.LightSkyBlue)

        ;

    }
     // implementacion de acordion
 }
*/
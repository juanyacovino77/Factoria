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

                new Border()
                {
                    new Grid("*", "*")
                    {
                        new HStack()
                        {
                             new Image("icono_mensaje.png")
                                .HeightRequest(30)
                                .Margin(10)
                                .VCenter()

                            ,

                             new VStack()
                             {
                                 new Label($"{_tipoMensaje}")
                                    .TextColor(Colors.Black)
                                    .Padding(5)
                                    .HStart()
                                    .GridRow(0)
                                    .GridColumn(1)
                                ,

                                new Label($"De {_mensaje.emisor.nombreEmpleado?.ToUpper()}")
                             }


                            ,
                            new VStack()
                            {
                                new Image(_imagenEstado)
                                    .HeightRequest(20)
                                    .HCenter()
                                    .VCenter()
                                    ,


                                new Label(_mensaje.estado)
                                    .FontSize(10)
                                    .Padding(5)
                                    .HCenter()
                            }
                            .VCenter()
                            .HEnd()
                        }
                    }
                }
                    .Shadow(new Shadow())
                    .StrokeCornerRadius(20, 15, 15, 20)
                    .Margin(2,8,8,2)
                    .Stroke(_mensaje.estado is Visto ? Colors.CadetBlue : Colors.Orange)
                    .StrokeThickness(3)
                    .BackgroundColor(Colors.LightSkyBlue)
            
            ;

    }
}

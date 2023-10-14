using Contratos;
using MauiReactor;
using MauiReactor.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

                    new Label($"De: {_mensaje.emisor.nombreEmpleado.ToUpper() } del sector {_mensaje.emisor?.nombreSector.ToUpper()} ")
                        .GridRow(1)
                        .Padding(5)
                        .GridColumn(1)
                        ,

                    new VStack()
                    {
                        new Image(_imagenEstado)
                            .HeightRequest(40)
                            .HCenter()
                            .VCenter()
                            ,


                        new Label(_mensaje.estado)
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
            .Stroke(MauiControls.Brush.CadetBlue)
            .StrokeThickness(3)
            .BackgroundColor(Colors.LightSkyBlue)            
            
            ;

    }
}

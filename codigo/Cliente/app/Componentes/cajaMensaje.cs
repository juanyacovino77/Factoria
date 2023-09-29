using Contratos;
using MauiReactor;
using MauiReactor.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.Componentes;

public class cajaMensaje : Component
{
    private Mensaje _mensaje;

    public cajaMensaje Mensaje(Mensaje mensaje) { _mensaje = mensaje; return this; }


    public override VisualNode Render()
    {
        return

            new Border()
            {
                new Grid("*,*,*","100,*")
                {
                    new Image("icono_mensaje.png")

                        .Margin(5,5,0,0)
                        .GridRowSpan(2)
                        .VCenter()
                        ,

                    new Label($"MENSAJE RECIBIDO")
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

                    new Label($"Descripcion: {_mensaje.notaMensaje}")
                        .GridRow(2)
                        .Padding(5)
                        .GridColumn(1)
                }
            }
            
            .Shadow(new Shadow())
            .StrokeCornerRadius(30, 15, 15, 30)
            .Padding(5)
            .Margin(10,10,15,3)
            .Stroke(MauiControls.Brush.CadetBlue)
            .StrokeThickness(3)
            .BackgroundColor(Colors.LightSkyBlue);

    }
}

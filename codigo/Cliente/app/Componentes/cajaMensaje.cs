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
    private string _titulo;
    private string _nombreEmisor;
    private string _descripcion;
    public cajaMensaje Titulo(string titulo) { _titulo = titulo; return this; }
    public cajaMensaje NombreEmisor(string nombre) { _nombreEmisor = nombre; return this; }
    public cajaMensaje Descripcion(string descrp) { _descripcion = descrp; return this; }


    public override VisualNode Render()
    {
        return

            new Border()
            {
                new Grid("*,*,*","*")
                {
                    new Label($"NUEVO MENSAJE RECIBIDO")
                        .TextColor(Colors.Black)
                        .Padding(5)
                        ,

                    new Label($"De: {_nombreEmisor}")
                        .GridRow(1)
                        .Padding(5)
                        ,

                    new Label($"Descripcion: {_descripcion}")
                        .GridRow(2)
                        .Padding(5)
                }
            }
            .Padding(10)
            .Stroke(MauiControls.Brush.Chocolate)
            .StrokeThickness(2)
            .BackgroundColor(Colors.Orange)
            .StrokeShape(new RoundRectangle().CornerRadius(15, 15, 15, 15));

    }
}

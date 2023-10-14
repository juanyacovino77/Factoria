using System.Collections.ObjectModel;

namespace app.Modelos;

/* Modelos (ViewModels) de la UI, independientes de los DTOs del Contrato entre cliente-servidor */



/* Es el usuario dueño de la sesión, tiene sus propios mensajes. */
public class Operario
{
    public Empleado datos { get; set; }
    public ObservableCollection<Mensaje> mensajes { get; set; }
}


/* Es un empleado generico, tiene sus datos esenciales y se usa
 * para referenciar a empleados ajenos al operario */
public class Empleado 
{
    public int idEmpleado { get; set; }
    public string nombreEmpleado { get; set; }
    public int idSector { get; set; }
    public string nombreSector { get; set; }
}

public class Mensaje
{
    public enum Estado
    {
        SinDespachar = 0,
        Despachado = 1,
        Recibido = 2,
        Visto = 3
    }

    public int idMensaje { get; set; }
    public string notaMensaje { get; set; }
    public Estado estadoMensaje { get; set; }

    public Cuerpo cuerpo { get; set; }

    public Empleado emisor { get; set; }
    public Empleado receptor { get; set; }
}

public interface Cuerpo
{
}
public class Notificacion : Cuerpo
{
    public enum Estado
    {
        NoConfirmado = 0,
        Confirmado = 1,
    };

    public Estado estadoActual { get; set; }
    public string? texto { get; set; }
    public string? urlImagen { get; set; }
}
public class Tareas : Cuerpo 
{


}
public class Receta : Cuerpo
{
    public string paso1 { get; set; }
    public string paso2 { get; set; }
}


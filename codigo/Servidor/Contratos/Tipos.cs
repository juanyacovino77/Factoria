using System.Collections.ObjectModel;

namespace Contratos; /* entre el cliente y api */


public class SolicitudIniciarSesion {
    public string idEmpleado { get; set; }
}

public class RespuestaIniciarSesion
{
    public string mensaje { get; set; }
    public bool exito { get; set; }
    public Empleado empleado { get; set; }
}

public class Empleado
{
    public int idEmpleado { get; set; }
    public string nombreEmpleado { get; set; }
    public int idSector { get; set; }
    public string nombreSector { get; set; }
    public ObservableCollection<Mensaje>? mensajes { get; set; }
}


public class Notificacion
{
    public enum Estado
    {
        NoEnviado = 0,
        Enviado = 1,
        Recibido = 2,
        Visto= 3,
    };

    public Estado estadoActual { get; set; }
    public string? texto { get; set; }
    public string? urlImagen { get; set; }
}

public class Tareas { }

public class Receta 
{
    public string paso1 { get; set; }
    public string paso2 { get; set; }
}

public class Mensaje
{
    public int idMensaje { get; set; }
    public string descripcionMensaje { get; set; }
    public int idAsunto { get; set; }
    public int idEstado { get; set; }
    public object cuerpo { get; set; } 
    public Empleado emisor { get; set; }
}


public class SolicitudEnviarMensaje 
{
    public Mensaje mensaje { get; set; }
}

public class RespuestaEnviarMensaje 
{
    public string respuesta { get; set; }
    public bool exito { get; set; }
    public Mensaje? mensaje { get; set; }
}
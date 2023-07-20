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
    public Mensaje[]? mensajes { get; set; }
}


public class Mensaje
{
    public int idMensaje { get; set; }
    public string descripcionMensaje { get; set; }
    public int idAsunto { get; set; }
    public int idEstado { get; set; }
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
using System.Collections.ObjectModel;

namespace Contratos; 

/* 
 * CONTRATOS PARA LA TRANSFERENCIA DE DATOS SERIALIZABLES ENTRE EL CLIENTE Y EL SERVIDOR
 * (solo para eso, ni el servidor ni el cliente tienen que acoplarse a estos contratos internamente)
 */


public class SolicitudIniciarSesion {
    public string idEmpleado { get; set; }
}
public class RespuestaIniciarSesion
{
    public string mensaje { get; set; }
    public bool exito { get; set; }
    public Operario operario { get; set; }
    public Empleado[] conectados { get; set; }
}

public record SolicitudCerrarSesion(string idEmpleado);
public record RespuestaCerrarSesion(string mensaje, bool exito);


public record Operario
{
    public Empleado datos { get; set; }
    public Mensaje[] mensajes { get; set; }
}
public record Empleado
{ 
    public int idEmpleado { get; set; }
    public string nombreEmpleado { get; set; }
    public int idSector { get; set; }
    public string nombreSector { get; set; }
}

public class Notificacion
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
public class Tareas 
{
    public enum Estado
    {
        NoIniciado=0,
        EnPreparacion=1,
        Finalizado=2,
        Rechazado=3,
    }
    public Tarea[] tareas { get; set; }
}
public class Tarea
{
    public enum Estado
    {
        NoRealizada = 0,
        Realizada = 1
    }
    public string instrucciones { get; set; }
    public Estado estadoTarea { get; set; }
}
public class Receta 
{
    public string paso1 { get; set; }
    public string paso2 { get; set; }
}
public class PasoReceta
{
    public string paso { get; set; }
    public string urlImagen { get; set; }
}
public class Mensaje
{
    public enum Estado
    {
        NoDespachado = 0, 
        Despachado = 1, 
        Recibido = 2,  
        Visto = 3 
    }

    public int idMensaje { get; set; }
    public string notaMensaje { get; set; }
    public Estado estado { get; set; }

    public Tareas? tareas { get; set; }
    public Receta? receta { get; set; }
    public Notificacion? notificacion { get; set; }
    public Mensaje? actualizacion { get; set; }

    public Empleado emisor { get; set; }
    public Empleado receptor { get; set; }
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

public class SolicitudActualizarMensaje
{
    // Este es el mensaje dirijido a los receptores de la nueva actualización del mensaje
    public Mensaje mensajeDeActualizacion { get; set; }

    // Este es el mensaje actualizado para pisar el viejo en los receptores
    public Mensaje mensajeActualizado { get; set; }
}
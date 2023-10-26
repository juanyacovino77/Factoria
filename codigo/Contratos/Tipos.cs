
namespace Contratos; 

/* 
 * CONTRATOS PARA LA TRANSFERENCIA DE DATOS SERIALIZABLES ENTRE EL CLIENTE Y EL SERVIDOR
 * (solo para eso, ni el servidor ni el cliente tendrian que acoplarse a estos contratos)
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
    public string? respuesta { get; set; }
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
    public Estado estado { get; set; }
    public Tarea[] tareas { get; set; }


    public int Cantidad()
    {
        return tareas.Length;
    }
    public int Progreso()
    {
        return tareas.Count(t => t.Realizada());
    }

    public bool EnPreparacion()
    {
        return estado is Estado.EnPreparacion;
    }
    public bool EnFinalizado()
    {
        return estado is Estado.Finalizado;
    }

    public Tareas PonerEnPreparacion()
    {
        estado = Estado.EnPreparacion;
        return this;
    }
    public Tareas PonerEnRechazado()
    {
        estado = Estado.Rechazado;
        return this;
    }
    public Tareas PonerEnFinalizado()
    {
        if (tareas.All(t => t.Realizada()))
            estado = Estado.Finalizado;

        return this;
    }
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

    public bool Realizada()
    {
        return estadoTarea is Estado.Realizada;
    }
}
public class Receta 
{
    public PasoReceta[] pasos { get; set; }
}
public class PasoReceta
{
    public string paso { get; set; }
    public string urlImagen { get; set; }
}
public class Proceso
{
    public enum Estado
    {
        NoIniciado = 0,
        EnProceso = 1,
        Completado = 3
    }
    public int id { get; set; }
    public Estado estado { get; set; }
    public Mensaje[] cadena { get; set; }


    public Mensaje IniciarProceso()
    {
        return cadena.First();
    }
    public Mensaje? Procesar(Mensaje msj)
    {
        var i = Array.FindIndex(cadena, m => m.idMensaje == msj.idMensaje);
        var m = cadena[i];
        var ultimo = (i > -1) && i == cadena.Length - 1;

        if (msj.tareas is { estado: Tareas.Estado.Finalizado } 
            || msj.notificacion is { estadoActual: Notificacion.Estado.Confirmado})

            if (ultimo) estado = Estado.Completado;
            else return cadena[i + 1];

        return null;
    }
    public Proceso PonerEnProceso()
    {
        estado = Estado.EnProceso;
        return this;
    }
}
public class Conversacion
{
    public Notificacion[] chats { get; set; }
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
    public Proceso? proceso { get; set; }

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
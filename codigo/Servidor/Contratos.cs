using System;

/* dtos compartidos entre proyecto cliente y servidor
 * son intercambiados entre cliente y api */

public record SolicitudIniciarSesion(string claveEmpleado);

public record RespuestaIniciarSesion(bool exito, object empleado);

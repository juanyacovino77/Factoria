using Microsoft.AspNetCore.SignalR;
using puertos;
using Contratos;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Riok.Mapperly.Abstractions;
using System.Collections.Generic;

namespace API.Hubs.PuertoDeEntrada;


/* Esta clase vendria a ser análoga a un controlador en API REST
 * Con este Hub, se pueden recibir solicitudes del exterior y 
 * ademas se pueden reenviar mensajes a los clientes pero 
 * si o si desde aca adentro del hub.
 * 
 * El flujo normal de operaciones en estos metodos seria:
 * 
 * 1) Mapear objeto recibido a los DTO de la capa Servicios
 * 2) Consumir los servicios de Servicios con esos DTOs.
 * 3) Mapear los DTO de respuesta de Servicios a los DTO de esta capa
 * 3) Segun la respueta de Servicios, reenviar mensajes a otros clientes
 * 
 * 
 */

public record Direccion(string connectionId, Empleado infoEmpleado);

public interface IMensajeriaHub
{
    Task OnConnectedAsync();
    Task<RespuestaIniciarSesion> IniciarSesion(SolicitudIniciarSesion solicitud);
    Task<RespuestaEnviarMensaje> EnviarMensaje(SolicitudEnviarMensaje solicitud);
    Task<RespuestaCerrarSesion> CerrarSesion(SolicitudCerrarSesion solicitud);
    Task OnDisconnectedAsync(Exception? exception);
}

// Hub con logica de negocio.
public class MensajeriaHub : Hub
{
    /* tabla de direcciones compartido por todas las instancias de hubs */
    private static readonly IDictionary<string, Direccion> _direcciones = new ConcurrentDictionary<string, Direccion>();

    private IServicios _servicios { get; set; }

    public MensajeriaHub(IServicios servicios)
    {
        _servicios = servicios;
    }

    public override Task OnConnectedAsync()
    {
        /* Se abre una conexion entre un POC y el HUB:
         *  
         *  Escenarios posibles:
         *  
         *      - Un solo empleado intenta loguearse en el POC:
         *          - Si logra loguearse, la conexion queda abierta
         *          - Si no, se cierra
         *        
         *      - Ya habia empleados logueados pero se cayó la conexión:
         *          - Ya que se perdió la conexion anterior
         *            se trata de recomponer las direcciones de la conexion..
         *            o qué usuarios estaban conectados o el nuevo id de conexion.
         * 
         */

        Console.WriteLine($"se conecto {Context.ConnectionId}");



        return base.OnConnectedAsync();
    }

    public async Task<RespuestaIniciarSesion> IniciarSesion(SolicitudIniciarSesion solicitud)
    {
        var solicitudParaServicio = new IServicios.SolicitudIniciarSesion() { idEmpleado = solicitud.idEmpleado };

        var respuestaDeServicio = _servicios.IniciarSesion(solicitudParaServicio);

        var empleadoOperativo = respuestaDeServicio.empleado.MapearEmpleado();

        // guardamos una copia sin mensajes del empleado en la tabla de direcciones
        var empleadoConectado = empleadoOperativo;

         if (respuestaDeServicio.exito)
        {
            // si el empleado es autenticado guardo empleado logueado en esta direccion
            _direcciones.TryAdd(solicitudParaServicio.idEmpleado, new Direccion(Context.ConnectionId, empleadoConectado));


            // notificamos a todos los clientes de que un nuevo empleado inició sesión
            await Clients.All.SendAsync("RecibirNotificacionEmpleadoConectado", empleadoConectado);
        }


        return new RespuestaIniciarSesion()
        {
            operario=new Operario(),
            mensaje=respuestaDeServicio.mensaje,
            exito= respuestaDeServicio.exito,
            conectados=_direcciones.Values.Select(d => d.infoEmpleado).ToArray()
        };
    }
        
    public async Task<RespuestaEnviarMensaje> EnviarMensaje(SolicitudEnviarMensaje solicitud)
    {
        /*
        var solicitudParaServicio = Mapear<Contratos.SolicitudEnviarMensaje, IServicios.SolicitudEnviarMensaje>(solicitud);

        var respuestaDeServico = _servicios.EnviarMensaje(solicitudParaServicio);

        var respuestaParaCliente = Mapear<IServicios.RespuestaEnviarMensaje, Contratos.RespuestaEnviarMensaje>(respuestaDeServico);
        */

        // logica de reenvio de mensajes a los clientes, segun id, grupo, sector, etc
        var idEmpleado = Convert.ToString(solicitud.mensaje.receptor.idEmpleado);

        var direccionEnvio = _direcciones[idEmpleado].connectionId;

        await this.Clients.Client(direccionEnvio).SendAsync("RecibirNuevoMensaje", solicitud.mensaje);

        return new RespuestaEnviarMensaje() { exito=true, respuesta="enviado", mensaje=null };
    }

    public async Task<RespuestaCerrarSesion> CerrarSesion(SolicitudCerrarSesion solicitud)
    {
        // buscar en direcciones y sacarlo de la conexion
        _direcciones.Remove(solicitud.idEmpleado, out _);

        return new RespuestaCerrarSesion("done", true);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Se cierra una conexion entre un POC y el HUB

        // Logica de saber si se desconectó por que se deslogueron todos los empleados
        // o por que si hubo un fallo en la red y hay que recomponer la conexion

        // puede cerrar la conexion el hub o el POC

        Console.WriteLine($"se desconecto {Context.ConnectionId}", exception);

        return base.OnDisconnectedAsync(exception);
    }
}



// Hub sin logica de negocio.
public class MensajeriaSinLogica : Hub, IMensajeriaHub
{
    /* tabla de direcciones compartido por todas las instancias de hubs */
    private static readonly IDictionary<Empleado, string> _direcciones = new ConcurrentDictionary<Empleado, string>();

    private static readonly List<Operario> operarios = new()
    {
        new Operario()
        {
            datos = new Empleado()
            {
                 idEmpleado = 1,
                 idSector = 2,
                 nombreEmpleado = "juamanuel",
                 nombreSector = "sistemas"
            },
            mensajes = new Mensaje[]
                 {
                     new Mensaje()
                     {
                         notaMensaje = "Este mensaje viene deL HIPER",
                         emisor = new Empleado(){ nombreEmpleado = "MAXIMO", nombreSector="CARNICERIA"},
                         notificacion= new Notificacion()
                             {
                                 estadoActual = Notificacion.Estado.NoConfirmado,
                                 texto = "Nueva lista de tareas requeridas:",
                                 urlImagen = "/imagen/limpia/pisos.png"
                             },
                         idMensaje = 1,
                     },

                     new Mensaje()
                     {
                         notaMensaje = "Este mensaje viene de FANTI",
                         emisor = new Empleado(){ nombreEmpleado = "erni", nombreSector = "COCINA"},
                         receta = new Receta()
                         {
                              pasos = new PasoReceta[]
                              {
                                  new PasoReceta()
                                  {
                                       paso="Conseguir 100g de aceite"
                                  }
                                  ,
                                  new PasoReceta()
                                  {
                                      paso="Mezclar con huevo"
                                  }
                                  ,
                                  new PasoReceta()
                                  {
                                      paso="Hacer 2kg milanes"
                                  }
                              }
                         },
                         idMensaje = 2,
                     },

                     new Mensaje()
                     {
                         notaMensaje = "Este mensaje viene de SUPER",
                         emisor = new Empleado(){ nombreEmpleado = "lula", nombreSector= "gestion"},
                         tareas= new Tareas(){ },
                         idMensaje = 3,
                     },
                 },

        },
        new Operario()
        {
            datos = new Empleado()
            {
                 idEmpleado = 2,
                 idSector = 2,
                 nombreEmpleado = "sebastian",
                 nombreSector = "carniceria"
            },
            mensajes = new Mensaje[]
                 {
                     new Mensaje()
                     {
                         notaMensaje = "Este mensaje viene deL HIPER",
                         emisor = new Empleado(){ nombreEmpleado = "MAXIMO", nombreSector="CARNICERIA"},
                         notificacion= new Notificacion()
                             {
                                 estadoActual = Notificacion.Estado.NoConfirmado,
                                 texto = "Nueva lista de tareas requeridas:",
                                 urlImagen = "/imagen/limpia/pisos.png"
                             },
                         idMensaje = 1,
                     },

                     new Mensaje()
                     {
                         notaMensaje = "Este mensaje viene de FANTI",
                         emisor = new Empleado(){ nombreEmpleado = "erni", nombreSector = "COCINA"},
                         receta = new Receta()
                         {
                              pasos = new PasoReceta[]
                              {
                                  new PasoReceta()
                                  {
                                       paso="Conseguir 100g de aceite"
                                  }
                                  ,
                                  new PasoReceta()
                                  {
                                      paso="Mezclar con huevo"
                                  }
                                  ,
                                  new PasoReceta()
                                  {
                                      paso="Hacer 2kg milanes"
                                  }
                              }
                         },
                         idMensaje = 2,
                     },

                     new Mensaje()
                     {
                         notaMensaje = "Este mensaje viene de SUPER",
                         emisor = new Empleado(){ nombreEmpleado = "lula", nombreSector= "gestion"},
                         idMensaje = 3,
                     },
                 },

        },
        new Operario()
        {
            datos = new Empleado()
            {
                 idEmpleado = 3,
                 idSector = 2,
                 nombreEmpleado = "karla",
                 nombreSector = "cocina"
            },
            mensajes = new Mensaje[]
                 {
                     new Mensaje()
                     {
                         notaMensaje = "Este mensaje viene deL HIPER",
                         emisor = new Empleado(){ nombreEmpleado = "MAXIMO", nombreSector="CARNICERIA"},
                         notificacion= new Notificacion()
                             {
                                 estadoActual = Notificacion.Estado.NoConfirmado,
                                 texto = "Nueva lista de tareas requeridas:",
                                 urlImagen = "/imagen/limpia/pisos.png"
                             },
                         idMensaje = 1,
                     },

                     new Mensaje()
                     {
                         notaMensaje = "Este mensaje viene de FANTI",
                         emisor = new Empleado(){ nombreEmpleado = "erni", nombreSector = "COCINA"},
                         receta = new Receta()
                         {
                              pasos = new PasoReceta[]
                              {
                                  new PasoReceta()
                                  {
                                       paso="Conseguir 100g de aceite"
                                  }
                                  ,
                                  new PasoReceta()
                                  {
                                      paso="Mezclar con huevo"
                                  }
                                  ,
                                  new PasoReceta()
                                  {
                                      paso="Hacer 2kg milanes"
                                  }
                              }
                         },
                         idMensaje = 2,
                     },

                     new Mensaje()
                     {
                         notaMensaje = "Este mensaje viene de SUPER",
                         emisor = new Empleado(){ nombreEmpleado = "lula", nombreSector= "gestion"},
                         idMensaje = 3,
                     },
                 },

        }
    };

    public override Task OnConnectedAsync()
    {
        /* Se abre una conexion entre un POC y el HUB:
         *  
         *  Escenarios posibles:
         *  
         *      - Un solo empleado intenta loguearse en el POC:
         *          - Si logra loguearse, la conexion queda abierta
         *          - Si no, se cierra
         *        
         *      - Ya habia empleados logueados pero se cayó la conexión:
         *          - Ya que se perdió la conexion anterior
         *            se trata de recomponer las direcciones de la conexion..
         *            o qué usuarios estaban conectados o el nuevo id de conexion.
         * 
         */

        Console.WriteLine($"se conecto {Context.ConnectionId}");



        return base.OnConnectedAsync();
    }

    public async Task<RespuestaIniciarSesion> IniciarSesion(SolicitudIniciarSesion solicitud)
    {
        var resultado = operarios.Find(e => e.datos.idEmpleado == Convert.ToInt16(solicitud.idEmpleado));
        var operario = resultado is null ? new Operario() : resultado;

        var empleadoConectado = operario.datos;

        // si el empleado es autenticado guardo empleado logueado en esta direccion
        if (resultado is not null) _direcciones.TryAdd(empleadoConectado, Context.ConnectionId);

        // notificamos a todos los clientes de que un nuevo empleado inició sesión
        await Clients.All.SendAsync("RecibirNotificacionEmpleadoConectado", empleadoConectado);
        Console.WriteLine($"{empleadoConectado.nombreEmpleado} inició sesión en POC {Context.ConnectionId}");

        return new RespuestaIniciarSesion()
        {
            operario = operario,
            mensaje = resultado is null ? "error" : "exito",
            exito = resultado is not null,
            conectados = _direcciones.Keys.ToArray()
        };
    }

    public async Task<RespuestaEnviarMensaje> EnviarMensaje(SolicitudEnviarMensaje solicitud)
    {
        if (solicitud.mensaje is { emisor: null, receptor:null }) return new RespuestaEnviarMensaje() { exito = false, respuesta= "Mensaje incompleto"};

        // logica de reenvio de mensajes a los clientes, segun id, grupo, sector, etc
        var idEmpleado = solicitud.mensaje.receptor.idEmpleado;
        var clave = _direcciones.Keys.ToList().Find(e => e.idEmpleado == idEmpleado);

        if (clave is null) return new RespuestaEnviarMensaje() { exito = false, respuesta = "El receptor no está activo" };

        var direccionEnvio = _direcciones[clave];
        Console.WriteLine($"{solicitud.mensaje.emisor.nombreEmpleado} le envio un msj a {solicitud.mensaje.receptor.nombreEmpleado} con cuerpo {solicitud.mensaje}");

        await this.Clients.Client(direccionEnvio).SendAsync("RecibirNuevoMensaje", solicitud.mensaje);

        return new RespuestaEnviarMensaje() { exito = true, respuesta = "enviado", mensaje = null };
    }

    public async Task<RespuestaCerrarSesion> CerrarSesion(SolicitudCerrarSesion solicitud)
    {
        // buscar en direcciones y sacarlo de la conexion
        var clave = _direcciones.Keys.ToList().Find(e => e.idEmpleado == Convert.ToInt16(solicitud.idEmpleado));

        if (clave is null) return new RespuestaCerrarSesion("error", false);

        Console.WriteLine($"{clave.nombreEmpleado} cerró sesión en POC {Context.ConnectionId}");
        _direcciones.Remove(clave, out _);

        // notificar a los clientes que se desconectó un empleado
        await Clients.All.SendAsync("RecibirNotificacionEmpleadoDesconectado", clave);

        return new RespuestaCerrarSesion("done", true);
    }

    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        // Se cierra una conexion entre un POC y el HUB

        // Logica de saber si se desconectó por que se deslogueron todos los empleados
        // o por que si hubo un fallo en la red y hay que recomponer la conexion

        // puede cerrar la conexion el hub o el POC
        var clave = _direcciones.ToList().Find(i => i.Value == Context.ConnectionId).Key;
        if (clave is null) return;
        _direcciones.Remove(clave);

        // notificar a los clientes que se desconectó un empleado
        await Clients.All.SendAsync("RecibirNotificacionEmpleadoDesconectado", clave);

        Console.WriteLine($"se desconecto {Context.ConnectionId}, {exception}");

        await base.OnDisconnectedAsync(exception);
    }
}

[Mapper]
public static partial class Mapeador
{
    public static partial Contratos.Empleado MapearEmpleado(this IServicios.EmpleadoDTO e);
}

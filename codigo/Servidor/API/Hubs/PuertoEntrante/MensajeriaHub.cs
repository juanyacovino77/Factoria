using Microsoft.AspNetCore.SignalR;
using puertos;
using Contratos;
using Nelibur.ObjectMapper;

namespace API.Hubs.PuertoDeEntrada;


/* Esta clase vendria a ser análoga a un controlador en API REST
 * Con este Hub, se pueden recibir solicitudes del exterior y 
 * ademas se pueden reenviar mensajes a los clientes pero 
 * si o si desde aca adentro del hub.
 * 
 * el flujo normal de operaciones en estos metodos seria:
 * 
 * 1) Mapear objeto recibido a los DTO de la capa Servicios
 * 2) Consumir los servicios de Servicios con esos DTOs.
 * 3) Mapear los DTO de respuesta de Servicios a los DTO de esta capa
 * 3) Segun la respueta de Servicios, reenviar mensajes a otros clientes
 * 
 * 
 */

public class MensajeriaHub : Hub
{
    private IServicios _servicios { get; set; }

    public MensajeriaHub()
    {
        _servicios = new Servicios();
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"se conecto {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"se desconecto {Context.ConnectionId}", exception);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task<RespuestaIniciarSesion> IniciarSesion(SolicitudIniciarSesion solicitud)
    {
        
        var solicitudParaServicio = Mapear<Contratos.SolicitudIniciarSesion, IServicios.SolicitudIniciarSesion>(solicitud);

        var respuestaDeServicio = _servicios.IniciarSesion(solicitudParaServicio);

        var respuestaParaCliente = Mapear<IServicios.RespuestaIniciarSesion, Contratos.RespuestaIniciarSesion>(respuestaDeServicio);
        

        await Clients.All.SendAsync("RecibirNotificacionEmpleadoConectado", respuestaParaCliente);

        return respuestaParaCliente;
    }

    public async Task<RespuestaEnviarMensaje> EnviarMensaje(SolicitudEnviarMensaje solicitud)
    {
        /*
        var solicitudParaServicio = Mapear<Contratos.SolicitudEnviarMensaje, IServicios.SolicitudEnviarMensaje>(solicitud);

        var respuestaDeServico = _servicios.EnviarMensaje(solicitudParaServicio);

        var respuestaParaCliente = Mapear<IServicios.RespuestaEnviarMensaje, Contratos.RespuestaEnviarMensaje>(respuestaDeServico);
        */

        // logica de reenvio de mensajes a los clientes, segun id, grupo, sector, etc
        

        await Clients.All.SendAsync("RecibirNuevoMensaje", solicitud.mensaje);

        return new RespuestaEnviarMensaje() { exito=true, respuesta="enviado", mensaje=null };
    }

    public static TSalida Mapear<TEntrada, TSalida>(TEntrada entrada)
    {
        TinyMapper.Bind<TEntrada, TSalida>();

        return TinyMapper.Map<TSalida>(entrada);
    }


}

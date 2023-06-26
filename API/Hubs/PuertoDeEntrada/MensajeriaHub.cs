using Microsoft.AspNetCore.SignalR;
using puertos;

namespace API.Hubs.PuertoDeEntrada;


/* Esta clase vendria a ser análoga a un controlador en API REST
 * Con este Hub, se pueden recibir solicitudes del exterior y 
 * ademas se pueden reenviar mensajes a todos los clientes pero 
 * si o si desde aca adentro del hub.
 * 
 * el flujo normal de operaciones en estos metodos seria:
 * 
 * 1) Mapear objeto recibido a los DTO de la capa Servicios
 * 2) Consumir los servicios de Servicios con esos DTOs.
 * 3) Mapear los DTO de Servicios a los DTO de esta capa
 * 3) Segun la respueta de Servicios, reenviar mensajes a otros clientes
 * 
 * 
 */

public class MensajeriaHub : Hub
{
    public Servicios servicios { get; set; }

    public MensajeriaHub()
    {

    }

    public async Task IniciarSesion(dynamic solicitud)
    {
        // mapear DTO entrante a DTO siguiente (hub -> Servicios)
        

    }

    public async Task EnviarMensaje(dynamic solicitud)
    {
        // mapear DTO de esta capa a los de Servicios
        var solicitudDTO = new SolicitudEnviarMensaje();

        // mandar el DTO de servicios al Servicio
        var respuesta = servicios.EnviarMensaje(solicitudDTO);

        // segun la respuesta, le puedo mandar mensajes a otros clientes
        // dominio, ya respondió, supero validaciones, etc etc
        // ahora el hub puede reenviar ese mensaje que viene en respueta

        await Clients.All.SendAsync("RecibirNuevoMensaje", respuesta); 

        // y le devuelvo la respuesta al cliente

    }
}

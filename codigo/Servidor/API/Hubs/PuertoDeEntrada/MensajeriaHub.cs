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
    private IServicios _servicios { get; set; }

    public MensajeriaHub()
    {
        _servicios = new Servicios();
    }

    public async Task<IServicios.RespuestaAccionarMensaje> AccionarMensaje(dynamic solicitud)
    {
        var respuesta = this._servicios.AccionarMensaje(solicitud);

    }






}

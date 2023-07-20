using API.Hubs.PuertoDeEntrada;
using Microsoft.AspNetCore.SignalR;


namespace API.Hubs.PuertoDeSalida
{
    /* Esta clase podria escuchar eventos de Servicios o inyectarse en Servicos
     * seria para atender a los eventos del dominio. Como es que el dominio genera eventos
     * por si solo? Por ejemplo si modelamos la hora del dia, y a las 12 de la noche queremos
     * mandar notificaciones, no hay ningun actor ejectutando eso, ahi es el Dominio que por si 
     * solo tomaria control y empujaria la información para afuera 
     */

    public class Despachante
    {
        private readonly IHubContext<MensajeriaHub> _hubContext;

        public Despachante(IHubContext<MensajeriaHub> hubContext)
        {
            _hubContext = hubContext;
        }



        public void DespacharMensajeAClientes(dynamic solicitud)
        {
            //_hubContext.Clients.All.SendAsync("RecibirNuevoMensaje", solicitud);
        }
    }
}

using dominio;
namespace puertos;


/* Esta capa es la interfaz o API o fachada del DOMINIO,
 * es la capa que coordina capas externas y responde
 * por el Dominio, usando los servicios del dominio.
 * Algunos la llaman Servicios, Aplicacion, ApplicationFacade segun Fowler, etc
 *
 * 
 * En terminos generales son los CASO DE USO a nivel de
 * Aplicacion, esto implica coordinar las transacciones de
 * logica de presentacion, infraestructura y dominio.
 * 
 * Aca se mapean tipos externos a internos para que los use el Dominio
 * por ejemplo desde una API Web viene el tipo EnviarMensajeRequest -> Mensaje (o primitivos)
 * se lo mando a Dominio, obtengo una respuesta del tipo Mensaje convierto ese
 * tipo a EnviarMensajeResponse y se lo devuelvo a la API Web
 * 
 * Aca es donde se manipulan los objetos del dominio y se los pone a jugar????
 * algo asi como el titiritero que mueve a los titeres?? Podria ser, esta capa segun
 * algunos autores puede ser mas fina o mas ancha, pero si metemos logica de negocio(dominio) 
 * aca hay que coordinarla con logica de infraestructura(bd) y otros servicios y con mapeos etc
 * por eso prefiero dejar TODA la logica de negocio en Dominio.
 * 
 * Aca se guardan los cambios en la base de datos y se recuperan cuando sea necesario
 * por ejemplo invocar un ORM como entity framework a traves de su interfaces
 * 
 * Aca se usan los servicios externos de infraestructura cuando sea necesario
 * por ejemplo invocar a signalR para que invoque una funcion en un cliente
 * usando las respectivas interfaces
 * 
 * Tambien esta clase iniciaria la del dominio, cargandole los datos de los 
 * objetos obtenidos de la BD a traves de los repositorios
 * 
 * Gracias a esta nueva capa de abstraccion puedo engancharle cualquier tipo de 
 * api, api rest, soap, grpc, graphql, winform, controllers sueltos, etcs
 * 
 * Aca la duda es el tema de la Autorización y la Autenticación de los usuarios.
 * De los roles, de quienes pueden disparar estos Casos de Uso.. la autenticacion
 * es a nivel de logica de infraestructura? a nivel de dominio??? Tambien el tema
 * de multiples sesiones en un POC.. complicados
 * 
 * 
*/


public class Servicios
{ // esto tendria que ser un singleton
    private IServiciosOfrecidosPorDominio _dominio { get; set; } // segregar esta interfaz


    public Servicios()
    {
        _dominio = new Puerto(); // interior
    }   

    public RespuestaCrearEmpleado CrearEmpleado(SolicitudCrearEmpleado solicitud)
    {
        // mapear dto exterior con dto interior
        // y mandar el dto de dominio 

        var respuesta = _dominio.SolicitudCrearEmpleado(solicitud, solicitud);

        /*
         
        // si es necesario, si supera la validaciones, usar ORM
        _repositorio.SaveContext(empleado); 

        // si es necesario, usar SMS chat o WhatsApp
        _comunicaciones.NotificarPorSMS("Nuevo empleado registrado");

        // si es necesario, usar logger
        _logger.LogearEstado("/app/empleados/log.txt", "Un nuevo empleado fue creado por { } ");

        // si es necesario, usar signalR o websockets
        _signalR.EnviarSeñal(); 
        
         */

        return new RespuestaCrearEmpleado(); // construir este objeto de devolucion
    }

    public RespuestaEnviarMensaje EnviarMensaje(SolicitudEnviarMensaje solicitud)
    {
        // mapear dto de Servicios con la de Dominio

        // consumir los servicios del Dominio
        var respuesta = _dominio.SolicitudEnviarMensaje(solicitud);

        // respuesta tendria el mensaje que tendria que retornar a Hub
        // para que desde ahí se reenvia a otros clientes

        /* Llamadas a los periféricos o accesorios de infraestructura
         * segun la respueta del Dominio
        
        // si es necesario, si supera la validaciones, usar ORM
        _repositorio.SaveContext(respuesta);

        // si es necesario, usar SMS chat o WhatsApp
        _comunicaciones.NotificarPorSMS("Nuevo mensaje registrado");

        // si es necesario, usar logger
        _logger.LogearEstado("/app/mensajes/log.txt", "Un nuevo mensaje fue creado por { } ");

        */

        return new RespuestaEnviarMensaje();
    }

}



public record RespuestaCrearEmpleado();
public record SolicitudCrearEmpleado();
public record RespuestaEnviarMensaje();
public record SolicitudEnviarMensaje();

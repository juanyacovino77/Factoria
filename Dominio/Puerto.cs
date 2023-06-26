namespace dominio
{
    public class Puerto : IServiciosOfrecidosPorDominio
    {

        /* Clase que comunica la capa de Servicios con los objetos de 
         * la logica del domino, intercede en el limite
         * entre el exterior y el interior, es un Puerto de Entrada.
         * 
         * Clase CONSOLA que manipula la mancomuna de objetos
         * que a su vez media entre el exterior (capa servicios) y el interior
         */

        private Negocio negocio { get; set; }

        public dynamic SolicitudCrearEmpleado(dynamic actor, dynamic solicitud)
        {
            var trabajador = negocio.ObtenerEmpleadoPorId(actor.id) as Administrador;
            var sector = negocio.ObtenerSectorPorId(solicitud.idSector) as Sector;

            // Autenticar al empleado. ¿Existe el empleado?
            var administrador = trabajador is not null
                ? trabajador as Administrador
                : throw new Exception("No sos empleado");

            // Autorizar al empleado. ¿Tiene autorizacion el empleado?
            var respuesta = administrador is not null
                ? administrador.CrearEmpleado(solicitud.id, solicitud.nombre, sector) 
                : throw new Exception("No sos administrador");

            this.negocio.AñadirEmpleado(respuesta);

            return new { msj = respuesta };
        }

        public dynamic SolicitudModificarEmpleado(dynamic actor, dynamic solicitud)
        {
            // depende de lo que quieras modificar. Si el empleado quiere
            // modificar su informacion personal? lo puede hacer...
            // si quiere modificar info sensible como el sector? no puede.. (solo admin)
            // o solo puede modificar cualquier info del empleado un admin

            var empleado = negocio.ObtenerEmpleadoPorId(actor.id) as Administrador;
            var empleado_a_modificar = negocio.ObtenerEmpleadoPorId(solicitud.id) as Trabajador;

            // Autenticar al empleado. ¿Existe el empleado?
            var administrador = empleado is not null
                ? empleado as Administrador
                : throw new Exception("No existe el empleado");

            // Autorizar al empleado. ¿Tiene autorizacion el empleado?
            var respuesta = administrador is not null
                ? new { empleado = administrador.ModificarEmpleado(solicitud) }
                : throw new Exception("No sos administrador");


            this.negocio.Guardar(empleado);

            return respuesta;
        }

        public dynamic SolicitudEnviarMensaje(dynamic solicitud)
        {
            var actor = solicitud.actor;

            var empleado = negocio.ObtenerEmpleadoPorId(actor.id) as Trabajador;
            var destinatarios = negocio.ObtenerEmpleadosPorId(solicitud.destinatarios) as List<Trabajador>;
            var actuadores = negocio.ObtenerEmpleadosPorId(solicitud.actuadores);
            
            // Autenticar al actor. ¿Es un empleado?
            empleado = empleado is not null
                ? actor as Administrador
                : throw new Exception("No sos empleado");

            // Autorizar al empleado. ¿Tiene autorizacion el empleado?

            var mensaje = empleado.CrearMensaje(destinatarios, actuadores, solicitud.tipoId);

            // guardar mensaje enviado

            // logica de envio de mensajes, lo puede hacer un empleado
            foreach (var destinatario in destinatarios)
            {
                destinatario.RecibirMensajeDelMensajero(mensaje);
                //negocio.Guardar(destinatario);
            }

            // esta logica tendria que retornar el mensaje a enviar
            // para que el hub lo reenvie en el retorno a este metodo

            return mensaje;
        }

        public dynamic SolicitudAccionarMensaje(dynamic actor, dynamic solicitud)
        {
            var empleado = negocio.ObtenerEmpleadoPorId(actor.id) as Trabajador;

            if (empleado is null) throw new Exception("No sos empleado");

            var mensaje_de_respuesta = empleado.AccionarMensaje(solicitud.idMensaje, solicitud.idNuevoEstado);

            foreach (var destinatario in mensaje_de_respuesta.destinatarios)
            {
                destinatario.RecibirMensajeDelMensajero(mensaje_de_respuesta);
                negocio.Guardar(destinatario);
            }

            return mensaje_de_respuesta;

        }

        public dynamic SolicitudIniciarProcesoProductivo(dynamic actor, dynamic solicitud)
        {
            var empleado = negocio.ObtenerEmpleadoPorId(actor.id) as Administrador;

            if (empleado is null) throw new Exception("No sos empleado");
            if (empleado is not Administrador) throw new Exception("No sos administrador");

            var mensaje = empleado.IniciarProcesoProductivo(solicitud.idpp);

            foreach (var destinatario in mensaje.destinatarios)
            {
                destinatario.RecibirMensajeDelMensajero(mensaje);
                negocio.Guardar(destinatario);
            }

            return mensaje;
        }
    }

    public interface IServiciosOfrecidosPorDominio
    {
        dynamic SolicitudCrearEmpleado(dynamic actor, dynamic solicitud);
        dynamic SolicitudModificarEmpleado(dynamic actor, dynamic solicitud);
        dynamic SolicitudEnviarMensaje(dynamic solicitud);
    }

}


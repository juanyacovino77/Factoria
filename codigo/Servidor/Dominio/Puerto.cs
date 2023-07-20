using static dominio.IDominio;

namespace dominio
{

    public class Puerto : IDominio
    {

        /* Clase que comunica la capa de Servicios con los objetos de 
         * la logica del domino, intercede en el limite
         * entre el exterior y el interior, es un Puerto de Entrada.
         * 
         * Clase CONSOLA que manipula la mancomuna de objetos
         * que a su vez media entre el exterior (capa servicios) y el interior
         */

        private Negocio _negocio { get; set; }
        public Puerto() 
        {
            _negocio = new Negocio();
        }


        public RespuestaIniciarSesion IniciarSesion(string claveEmpleado)
        {
            //Descomponer y usar IDominio.SolicitudIniciarSesion
            //Recomponer y devolver IDominio.RespuestaIniciarSesion

            // existe el empleado?
            var actor = _negocio.ObtenerEmpleadoPorId(claveEmpleado);

            /*
            var empleadosActivos = _negocio.ObtenerEmpleadosActivos();

            // si existe, le devolve su informacion y otras cosas 
            // como inforamcion de los empleados activos en otros sectores
            return actor is null
                ? new { auth = "No sos empleado" }
                : new { auth = "Correcto", empleado = actor, activos = empleadosActivos };
            */

            return new RespuestaIniciarSesion(actor);
        }
        public dynamic AbrirTurno(dynamic solicitud)
        {
            var actor = _negocio.ObtenerEmpleadoPorId(solicitud.actor.id);

            var respuesta = actor.AbrirTurno();

            /* esto tendria que devolver, un objeto que contenga info de,
             * si existe como empleado, qué tipo de empleado es, y 
             * toda la info y permisos que necesita la app cliente
             * para que pueda trabajar, y para que el fronton pueda
             * renderizar esta info. por ejemplo, permisos, mensajes,
             * etc etc
             */
            return respuesta;
        }
        public dynamic CerrarTurno(dynamic solicitud)
        {
            var actor = this._negocio.ObtenerEmpleadoPorId(solicitud.actor.id) as Trabajador;

            return actor.CerrarTurno();
        }
        public dynamic CrearEmpleado(dynamic solicitud)
        {
            var trabajador = _negocio.ObtenerEmpleadoPorId(solicitud.actor.id) as Administrador;
            var sector = _negocio.ObtenerSectorPorId(solicitud.idSector) as Sector;

            // Autenticar al empleado. ¿Existe el empleado?
            var administrador = trabajador is not null
                ? trabajador as Administrador
                : throw new Exception("No sos empleado");

            // Autorizar al empleado. ¿Tiene autorizacion el empleado?
            var respuesta = administrador is not null
                ? administrador.CrearEmpleado(solicitud.id, solicitud.nombre, sector) 
                : throw new Exception("No sos administrador");

            this._negocio.AñadirEmpleado(respuesta);

            return new { msj = respuesta };
        }
        public dynamic ModificarEmpleado(dynamic solicitud)
        {
            // depende de lo que quieras modificar. Si el empleado quiere
            // modificar su informacion personal? lo puede hacer...
            // si quiere modificar info sensible como el sector? no puede.. (solo admin)
            // o solo puede modificar cualquier info del empleado un admin

            var empleado = _negocio.ObtenerEmpleadoPorId(solicitud.id) as Administrador;
            var empleado_a_modificar = _negocio.ObtenerEmpleadoPorId(solicitud.id) as Trabajador;

            // Autenticar al empleado. ¿Existe el empleado?
            var administrador = empleado is not null
                ? empleado as Administrador
                : throw new Exception("No existe el empleado");

            // Autorizar al empleado. ¿Tiene autorizacion el empleado?
            var respuesta = administrador is not null
                ? new { empleado = administrador.ModificarEmpleado(solicitud) }
                : throw new Exception("No sos administrador");


            this._negocio.Guardar(empleado);

            return respuesta;
        }
        public dynamic EnviarMensaje(dynamic solicitud)
        {
            var actor = solicitud.actor;

            var empleado = _negocio.ObtenerEmpleadoPorId(actor.id) as Trabajador;
            var destinatarios = _negocio.ObtenerEmpleadosPorId(solicitud.destinatarios) as List<Trabajador>;
            var actuadores = _negocio.ObtenerEmpleadosPorId(solicitud.actuadores);
            
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
        public dynamic AccionarMensaje(dynamic solicitud)
        {
            var empleado = _negocio.ObtenerEmpleadoPorId(solicitud.actor.id) as Trabajador;

            if (empleado is null) throw new Exception("No sos empleado");

            var mensaje_de_respuesta = empleado.AccionarMensaje(solicitud.idMensaje, solicitud.idNuevoEstado);


            // esto tiene sentido a nivel UI, son notificaciones
            // para resfrescar la UI y hacerle enteerar a la app cliente
            // que cambió el estado de un Mensaje.Dentro del dominio, solo
            // se le cambiaria el estado a la instancia del Mensaje y nada mas
            // porque despues se persiste eso y cuando otro empleado levanta ese
            // mensaje, ya tendria que tener la actualziacion de estado teoricamente.

            // y en el caso de proceso productivos???

            foreach (var destinatario in mensaje_de_respuesta.destinatarios)
            {
                destinatario.RecibirMensajeDelMensajero(mensaje_de_respuesta);
                _negocio.Guardar(destinatario);
            }

            return mensaje_de_respuesta;

        }
        public dynamic DesencadenarProcesoProductivo(dynamic solicitud)
        {
            var empleado = _negocio.ObtenerEmpleadoPorId(solicitud.actor.id) as Administrador;

            if (empleado is null) throw new Exception("No sos empleado");
            if (empleado is not Administrador) throw new Exception("No sos administrador");

            var mensaje = empleado.IniciarProcesoProductivo(solicitud.idpp);



            return mensaje;
        }




    }

    public interface IDominio
    {
        RespuestaIniciarSesion IniciarSesion(string claveEmpleado);
        dynamic AbrirTurno(dynamic solicitud);
        dynamic CerrarTurno(dynamic solicitud);
        dynamic CrearEmpleado(dynamic solicitud);
        dynamic ModificarEmpleado(dynamic solicitud);
        dynamic EnviarMensaje(dynamic solicitud);
        dynamic AccionarMensaje(dynamic solicitud);


        public record RespuestaIniciarSesion(Trabajador? empleado);

    }

}


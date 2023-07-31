
namespace dominio
{
    public class Trabajador
    {
        public Trabajador(string dni, string nombre, Sector sector) 
        {
            this.dni = dni;
            this.nombre = nombre;
            this.seccion = sector;
            this.enTurno = false;
            this.turnos = new List<Turno> { new Turno() };
        }

        public string nombre { get; set; }
        public string dni { get; set; }
        protected bool enTurno { get; set; }
        public Sector seccion { get; set; }
        public List<Turno> turnos { get; set; }
        
        public List<Mensaje> ObtenerMensajes()
        {
            return this.turnos.First().mensajesRecibidos;
        }
        public virtual Mensaje EnviarMensajeAlMensajero(Mensaje msj)
        {
            //var mensaje_de_vuelta = negocio.RecibirMensajeDeEmpleado(msj);

            this.turnos[0].GuardarMensajes(msj);

            return msj;
        }
        public virtual Mensaje RecibirMensajeDelMensajero(Mensaje msj)
        {
            this.turnos[0].GuardarMensajes(msj);

            /* disparar eventos segun el tipo de mensaje recibido 
             * por ejemplo, si llega una notificacion, tendria que disparar
             * una signalR para que el cliente consulte la API y se obtenga
             * las novedades
             * otro ejemplos, si llega una notificacion de actualizacion de mensaje
             * tengo que disparar un signalR de que un cuerpo se actualizó
             */

            return new Mensaje { cuerpo = new Notificacion { estado = "recibido" } };
        }

        public Mensaje AccionarMensaje(int idMensaje, int idNuevoEstado)
        {
            var msj = this.turnos[0].mensajesRecibidos.Find(m => m.codigo == idMensaje);

            if (msj.ConcederAutorizacion(this))
            {
                // Es uno de los actuadores, puede accionar el mensaje.. cambiarle el estado.
                msj.codigo = idNuevoEstado;
                msj.cuerpo.establecer_estado("algo cambio");


                // Actualizar el estado del mensaje para los otros clientes?? quienes son los clientes? 
                // En vez de poner muchos etiquetados en un mensaje..
                // hacer un mensaje para cada etiquetado

                //Ahora que actualizó el mensaje, debe avisarle a todos los 
                //interesados en que cambió el mensaje
          
                //this.EnviarMensajeAlMensajero(nuevo_msj);

            }

            return this.CrearMensaje(msj.receptores, msj.actuadores, 1);

        }

        public Mensaje CrearMensaje(List<Trabajador> destinatarios, List<Trabajador> actuadores, int tipoCuerpo)
        {
            var cuerpo = this.CrearAsunto(tipoCuerpo);

            return new Mensaje { 
                actuadores = actuadores,
                codigo = 1,
                cuerpo = cuerpo,
                emisor = this,
                receptores = destinatarios 
            };
        }

        public IEnviable CrearAsunto(int tipoCuerpo)
        {
            throw new NotImplementedException();
        }

        public Mensaje AbrirTurno()
        {
            Turno turno = new();
            this.turnos.Add(turno);
            this.enTurno = true;

            return new Mensaje { };
        }

        public Mensaje CerrarTurno()
        {
            var turno_actual = this.turnos.Last();
            turno_actual.CerrarTurno();
            this.enTurno = false;

            return new Mensaje { };
        }
    }

    public class Administrador : Trabajador
    {
        private List<ProcesoProductivo> procesosProductivos { get; set; }


        public Administrador(string dni, string nombre, Sector sector) 
            : base(dni, nombre, sector)
        {
            
            
        }

        public Mensaje IniciarProcesoProductivo(int idpp)
        {
            var mensaje = this.procesosProductivos.Find(pp => pp.codigo == idpp).IniciarProcesoProductivo();
            var destinatarios = mensaje.receptores;
            destinatarios[0].RecibirMensajeDelMensajero(mensaje);

            return mensaje;
        }

        public void AsociarProcesoProductivo(List<Mensaje> mensajes)
        {
            // la responsabilidad de crear un PP es de negocio
        }

        public Trabajador CrearEmpleado(int id, string nombre, Sector sector)
        {

            var nuevo_empleado = sector.descripcion == "CARNICERIA"
                ? new Trabajador(nombre, nombre, sector)
                : new Administrador(nombre, nombre, sector);

            return nuevo_empleado;
        }

        public Trabajador ModificarEmpleado(Trabajador empleado_a_modificar)
        {
            return empleado_a_modificar;
        }
    }

}
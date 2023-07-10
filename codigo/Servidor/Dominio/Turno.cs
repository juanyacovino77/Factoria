namespace dominio
{
    public class Turno
    {
        public int codigo { get; set; }
        public DateTime horaInicio { get; set; }
        public DateTime horaFin { get; set; }
        public List<Mensaje> mensajesRecibidos { get; set; } = new List<Mensaje>();
        public List<Mensaje> mensajesEnviados { get; set; } = new List<Mensaje>();

        public bool GuardarMensajes(Mensaje msj)
        {
            this.mensajesRecibidos.Add(msj);

            return true;
        }

        public Mensaje ObtenerUltimoMensajeRecibido()
        {
            return this.mensajesRecibidos.Last();
        }
        public void CerrarTurno()
        {
            this.horaFin=DateTime.Now; 
        }
    }
}
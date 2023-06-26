namespace dominio
{
    public class Mensaje
    {
        public Mensaje()
        {
        }

        public int codigo { get; set; }
        public Trabajador emisor { get; set; }
        public List<Trabajador> receptores { get; set; }
        public List<Trabajador> actuadores { get; set; }
        public IEnviable cuerpo { get; set; }

        public bool ConcederAutorizacion(Trabajador empleado)
        {
            return actuadores.Any(e => e == empleado);
        }
    }
}
namespace dominio
{
    public class Mensaje
    {
        public Mensaje()
        {
        }

        public int codigo { get; set; }
        public Trabajador emisor { get; set; } = new Trabajador();
        public List<Trabajador> receptores { get; set; } = new List<Trabajador>();
        public List<Trabajador> actuadores { get; set; } = new List<Trabajador>();  
        public IEnviable cuerpo { get; set; }

        public bool ConcederAutorizacion(Trabajador empleado)
        {
            return actuadores.Any(e => e == empleado);
        }
    }
}
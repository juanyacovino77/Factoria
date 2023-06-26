namespace dominio
{
    public class ProcesoProductivo
    {
        public int codigo { get; set; }
        public string descripcion { get; set; }
        public Queue<Mensaje> programacion { get; set; }
        public DateTime inicio { get; set; }

        public Mensaje IniciarProcesoProductivo() 
        {
            // logica de inicio
            // saca el primer mensaje y lo envia.
            throw new NotImplementedException();

        }
    }
}

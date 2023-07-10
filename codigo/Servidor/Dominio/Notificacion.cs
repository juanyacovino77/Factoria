namespace dominio
{
    public class Notificacion : IEnviable
    {
        public string estado { get; set; }

        public string establecer_estado(string estado)
        {
            return this.estado = estado;
        }

        public string obtener_estado()
        {
            throw new NotImplementedException();
        }
    }
}
namespace dominio
{
    public interface IEnviable

    {
        string obtener_estado();
        string establecer_estado(string nuevoEstado);
    }
}
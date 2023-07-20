namespace dominio
{
    public class Negocio
    {

        /* Esta clase nos abstrae del disco presentando los objetos del dominio
         * como si fuesen una colección de objetos que están en memoria 
         * pero por atras carga, descarga y guarda objetos en el disco usando un ORM o 
         * cualquier otro mecanismo que al dominio no le interesa conocer.
         * 
         * Esta clase vendria a ser un Puerto de Salida que pasa el control
         * hacia afuera del dominiomi mientras Puerto seria un Puerto de Entrada
         * 
         * Para lograr esto hay que comunicarse con otras capas, esta capa
         * se encargaria de encapsular esa responsabilidad, es una Facade, podria usar
         * eventos que escuchen otras capas, o inyectandole un 
         * servicio de otra capa por constructor o usando MediatR
         * 
         */
        public List<Trabajador> empleados { get; set; }

        public Negocio()
        {
            // especificar empleados
            empleados = new List<Trabajador>
            {
                new Trabajador("1", "juan", new Sector()),
                new Trabajador("2", "eugenia", new Sector()),
                new Trabajador("3", "octavio", new Sector())
            };
        }

        #region IMPLEMENTACION DEL CONTRATO QUE CONSUMEN OBJETOS DEL DOMINIO
        public Trabajador ObtenerEmpleadoPorId(string id)
        {
            return this.empleados.Find(e => e.dni == id);

        }
        
        public List<Trabajador> ObtenerEmpleadosPorId(List<int> ids)
        {
            throw new NotImplementedException();

        }

        public List<Trabajador> ObtenerEmpleadosActivos()
        {
            // logica de obtener empleados filtrando por EnTurno = true;
            throw new NotImplementedException();
        }

        public Sector ObtenerSectorPorId(int id)
        {
            throw new NotImplementedException();

        }

        public Mensaje AñadirEmpleado(Trabajador empleado)
        {
            throw new NotImplementedException();
        }

        public Mensaje AñadirSector(Sector sector)
        {
            throw new NotImplementedException();

        }

        public Mensaje RecibirMensajeDeEmpleado(Mensaje msj)
        {
            throw new NotImplementedException();


        }

        public void Guardar(Trabajador trabajador)
        {
            throw new NotImplementedException();


        }
        #endregion

    }


}

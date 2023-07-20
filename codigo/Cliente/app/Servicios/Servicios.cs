using Contratos;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.Servicios
{ 
    public class Servicios
    {
        public HubConnection _conexion;

        public Servicios() {
            _conexion = new HubConnectionBuilder()
                .WithUrl(new Uri("https://localhost:7186/Mensajeria"))
                .Build();

            _conexion.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _conexion.StartAsync();
            };

        }

        public async Task<RespuestaIniciarSesion> IniciarSesion(string clave)
        {
            await _conexion.StartAsync();

            var respuesta = await _conexion.InvokeAsync<RespuestaIniciarSesion>
                                        ("IniciarSesion", new SolicitudIniciarSesion { idEmpleado=clave });

            await _conexion.DisposeAsync();

            return respuesta;
        }
    }
}

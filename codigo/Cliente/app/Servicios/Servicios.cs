using Contratos;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.Servicios;

public class Servidor
{
    public HubConnection _conexion;
    public event EventHandler<Contratos.Mensaje>? MensajeRecibido;
    public event EventHandler<string>? EmpleadoConectado;


    public Servidor() {
        _conexion = new HubConnectionBuilder()
            .WithUrl(new Uri("http://192.168.0.112:7186/Mensajeria"))
            .Build();

        _conexion.On<Contratos.Mensaje>("RecibirNuevoMensaje", (msj) =>
        {
            MensajeRecibido?.Invoke(this, msj);
        });

        _conexion.On<string>("SeConectoEmpleado", (msj) =>
        {
            EmpleadoConectado?.Invoke(this, msj);
        });

        IniciarConexion();

        /*
        _conexion.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await _conexion.StartAsync();
        };
        */


    }

    private async void IniciarConexion()
    {
        await _conexion.StartAsync();
    }

    public async Task<RespuestaIniciarSesion> IniciarSesion(string clave)
    {

        var respuesta = await _conexion.InvokeAsync<RespuestaIniciarSesion>
                                    ("IniciarSesion", new SolicitudIniciarSesion { idEmpleado=clave });
        return respuesta;
    }

    public async Task<RespuestaEnviarMensaje> EnviarMensaje(SolicitudEnviarMensaje solicitud)
    {

        var respuesta = await _conexion.InvokeAsync<RespuestaEnviarMensaje>
                                    ("EnviarMensaje", new SolicitudEnviarMensaje { mensaje = solicitud.mensaje });

        return respuesta;
    }
}

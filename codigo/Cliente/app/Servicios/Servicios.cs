using Contratos;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace app.Servicios;

public interface IServicios
{
    event EventHandler<Mensaje> MensajeRecibido;
    event EventHandler<Empleado> EmpleadoConectado;
    event EventHandler<Empleado> EmpleadoDesconectado;

    Task<RespuestaIniciarSesion> IniciarSesion(SolicitudIniciarSesion solicitud);
    Task<RespuestaEnviarMensaje> EnviarMensaje(SolicitudEnviarMensaje solicitud);
    Task<RespuestaCerrarSesion> CerrarSesion(SolicitudCerrarSesion soliciutd);
}

public class Servidor : IServicios
{
    public HubConnection _conexion;
    public event EventHandler<Mensaje> MensajeRecibido;
    public event EventHandler<Empleado> EmpleadoConectado;
    public event EventHandler<Empleado> EmpleadoDesconectado;

    public Servidor() {
        var lastConectionId = "";

        _conexion = new HubConnectionBuilder()
                .WithUrl(new Uri("http://192.168.0.112:7186/Mensajeria"))
                .Build();

        _conexion.On<Mensaje>("RecibirNuevoMensaje", (msj) =>
        {
            MensajeRecibido?.Invoke(this, msj);
        });
        _conexion.On<Empleado>("RecibirNotificacionEmpleadoConectado", (msj) =>
        {
            EmpleadoConectado?.Invoke(this, msj);
        });
        _conexion.On<Empleado>("RecibirNotificacionEmpleadoDesconectado", (msj) =>
        {
            EmpleadoDesconectado?.Invoke(this, msj);
        });

        _conexion.Closed += async (error) =>
        {
            lastConectionId = _conexion.ConnectionId;

            await Task.Delay(new Random().Next(0, 5) * 1000);


            await _conexion.StartAsync();
            //RevocarConexionCaida(lastConectionId);
        };
        _conexion.Reconnecting += (error) =>
        {
            Debug.Assert(_conexion.State == HubConnectionState.Reconnecting);

            // Notify users the connection was lost and the client is reconnecting.
            // Start queuing or dropping messages.

            return Task.CompletedTask;
        };
        _conexion.Reconnected += (connectionId) =>
        {
            Debug.Assert(_conexion.State == HubConnectionState.Connected);

            // Notify users the connection was reestablished.
            // Start dequeuing messages queued while reconnecting if any.

            return Task.CompletedTask;
        };

        IniciarConexion();
    }

    private async void RevocarConexionCaida(string idUltimaConexion)
    {
        await _conexion.InvokeAsync("RevocarConexionCaida", idUltimaConexion);
    }

    private async void IniciarConexion()
    {
        if (_conexion.State is not HubConnectionState.Connected)
            await _conexion.StartAsync();
    }

    public async Task<RespuestaIniciarSesion> IniciarSesion(SolicitudIniciarSesion solicitud)
    {
        var respuesta = await _conexion.InvokeAsync<RespuestaIniciarSesion>
                                     ("IniciarSesion", new SolicitudIniciarSesion { idEmpleado = solicitud.idEmpleado });


        return respuesta;
    }

    public async Task<RespuestaEnviarMensaje> EnviarMensaje(SolicitudEnviarMensaje solicitud)
    {

        var respuesta = await _conexion.InvokeAsync<RespuestaEnviarMensaje>
                                    ("EnviarMensaje", new SolicitudEnviarMensaje { mensaje = solicitud.mensaje });

        return respuesta;
    }

    public async Task<RespuestaCerrarSesion> CerrarSesion(SolicitudCerrarSesion soliciutd)
    {
        throw new NotImplementedException();
    }
}

public class Mock : IServicios
{
    public event EventHandler<Mensaje> MensajeRecibido;
    public event EventHandler<Empleado> EmpleadoConectado;
    public event EventHandler<Empleado> EmpleadoDesconectado;

    public Task<RespuestaCerrarSesion> CerrarSesion(SolicitudCerrarSesion soliciutd)
    {
        throw new NotImplementedException();
    }

    public async Task<RespuestaEnviarMensaje> EnviarMensaje(SolicitudEnviarMensaje solicitud)
    {
        return new RespuestaEnviarMensaje();
    }

    public async Task<RespuestaIniciarSesion> IniciarSesion(SolicitudIniciarSesion solicitud)
    {
        return new RespuestaIniciarSesion();
    }
}
    
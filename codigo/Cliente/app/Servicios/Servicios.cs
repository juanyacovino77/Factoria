using Contratos;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace app.Servicios;

public interface IServicios
{
    event EventHandler<Mensaje> MensajeRecibido;
    event EventHandler<Empleado> EmpleadoConectado;
    event EventHandler<Empleado> EmpleadoDesconectado;

    Task PrenderConexion();

    Task<RespuestaIniciarSesion> IniciarSesion(SolicitudIniciarSesion solicitud);
    Task<RespuestaEnviarMensaje> EnviarMensaje(SolicitudEnviarMensaje solicitud);
    Task<RespuestaCerrarSesion> CerrarSesion(SolicitudCerrarSesion soliciutd);
}

public class Servidor : IServicios
{
    private HubConnection _conexion;
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
    }

    private async void RevocarConexionCaida(string idUltimaConexion)
    {
        await _conexion.InvokeAsync("RevocarConexionCaida", idUltimaConexion);
    }

    public async Task PrenderConexion()
    {
        if (_conexion.State is not HubConnectionState.Connected)
        {
            await _conexion.StartAsync();
        }
    }

    public async Task<RespuestaIniciarSesion> IniciarSesion(SolicitudIniciarSesion solicitud)
    {
        if (_conexion.State is not HubConnectionState.Connected) return new RespuestaIniciarSesion() { exito=false, mensaje="Servidor apagado"};

        var respuesta = await _conexion.InvokeAsync<RespuestaIniciarSesion>
                                     ("IniciarSesion", solicitud);


        return respuesta;
    }

    public async Task<RespuestaEnviarMensaje> EnviarMensaje(SolicitudEnviarMensaje solicitud)
    {

        var respuesta = await _conexion.InvokeAsync<RespuestaEnviarMensaje>
                                    ("EnviarMensaje", solicitud);

        return respuesta;
    }

    public async Task<RespuestaCerrarSesion> CerrarSesion(SolicitudCerrarSesion solicitud)
    {
        var respuesta = await _conexion.InvokeAsync<RespuestaCerrarSesion>
                             ("CerrarSesion", solicitud);


        return respuesta;
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

    public Task<bool> ConectarAlServidor()
    {
        throw new NotImplementedException();
    }

    public async Task<RespuestaEnviarMensaje> EnviarMensaje(SolicitudEnviarMensaje solicitud)
    {
        return new RespuestaEnviarMensaje();
    }

    public void IniciarConexion()
    {
        throw new NotImplementedException();
    }

    public async Task<RespuestaIniciarSesion> IniciarSesion(SolicitudIniciarSesion solicitud)
    {
        return new RespuestaIniciarSesion();
    }

    public Task PrenderConexion()
    {
        throw new NotImplementedException();
    }
}
    
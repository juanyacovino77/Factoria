namespace API;
using API.Hubs.PuertoDeEntrada;
using puertos;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddSignalR(cfg => { cfg.MaximumReceiveMessageSize = 10048576; })
            .AddJsonProtocol();

        builder.Services.AddTransient<IServicios, Servicios>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();
        app.MapHub<MensajeriaSinLogica>("/Mensajeria");

        app.Run();
    }
}
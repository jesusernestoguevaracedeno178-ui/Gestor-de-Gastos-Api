using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GestorGastosApi.Data;

/// <summary>
/// Fábrica que le dice a las herramientas de EF Core (dotnet ef) cómo crear
/// una instancia de GestorGastosDbContext durante el tiempo de diseño (migraciones).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GestorGastosDbContext>
{
    public GestorGastosDbContext CreateDbContext(string[] args)
    {
        // 1. Construimos la configuración para leer el archivo appsettings.json.
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // 2. Obtenemos la cadena de conexión.
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // 3. Construimos las opciones del DbContext.
        var optionsBuilder = new DbContextOptionsBuilder<GestorGastosDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        // 4. Devolvemos una nueva instancia del DbContext.
        return new GestorGastosDbContext(optionsBuilder.Options);
    }
}
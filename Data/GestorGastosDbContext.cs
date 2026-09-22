using System.Transactions;
using GestorGastosApi.Entities;
using Microsoft.EntityFrameworkCore;
using Transaction = GestorGastosApi.Entities.Transaction;
namespace GestorGastosApi.Data;

/// <summary>
/// Representa la sesión con la base de datos de SQL Server.
/// Hereda de DbContext, que es la clase base de Entity Framework Core.
/// Aquí se declaran las tablas (DbSet) que queremos manejar.
/// </summary>
public class GestorGastosDbContext : DbContext
{
    //<sumary>
    // Constructor que recibe las opciones de configuración (cadena de conexión, proveedor, etc.)
    // desde el contenedor de Inyección de Dependencias.
    //</sumary>
    public GestorGastosDbContext(DbContextOptions<GestorGastosDbContext> options) : base(options){}

    public DbSet<Category> Categories{get; set;}// Representa la tabla "Categories" en SQL Server.
    
    public DbSet<Transaction> Transactions{get; set;}// Representa la tabla "Transactions" en SQL Server.

    
    // <summary>
    // Configura detalles finos del modelo que no se pueden expresar
    // solo con atributos en las entidades.
    // </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // --- Configuración de Category ---
        modelBuilder.Entity<Category>(entity =>
        { 
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100); 
            entity.Property(c => c.Description).HasMaxLength(500);
        });

        // --- Configuración de Transaction ---
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(500);
            entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.Type).HasConversion<int>();

            // Relación 1:N entre Category y Transaction
            entity.HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}
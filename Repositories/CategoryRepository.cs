using GestorGastosApi.Data;
using GestorGastosApi.Entities;
using GestorGastosApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestorGastosApi.Repositories;

// <summary>
// Implementación del repositorio de categorías usando Entity Framework Core.
// Aquí SÍ vivimos el acceso a la base de datos.
// </summary>
public class CategoryRepository : ICategoryRepository
{    
    
    // El DbContext viene inyectado desde el contenedor de DI (Program.cs).
    private readonly GestorGastosDbContext _context;
    
    public CategoryRepository(GestorGastosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {   
        // ToListAsync() ejecuta: SELECT * FROM Categories
        return await _context.Categories
            .AsNoTracking()   // Solo lectura: más rápido, no rastrea cambios.
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        // FirstOrDefaultAsync devuelve null si no encuentra nada.
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> HasTransactionsAsync(int categoryId)
    {
        // AnyAsync() es más eficiente que Count() > 0
        // Genera: SELECT CASE WHEN EXISTS (...) THEN 1 ELSE 0 END
        return await _context.Transactions.AnyAsync(t => t.CategoryId == categoryId);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        // Add() marca la entidad como "nueva" en el change tracker.
        _context.Categories.Add(category);

        // SaveChangesAsync() ejecuta el INSERT y asigna el Id generado.
        await _context.SaveChangesAsync();

        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        // Update() marca la entidad como "modificada".
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // Buscamos primero para asegurarnos de que existe.
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

}
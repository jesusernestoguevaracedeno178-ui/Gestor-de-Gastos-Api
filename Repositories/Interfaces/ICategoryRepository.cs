using GestorGastosApi.Entities;

namespace GestorGastosApi.Repositories.Interfaces;

// <summary>
// Contrato del repositorio de categorías.
// Define QUÉ operaciones existen, pero no CÓMO se implementan.
// </summary>
public interface ICategoryRepository
{

    Task<Category?> GetByIdAsync(int id);// Obtiene una categoría por su Id. Devuelve null si no existe.
    Task<bool> HasTransactionsAsync(int categoryId);// Verifica si una categoría tiene transacciones asociadas.
                                                    // Se usa antes de eliminar una categoría (regla de negocio).
    Task<Category> UpdateAsync(Category category);// Actualiza una categoría existente.
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category> CreateAsync(Category category);
    Task<bool> DeleteAsync(int id);
    
}
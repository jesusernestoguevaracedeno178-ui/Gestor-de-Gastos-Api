using GestorGastosApi.DTOs;
using GestorGastosApi.Entities;
using GestorGastosApi.Repositories.Interfaces;
using GestorGastosApi.Services.Interfaces;

namespace GestorGastosApi.Services;

/// <summary>
/// Servicio de categorías.
/// Aquí viven las reglas de negocio y la conversión Entity ↔ DTO.
/// </summary>
public class CategoryService : ICategoryService
{
    // El repositorio viene inyectado desde el contenedor de DI.
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _repository.GetAllAsync();

        // Convertimos cada entidad a DTO antes de devolverla.
        // El cliente NUNCA ve la entidad Category.
        return categories.Select(c => ToDto(c));
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);
        return category is null ? null : ToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto)
    {
        // 1. Convertimos el DTO a entidad
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        // 2. Guardamos en BD a través del repositorio
        var created = await _repository.CreateAsync(category);

        // 3. Devolvemos el DTO con el Id ya asignado
        return ToDto(created);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, CategoryUpdateDto dto)
    {
        // REGLA DE NEGOCIO: no se puede actualizar algo que no existe.
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return null;

        // Actualizamos solo los campos permitidos.
        existing.Name = dto.Name;
        existing.Description = dto.Description;

        var updated = await _repository.UpdateAsync(existing);
        return ToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // REGLA DE NEGOCIO: no se puede eliminar una categoría con transacciones.
        var hasTransactions = await _repository.HasTransactionsAsync(id);
        if (hasTransactions)
            throw new InvalidOperationException(
                "No se puede eliminar una categoría que tiene transacciones asociadas.");

        return await _repository.DeleteAsync(id);
    }

    /// <summary>
    /// Método privado de conversión Entity → DTO.
    /// Es la traducción entre el mundo interno (BD) y el externo (cliente).
    /// </summary>
    private static CategoryDto ToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}
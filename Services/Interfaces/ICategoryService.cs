using GestorGastosApi.DTOs;

namespace GestorGastosApi.Services.Interfaces;

/// <summary>
/// Contrato del servicio de categorías.
/// Define QUÉ operaciones de negocio existen para las categorías.
/// Los Controllers solo conocen esta interfaz, no la implementación.
/// </summary>
public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CategoryCreateDto dto);
    Task<CategoryDto?> UpdateAsync(int id, CategoryUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
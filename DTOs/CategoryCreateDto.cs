namespace GestorGastosApi.DTOs;

// <summary>
// DTO de entrada para crear una nueva categoría.
// NO incluye Id porque lo genera la base de datos automáticamente.
// </summary>
public class CategoryCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
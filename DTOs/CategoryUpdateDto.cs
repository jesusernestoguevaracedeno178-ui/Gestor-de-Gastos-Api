namespace GestorGastosApi.DTOs;

/// <summary>
/// DTO de entrada para actualizar una categoría existente.
/// </summary>
public class CategoryUpdateDto
{
    public int Id{get; set;}
    public string Name{get; set;} = string.Empty;
    public string Description{get; set;} = string.Empty;
}
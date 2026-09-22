namespace GestorGastosApi.DTOs;

// <summary>
// DTO de salida para una categoría.
// Se devuelve al cliente cuando consulta categorías.
// No incluye la lista de transacciones para evitar respuestas gigantes.
// </summary>
public class CategoryDto
{
    public int Id{get; set;}
    public string Name{get; set;} = string.Empty;
    public string Description{get; set;} = string.Empty;
}
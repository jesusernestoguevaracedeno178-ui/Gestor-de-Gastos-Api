using GestorGastosApi.Entities;
namespace  GestorGastosApi.DTOs;

/// <summary>
/// DTO de salida para una transacción.
/// Incluye el nombre de la categoría para que el cliente no tenga que hacer otra petición.
/// </summary>
public class TransactionDto
{
    public int Id{get; set;}
    public decimal Amount{get; set;}
    public DateTime Date{get; set;}
    public string Description{get; set;} = string.Empty;
    public TransactionType Type{get; set;}
    public int CategoryId{get; set;}
    public string CategoryName{get; set;} =string.Empty;
}
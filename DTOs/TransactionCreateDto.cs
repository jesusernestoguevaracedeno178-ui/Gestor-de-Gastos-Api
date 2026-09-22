using GestorGastosApi.Entities;

namespace  GestorGastosApi.DTOs;

// <summary>
// DTO de entrada para crear una nueva transacción.
// </summary>
public class TransactionCreateDto
{
    public decimal Amount{get; set;}
    public DateTime Date{get; set;}
    public string Description{get; set;} = string.Empty;
    public TransactionType Type {get; set;}
    public int CategoryId{get; set;}
}
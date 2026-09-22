namespace GestorGastosApi.Entities;

//<sumary>
//Define los tipo posibles de una transaccion:
//Un enumlimita los valores a un conjunto fijo y controlado.
//lo que evita errores (no puedes escribir"Gasto" mal escrito, por ejemplo).
//</sumary>
public enum TransactionType
{
    //Regresa un ingreso de dinerp(Ej; sueldo, ventas, regalo recibido)
    Income = 1,
    //Representa un gasto de dinero(Ej: comida, transporte o alquiler)
    Expense = 2
}
[README.md](https://github.com/user-attachments/files/32524761/README.md)
# GestorGastosApi

API REST para la gestión de finanzas personales: permite registrar **categorías** (por ejemplo, Alimentación, Transporte, Sueldo) y **transacciones** (ingresos y gastos) asociadas a esas categorías.

El proyecto está construido con **ASP.NET Core**, **Entity Framework Core** y **SQLite**, y sigue una arquitectura en capas (Controllers → Services → Repositories → DbContext) que separa las responsabilidades y mantiene el código testeable y mantenible.

---

## Stack tecnológico

| Componente | Versión / Detalle |
|---|---|
| Framework | .NET 10 (`net10.0`) |
| Lenguaje | C# con `Nullable` e `ImplicitUsings` habilitados |
| Acceso a datos | Entity Framework Core 10.0.12 |
| Base de datos | SQLite (archivo local `gestorgastos.db`) |
| Documentación de API | Swashbuckle.AspNetCore 10.2.3 (Swagger UI) |
| OpenAPI | Microsoft.AspNetCore.OpenApi 10.0.11 |

### Requisitos previos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) o superior
- Herramienta de migraciones de EF Core (solo si vas a trabajar con migraciones):

```bash
dotnet tool install --global dotnet-ef
```

---

## Estructura del proyecto

```
GestorGastosApi/
├── src/
│   └── GestorGastosApi/
│       ├── Controllers/            # Endpoints HTTP
│       │   ├── CategoriesController.cs
│       │   └── TransactionsController.cs
│       ├── Services/               # Reglas de negocio y mapeo Entity ↔ DTO
│       │   ├── Interfaces/
│       │   │   ├── ICategoryService.cs
│       │   │   └── ITransactionService.cs
│       │   ├── CategoryService.cs
│       │   └── TransactionService.cs
│       ├── Repositories/           # Acceso a datos con EF Core
│       │   ├── Interfaces/
│       │   │   ├── ICategoryRepository.cs
│       │   │   └── ITransactionRepository.cs
│       │   ├── CategoryRepository.cs
│       │   └── TransactionRepository.cs
│       ├── Entites/                # Entidades del dominio (modelo de datos)
│       │   ├── Category.cs
│       │   ├── Transaction.cs
│       │   └── TransactionType.cs
│       ├── DTOs/                   # Contratos de entrada y salida
│       ├── Data/                   # DbContext y fábrica de tiempo de diseño
│       ├── Migrations/             # Migraciones de EF Core
│       ├── Program.cs              # Punto de entrada y configuración de DI
│       ├── appsettings.json        # Cadena de conexión y logging
│       └── gestorgastos.db         # Base de datos SQLite
└── README.md
```

> **Nota:** la carpeta se llama `Entites` (sin la "i"), pero el *namespace* de las entidades sí es `GestorGastosApi.Entities`. Es una inconsistencia de nombre heredada; corregirla implica renombrar la carpeta y actualizar los `using` correspondientes.

---

## Arquitectura

El flujo de una petición atraviesa cuatro capas, cada una con una única responsabilidad:

```
Cliente HTTP
    │
    ▼
Controller  ──►  Service  ──►  Repository  ──►  DbContext  ──►  SQLite
(HTTP)          (negocio)     (datos)          (EF Core)       (archivo .db)
    │               │              │
    │               │              └── Consultas LINQ, Include, AsNoTracking
    │               └── Reglas de negocio + mapeo Entity ↔ DTO
    └── Valida entrada, traduce resultados a códigos HTTP
```

### Responsabilidades por capa

| Capa | Responsabilidad | Qué NO hace |
|---|---|---|
| **Controllers** | Recibir la petición, validar el contrato de entrada y devolver el código HTTP correcto. | No contiene lógica de negocio ni acceso a datos. |
| **Services** | Aplicar las reglas de negocio y convertir entidades a DTO. | No conoce HTTP ni EF Core. |
| **Repositories** | Ejecutar consultas con EF Core. | No aplica reglas de negocio. |
| **DTOs** | Definir el contrato público de la API. | Las entidades **nunca** se exponen directamente al cliente. |

### Inyección de dependencias

Toda la composición se registra en `Program.cs`:

```csharp
// DbContext con SQLite
builder.Services.AddDbContext<GestorGastosDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorios y servicios: una instancia por petición HTTP (AddScoped)
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

Los controllers dependen **solo de las interfaces** (`ICategoryService`, `ITransactionService`), lo que permite sustituir implementaciones (por ejemplo, mocks en tests) sin tocar el código HTTP.

---

## Modelo de datos

### `Category` → tabla `Categories`

| Propiedad | Tipo | Restricciones |
|---|---|---|
| `Id` | `int` | Clave primaria, autoincremental |
| `Name` | `string` | Requerido, máximo 100 caracteres |
| `Description` | `string` | Máximo 500 caracteres |
| `Transactions` | `List<Transaction>` | Propiedad de navegación (1 → N), **no** es una columna |

### `Transaction` → tabla `Transactions`

| Propiedad | Tipo | Restricciones |
|---|---|---|
| `Id` | `int` | Clave primaria, autoincremental |
| `Amount` | `decimal` | Requerido, columna `decimal(18,2)` |
| `Date` | `DateTime` | Requerido |
| `Description` | `string` | Requerido, máximo 500 caracteres |
| `Type` | `TransactionType` | Requerido, se guarda como `int` (1 = Income, 2 = Expense) |
| `CategoryId` | `int` | Clave foránea hacia `Categories.Id` |
| `Category` | `Category?` | Propiedad de navegación, **no** es una columna |

### `TransactionType` (enum)

```csharp
public enum TransactionType
{
    Income = 1,   // Ingreso: sueldo, ventas, regalo recibido
    Expense = 2   // Gasto: comida, transporte, alquiler
}
```

### Relación y borrado

- Una `Category` tiene muchas `Transaction` (1 → N).
- La clave foránea `Transaction.CategoryId` usa `DeleteBehavior.Restrict`: **no se puede borrar una categoría mientras tenga transacciones**. Esta restricción existe tanto a nivel de base de datos como a nivel de regla de negocio (ver más abajo).
- Se crea un índice `IX_Transactions_CategoryId` para acelerar el filtrado por categoría.

### Sobre el `decimal`

`Amount` usa `decimal` en lugar de `double` o `float` **a propósito**: es el tipo correcto para valores monetarios porque evita errores de redondeo binario.

---

## Configuración

### Cadena de conexión (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=gestorgastos.db"
  }
}
```

La base de datos es un archivo SQLite llamado `gestorgastos.db` ubicado en la carpeta del proyecto.

### Migraciones y `DesignTimeDbContextFactory`

`Data/DesignTimeDbContextFactory.cs` le indica a las herramientas de `dotnet ef` cómo construir el `DbContext` en tiempo de diseño, leyendo la cadena de conexión desde `appsettings.json`. Esto permite ejecutar migraciones sin levantar la aplicación completa.

---

## Cómo ejecutar

Desde la carpeta `src/GestorGastosApi`:

```bash
# 1. Restaurar dependencias
dotnet restore

# 2. Aplicar las migraciones y crear la base de datos
dotnet ef database update

# 3. Levantar la API
dotnet run
```

La API queda disponible en `http://localhost:5158` y la documentación interactiva en:

```
http://localhost:5158/swagger
```

> Swagger está habilitado **siempre**, no solo en desarrollo (el `if (app.Environment.IsDevelopment())` está comentado en `Program.cs`). Tenelo en cuenta antes de desplegar a producción.

---

## Endpoints

### Categorías — `/api/categories`

| Método | Ruta | Descripción | Códigos de respuesta |
|---|---|---|---|
| `GET` | `/api/categories` | Lista todas las categorías | `200` |
| `GET` | `/api/categories/{id}` | Obtiene una categoría por Id | `200`, `404` |
| `POST` | `/api/categories` | Crea una categoría | `201`, `400` |
| `PUT` | `/api/categories/{id}` | Actualiza una categoría | `200`, `400`, `404` |
| `DELETE` | `/api/categories/{id}` | Elimina una categoría | `204`, `400`, `404` |

> ⚠️ **Bug conocido en `PUT`.** La acción `Update` está decorada con `[HttpPut("id:int")]` en lugar de `[HttpPut("{id:int}")]`. Sin las llaves, `id:int` se interpreta como un **segmento literal**, por lo que la ruta real es `/api/categories/id:int` y `/api/categories/{id}` **no funciona**. Ver [Problemas conocidos](#problemas-conocidos-y-deuda-técnica).

### Transacciones — `/api/transactions`

| Método | Ruta | Descripción | Códigos de respuesta |
|---|---|---|---|
| `GET` | `/api/transactions` | Lista todas las transacciones (con su categoría) | `200` |
| `GET` | `/api/transactions/{id}` | Obtiene una transacción por Id | `200`, `404` |
| `GET` | `/api/transactions/Category/{categoryId}` | Filtra transacciones por categoría | `200` |
| `GET` | `/api/transactions/range?start=&end=` | Filtra por rango de fechas (orden descendente) | `200`, `400` |
| `POST` | `/api/transactions` | Crea una transacción | `201`, `400` |
| `PUT` | `/api/transactions/{id}` | Actualiza una transacción | `200`, `400`, `404` |
| `DELETE` | `/api/transactions/{id}` | Elimina una transacción | `204`, `404` |

---

## Contratos (DTOs)

### Categorías

```jsonc
// CategoryDto (respuesta)
{ "id": 1, "name": "Alimentación", "description": "Comida y supermercado" }

// CategoryCreateDto (entrada — sin Id, lo genera la base)
{ "name": "Alimentación", "description": "Comida y supermercado" }

// CategoryUpdateDto (entrada — incluye Id)
{ "id": 1, "name": "Alimentación", "description": "Comida y supermercado" }
```

### Transacciones

```jsonc
// TransactionDto (respuesta)
{
  "id": 10,
  "amount": 1500.50,
  "date": "2026-09-21T00:00:00",
  "description": "Almuerzo en el trabajo",
  "type": 2,              // 1 = Income, 2 = Expense (se serializa como número)
  "categoryId": 1,
  "categoryName": "Alimentación"
}

// TransactionCreateDto (entrada — sin Id)
{
  "amount": 1500.50,
  "date": "2026-09-21T00:00:00",
  "description": "Almuerzo en el trabajo",
  "type": 2,
  "categoryId": 1
}

// TransactionUpdateDto (entrada — incluye Id)
{
  "id": 10,
  "amount": 1500.50,
  "date": "2026-09-21T00:00:00",
  "description": "Almuerzo en el trabajo",
  "type": 2,
  "categoryId": 1
}
```

> El campo `type` viaja como número porque no hay configuración de serialización de enums como string. Si preferís `"Income"` / `"Expense"`, hay que agregar `JsonStringEnumConverter`.

---

## Reglas de negocio

Las reglas viven en los **Services** y se comunican al cliente mediante `InvalidOperationException`, que los controllers traducen a `400 Bad Request`:

| Regla | Dónde se aplica | Resultado si falla |
|---|---|---|
| No se puede eliminar una categoría que tiene transacciones asociadas | `CategoryService.DeleteAsync` | `400 Bad Request` |
| La categoría referenciada debe existir al crear una transacción | `TransactionService.CreateAsync` | `400 Bad Request` |
| El monto de la transacción debe ser mayor que cero | `TransactionService.CreateAsync` | `400 Bad Request` |
| La categoría referenciada debe existir al actualizar una transacción | `TransactionService.UpdateAsync` | `400 Bad Request` |
| El Id de la URL debe coincidir con el Id del body | Controllers (`Update`) | `400 Bad Request` |
| La fecha inicial no puede ser mayor a la final | `TransactionsController.GetByDateRange` | `400 Bad Request` |

> ⚠️ **Inconsistencia:** `TransactionService.UpdateAsync` **no** valida que `Amount > 0`, a diferencia de `CreateAsync`. Es posible actualizar una transacción dejándola con monto cero o negativo.

---

## Ejemplos de uso

```bash
# Crear una categoría
curl -X POST http://localhost:5158/api/categories \
  -H "Content-Type: application/json" \
  -d '{"name":"Alimentación","description":"Comida y supermercado"}'

# Listar categorías
curl http://localhost:5158/api/categories

# Crear una transacción (gasto)
curl -X POST http://localhost:5158/api/transactions \
  -H "Content-Type: application/json" \
  -d '{"amount":1500.50,"date":"2026-09-21T00:00:00","description":"Almuerzo","type":2,"categoryId":1}'

# Filtrar transacciones por categoría
curl http://localhost:5158/api/transactions/Category/1

# Filtrar transacciones por rango de fechas
curl "http://localhost:5158/api/transactions/range?start=2026-01-01&end=2026-12-31"

# Eliminar una transacción
curl -X DELETE http://localhost:5158/api/transactions/10
```

---

## Problemas conocidos y deuda técnica

| # | Problema | Impacto | Archivo |
|---|---|---|---|
| 1 | `[HttpPut("id:int")]` sin llaves: la ruta real es `/api/categories/id:int` | El endpoint de actualización de categorías **no funciona** | `Controllers/CategoriesController.cs` |
| 2 | `UpdateAsync` de transacciones no valida `Amount > 0` | Se pueden guardar montos inválidos por update | `Services/TransactionService.cs` |
| 3 | `using SQLitePCL;` sin usar en ambos controllers | Ruido, advertencias de compilación | `Controllers/*.cs` |
| 4 | `using System.Transactions;` en el `DbContext` (por colisión de nombres con `Transaction`) | Confuso; ya se resuelve con un alias | `Data/GestorGastosDbContext.cs` |
| 5 | Los comentarios dicen "SQL Server" pero el proveedor es SQLite | Documentación interna engañosa | `Data/`, `Entites/` |
| 6 | Swagger habilitado en todos los entornos | Exposición innecesaria en producción | `Program.cs` |
| 7 | `GestorGastosApi.http` apunta a `/weatherforecast/`, que no existe | Ejemplo obsoleto del template | `GestorGastosApi.http` |
| 8 | `TransactionsController` usa el namespace `GestorGastosApiControllers`; `CategoriesController` usa `GestorGastosApi.Controllers` | Inconsistencia de convención | `Controllers/TransactionsController.cs` |
| 9 | El archivo `gestorgastos.db` está dentro de la carpeta del proyecto | Riesgo de versionar datos locales | `gestorgastos.db` |
| 10 | No hay proyecto de tests | Sin red de seguridad para refactors | — |

### Mejoras recomendadas (en orden de prioridad)

1. **Corregir el bug de la ruta `PUT` de categorías** (`[HttpPut("{id:int}")]`).
2. **Unificar la validación de monto** entre `CreateAsync` y `UpdateAsync`.
3. **Agregar un middleware de manejo global de errores** (`IExceptionHandler`) para no traducir excepciones a mano en cada controller.
4. **Aplicar las migraciones al arrancar** (`app.Database.Migrate()`) o documentar el paso explícitamente.
5. **Agregar validación de DTOs** con Data Annotations o FluentValidation (`[Required]`, `[Range]`, `[MaxLength]`).
6. **Agregar paginación** a los endpoints de listado.
7. **Agregar tests unitarios** de los Services (son puros y fáciles de testear con mocks de los repositorios).
8. **Configurar CORS** si se va a consumir desde un frontend separado.
9. **Agregar autenticación/autorización** — hoy `app.UseAuthorization()` está en el pipeline pero no hay ningún esquema de autenticación configurado.

---

## Conceptos clave aplicados

- **Separación de responsabilidades**: cada capa tiene un único motivo para cambiar.
- **Inversión de dependencias**: los controllers y services dependen de abstracciones (interfaces), no de implementaciones concretas.
- **DTOs como contrato**: las entidades de EF Core nunca se exponen al cliente, lo que evita acoplar la API al modelo de datos y previene ciclos de serialización.
- **`AsNoTracking()`** en consultas de solo lectura: mejora el rendimiento al no cargar el change tracker.
- **`decimal` para dinero**: evita errores de redondeo propios de los tipos de punto flotante.
- **`DeleteBehavior.Restrict`**: protege la integridad referencial a nivel de base de datos.

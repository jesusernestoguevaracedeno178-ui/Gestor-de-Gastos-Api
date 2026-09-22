using GestorGastosApi.Data;
using GestorGastosApi.Repositories;
using GestorGastosApi.Repositories.Interfaces;
using GestorGastosApi.Services;
using GestorGastosApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Registra el DbContext en el contenedor de dependencias y le indica
// que use SQLite con la cadena de conexión del appsettings.json.
builder.Services.AddDbContext<GestorGastosDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Repositorios (AddScoped: una instancia por petición HTTP)
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// 3. Servicios (también AddScoped)
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();    

// 4. Controladores HTTP
builder.Services.AddControllers();

// 5. Swagger: documentación interactiva
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app =builder.Build();

// 6. Pipeline HTTP
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

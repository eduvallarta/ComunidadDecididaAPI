using Microsoft.EntityFrameworkCore;
using Comunidad_Decidida.Infrastructure;
using Comunidad_Decidida.Repositories;
using Comunidad_Decidida.Services;
using BlackList.Services;
using Comunidad_Decidida.Interfaces;
using Comunidad_Decidida.Manager;
using Comunidad_Decidida.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de la cadena de conexión y DbContext
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Registro de servicios y repositorios
builder.Services.AddScoped<IAsociadoRepository, AsociadoRepository>();
builder.Services.AddScoped<AsociadoService>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IServicesLoginManager, ServicesLoginManager>();
builder.Services.AddHttpClient<AutenticacionManager>();
builder.Services.AddScoped<UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Habilitar la servida de archivos estáticos

// Aplicar CORS
app.UseCors("AllowAllOrigins");

app.UseAuthorization();

app.MapControllers();

app.Run();

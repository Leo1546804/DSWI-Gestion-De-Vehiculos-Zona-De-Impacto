using ZonaDeImpacto.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Habilitar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(600);
});

// Acceso al HttpContext
builder.Services.AddHttpContextAccessor();




// Registrar repositorio en el ADO.NET
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<LoginRepository>();
builder.Services.AddScoped<VehiculoRepository>();

builder.Services.AddScoped<TipoGastoRepository>();
builder.Services.AddScoped<MantenimientoRepository>();
builder.Services.AddScoped<GastoRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//authentication 
app.UseAuthentication();
app.UseAuthorization();

// Activar las sesiones
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();

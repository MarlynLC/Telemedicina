using Microsoft.EntityFrameworkCore;
using Telemedicina.Data;
using FluentValidation.AspNetCore;
using FluentValidation;
using Telemedicina.Models.AsistenteLocalFormularios;
using Telemedicina.Models.AsistenteRemotoFormularios;
using Telemedicina.Models.DoctorFormularios;
using Telemedicina.Models.PacienteFormularios;
using Telemedicina.Models.LoginFormulario;
using Telemedicina.Models.Localidad;
using Telemedicina.Models.Complejo;
using Telemedicina.Models.Agenda;
using Telemedicina.Models.Video;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//VideoHub
builder.Services.AddSignalR();

//Api
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyPolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:7051")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});


//Agregamos el servicio de validar datos
builder.Services.AddMvc().AddFluentValidation();
//Paciente
builder.Services.AddTransient<IValidator<agregarPacienteViewModel>, agregarPacienteViewModel>();
builder.Services.AddTransient<IValidator<UpdatePacienteViewModel>, UpdatePacienteViewModel>();
//Doctor
builder.Services.AddTransient<IValidator<agregardoctorViewModel>, agregardoctorViewModel>();
builder.Services.AddTransient<IValidator<UpdateDoctorViewModel>, UpdateDoctorViewModel>();
//AsistenteLocal
builder.Services.AddTransient<IValidator<agregarasistentelocalViewModel>, agregarasistentelocalViewModel>();
builder.Services.AddTransient<IValidator<UpdateAsistenteLocalViewModel>, UpdateAsistenteLocalViewModel>();
//AsistenteRemoto
builder.Services.AddTransient<IValidator<agregarasistenteremotoViewModel>, agregarasistenteremotoViewModel>();
builder.Services.AddTransient<IValidator<UpdateAsistenteRemotoViewModel>, UpdateAsistenteRemotoViewModel>();
//Login
builder.Services.AddTransient<IValidator<loginViewModel>, loginViewModel>();
//Localidad
builder.Services.AddTransient<IValidator<Localidadd>, Localidadd>();
builder.Services.AddTransient<IValidator<Localidadm>, Localidadm>();
//Complejo
builder.Services.AddTransient<IValidator<Complejod>, Complejod>();
//CitaTemp
builder.Services.AddTransient<IValidator<CitaTemp>, CitaTemp>();
//CitaFinal
builder.Services.AddTransient<IValidator<CitaFinalF>, CitaFinalF>();

//Colocamos esta instruccion para poder crear/vincular la base de datos, esto utilizando unas lineas de codigo
//que estan ubicadas en appsettings.json, la linea a modificar es TelemedicinaDemoConeccionString
builder.Services.AddDbContext<TeleDemoDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TelemedicinaDemoConeccionString")));


//Instrucciones para utilizar la cache para hacer las sesiones
//Con esta instrucion nos permite guardas objetos en la memoria
builder.Services.AddDistributedMemoryCache();
//Agregamos las sessiones
builder.Services.AddSession(options =>
{
    //Configuracion si esta 10 minutos afk se cierra la sesion/cache
    options.IdleTimeout= TimeSpan.FromMinutes(10);
});


var app = builder.Build();

//Seguimos configurando las sesiones
app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//Agregamos el uso de rotativa para el pdf
IWebHostEnvironment env = app.Environment;
RotativaConfiguration.Setup((Microsoft.AspNetCore.Hosting.IHostingEnvironment)env, "../Rotativa");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//VideoHub
app.MapHub<DefaultHub>("/meeting");

//Api
app.UseCors(MyAllowSpecificOrigins);
    
app.Run();

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Telemedicina.Data;
using Telemedicina.Models;
using Telemedicina.Models.Dominio;
using Telemedicina.Models.LoginFormulario;

namespace Telemedicina.Controllers
{
    public class HomeController : Controller
    {

        //Contexto de la base de datos
        private readonly TeleDemoDBContext teleDemoDBContext;

        public HomeController(TeleDemoDBContext teleDemoDBContext)
        {
            this.teleDemoDBContext = teleDemoDBContext;
        }
        
        public async Task<IActionResult> Index()
        {
            //Verificamos si el usuario ya se logueo
            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyId)))
            {
                return RedirectToAction("login");
            }

            //Recepcion si es que existe del mensaje por los datos del formulario o algo de session
            //Comprobamos si no existe mensaje por parte de session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionMessage)))
            {
                //Si no existe, vemos si hay algo de temporal
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                    TempData["Message"] = "";
                }
            } else
            { 
                //Si lo hay lo decimos y reiniciamos
                ViewBag.Message = HttpContext.Session.GetString(SessionVariables.SessionMessage);
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "");
            }

            //Usuario 0
            var asistente = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync();
            if (asistente == null)
            {
                //Creamos el objeto asistentelocal para realizar el registro
                var asistentelocal = new AsistenteLocal()
                {
                    Nombre = "Admin",
                    Apellidos = "0",
                    Sexo = "Masculino",
                    Nacimiento = DateTime.Now,
                    Sangre = "O-",
                    Curp = "AAAA111111AAAAAA4",
                    Correo = "correo@correo.com",
                    Telefono = "1231231234",
                    Matricula = "1234567890",
                    Especialidad = "Control general",
                    Jerarquia = "Jefe",
                    Password = "0606",
                    Imagen = "",
                    FechaIngreso = DateTime.Now,
                    Visilbe = "Si"
                };

                await teleDemoDBContext.AsistenteLocal.AddAsync(asistentelocal);
                await teleDemoDBContext.SaveChangesAsync();

                TempData["Message"] = "Usuario 0";
            }
                
            return View();
        }

        [HttpPost]
        //Login por parte del formulario/datos
        public async Task<IActionResult> login(loginViewModel login)
        {
            //Verificamos si tiene algun error y en el caso regresar y enviar error
            if (!ModelState.IsValid && ModelState.ErrorCount != 0)
            {
                return View("Index");
            }

            //Verificamos que el "usuario exista"

            //Verificamos si es un asistenteLocal
            var asisLocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Curp == login.usuario);
            //Vemos si encontramos algo
            if (asisLocal != null)
            {// Si encontramos algo entramos nice
                if (asisLocal.Password == login.password)
                {
                    HttpContext.Session.SetString(SessionVariables.SessionKeyId, asisLocal.Id.ToString());
                    HttpContext.Session.SetString(SessionVariables.SessionKeyPass, asisLocal.Password);
                    HttpContext.Session.SetString(SessionVariables.SessionKeyName, asisLocal.Nombre + " " + asisLocal.Apellidos);
                    HttpContext.Session.SetString(SessionVariables.SessionKeyType, "AsistLocal");
                    Session datos = new Session();
                    datos.Nombre = HttpContext.Session.GetString(SessionVariables.SessionKeyName);
                    datos.rango = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
                    return View("login",datos);
                }
                else
                {
                    ViewBag.Message = "Usuario o Contraseña incorrecta";
                    return View("Index");
                }
            }

            //Verificamos si es un doctor
            var doctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Curp == login.usuario);
            //Vemos si encontramos algo
            if (doctor != null)
            {// Si encontramos algo entramos nice
                if(doctor.Password == login.password)
                {//Datos Correctos
                    HttpContext.Session.SetString(SessionVariables.SessionKeyId, doctor.Id.ToString());
                    HttpContext.Session.SetString(SessionVariables.SessionKeyPass, doctor.Password);
                    HttpContext.Session.SetString(SessionVariables.SessionKeyName, doctor.Nombre + " " + doctor.Apellidos);
                    HttpContext.Session.SetString(SessionVariables.SessionKeyType, "Doctor");
                    Session datos = new Session();
                    datos.Nombre = HttpContext.Session.GetString(SessionVariables.SessionKeyName);
                    datos.rango = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
                    return View("login",datos);
                } else
                {//Datos Incorrectos
                    ViewBag.Message = "Usuario o Contraseña incorrecta";
                    return View("Index");
                }
                
            }


            //Verificamos si es un asistenteRemoto
            var asisRemoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Curp == login.usuario);
            if (asisRemoto != null)
            {// Si encontramos algo entramos nice
                if (asisRemoto.Password == login.password)
                {
                    HttpContext.Session.SetString(SessionVariables.SessionKeyId, asisRemoto.Id.ToString());
                    HttpContext.Session.SetString(SessionVariables.SessionKeyPass, asisRemoto.Password);
                    HttpContext.Session.SetString(SessionVariables.SessionKeyName, asisRemoto.Nombre + " " + asisRemoto.Apellidos);
                    HttpContext.Session.SetString(SessionVariables.SessionKeyType, "AsistRemoto");
                    Session datos = new Session();
                    datos.Nombre = HttpContext.Session.GetString(SessionVariables.SessionKeyName);
                    datos.rango = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
                    return View("login",datos);
                }
                else
                {
                    ViewBag.Message = "Usuario o Contraseña incorrecta";
                    return View("Index");
                }
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }
            return View("Index");
        }//Fin de la verificacion de Login


        //Ahora lo vamos al login post page
        [HttpGet]
        public async Task<IActionResult> login()
        {
            //Comprobacion si tiene el permiso de entrar aqui
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                ViewBag.Message = "Acceso Restringido";
                return Redirect("Index");
            }

            //Comprobamos si no existe mensaje por parte de session
            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionMessage)))
            {
                //Si lo hay lo decimos y reiniciamos el mensaje
                ViewBag.Message = HttpContext.Session.GetString(SessionVariables.SessionMessage);
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "");
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }

            Session datos = new Session();
            datos.Nombre = HttpContext.Session.GetString(SessionVariables.SessionKeyName);
            datos.rango = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            //Mandamos a la vista
            return View(datos);
        }

        //Boton de login de cerrar session
        [HttpPost]
        public IActionResult cerrar()
        {
            //Eliminamos las credenciales y mandamos a home 
            HttpContext.Session.SetString(SessionVariables.SessionKeyPass, "");
            HttpContext.Session.SetString(SessionVariables.SessionKeyId, "");
            HttpContext.Session.SetString(SessionVariables.SessionKeyType, "");
            return View("Index");
        }

        //Ahora lo vamos al login post page menu
        [HttpGet]
        public async Task<IActionResult> menu()
        {
            //Comprobacion si tiene el permiso de entrar aqui
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                ViewBag.Message = "Acceso Restringido";
                return Redirect("Index");
            }
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Comprobamos si no existe mensaje por parte de session
            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionMessage)))
            {
                //Si lo hay lo decimos y reiniciamos el mensaje
                ViewBag.Message = HttpContext.Session.GetString(SessionVariables.SessionMessage);
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "");
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }
            //Mandamos a la vista 
            return View();
        }

        

        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
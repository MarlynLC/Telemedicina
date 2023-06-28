using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Plugins;
using Telemedicina.Data;
using Telemedicina.Models.TablasClase;
using static Azure.Core.HttpHeader;

namespace Telemedicina.Controllers
{
    public class MTablasController : Controller
    {
        //Poder utilizar el contexto de la base de datos
        private readonly TeleDemoDBContext teleDemoDBContext;
        public MTablasController(TeleDemoDBContext teleDemoDBContext)
        {
            this.teleDemoDBContext = teleDemoDBContext;
        }

        //Metodo para cancelar y regresar a la vista principal
        [HttpPost]
        public async Task<IActionResult> back()
        {
            return RedirectToAction("Index");
        }

        //Ahora lo vamos al tablero de tablas
        [HttpGet]
        public async Task<IActionResult> Index()
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

        //Metodo que lleva al Index/Tabla de los Mpacientes
        [HttpGet]
        public async Task<IActionResult> MPaciente()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Query a la tabla MPaciente, trae TODOS LOS Registros
            var mpacientequery = await teleDemoDBContext.MPaciente.ToListAsync();
            //Lista de la clase donde guardaremos datos
            List<MPacienteDatos> lista = new List<MPacienteDatos>();
            
            //Ciclo de llenado de lista de objetos
            foreach (var mpacienterow in mpacientequery)
            {
                //Creando objeto mpacientedatos para almacenar
                MPacienteDatos mpacientedatos = new MPacienteDatos();
                
                //Consulta para unir la id de la tabla MPaciente con Paciente
                var pacientedatos = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Id == mpacienterow.PacienteId);
                
                //Consulta para unir la id de la tabla MPaciente con quien la modifico
                if(mpacienterow.UsuarioTipo == "Doctor")
                {
                    //Es doctor sacamos su nombre
                    var doctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Id == mpacienterow.UsuarioId);
                    mpacientedatos.Usuario = doctor.Nombre + " " + doctor.Apellidos;

                } else if(mpacienterow.UsuarioTipo == "AsistLocal") 
                {
                    //Es asistente local sacamos su nombre
                    var asislocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Id == mpacienterow.UsuarioId);
                    mpacientedatos.Usuario = asislocal.Nombre + " " + asislocal.Apellidos;
                }
                else if (mpacienterow.UsuarioTipo == "AsistRemoto")
                {
                    //Es asistente remoto sacamos su nombre
                    var asisremoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Id == mpacienterow.UsuarioId);
                    mpacientedatos.Usuario = asisremoto.Nombre + " " + asisremoto.Apellidos;
                }

                if (pacientedatos == null) continue;
                mpacientedatos.Nombre = pacientedatos.Nombre + " " + pacientedatos.Apellidos;
                mpacientedatos.Tipo = mpacienterow.TipoModificacion;
                mpacientedatos.UsuarioTipo = mpacienterow.UsuarioTipo;
                mpacientedatos.Fecha = mpacienterow.Fecha;

                lista.Add(mpacientedatos);

            }

            return View(lista);
        }

        //Metodo que lleva al Index/Tabla de los MDoctores
        [HttpGet]
        public async Task<IActionResult> MDoctor()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Query a la tabla Mdoctor, trae TODOS LOS Registros
            var mdoctorquery = await teleDemoDBContext.MDoctor.ToListAsync();
            //Lista de la clase donde guardaremos datos
            List<MDoctorDatos> lista = new List<MDoctorDatos>();

            //Ciclo de llenado de lista de objetos
            foreach (var mdoctorrow in mdoctorquery)
            {
                //Creando objeto MDoctorDatos para almacenar
                MDoctorDatos mdoctordatos = new MDoctorDatos();
                
                //Consulta para unir la id de la tabla MDoctor con Doctor
                var doctordatos = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Id == mdoctorrow.DoctorId);
                
                //Consulta para unir la id de la tabla MDoctor con quien la modifico
                //Es asistente local sacamos su nombre
                var asislocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Id == mdoctorrow.UsuarioId);
                mdoctordatos.Usuario = asislocal.Nombre + " " + asislocal.Apellidos;
                
                if (doctordatos == null) continue;

                mdoctordatos.Nombre = doctordatos.Nombre + " " + doctordatos.Apellidos;
                mdoctordatos.Tipo = mdoctorrow.TipoModificacion;
                mdoctordatos.UsuarioTipo = mdoctorrow.UsuarioTipo;
                mdoctordatos.Fecha = mdoctorrow.Fecha;

                lista.Add(mdoctordatos);

            }
            //Nos vamos a la vista
            return View(lista);
        }

        //Metodo que lleva al Index/Tabla de los MAsistentesLocales
        [HttpGet]
        public async Task<IActionResult> MAsisL()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Query a la tabla MAsistenteLocal, trae TODOS LOS Registros
            var masisLquerry = await teleDemoDBContext.MAsistLocal.ToListAsync();
            //Lista de la clase donde guardaremos datos
            List<MAsisLDatos> lista = new List<MAsisLDatos>();

            //Ciclo de llenado de lista de objetos
            foreach (var masisLrow in masisLquerry)
            {
                //Creando objeto MAsisLDatos para almacenar
                MAsisLDatos masisLdatos = new MAsisLDatos();

                //Consulta para unir la id de la tabla MAsisLocal con AsistneteLocal
                var asisLdatos = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Id == masisLrow.AsistenteLocalId);
                
                //Consulta para unir la id de la tabla MAsisLocal con quien la modifico
                //Es asistente local sacamos su nombre
                var asislocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Id == masisLrow.UsuarioId);
                if(masisLdatos != null && asislocal != null)
                {
                    masisLdatos.Usuario = asislocal.Nombre + " " + asislocal.Apellidos;

                    if (asisLdatos == null) continue;

                    masisLdatos.Nombre = asisLdatos.Nombre + " " + asisLdatos.Apellidos;
                    masisLdatos.Tipo = masisLrow.TipoModificacion;
                    masisLdatos.UsuarioTipo = masisLrow.UsuarioTipo;
                    masisLdatos.Fecha = masisLrow.Fecha;

                    lista.Add(masisLdatos);
                }

            }
            //Ahora si lo enviamos a la vista Index
            return View(lista);
        }

        //Metodo que lleva al Index/Tabla de los MAsistentesRemoto
        [HttpGet]
        public async Task<IActionResult> MAsisR()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Query a la tabla MAsistenteRemoto, trae TODOS LOS Registros
            var masisRquerry = await teleDemoDBContext.MAsistRemoto.ToListAsync();
            //Lista de la clase donde guardaremos datos
            List<MAsisRDatos> lista = new List<MAsisRDatos>();

            //Ciclo de llenado de lista de objetos
            foreach (var masisRrow in masisRquerry)
            {
                //Creando objeto MAsisRDatos para almacenar
                MAsisRDatos masisRdatos = new MAsisRDatos();

                //Consulta para unir la id de la tabla MAsisRemoto con AsistneteRemoto
                var asisRdatos = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Id == masisRrow.AsistenteRemotoId);

                //Consulta para unir la id de la tabla MAsisRemoto con quien la modifico
                //Es asistente local sacamos su nombre
                var asislocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Id == masisRrow.UsuarioId);
                masisRdatos.Usuario = asislocal.Nombre + " " + asislocal.Apellidos;

                if (asisRdatos == null) continue;

                masisRdatos.Nombre = asisRdatos.Nombre + " " + asisRdatos.Apellidos;
                masisRdatos.Tipo = masisRrow.TipoModificacion;
                masisRdatos.UsuarioTipo = masisRrow.UsuarioTipo;
                masisRdatos.Fecha = masisRrow.Fecha;

                lista.Add(masisRdatos);

            }
            //Ahora si lo enviamos a la vista Index
            return View(lista);
        }

    }
}

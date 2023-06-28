using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Telemedicina.Data;
using Telemedicina.Models.Complejo;
using Telemedicina.Models.Dominio;
using Telemedicina.Models.PacienteFormularios;

namespace Telemedicina.Controllers
{
    public class PacientesController : Controller
    {
        //Poder utilizar el contexto de la base de datos
        private readonly TeleDemoDBContext teleDemoDBContext;
        public PacientesController(TeleDemoDBContext teleDemoDBContext)
        {
            this.teleDemoDBContext = teleDemoDBContext;
        }

        //Metodo para cancelar y regresar a la vista principal
        [HttpPost]
        public async Task<IActionResult> back()
        {
            return RedirectToAction("Index");
        }

        //Metodo que lleva al Index/Tabla de los pacientes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }

            //Recepcion si es que existe del mensaje de registro de paciente
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }
            var pacientes = await teleDemoDBContext.Pacientes.ToListAsync();
            //Ahora si lo enviamos a la vista Index
            return View(pacientes);
        }

        //Metodo que lleva a la vista de agregar un paciente
        [HttpGet]
        public async Task<IActionResult> agregarpaciente()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }

            //Recepcion si es que existe del mensaje de error
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }

            agregarPacienteViewModel pacientelista = new agregarPacienteViewModel();
            pacientelista.Localidades = await teleDemoDBContext.Localidad.ToListAsync();

            return View(pacientelista);
        }

        //Una vez realizado el registro, se llamara a este metodo, donde formularemos la informacion
        //para la base de datos.
        [HttpPost]
        public async  Task<IActionResult> agregarpaciente(agregarPacienteViewModel addPasienteRequest)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }

            //Verificamos si tiene algun error y en el caso regresar y enviar error
            if (!ModelState.IsValid && ModelState.ErrorCount != 0)
            {
                addPasienteRequest.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("agregarpaciente", addPasienteRequest);
            }

            //Verificacion de Curp
            //Verificamos si tenemos un paciente
            var curppasiente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Curp == addPasienteRequest.Curp);
            if(curppasiente != null)
            {
                ViewBag.Message = "Curp Ya registrada";
                addPasienteRequest.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("agregarpaciente", addPasienteRequest);
            }
            //Verificamos si tenemos un doctor
            var curpdoctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Curp == addPasienteRequest.Curp);
            if (curpdoctor != null)
            {
                ViewBag.Message = "Curp ya registrada";
                addPasienteRequest.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("agregarpaciente", addPasienteRequest);
            }
            //Verificamos si tenemos un asistente local
            var curpasisLocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Curp == addPasienteRequest.Curp);
            if (curpasisLocal != null)
            {
                ViewBag.Message = "Curp ya registrada";
                addPasienteRequest.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("agregarpaciente", addPasienteRequest);
            }
            //Verificamos si tenemos un asistente remoto
            var curpasisremoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Curp == addPasienteRequest.Curp);
            if (curpasisremoto != null)
            {
                ViewBag.Message = "Curp ya registrada";
                addPasienteRequest.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("agregarpaciente", addPasienteRequest);
            }


            //Una vez verificada la informacion base pasamos a la imagen
            var formFile = addPasienteRequest.File;
            var formFinalPath = "";
            //Verificacion de Imagen
            if (formFile != null && formFile.Length > 0)
            {
                //var filePath = Path.GetTempFileName();
                //Verificamos si el tipo de archivo es el correcto
                var formtype = formFile.ContentType;

                if (formtype != "image/jpeg")
                {   //Regresamos en el caso que no suba ningun archivo
                    ViewBag.Message = "Formato de Archivo no Invalido";
                    addPasienteRequest.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                    return View("agregarpaciente", addPasienteRequest);
                }

                //Ahora verificamos si tiene un peso adecuado que es Maximo 2MB
                var formsize = formFile.Length;
                System.Diagnostics.Debug.WriteLine(formsize);
                if (formsize > 2000000)
                {   //Regresamos en el caso que no suba ningun archivo
                    ViewBag.Message = "Archivo Pesado, Maximo 2MB";
                    addPasienteRequest.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                    return View("agregarpaciente", addPasienteRequest);
                }

                //Obtemenos la direccion del servidor, para meter la imagen
                string localPath = Directory.GetCurrentDirectory();
                localPath.Replace(@"\\", @"/");

                //Sacamos el nombre del archivo, tiene que ser diferente por cada paciente
                var fileName = Guid.NewGuid() + ".jpg";
                var ruta = string.Format("\\wwwroot\\images\\Paciente\\{0}", fileName);

                //Combinamos la ruta del proyecto con el archivo para subirlo
                string oPath = localPath + ruta;
                System.Diagnostics.Debug.WriteLine(oPath);

                //Ahora guardamos la ruta que se guardara en la base de datos
                formFinalPath = "\\images\\Paciente\\" + fileName;

                //Subimos el archivo a la carpeta
                using (var stream = System.IO.File.Create(oPath))
                {
                    await formFile.CopyToAsync(stream);
                }

            }//Fin del Analisis de la Imagen que subio
            else
            {
                //Regresamos en el caso que no suba ningun archivo
                ViewBag.Message = "Ingrese una Imagen del Paciente";
                addPasienteRequest.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("agregarpaciente", addPasienteRequest);
            }

            //Creamos el objeto con la informacion
            var paciente = new Paciente()
            {
                Nombre = addPasienteRequest.Nombre,
                Apellidos = addPasienteRequest.Apellidos,
                Sexo = addPasienteRequest.Sexo,
                Nacimiento = addPasienteRequest.Nacimiento,
                Sangre = addPasienteRequest.Sangre,
                Curp = addPasienteRequest.Curp,
                Domicilio = addPasienteRequest.Domicilio,
                Localidad = addPasienteRequest.Localidad,
                Municipio = addPasienteRequest.Municipio,
                Estado = addPasienteRequest.Estado,
                CodigoPostal = addPasienteRequest.CodigoPostal,
                Telefono = addPasienteRequest.Telefono,
                Correo = addPasienteRequest.Correo,
                Escolaridad = addPasienteRequest.Escolaridad,
                Ocupacion = addPasienteRequest.Ocupacion,
                Familiares = addPasienteRequest.Familiares,
                Hereditarios = addPasienteRequest.Hereditarios,
                Imagen = formFinalPath,
                Visilbe = "Si"
            };

            //Lo enviamos al contexto y gaurdamos
            await teleDemoDBContext.Pacientes.AddAsync(paciente);
            await teleDemoDBContext.SaveChangesAsync();
            //Mensaje para decir que si se guardo
            TempData["Message"] = "Paciente Registrado con Exito";


            //Sacamos el paciente creado, para sacar su respectiva id
            var pacientefinal = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Curp == paciente.Curp);
            //Registramos quien lo hizo
            var Mpaciente = new MPaciente()
            {
                PacienteId = pacientefinal.Id,
                TipoModificacion = "Registo",
                UsuarioId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                UsuarioTipo = HttpContext.Session.GetString(SessionVariables.SessionKeyType),
                Fecha = DateTime.Now
            };
            await teleDemoDBContext.MPaciente.AddAsync(Mpaciente);
            await teleDemoDBContext.SaveChangesAsync();

            //Terminamos y enviamos a una vista
            return RedirectToAction("Index");
        }

        //Metodo para poder colocar la informacion de un paciente en la vista para modificar
        [HttpGet]
        public async Task<IActionResult> modificarpaciente(int id)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }

            //Llamamos al contexto para poder comparar la id
            var paciente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x=> x.Id == id);

            if (paciente != null)
            {
                if (TempData["Message"] != null) 
                {
                    ViewBag.Message = TempData["Message"];
                    TempData["Message"] = "";
                }
                //En el caso de encontrar una id sacamos la informacion
                var viewModel = new UpdatePacienteViewModel()
                {
                    Id = paciente.Id,
                    Nombre = paciente.Nombre,
                    Apellidos = paciente.Apellidos,
                    Sexo = paciente.Sexo,
                    Nacimiento = paciente.Nacimiento,
                    Sangre = paciente.Sangre,
                    Curp = paciente.Curp,
                    Domicilio = paciente.Domicilio,
                    Localidad = paciente.Localidad,
                    Municipio = paciente.Municipio,
                    Estado = paciente.Estado,
                    CodigoPostal = paciente.CodigoPostal,
                    Telefono = paciente.Telefono,
                    Correo = paciente.Correo,
                    Escolaridad = paciente.Escolaridad,
                    Ocupacion = paciente.Ocupacion,
                    Familiares = paciente.Familiares,
                    Hereditarios = paciente.Hereditarios,
                    Localidades = await teleDemoDBContext.Localidad.ToListAsync()
                };
                //Llevamos a la vista con la informacion
                return await Task.Run(() => View("modificarpaciente", viewModel));
            }
            //En el caso de no encontrar una id asi, se reenvia al index
            ViewBag.Message = "No se encontro el Paciente";
            return RedirectToAction("Index");
        }

        //Una vez modificada y presiondo el boton de guardar, se usa este metodo
        [HttpPost]
        public async Task<IActionResult> modificarpaciente(UpdatePacienteViewModel model)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }

            //Verificamos si tiene algun error y en el caso regresar y enviar error
            if (!ModelState.IsValid && ModelState.ErrorCount != 0)
            {
                model.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("modificarpaciente", model);
            }

            var curppasiente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curppasiente != null && curppasiente.Id!=model.Id)
            {
                ViewBag.Message = "Curp Ya registrada";
                model.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("modificarpaciente", model);
            }

            //Verificacion de Curp
            //Verificamos si tenemos un doctor
            var curpdoctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curpdoctor != null)
            {
                ViewBag.Message = "Curp ya registrada";
                model.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("modificarpaciente", model);
            }
            //Verificamos si tenemos un asistente local
            var curpasisLocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curpasisLocal != null)
            {
                ViewBag.Message = "Curp ya registrada";
                model.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("modificarpaciente", model);
            }
            //Verificamos si tenemos un asistente remoto
            var curpasisremoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curpasisremoto != null)
            {
                ViewBag.Message = "Curp ya registrada";
                model.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("modificarpaciente", model);
            }


            //Llamamos al contexto para poder comparar la id
            var paciente = await teleDemoDBContext.Pacientes.FindAsync(model.Id);
            //en el caso de existir esa id, asignamos la informacion
            if (paciente != null) {

                //Modificacion de la Imagen
                var formFile = model.File;
                var formFinalPath = "";
                //Verificacion si es que actualizo la Imagen
                if (formFile != null && formFile.Length > 0)
                {
                    //Verificamos si el tipo de archivo es el correcto
                    var formtype = formFile.ContentType;

                    if (formtype != "image/jpeg")
                    {   //Regresamos en el caso que no suba ningun archivo
                        ViewBag.Message = "Formato de Archivo no Invalido";
                        model.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                        return View("modificarpaciente", model);
                    }

                    //Ahora verificamos si tiene un peso adecuado que es Maximo 2MB
                    var formsize = formFile.Length;
                    System.Diagnostics.Debug.WriteLine(formsize);
                    if (formsize > 2000000)
                    {   //Regresamos en el caso que no suba ningun archivo
                        ViewBag.Message = "Archivo Pesado, Maximo 2MB";
                        model.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                        return View("modificarpaciente", model);
                    }

                    //Obtemenos la direccion del servidor, para meter la imagen
                    string localPath = Directory.GetCurrentDirectory();
                    localPath.Replace(@"\\", @"/");

                    //Sacamos el nombre del archivo, tiene que ser diferente por cada paciente
                    var fileName = Guid.NewGuid() + ".jpg";
                    var ruta = string.Format("\\wwwroot\\images\\Paciente\\{0}", fileName);
                    System.Diagnostics.Debug.WriteLine(ruta);

                    //Combinamos la ruta del proyecto con el archivo para subirlo
                    string oPath = localPath + ruta;
                    System.Diagnostics.Debug.WriteLine(oPath);

                    //Ahora guardamos la ruta que se guardara en la base de datos
                    formFinalPath = "\\images\\Paciente\\" + fileName;

                    //Subimos el archivo a la carpeta
                    using (var stream = System.IO.File.Create(oPath))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                }//Fin del Analisis de la Imagen que subio

                //Empezamos a actualizar la informacion dada
                paciente.Nombre = model.Nombre;
                paciente.Apellidos = model.Apellidos;
                paciente.Sexo = model.Sexo;
                paciente.Nacimiento = model.Nacimiento;
                paciente.Sangre = model.Sangre;
                paciente.Curp = model.Curp;
                paciente.Domicilio = model.Domicilio;
                paciente.Localidad = model.Localidad;
                paciente.Municipio = model.Municipio;
                paciente.Estado = model.Estado;
                paciente.CodigoPostal = model.CodigoPostal;
                paciente.Telefono = model.Telefono;
                paciente.Correo = model.Correo;
                paciente.Escolaridad = model.Escolaridad;
                paciente.Ocupacion = model.Ocupacion;
                paciente.Familiares = model.Familiares;
                paciente.Hereditarios = model.Hereditarios;

                //Comprobamos si hizo la actualizacion de la foto
                if (formFinalPath.Length > 5)
                {
                    paciente.Imagen = formFinalPath;
                }
                
                //Agregamos la modificacion a la tabla
                var Mpaciente = new MPaciente()
                {
                    PacienteId = paciente.Id,
                    TipoModificacion = "Modificacion",
                    UsuarioId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                    UsuarioTipo = HttpContext.Session.GetString(SessionVariables.SessionKeyType),
                    Fecha = DateTime.Now
                };

                //Guardamos finalmente los datos en dicho pasiente
                await teleDemoDBContext.MPaciente.AddAsync(Mpaciente);
                await teleDemoDBContext.SaveChangesAsync();
                //Mensaje para decir que si se guardo
                TempData["Message"] = "Paciente Actualizado";
                //Retornamos al index
                return RedirectToAction("Index");
            }
            //No encontro id, vamos al index
            TempData["Message"] = "No se encontro al Paciente";
            return RedirectToAction("Index");
        }

        //Metodo cuando vamos a eliminar de forma visual un paciente
        [HttpPost]
        public async Task<IActionResult> eliminarpaciente(UpdatePacienteViewModel model)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }

            //Sacamos la id del paciente del contexto
            var paciente = await teleDemoDBContext.Pacientes.FindAsync(model.Id);
            //Vemos si esta vacia
            if(paciente != null)
            {
                //Asignamos que no se vea ese paciente
                paciente.Visilbe = "No";

                //Agregamos la eliminacion a la tabla
                var Mpaciente = new MPaciente()
                {
                    PacienteId = paciente.Id,
                    TipoModificacion = "Eliminar",
                    UsuarioId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                    UsuarioTipo = HttpContext.Session.GetString(SessionVariables.SessionKeyType),
                    Fecha = DateTime.Now
                };
                //Guardamos
                await teleDemoDBContext.MPaciente.AddAsync(Mpaciente);
                await teleDemoDBContext.SaveChangesAsync();
                //Regresamos al index
                return RedirectToAction("Index");
            }
            //En el caso que no se encontro, vamos al index
            return RedirectToAction("Index");
        }

    }
}

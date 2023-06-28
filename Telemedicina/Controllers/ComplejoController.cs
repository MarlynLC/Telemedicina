using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telemedicina.Data;
using Telemedicina.Models.Complejo;
using Telemedicina.Models.Dominio;

namespace Telemedicina.Controllers
{
    public class ComplejoController : Controller
    {
        //Referencia a la base de datos
        private readonly TeleDemoDBContext teleDemoDBContext;
        public ComplejoController(TeleDemoDBContext teleDemoDBContext)
        {
            this.teleDemoDBContext = teleDemoDBContext;
        }

        //Metodo para cancelar y regresar a la vista principal
        [HttpPost]
        public async Task<IActionResult> back()
        {
            return RedirectToAction("Index");
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


        //Dirigimos a la tabla de Complejos Existentes
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }
            //Recepcion si es que existe del mensaje de registro de paciente
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }
            //Querry a la tabla Complejo, trae todos los registros
            var complejoquerry = await teleDemoDBContext.Complejo.ToListAsync();
            //Lista donde vamos a guardar los datos
            List<Telemedicina.Models.Complejo.Complejo> lista = new List<Telemedicina.Models.Complejo.Complejo>();
            //Ahora vamos a llenar los los campos
            foreach (var complejorow in complejoquerry)
            {
                //Creamos objeto de la lista
                Telemedicina.Models.Complejo.Complejo ComplejoDatos = new Telemedicina.Models.Complejo.Complejo();

                //Consulta para unir la id de localidad de la tabla Complejo con Localidad
                var localidaddatos = await teleDemoDBContext.Localidad.FirstOrDefaultAsync(x=> x.Id == complejorow.IdLocalidad);

                //Unir los datos
                ComplejoDatos.Id = complejorow.Id;
                ComplejoDatos.Nombre = complejorow.Nombre;
                ComplejoDatos.Ubicacion = complejorow.Ubicacion;
                ComplejoDatos.Localidad = localidaddatos.Nombre;

                lista.Add(ComplejoDatos);
            }

            //Ahora mandamos a al vista index
            return View(lista);
        }//Fin del Metodo de llevar a la vista de Complejo(Tabla)

        //Enviamos a la vista para agregar un Complejo
        [HttpGet]
        public async Task<IActionResult> Complejod()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Recepcion si es que existe del mensaje de error
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }
            Complejod complejod = new Complejod();
            complejod.Localidades = await teleDemoDBContext.Localidad.ToListAsync();

            return View(complejod);
        }//Fin del Metodo de llevar el complejo a la vista de Complejod(Agregar)

        //Metodo donde se registra un complejo con la informacuon del formulario
        [HttpPost]
        public async Task<IActionResult> Complejod(Complejod complejod)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Verificamos si tiene algun error y en el caso regresar y enviar error
            if (!ModelState.IsValid && ModelState.ErrorCount != 0)
            {
                complejod.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("Complejod", complejod);
            }

            //Guardamos el complejo
            var complejonuevo = new Telemedicina.Models.Dominio.Complejo()
            {
                Nombre = complejod.Nombre,
                Ubicacion = complejod.Ubicacion,
                IdLocalidad = complejod.Localidadid
            };

            //Lo enviamos al contexto y guardamos
            await teleDemoDBContext.Complejo.AddAsync(complejonuevo);
            await teleDemoDBContext.SaveChangesAsync();
            //Mensaje para decir que si se guardo
            TempData["Message"] = "Localidad guardada con Exito";

            //Terminamos y enviamos a una vista
            return RedirectToAction("Index");

        }//Fin del metodo de registrar un complejo con los datos del formulario

        //Metodo para colocar la informacion de complejo en la vista de modificar
        [HttpGet]
        public async Task<IActionResult> Complejom(int id)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Buscamos la id en la tabla
            var complejo = await teleDemoDBContext.Complejo.FirstOrDefaultAsync(x => x.Id == id);
            //Checamos si llego algo
            if(complejo != null)
            {
                //Si lo encontramos
                var complejom = new Telemedicina.Models.Complejo.Complejod()
                {
                    Id = complejo.Id,
                    Nombre = complejo.Nombre,
                    Ubicacion = complejo.Ubicacion,
                    Localidadid = complejo.IdLocalidad,
                    Localidades = await teleDemoDBContext.Localidad.ToListAsync()
                };
                //Lo llevamos a la vista de informacion
                return await Task.Run(() => View("Complejom", complejom));
            }
            //El el caso de que no se encontro dicha Id, lo reenviamos a la vista de Complejo
            ViewBag.Message = "No se encontro el Complejo";
            return RedirectToAction("Index");
        }//Fin del metodo para colocar la informacion en la vista de modificar

        //Se modifico la informacion y la mando a comprobar para guardar
        [HttpPost]
        public async Task<IActionResult> Complejom(Complejod model)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Verificamos si tiene algun error y en el caso regresar y enviar error
            if (!ModelState.IsValid && ModelState.ErrorCount != 0)
            {
                model.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("Complejom", model);
            }

            //Llamamos al contexto para poder modificar la informacion
            var complejo = await teleDemoDBContext.Complejo.FindAsync(model.Id);
            //Comprovamos si existe esa id
            if( complejo != null )
            {
                //Si existe actualizar info
                complejo.Nombre = model.Nombre;
                complejo.Ubicacion = model.Ubicacion;
                complejo.IdLocalidad = model.Localidadid;

                //Actualizamos la informacion en la base de datos
                await teleDemoDBContext.SaveChangesAsync();
                //Mensaje para decir que si se guardo
                TempData["Message"] = "Complejo Actualizada";
                //Retornamos al index
                return RedirectToAction("Index");
            }//Fin del Complejo Null
            //No se encontro la id, vamso al index
            TempData["Message"] = "No se encontro el Complejo";
            return RedirectToAction("Index");

        }//Fin del metodo de modificacion de datos de Complejo





        //Metodo para cancelar y regresar a la vista principal
        [HttpPost]
        public async Task<IActionResult> back2()
        {
            return RedirectToAction("ComplejoDoctor");
        }

        //Enviamos a la tabla de Complejos Doctores
        [HttpGet]
        public async Task<ActionResult> ComplejoDoctor()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Recepcion si es que existe del mensaje de registro de paciente
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }

            //Querry a la tabla Complejo, trae todos los registros
            var ComplejoDoctorTabla = await teleDemoDBContext.ComplejoDoctor.ToListAsync();
            
            //Lista donde vamos a guardar los datos
            List<Telemedicina.Models.Complejo.ComplejoUsuario> listaComplejoDoctor = new List<Telemedicina.Models.Complejo.ComplejoUsuario>();
            
            //Ahora vamos a llenar los los campos
            foreach (var ComplejoDoctorRow in ComplejoDoctorTabla)
            {
                //Creamos objeto de la lista
                Telemedicina.Models.Complejo.ComplejoUsuario ComplejoDoctorDatos = new Telemedicina.Models.Complejo.ComplejoUsuario();

                //Consulta para unir la id de localidad de la tabla Complejo con Localidad
                var Complejodatos = await teleDemoDBContext.Complejo.FirstOrDefaultAsync(x => x.Id == ComplejoDoctorRow.ComplejoId);
                var Doctordatos = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Id == ComplejoDoctorRow.DoctorId);

                //Unir los datos
                ComplejoDoctorDatos.Id = ComplejoDoctorRow.Id;
                ComplejoDoctorDatos.UsuarioNombre = Doctordatos.Nombre + " " + Doctordatos.Apellidos;
                ComplejoDoctorDatos.ComplejoNombre = Complejodatos.Nombre;

                listaComplejoDoctor.Add(ComplejoDoctorDatos);
            }

            //Ahora mandamos a al vista index
            return View(listaComplejoDoctor);
        }//Fin del Metodo de llevar a la vista de ComplejoDoctor(Tabla)

        //Enviamos a la vista de agregar complejodoctor
        [HttpGet]
        public async Task<IActionResult> ComplejoDoctorD()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Recepcion si es que existe del mensaje de error
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }
            ComplejoUsuario complejodoc = new ComplejoUsuario();
            complejodoc.Doctores = await teleDemoDBContext.Doctors.ToListAsync();
            complejodoc.Complejos = await teleDemoDBContext.Complejo.ToListAsync();

            return View(complejodoc);
        }//Fin del Metodo de llevar el complejo a la vista de ComplejoDoctorD(Agregar)

        //Metodo donde se registra un complejodoctor con la informacuon del formulario
        [HttpPost]
        public async Task<IActionResult> ComplejoDoctorD(ComplejoUsuario complejoUsuario)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Verificamos si tiene algun error y en el caso regresar y enviar error
            if (!ModelState.IsValid && ModelState.ErrorCount != 0)
            {
                complejoUsuario.Doctores = await teleDemoDBContext.Doctors.ToListAsync();
                return View("ComplejoDoctorD", complejoUsuario);
            }

            //Guardamos el complejo
            var complejodoctor = new Telemedicina.Models.Dominio.ComplejoDoctor()
            {
                DoctorId = complejoUsuario.DoctorId,
                ComplejoId = complejoUsuario.ComplejoId
            };

            //Lo enviamos al contexto y guardamos
            await teleDemoDBContext.ComplejoDoctor.AddAsync(complejodoctor);
            await teleDemoDBContext.SaveChangesAsync();
            //Mensaje para decir que si se guardo
            TempData["Message"] = "Doctor Asigando a Complejo con Exito";

            //Terminamos y enviamos a una vista
            return RedirectToAction("ComplejoDoctor");

        }//Fin del metodo de registrar un complejo con los datos del formulario

        //Metodo para colocar la informacion de complejodoctor en la vista de modificar
        [HttpGet]
        public async Task<IActionResult> ComplejoDoctorM(int id)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Buscamos la id en la tabla
            var complejodoctor = await teleDemoDBContext.ComplejoDoctor.FirstOrDefaultAsync(x => x.Id == id);

            //Checamos si llego algo
            if (complejodoctor != null)
            {
                //Si lo encontramos
                var complejom = new Telemedicina.Models.Complejo.ComplejoUsuario()
                {
                    Id = complejodoctor.Id,
                    DoctorId = complejodoctor.DoctorId,
                    ComplejoId = complejodoctor.ComplejoId,
                    Doctores = await teleDemoDBContext.Doctors.ToListAsync(),
                    Complejos = await teleDemoDBContext.Complejo.ToListAsync(),
                };
                //Lo llevamos a la vista de informacion
                return await Task.Run(() => View("ComplejoDoctorM", complejom));
            }
            //El el caso de que no se encontro dicha Id, lo reenviamos a la vista de Complejo
            ViewBag.Message = "No se encontro esa vinculacion";
            return RedirectToAction("ComplejoDoctor");
        }//Fin del metodo para colocar la informacion en la vista de modificar

        //Se modifico la informacion y la mando a comprobar para guardar
        [HttpPost]
        public async Task<IActionResult> ComplejoDoctorM(ComplejoUsuario model)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Verificamos si tiene algun error y en el caso regresar y enviar error
            if (!ModelState.IsValid && ModelState.ErrorCount != 0)
            {
                model.Doctores = await teleDemoDBContext.Doctors.ToListAsync();
                model.Complejos = await teleDemoDBContext.Complejo.ToListAsync();
                return View("ComplejoDoctorM", model);
            }

            //Llamamos al contexto para poder modificar la informacion
            var complejo = await teleDemoDBContext.ComplejoDoctor.FindAsync(model.Id);
            //Comprovamos si existe esa id
            if (complejo != null)
            {
                //Si existe actualizar info
                complejo.DoctorId = model.DoctorId;
                complejo.ComplejoId = model.ComplejoId;

                //Actualizamos la informacion en la base de datos
                await teleDemoDBContext.SaveChangesAsync();
                //Mensaje para decir que si se guardo
                TempData["Message"] = "Vinculacion Actualizada";
                //Retornamos al index
                return RedirectToAction("ComplejoDoctor");
            }//Fin del Complejo Null
            //No se encontro la id, vamso al index
            TempData["Message"] = "No se encontro la Vinculacion";
            return RedirectToAction("ComplejoDoctor");

        }//Fin del metodo de modificacion de datos de Complejo-Doctor



        //Metodo para cancelar y regresar a la vista principal
        [HttpPost]
        public async Task<IActionResult> back3()
        {
            return RedirectToAction("ComplejoAsistR");
        }

        //Enviamos a la tabla de Complejos AsistentesRemotos
        [HttpGet]
        public async Task<ActionResult> ComplejoAsistR()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Recepcion si es que existe del mensaje de registro de paciente
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }

            //Querry a la tabla Complejo, trae todos los registros
            var ComplejoAsistRTabla = await teleDemoDBContext.ComplejoAsistR.ToListAsync();

            //Lista donde vamos a guardar los datos
            List<Telemedicina.Models.Complejo.ComplejoUsuario> listaComplejoAsistR = new List<Telemedicina.Models.Complejo.ComplejoUsuario>();

            //Ahora vamos a llenar los los campos
            foreach (var ComplejoAsistRRow in ComplejoAsistRTabla)
            {
                //Creamos objeto de la lista
                Telemedicina.Models.Complejo.ComplejoUsuario ComplejoAsistRDatos = new Telemedicina.Models.Complejo.ComplejoUsuario();

                //Consulta para unir la id de localidad de la tabla Complejo con Localidad
                var Complejodatos = await teleDemoDBContext.Complejo.FirstOrDefaultAsync(x => x.Id == ComplejoAsistRRow.ComplejoId);
                var AsistRdatos = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Id == ComplejoAsistRRow.AsistRId);

                //Unir los datos
                ComplejoAsistRDatos.Id = ComplejoAsistRRow.Id;
                ComplejoAsistRDatos.UsuarioNombre = AsistRdatos.Nombre + " " + AsistRdatos.Apellidos;
                ComplejoAsistRDatos.ComplejoNombre = Complejodatos.Nombre;

                listaComplejoAsistR.Add(ComplejoAsistRDatos);
            }

            //Ahora mandamos a al vista index
            return View(listaComplejoAsistR);
        }//Fin del Metodo de llevar a la vista de ComplejoAsistR(Tabla)

        //Enviamos a la vista de agregar complejoasistR
        [HttpGet]
        public async Task<IActionResult> ComplejoAsistRD()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Recepcion si es que existe del mensaje de error
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = ""; 
            }
            ComplejoUsuario complejoasitR = new ComplejoUsuario();
            complejoasitR.AsistenteRemotos = await teleDemoDBContext.AsistenteRemotos.ToListAsync();
            complejoasitR.Complejos = await teleDemoDBContext.Complejo.ToListAsync();

            return View(complejoasitR);
        }//Fin del Metodo de llevar el complejo a la vista de ComplejoAsistRD(Agregar)

        //Metodo donde se registra un complejodoctor con la informacuon del formulario
        [HttpPost]
        public async Task<IActionResult> ComplejoAsistRD(ComplejoUsuario complejoUsuario)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Verificamos si tiene algun error y en el caso regresar y enviar error
            if (!ModelState.IsValid && ModelState.ErrorCount != 0)
            {
                complejoUsuario.Doctores = await teleDemoDBContext.Doctors.ToListAsync();
                return View("ComplejoAsistRD", complejoUsuario);
            }

            //Guardamos el complejo
            var complejoAsistR = new Telemedicina.Models.Dominio.ComplejoAsistR()
            {
                AsistRId = complejoUsuario.AsistenteRemotoId,
                ComplejoId = complejoUsuario.ComplejoId
            };

            //Lo enviamos al contexto y guardamos
            await teleDemoDBContext.ComplejoAsistR.AddAsync(complejoAsistR);
            await teleDemoDBContext.SaveChangesAsync();
            //Mensaje para decir que si se guardo
            TempData["Message"] = "Asistente Remoto Asigando a Complejo con Exito";

            //Terminamos y enviamos a una vista
            return RedirectToAction("ComplejoAsistR");

        }//Fin del metodo de registrar un complejo con los datos del formulario

        //Metodo para colocar la informacion de complejodoctor en la vista de modificar
        [HttpGet]
        public async Task<IActionResult> ComplejoAsistRM(int id)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Buscamos la id en la tabla
            var complejoAsistR = await teleDemoDBContext.ComplejoAsistR.FirstOrDefaultAsync(x => x.Id == id);

            //Checamos si llego algo
            if (complejoAsistR != null)
            {
                //Si lo encontramos
                var complejom = new Telemedicina.Models.Complejo.ComplejoUsuario()
                {
                    Id = complejoAsistR.Id,
                    AsistenteRemotoId = complejoAsistR.AsistRId,
                    ComplejoId = complejoAsistR.ComplejoId,
                    AsistenteRemotos = await teleDemoDBContext.AsistenteRemotos.ToListAsync(),
                    Complejos = await teleDemoDBContext.Complejo.ToListAsync(),
                };
                //Lo llevamos a la vista de informacion
                return await Task.Run(() => View("ComplejoAsistRM", complejom));
            }
            //El el caso de que no se encontro dicha Id, lo reenviamos a la vista de Complejo
            ViewBag.Message = "No se encontro esa vinculacion";
            return RedirectToAction("ComplejoAsistR");
        }//Fin del metodo para colocar la informacion en la vista de modificar

        //Se modifico la informacion y la mando a comprobar para guardar
        [HttpPost]
        public async Task<IActionResult> ComplejoAsistRM(ComplejoUsuario model)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Verificamos si tiene algun error y en el caso regresar y enviar error
            if (!ModelState.IsValid && ModelState.ErrorCount != 0)
            {
                model.AsistenteRemotos = await teleDemoDBContext.AsistenteRemotos.ToListAsync();
                model.Complejos = await teleDemoDBContext.Complejo.ToListAsync();
                return View("ComplejoAsistRM", model);
            }

            //Llamamos al contexto para poder modificar la informacion
            var complejo = await teleDemoDBContext.ComplejoAsistR.FindAsync(model.Id);
            //Comprovamos si existe esa id
            if (complejo != null)
            {
                //Si existe actualizar info
                complejo.AsistRId = model.AsistenteRemotoId;
                complejo.ComplejoId = model.ComplejoId;

                //Actualizamos la informacion en la base de datos
                await teleDemoDBContext.SaveChangesAsync();
                //Mensaje para decir que si se guardo
                TempData["Message"] = "Vinculacion Actualizada";
                //Retornamos al index
                return RedirectToAction("ComplejoAsistR");
            }//Fin del Complejo Null
            //No se encontro la id, vamso al index
            TempData["Message"] = "No se encontro la Vinculacion";
            return RedirectToAction("ComplejoAsistR");

        }//Fin del metodo de modificacion de datos de Complejo



    }
}

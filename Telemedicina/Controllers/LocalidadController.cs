using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telemedicina.Data;
using Telemedicina.Models.Complejo;
using Telemedicina.Models.Dominio;
using Telemedicina.Models.Localidad;

namespace Telemedicina.Controllers
{
    public class LocalidadController : Controller
    {
        //Poder utilizar el contexto de la base de datos
        private readonly TeleDemoDBContext teleDemoDBContext;
        public LocalidadController(TeleDemoDBContext teleDemoDBContext)
        {
            this.teleDemoDBContext = teleDemoDBContext;
        }

        //Metodo para cancelar y regresar a la vista principal
        [HttpPost]
        public async Task<IActionResult> back()
        {
            return RedirectToAction("Index");
        }

        //Dirigimos a la Tabla de Localidades Existentes
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
            var Localidades = await teleDemoDBContext.Localidad.ToListAsync();
            //Ahora si lo enviamos a la vista Index
            return View(Localidades);
        }//Fin del Metodo de llevar a la vista de Localidad(Tabla)


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


        //Enviamos a la vista para agregar una localidad
        [HttpGet]
        public IActionResult Localidadd()
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
            return View();
        }//Fin del Metodo de Llevar al a vista de Localidadd(Agregar)


        //Vista donde se registra la localidad colocada en el formulario
        [HttpPost]
        public async Task<IActionResult> Localidadd(Localidadd localidad)
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
                return View("Localidadd", localidad);
            }

            //Verificamos si el nombre no esta ya en uso
            var querryLocalidad = await teleDemoDBContext.Localidad.FirstOrDefaultAsync(x => x.Nombre == localidad.Nombre);
            if(querryLocalidad != null) 
            {
                ViewBag.Message = "Localidad ya registrada";
                return View("Localidadd", localidad);
            }

            //Ahora si Guardamos la localidad
            var localidadnueva = new Localidad()
            {
                Nombre = localidad.Nombre,
                Municipio = localidad.Municipio,
                Estado = localidad.Estado
            };

            //Lo enviamos al contexto y guardamos
            await teleDemoDBContext.Localidad.AddAsync(localidadnueva);
            await teleDemoDBContext.SaveChangesAsync();
            //Mensaje para decir que si se guardo
            TempData["Message"] = "Localidad guardada con Exito";

            //Terminamos y enviamos a una vista
            return RedirectToAction("Index");

        }//Fin del Metodo de Guardar Localidad

        //Metodo para colocar la informacion de Localidad en la vista para modificar
        [HttpGet]
        public async Task<IActionResult> Localidadm(int id)
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

            //Llamamos al contexto para poder comparar la id que llego
            var localidad = await teleDemoDBContext.Localidad.FirstOrDefaultAsync(x => x.Id == id);
            //Checamos si llego algo
            if(localidad != null)
            {
                //Si encontramos una Id que conside
                var localidadm = new Localidadm()
                {
                    Id = localidad.Id,
                    Nombre = localidad.Nombre,
                    Municipio = localidad.Municipio,
                    Estado = localidad.Estado
                };
                //Lo llevamos a la vista con la informacion
                return await Task.Run(() => View("Localidadm", localidadm));
            }
            //El el caso de que no se encontro dicha Id, lo reenviamos a la vista de Localidad
            ViewBag.Message = "No se encontro la Localidad";
            return RedirectToAction("Index");
        }//Fin del Metodo de Llevar a la vista de modificar Localidad

        //Se modifico la informacion y la mando a comprobar para guardar
        [HttpPost]
        public async Task<IActionResult> Localidadm(Localidadm model)
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
                return View("Localidadm", model);
            }

            //Verificamos si el nombre no esta en uso por otra localidad
            var querryLocalidad = await teleDemoDBContext.Localidad.FirstOrDefaultAsync(x => x.Nombre == model.Nombre);
            if (querryLocalidad != null && querryLocalidad.Id != model.Id)
            {
                ViewBag.Message = "Localidad ya registrada";
                return View("Localidadm", model);
            }

            //LLamamos al contexto para poder modificar la informacion
            var Localidad = await teleDemoDBContext.Localidad.FindAsync(model.Id);
            //Comprovamos si existe esa id
            if(Localidad != null)
            {
                //Si existe, a actualizar
                Localidad.Nombre = model.Nombre;
                Localidad.Estado = model.Estado;
                Localidad.Municipio = model.Municipio;

                //Actualizamos los datos en la base de datos
                await teleDemoDBContext.SaveChangesAsync();
                //Mensaje para decir que si se guardo
                TempData["Message"] = "Localidad Actualizada";
                //Retornamos al index
                return RedirectToAction("Index");
            }//Fin el If Localidad Null
            //No se encontro la id, vamso al index
            TempData["Message"] = "No se encontro la Localidad";
            return RedirectToAction("Index");

        }//Fin del Metodo de guardar modificacion en Localidad


        //Metodo para cancelar y regresar a la vista principal
        [HttpPost]
        public async Task<IActionResult> back2()
        {
            return RedirectToAction("LocalAsistente");
        }

        //Enviamos a la tabla de Complejos AsistentesLocales
        [HttpGet]
        public async Task<ActionResult> LocalAsistente()
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
            var LocalAsistenteTabla = await teleDemoDBContext.LocalAsistente.ToListAsync();

            //Lista donde vamos a guardar los datos
            List<Telemedicina.Models.Localidad.LocalAsistente> listaLocalidadAsistente = new List<Telemedicina.Models.Localidad.LocalAsistente>();

            //Ahora vamos a llenar los los campos
            foreach (var LocalidadAsistenteRow in LocalAsistenteTabla)
            {
                //Creamos objeto de la lista
                Telemedicina.Models.Localidad.LocalAsistente LocalidadAsistenteDatos = new Telemedicina.Models.Localidad.LocalAsistente();

                //Consulta para unir la id de localidad de la tabla Complejo con Localidad
                var Localidaddatos = await teleDemoDBContext.Localidad.FirstOrDefaultAsync(x => x.Id == LocalidadAsistenteRow.LocalidadID);
                var Asistentedatos = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Id == LocalidadAsistenteRow.AsistenteLID);

                //Unir los datos
                LocalidadAsistenteDatos.Id = LocalidadAsistenteRow.Id;
                LocalidadAsistenteDatos.AsistenteNombre = Asistentedatos.Nombre + " " + Asistentedatos.Apellidos;
                LocalidadAsistenteDatos.LocalidadNombre = Localidaddatos.Nombre;

                listaLocalidadAsistente.Add(LocalidadAsistenteDatos);
            }

            //Ahora mandamos a al vista index
            return View(listaLocalidadAsistente);
        }//Fin del Metodo de llevar a la vista de LocalidadAsistente(Tabla)

        //Enviamos a la vista de agregar Localidades Asistentes Remotos
        [HttpGet]
        public async Task<IActionResult> LocalAsistenteD()
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
            Models.Localidad.LocalAsistente localidadesAsistenteLocal = new Models.Localidad.LocalAsistente();
            localidadesAsistenteLocal.AsistentesLocales = await teleDemoDBContext.AsistenteLocal.ToListAsync();
            localidadesAsistenteLocal.Localidades = await teleDemoDBContext.Localidad.ToListAsync();

            return View(localidadesAsistenteLocal);
        }//Fin del Metodo de llevar el complejo a la vista de LocalidadAsistente(Agregar)

        //Metodo donde se registra un LocalidadAsistente con la informacuon del formulario
        [HttpPost]
        public async Task<IActionResult> LocalAsistenteD(Models.Localidad.LocalAsistente localAsistente)
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
                localAsistente.AsistentesLocales = await teleDemoDBContext.AsistenteLocal.ToListAsync();
                localAsistente.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("LocalAsistenteD", localAsistente);
            }

            //Guardamos el complejo
            var localAsistentefinal = new Telemedicina.Models.Dominio.LocalAsistente()
            {
                AsistenteLID = localAsistente.AsistenteLID,
                LocalidadID = localAsistente.LocalidadID
            };

            //Lo enviamos al contexto y guardamos
            await teleDemoDBContext.LocalAsistente.AddAsync(localAsistentefinal);
            await teleDemoDBContext.SaveChangesAsync();
            //Mensaje para decir que si se guardo
            TempData["Message"] = "Asistente Local Asigando a Localidad con Exito";

            //Terminamos y enviamos a una vista
            return RedirectToAction("LocalAsistente");

        }//Fin del metodo de registrar un complejo con los datos del formulario

        //Metodo para colocar la informacion de LocalAsistente en la vista de modificar
        [HttpGet]
        public async Task<IActionResult> LocalAsistenteM(int id)
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
            var localiAsistente = await teleDemoDBContext.LocalAsistente.FirstOrDefaultAsync(x => x.Id == id);

            //Checamos si llego algo
            if (localiAsistente != null)
            {
                //Si lo encontramos
                var localidadA = new Telemedicina.Models.Localidad.LocalAsistente()
                {
                    Id = localiAsistente.Id,
                    AsistenteLID = localiAsistente.AsistenteLID,
                    LocalidadID = localiAsistente.LocalidadID,
                    AsistentesLocales = await teleDemoDBContext.AsistenteLocal.ToListAsync(),
                    Localidades = await teleDemoDBContext.Localidad.ToListAsync(),
                };
                //Lo llevamos a la vista de informacion
                return await Task.Run(() => View("LocalAsistenteM", localidadA));
            }
            //El el caso de que no se encontro dicha Id, lo reenviamos a la vista de LocalidadAsistenteLocal
            ViewBag.Message = "No se encontro esa vinculacion";
            return RedirectToAction("LocalAsistente");
        }//Fin del metodo para colocar la informacion en la vista de modificar

        //Se modifico la informacion y la mando a comprobar para guardar
        [HttpPost]
        public async Task<IActionResult> LocalAsistenteM(Models.Localidad.LocalAsistente model)
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
                model.AsistentesLocales = await teleDemoDBContext.AsistenteLocal.ToListAsync();
                model.Localidades = await teleDemoDBContext.Localidad.ToListAsync();
                return View("LocalAsistenteM", model);
            }

            //Llamamos al contexto para poder modificar la informacion
            var localidadAsistente = await teleDemoDBContext.LocalAsistente.FindAsync(model.Id);
            //Comprovamos si existe esa id
            if (localidadAsistente != null)
            {
                //Si existe actualizar info
                localidadAsistente.AsistenteLID = model.AsistenteLID;
                localidadAsistente.LocalidadID = model.LocalidadID;

                //Actualizamos la informacion en la base de datos
                await teleDemoDBContext.SaveChangesAsync();
                //Mensaje para decir que si se guardo
                TempData["Message"] = "Vinculacion Actualizada";
                //Retornamos al index
                return RedirectToAction("LocalAsistente");
            }//Fin del Complejo Null
            //No se encontro la id, vamso al index
            TempData["Message"] = "No se encontro la Vinculacion";
            return RedirectToAction("LocalAsistente");

        }//Fin del metodo de modificacion de datos de Localidad-AsistenteLocal


    }
}

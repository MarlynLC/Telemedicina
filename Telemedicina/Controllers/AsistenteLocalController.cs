using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telemedicina.Data;
using Telemedicina.Models.Dominio;
using Telemedicina.Models.AsistenteLocalFormularios;
using System.Numerics;
using Microsoft.IdentityModel.Tokens;

namespace Telemedicina.Controllers
{
    public class AsistenteLocalController : Controller
    {
        //Poder utilizar el contexto de la base de datos
        private readonly TeleDemoDBContext teleDemoDBContext;

        public AsistenteLocalController(TeleDemoDBContext teleDemoDBContext)
        {
            this.teleDemoDBContext = teleDemoDBContext;
        }

        //Metodo para cancelar y regresar a la vista principal
        [HttpPost]
        public async Task<IActionResult> back()
        {
            return RedirectToAction("Index");
        }

        //Metodo para ir a Index/tabla de AsistenteLocal
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

            //Recepcion si es que existe el mensaje por registrar un asistenteLocal
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }
            var asistentelocal = await teleDemoDBContext.AsistenteLocal.ToListAsync();
            return View(asistentelocal);
        }



        //Metodo para ir a la vista de agregar AsistenteLocal
        [HttpGet]
        public IActionResult agregarasistentelocal()
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

            //Recepcion si es que existe el mensaje de error
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }
            return View();
        }



        //Una vez realizado el registro, se llamara a este metodo, donde formularemos la informacion para la base de datos.
        [HttpPost]
        public async Task<IActionResult> agregarasistentelocal(agregarasistentelocalViewModel addasistentelocal)
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
                return View("agregarasistentelocal", addasistentelocal);
            }

            //Verificacion de Curp
            //Verificamos si tenemos un paciente
            var curppasiente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Curp == addasistentelocal.Curp);
            if (curppasiente != null)
            {
                ViewBag.Message = "Curp Ya registrada";
                return View("agregarasistentelocal", addasistentelocal);
            }
            //Verificamos si tenemos un doctor
            var curpdoctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Curp == addasistentelocal.Curp);
            if (curpdoctor != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("agregarasistentelocal", addasistentelocal);
            }
            //Verificamos si tenemos un asistente local
            var curpasisLocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Curp == addasistentelocal.Curp);
            if (curpasisLocal != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("agregarasistentelocal", addasistentelocal);
            }
            //Verificamos si tenemos un asistente remoto
            var curpasisremoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Curp == addasistentelocal.Curp);
            if (curpasisremoto != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("agregarasistentelocal", addasistentelocal);
            }

            //Una vez verificada la informacion inicial pasamos a la imagen
            var formFile = addasistentelocal.File;
            var formFinalPath = "";
            //Verificacion de la imagen
            if (formFile != null && formFile.Length > 0)
            {

                //Verificamos si el tipo de archivo es el correcto
                var formtype = formFile.ContentType;
                //System.Diagnostics.Debug.WriteLine(formtype);
                if (formtype != "image/jpeg")
                {   
                    //Regresamos en el caso que no suba un archivo correcto
                    ViewBag.Message = "Formato de Archivo no Valido para Foto";
                    return View("agregarasistentelocal", addasistentelocal);
                }


                //Ahora verificamos si tiene un peso adecuado que es Maximo 2MB
                var formsize = formFile.Length;
                if (formsize > 2000000)
                {
                    //Regresamos en el caso que sobrepase el peso maximo
                    ViewBag.Message = "Archivo Foto muy Pesado, Maximo 2MB";
                    return View("agregarasistentelocal", addasistentelocal);
                }


                //Procedemos a Guardar la foto

                //Obtemenos la direccion del servidor, para meter la imagen
                string localPath = Directory.GetCurrentDirectory();
                localPath.Replace(@"\\", @"/");


                //Sacamos el nombre del archivo, tiene que ser diferente por cada asistente local
                var fileName = Guid.NewGuid() + ".jpg";
                //Creamos la ruta hacia la carpeta de imagenes de asistente local
                var ruta = string.Format("\\wwwroot\\images\\Asistentelocal\\{0}", fileName);


                //Combinamos la ruta del proyecto con el archivo para subirlo
                string oPath = localPath + ruta;


                //Ahora guardamos la ruta que se guardara en la base de datos
                formFinalPath = "\\images\\Asistentelocal\\" + fileName;


                //Subimos el archivo foto a la carpeta
                using (var stream = System.IO.File.Create(oPath))
                {
                    await formFile.CopyToAsync(stream);
                }

            }//Fin del Analisis de la Imagen que subio
            else
            {
                //Regresamos en el caso que no suba ningun archivo
                return View("agregarasistentelocal", addasistentelocal);
            }//Final de la verificacion de imagenes



            //Creamos el objeto asistentelocal para realizar el registro
            var asistentelocal = new AsistenteLocal()
            {
                Nombre = addasistentelocal.Nombre,
                Apellidos = addasistentelocal.Apellidos,
                Sexo = addasistentelocal.Sexo,
                Nacimiento = addasistentelocal.Nacimiento,
                Sangre = addasistentelocal.Sangre,
                Curp = addasistentelocal.Curp,
                Correo = addasistentelocal.Correo,
                Telefono = addasistentelocal.Telefono,
                Matricula = addasistentelocal.Matricula,
                Especialidad = addasistentelocal.Especialidad,
                Jerarquia = addasistentelocal.Jerarquia,
                Password = addasistentelocal.Password,
                Imagen = formFinalPath,
                FechaIngreso = DateTime.Now,
                Visilbe = "Si"
            };


            //Lo enviamos al contexto y guardamos
            await teleDemoDBContext.AsistenteLocal.AddAsync(asistentelocal);
            await teleDemoDBContext.SaveChangesAsync();
            //Mensaje para decir que si se guardo
            TempData["Message"] = "Asistente Local Guardado con Exito";
            
            //Sacamos el asistente local, para sacar su respectiva id
            var asislocalf = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Curp == asistentelocal.Curp);
            //Registramos quien lo hizo
            var MAsisL = new MAsistLocal()
            {
                AsistenteLocalId = asislocalf.Id,
                TipoModificacion = "Registro",
                UsuarioId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                UsuarioTipo = HttpContext.Session.GetString(SessionVariables.SessionKeyType),
                Fecha = DateTime.Now
            };
            await teleDemoDBContext.MAsistLocal.AddAsync(MAsisL);
            await teleDemoDBContext.SaveChangesAsync();

            //Terminamos y enviamos a una vista
            return RedirectToAction("Index");
        }//Fin del Metodo de Guardar Doctor


        //Metodo para poder colocar la informacion de un asistente local en la vista para modificar
        [HttpGet]
        public async Task<IActionResult> modificarasistentelocal(int id)
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

            //Llamamos al contexto para poder comparar la id
            var asistentelocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Id == id);

            if (asistentelocal != null)
            {
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                    TempData["Message"] = "";
                }
                //En el caso de encontrar una id sacamos la informacion
                var viewModel = new UpdateAsistenteLocalViewModel()
                {
                    Id = asistentelocal.Id,
                    Nombre = asistentelocal.Nombre,
                    Apellidos = asistentelocal.Apellidos,
                    Sexo = asistentelocal.Sexo,
                    Sangre = asistentelocal.Sangre,
                    Nacimiento = asistentelocal.Nacimiento,
                    Curp = asistentelocal.Curp,
                    Correo = asistentelocal.Correo,
                    Telefono = asistentelocal.Telefono,
                    Matricula = asistentelocal.Matricula,
                    Especialidad = asistentelocal.Especialidad,
                    Jerarquia = asistentelocal.Jerarquia,
                    Password = asistentelocal.Password,
                };//Fin del if de null asistentelocal
                //Llevamos a la vista con la informacion
                return await Task.Run(() => View("modificarasistentelocal", viewModel));
            }
            //En el caso de no encontrar una id asi, se reenvia al index
            ViewBag.Message = "No se encontro el Asistente Local";
            return RedirectToAction("Index");
        }//Fin del metodo para mostrar los datos del asistente local



        //Se modifico la informacion y la mando a comprobar para guardar
        [HttpPost]
        public async Task<IActionResult> modificarasistentelocal(UpdateAsistenteLocalViewModel model)
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
                //var errors = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();

                return View("modificarasistentelocal", model);
            }

            //Verificacion de Curp
            //Verificamos si tenemos un paciente
            var curppasiente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curppasiente != null)
            {
                ViewBag.Message = "Curp Ya registrada";
                return View("modificarasistentelocal", model);
            }
            //Verificamos si tenemos un doctor
            var curpdoctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curpdoctor != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("modificarasistentelocal", model);
            }
            //Verificamos si tenemos un asistente local
            var curpasisLocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curpasisLocal != null && curpasisLocal.Id!=model.Id)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("modificarasistentelocal", model);
            }
            //Verificamos si tenemos un asistente remoto
            var curpasisremoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curpasisremoto != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("modificarasistentelocal", model);
            }

            //Llamamos al contexto para poder comparar la id
            var asistentelocal = await teleDemoDBContext.AsistenteLocal.FindAsync(model.Id);
            //en el caso de existir esa id, asignamos la informacion
            if (asistentelocal != null)
            {


                //Modificacion de la Imagen
                var formFile = model.File;
                var formFinalPath = "";
                //Verificacion si es que actualizo la Imagen
                if (formFile != null && formFile.Length > 0)
                {
                    //Verificamos si el tipo de archivo es el correcto
                    var formtype = formFile.ContentType;
                    //System.Diagnostics.Debug.WriteLine(formtype);
                    if (formtype != "image/jpeg")
                    {   //Regresamos en el caso que no suba ningun archivo
                        ViewBag.Message = "Formato de Archivo no Invalido para Imagen";
                        return View("modificarasistentelocal", model);
                    }

                    //Ahora verificamos si tiene un peso adecuado que es Maximo 2MB
                    var formsize = formFile.Length;
                    System.Diagnostics.Debug.WriteLine(formsize);
                    if (formsize > 2000000)
                    {   //Regresamos en el caso que no suba ningun archivo
                        ViewBag.Message = "Archivo Pesado en Imagen, Maximo 2MB";
                        return View("modificarasistentelocal", model);
                    }

                    //Obtemenos la direccion del servidor, para meter la imagen
                    string localPath = Directory.GetCurrentDirectory();
                    localPath.Replace(@"\\", @"/");

                    //Sacamos el nombre del archivo, tiene que ser diferente por cada asistente local
                    var fileName = Guid.NewGuid() + ".jpg";
                    var ruta = string.Format("\\wwwroot\\images\\Asistentelocal\\{0}", fileName);
                    System.Diagnostics.Debug.WriteLine(ruta);

                    //Combinamos la ruta del proyecto con el archivo para subirlo
                    string oPath = localPath + ruta;

                    //Ahora guardamos la ruta que se guardara en la base de datos
                    formFinalPath = "\\images\\Asistentelocal\\" + fileName;

                    //Subimos el archivo a la carpeta
                    using (var stream = System.IO.File.Create(oPath))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                }//Fin del Analisis de la Imagen que subio



                //Empezamos a actualizar la informacion dada
                asistentelocal.Nombre = model.Nombre;
                asistentelocal.Apellidos = model.Apellidos;
                asistentelocal.Sexo = model.Sexo;
                asistentelocal.Nacimiento = model.Nacimiento;
                asistentelocal.Sangre = model.Sangre;
                asistentelocal.Curp = model.Curp;
                asistentelocal.Telefono = model.Telefono;
                asistentelocal.Correo = model.Correo;
                asistentelocal.Matricula = model.Matricula;
                asistentelocal.Especialidad = model.Especialidad;
                asistentelocal.Jerarquia = model.Jerarquia;
                asistentelocal.Password = model.Password;

                //Comprobamos si hizo la actualizacion de la foto
                if (formFinalPath.Length > 5)
                {
                    asistentelocal.Imagen = formFinalPath;
                }

                //Agregamos la modificacion a la tabla
                var MAsisL = new MAsistLocal()
                {
                    AsistenteLocalId = asistentelocal.Id,
                    TipoModificacion = "Modificacion",
                    UsuarioId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                    UsuarioTipo = HttpContext.Session.GetString(SessionVariables.SessionKeyType),
                    Fecha = DateTime.Now
                };

                //Guardamos finalmente los datos en dicho asistente local
                await teleDemoDBContext.MAsistLocal.AddAsync(MAsisL);
                await teleDemoDBContext.SaveChangesAsync();
                //Mensaje para decir que si se guardo
                TempData["Message"] = "Asistente Local Actualizado";
                //Retornamos al index
                return RedirectToAction("Index");
            }
            //No encontro id, vamos al index
            TempData["Message"] = "No se encontro al Asistente Local";
            return RedirectToAction("Index");
        }//Fin del metodo de actualizacion de datos de asistente local



        //Metodo cuando vamos a eliminar de forma visual un asistente local
        [HttpPost]
        public async Task<IActionResult> eliminarasistentelocal(UpdateAsistenteLocalViewModel model)
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

            //Sacamos la id del asistente local del contexto
            var asistentelocal = await teleDemoDBContext.AsistenteLocal.FindAsync(model.Id);
            //Vemos si esta vacia
            if (asistentelocal != null)
            {
                //Asignamos que no se vea ese asistente local
                asistentelocal.Visilbe = "No";

                var MAsisL = new MAsistLocal()
                {
                    AsistenteLocalId = asistentelocal.Id,
                    TipoModificacion = "Eliminacion",
                    UsuarioId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                    UsuarioTipo = HttpContext.Session.GetString(SessionVariables.SessionKeyType),
                    Fecha = DateTime.Now
                };

                //Guardamos
                await teleDemoDBContext.MAsistLocal.AddAsync(MAsisL);
                await teleDemoDBContext.SaveChangesAsync();
                //Regresamos al index
                return RedirectToAction("Index");
            } 
            //En el caso que no se encontro, vamos al index
            return RedirectToAction("Index");
        }

    }//Fin del Controlador
}

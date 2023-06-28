using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Numerics;
using Telemedicina.Data;
using Telemedicina.Models.AsistenteRemotoFormularios;
using Telemedicina.Models.Dominio;

namespace Telemedicina.Controllers
{
    public class AsistenteRemotoController : Controller
    {
        //Poder utilizar el contexto de la base de datos
        private readonly TeleDemoDBContext teleDemoDBContext;

        public AsistenteRemotoController(TeleDemoDBContext teleDemoDBContext)
        {
            this.teleDemoDBContext = teleDemoDBContext;
        }

        //Metodo para cancelar y regresar a la vista principal
        [HttpPost]
        public async Task<IActionResult> back()
        {
            return RedirectToAction("Index");
        }

        //Metodo para ir a Index/tabla de AsistenteRemoto
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

            //Recepcion si es que existe el mensaje por registrar un asistente Remoto
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }
            var asistenteremot = await teleDemoDBContext.AsistenteRemotos.ToListAsync();
            //Ahora si mandamos a la vista de Index
            return View(asistenteremot);
        }

        //Metodo para ir a la vista de agregar AsistenteRemoto
        [HttpGet]
        public IActionResult agregarasistenteremoto()
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

        //Una vez realizado el registro, se llamara a este metodo, donde formularemos la informacion para la base de datos
        [HttpPost]
        public async Task<IActionResult> agregarasistenteremoto(agregarasistenteremotoViewModel addasistenteremoto)
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

            //Verificamos si tiene algun error y  en el caso regresar y eniar error
            if (!ModelState.IsValid && ModelState.ErrorCount !=0)
            {
                return View("agregarasistenteremoto",addasistenteremoto);
            }


            //Verificacion de Curp
            //Verificamos si tenemos un paciente
            var curppasiente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Curp == addasistenteremoto.Curp);
            if (curppasiente != null)
            {
                ViewBag.Message = "Curp Ya registrada";
                return View("agregarasistenteremoto", addasistenteremoto);
            }
            //Verificamos si tenemos un doctor
            var curpdoctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Curp == addasistenteremoto.Curp);
            if (curpdoctor != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("agregarasistenteremoto", addasistenteremoto);
            }
            //Verificamos si tenemos un asistente local
            var curpasisLocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Curp == addasistenteremoto.Curp);
            if (curpasisLocal != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("agregarasistenteremoto", addasistenteremoto);
            }
            //Verificamos si tenemos un asistente remoto
            var curpasisremoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Curp == addasistenteremoto.Curp);
            if (curpasisremoto != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("agregarasistenteremoto", addasistenteremoto);
            }


            //Una vez verificada la informacion inicial pasamos a la imagen
            var formFile = addasistenteremoto.File;
            var formFinalPath = "";
            if (formFile !=null && formFile.Length > 0)
            {
                //Verificamos si el tipo de archivoes el correcto
                var formtype = formFile.ContentType;
                if(formtype != "image/jpeg")
                {
                    //Regresamos en el caso de que no suba un archivo no valido
                    ViewBag.Message = "Formato de Archivo no Valido para Foto";
                    return View("agregarasistenteremoto", addasistenteremoto);
                }


                //Ahora verificamos si tiene un peso adecuado que es Maximo 2MB
                var formsize = formFile.Length;
                if(formsize > 2000000)
                {
                    //Regresamos en el caso que sobrepase el peso maximo
                    ViewBag.Message = "Archivo Foto muy pesado, Maximo 2MB";
                    return View("agregarasistenteremoto", addasistenteremoto);
                }


                //Procedemos a guardar la foto

                //Obtenemos la direcccion del servidor, para meter la imagen
                string localPath = Directory.GetCurrentDirectory();
                localPath.Replace(@"\\", @"/");

                //Sacamos el nombre del archivo, tiene que se diferente por cada asistente remoto
                var fileName = Guid.NewGuid() + ".jpg";
                
                //Creamos la ruta hacia la carpeta de imagenes de asistente remoto
                var ruta = string.Format("\\wwwroot\\images\\Asistenteremoto\\{0}", fileName);

                //Combinamos la ruta del proyecto con el archivo para subirlo
                string oPath = localPath + ruta;

                //Ahora guardamos la ruta que se guarda en la base de datos
                formFinalPath = "\\images\\Asistenteremoto\\" + fileName;

                //Subios el archivo foto a la carpeta
                using(var stream = System.IO.File.Create(oPath))
                {
                    await formFile.CopyToAsync(stream);
                }
            }//Fin del Analisis de la imagen que subio
            else
            {
                //Regresamos en el caso que no suba ningun archivo
                return View("agregarasistenteremoto", addasistenteremoto);
            }//Fin de la verificaciond de imagenes


            //Creamos el objeto asistenteremoto para realizar el registro
            var asistenteremoto = new AsistenteRemoto()
            {
                Nombre = addasistenteremoto.Nombre,
                Apellidos = addasistenteremoto.Apellidos,
                Sexo = addasistenteremoto.Sexo,
                Nacimiento = addasistenteremoto.Nacimiento,
                Sangre = addasistenteremoto.Sangre,
                Curp = addasistenteremoto.Curp,
                Correo = addasistenteremoto.Correo,
                Telefono = addasistenteremoto.Telefono,
                Comentarios = addasistenteremoto.Comentarios,
                Password = addasistenteremoto.Password,
                Imagen = formFinalPath,
                FechaIngreso = DateTime.Now,
                Visilbe = "Si"
            };

            //Lo enviamos al contexto y guardamos
            await teleDemoDBContext.AsistenteRemotos.AddAsync(asistenteremoto);
            await teleDemoDBContext.SaveChangesAsync();
            //Mensaje para decir que si se guardo
            TempData["Message"] = "Asistente Remoto Guardado con exito";

            //Sacamos el asistente creado, para sacar su id
            var asisrfinal = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Curp == asistenteremoto.Curp);
            //Registramos quine lo hizo
            var MAsisRem = new MAsistRemoto()
            {
                AsistenteRemotoId = asisrfinal.Id,
                TipoModificacion = "Registro",
                UsuarioId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                UsuarioTipo = HttpContext.Session.GetString(SessionVariables.SessionKeyType),
                Fecha = DateTime.Now
            };
            await teleDemoDBContext.MAsistRemoto.AddAsync(MAsisRem);
            await teleDemoDBContext.SaveChangesAsync();

            //Terminamos y mandamos a la vista
            return RedirectToAction("Index");
        }// Fin del metodo de agregar asistente remoto


        //Metodo para poder colocar la informacion de un asistente remoto en la vista para modificar
        [HttpGet]
        public async Task<IActionResult> modificarasistenteremoto(int id)
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

            //llamamos al contexto para poder comparar la id
            var asistenteremoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Id == id);

            if(asistenteremoto != null)
            {
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                    TempData["Message"] = "";
                }
                //En el caso de encontrar una id sacamos la informacion
                var viewModel = new UpdateAsistenteRemotoViewModel()
                {
                    Id = asistenteremoto.Id,
                    Nombre = asistenteremoto.Nombre,
                    Apellidos = asistenteremoto.Apellidos,
                    Sexo = asistenteremoto.Sexo,
                    Sangre = asistenteremoto.Sangre,
                    Nacimiento = asistenteremoto.Nacimiento,
                    Curp = asistenteremoto.Curp,
                    Correo = asistenteremoto.Correo,
                    Telefono = asistenteremoto.Telefono,
                    Comentarios = asistenteremoto.Comentarios,
                    Password = asistenteremoto.Password,
                };
                //Llevamos a la vista con la informacion
                return await Task.Run(() => View("modificarasistenteremoto",viewModel));
            }//Fin del if de null asistenteremoto
            //En el caso de no encontrar una id asi, se reenvia al index
            ViewBag.Message = "No se encontro el Asistente Remoto";
            return RedirectToAction("Index");
        }//Fin del metodo para mostrar los datos del asistente remoto



        //Se modifico la informacion y la mando a comprobar para guardar
        [HttpPost]
        public async Task<IActionResult> modificarasistenteremoto(UpdateAsistenteRemotoViewModel model)
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
                var errors = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();

                return View("modificarasistenteremoto", model);
            }

            //Verificacion de Curp
            //Verificamos si tenemos un paciente
            var curppasiente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curppasiente != null)
            {
                ViewBag.Message = "Curp Ya registrada";
                return View("modificarasistenteremoto", model);
            }
            //Verificamos si tenemos un doctor
            var curpdoctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curpdoctor != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("modificarasistenteremoto", model);
            }
            //Verificamos si tenemos un asistente local
            var curpasisLocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curpasisLocal != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("modificarasistenteremoto", model);
            }
            //Verificamos si tenemos un asistente remoto
            var curpasisremoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curpasisremoto != null && curpasisremoto.Id != model.Id)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("modificarasistenteremoto", model);
            }

            //Llamamos al contexto para poder comparar la id
            var asistenteremoto = await teleDemoDBContext.AsistenteRemotos.FindAsync(model.Id);
            //en el caso de existir esa id, asignamos la informacion
            if (asistenteremoto != null)
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
                        return View("modificarasistenteremoto", model);
                    }

                    //Ahora verificamos si tiene un peso adecuado que es Maximo 2MB
                    var formsize = formFile.Length;
                    System.Diagnostics.Debug.WriteLine(formsize);
                    if (formsize > 2000000)
                    {   //Regresamos en el caso que no suba ningun archivo
                        ViewBag.Message = "Archivo Pesado en Imagen, Maximo 2MB";
                        return View("modificarasistenteremoto", model);
                    }

                    //Obtemenos la direccion del servidor, para meter la imagen
                    string localPath = Directory.GetCurrentDirectory();
                    localPath.Replace(@"\\", @"/");

                    //Sacamos el nombre del archivo, tiene que ser diferente por cada asistente remoto
                    var fileName = Guid.NewGuid() + ".jpg";
                    var ruta = string.Format("\\wwwroot\\images\\Asistenteremoto\\{0}", fileName);
                    System.Diagnostics.Debug.WriteLine(ruta);

                    //Combinamos la ruta del proyecto con el archivo para subirlo
                    string oPath = localPath + ruta;

                    //Ahora guardamos la ruta que se guardara en la base de datos
                    formFinalPath = "\\images\\Asistenteremoto\\" + fileName;

                    //Subimos el archivo a la carpeta
                    using (var stream = System.IO.File.Create(oPath))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                }//Fin del Analisis de la Imagen que subio



                //Empezamos a actualizar la informacion dada
                asistenteremoto.Nombre = model.Nombre;
                asistenteremoto.Apellidos = model.Apellidos;
                asistenteremoto.Sexo = model.Sexo;
                asistenteremoto.Nacimiento = model.Nacimiento;
                asistenteremoto.Sangre = model.Sangre;
                asistenteremoto.Curp = model.Curp;
                asistenteremoto.Telefono = model.Telefono;
                asistenteremoto.Correo = model.Correo;
                asistenteremoto.Comentarios = model.Comentarios;
                asistenteremoto.Password = model.Password;

                //Comprobamos si hizo la actualizacion de la foto
                if (formFinalPath.Length > 5)
                {
                    asistenteremoto.Imagen = formFinalPath;
                }

                //Agregamos la modificacion a la tabla
                var MasisR = new MAsistRemoto()
                {
                    AsistenteRemotoId = asistenteremoto.Id,
                    TipoModificacion = "Modificacion",
                    UsuarioId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                    UsuarioTipo = HttpContext.Session.GetString(SessionVariables.SessionKeyType),
                    Fecha = DateTime.Now
                };

                //Guardamos finalmente los datos en dicho asistente remoto
                await teleDemoDBContext.MAsistRemoto.AddAsync(MasisR);
                await teleDemoDBContext.SaveChangesAsync();
                //Mensaje para decir que si se guardo
                TempData["Message"] = "Asistente Remoto Actualizado";
                //Retornamos al index
                return RedirectToAction("Index");
            }
            //No encontro id, vamos al index
            TempData["Message"] = "No se encontro al Asistente Remoto";
            return RedirectToAction("Index");
        }//Fin del metodo de actualizacion de datos de asistente Remoto


        //Metodo cuando vamos a eliminar de forma visual un asistente remoto
        [HttpPost]
        public async Task<IActionResult> eliminarasistenteremoto(UpdateAsistenteRemotoViewModel model)
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
            var asistentelocal = await teleDemoDBContext.AsistenteRemotos.FindAsync(model.Id);
            //Vemos si esta vacia
            if (asistentelocal != null)
            {
                //Asignamos que no se vea ese asistente remoto
                asistentelocal.Visilbe = "No";
                 
                //Agregamos la modificacion a la tabla
                var MasisR = new MAsistRemoto()
                {
                    AsistenteRemotoId = asistentelocal.Id,
                    TipoModificacion = "Eliminacion",
                    UsuarioId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                    UsuarioTipo = HttpContext.Session.GetString(SessionVariables.SessionKeyType),
                    Fecha = DateTime.Now
                };

                //Guardamos
                await teleDemoDBContext.MAsistRemoto.AddAsync(MasisR);
                await teleDemoDBContext.SaveChangesAsync();
                //Regresamos al index
                return RedirectToAction("Index");
            }
            //En el caso que no se encontro, vamos al index
            return RedirectToAction("Index");
        }


    }//Fin de la clase de AsistenteRemotoController
}

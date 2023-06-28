using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Telemedicina.Data;
using Telemedicina.Models.DoctorFormularios;
using Telemedicina.Models.Dominio;

namespace Telemedicina.Controllers
{
    public class DoctorController : Controller
    {
        //Poder utilizar el contexto de la base de datos
        private readonly TeleDemoDBContext teleDemoDBContext;

        public DoctorController(TeleDemoDBContext teleDemoDBContext)
        {
            this.teleDemoDBContext = teleDemoDBContext;
        }

        //Metodo para cancelar y regresar a la vista principal
        [HttpPost]
        public async Task<IActionResult> back()
        {
            return RedirectToAction("Index");
        }

        //Entra al Index/Tabla de Doctor
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
            if(usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("Home/login");
            }

            //Recepcion si es que existe del mensaje por registrar un doctor
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }
            var doctores = await teleDemoDBContext.Doctors.ToListAsync();
            //Ahora si mandamos a la vista de Index
            return View(doctores);
        }

        //Enviamos a la vista para agregar un doctor
        [HttpGet]
        public IActionResult agregardoctor()
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
        }



        //Una vez realizado el registro, se llamara a este metodo, donde formularemos la informacion
        //para la base de datos.
        [HttpPost]
        public async Task<IActionResult> agregardoctor(agregardoctorViewModel adddoctor)
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
                return View("agregardoctor", adddoctor);
            }


            //Verificacion de Curp
            //Verificamos si tenemos un paciente
            var curppasiente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Curp == adddoctor.Curp);
            if (curppasiente != null)
            {
                ViewBag.Message = "Curp Ya registrada";
                return View("agregardoctor", adddoctor);
            }
            //Verificamos si tenemos un doctor
            var curpdoctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Curp == adddoctor.Curp);
            if (curpdoctor != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("agregardoctor", adddoctor);
            }
            //Verificamos si tenemos un asistente local
            var curpasisLocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Curp == adddoctor.Curp);
            if (curpasisLocal != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("agregardoctor", adddoctor);
            }
            //Verificamos si tenemos un asistente remoto
            var curpasisremoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Curp == adddoctor.Curp);
            if (curpasisremoto != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("agregardoctor", adddoctor);
            }


            //Una vez verificada la informacion inicial pasamos a las imagenes
            var formFile = adddoctor.File;
            var formFinalPath = "";
            var formFileFirma = adddoctor.FileFirma;
            var formFirmaFinalPath = "";
            //Verificacion de ambas imagenes
            if (formFile != null && formFile.Length > 0 && formFileFirma != null && formFileFirma.Length > 0)
            {
                //var filePath = Path.GetTempFileName();

                //Verificacion de Imagen de Perfil/Foto

                //Verificamos si el tipo de archivo es el correcto
                var formtype = formFile.ContentType;
                //System.Diagnostics.Debug.WriteLine(formtype);
                if (formtype != "image/jpeg")
                {   //Regresamos en el caso que no suba ningun archivo
                    ViewBag.Message = "Formato de Archivo no Invalido para Foto";
                    return View("agregardoctor", adddoctor);
                }


                //Ahora verificamos si tiene un peso adecuado que es Maximo 2MB
                var formsize = formFile.Length;
                System.Diagnostics.Debug.WriteLine(formsize);
                if (formsize > 2000000)
                {   //Regresamos en el caso que no suba ningun archivo
                    ViewBag.Message = "Archivo Foto muy Pesado, Maximo 2MB";
                    return View("agregardoctor", adddoctor);
                }

                //Verificacion de Imagen Firma

                //Verificamos si el tipo de archivo es el correcto
                var formfirmatype = formFileFirma.ContentType;
                //System.Diagnostics.Debug.WriteLine(formtype);
                if (formfirmatype != "image/jpeg")
                {   //Regresamos en el caso que no suba ningun archivo
                    ViewBag.Message = "Formato de Archivo no Invalido para Firma";
                    return View("agregardoctor", adddoctor);
                }

                //Ahora verificamos si tiene un peso adecuado que es Maximo 2MB
                var formfirmasize = formFileFirma.Length;
                System.Diagnostics.Debug.WriteLine(formfirmasize);
                if (formfirmasize > 2000000)
                {   //Regresamos en el caso que no suba ningun archivo
                    ViewBag.Message = "Archivo Firma muy Pesado, Maximo 2MB";
                    return View("agregardoctor", adddoctor);
                }

                //Procedemos a Guardas ambas fotos

                //Obtemenos la direccion del servidor, para meter la imagen
                string localPath = Directory.GetCurrentDirectory();
                localPath.Replace(@"\\", @"/");
                //System.Diagnostics.Debug.WriteLine(localPath);


                //Sacamos el nombre del archivo, tiene que ser diferente por cada paciente
                var fileName = Guid.NewGuid() + ".jpg";
                var fileFirmaName = Guid.NewGuid() + ".jpg";
                //Creamos la ruta hacia la carpeta de imagenes de doctor
                var ruta = string.Format("\\wwwroot\\images\\Doctor\\{0}", fileName);
                var rutaFirma = string.Format("\\wwwroot\\images\\Doctor\\{0}", fileFirmaName);
                //System.Diagnostics.Debug.WriteLine(ruta);
                

                //Combinamos la ruta del proyecto con el archivo para subirlo
                string oPath = localPath + ruta;
                string oPathFirma = localPath + rutaFirma;
                //System.Diagnostics.Debug.WriteLine(oPath);


                //Ahora guardamos la ruta que se guardara en la base de datos
                formFinalPath = "\\images\\Doctor\\" + fileName;
                formFirmaFinalPath = "\\images\\Doctor\\" + fileFirmaName;


                //Subimos el archivo foto a la carpeta
                using (var stream = System.IO.File.Create(oPath))
                {
                    await formFile.CopyToAsync(stream);
                }
                //Subimos el archivo firma a la carpeta
                using (var stream = System.IO.File.Create(oPathFirma))
                {
                    await formFileFirma.CopyToAsync(stream);
                }

            }//Fin del Analisis de la Imagen que subio
            else
            {
                //Regresamos en el caso que no suba ningun archivo
                return View("agregardoctor", adddoctor);
            }//Final de la verificacion de imagenes



            //Creamos el objeto Doctor para realizar el registro
            var doctor = new Doctor()
            {
                //Id = Guid.NewGuid(),
                Nombre = adddoctor.Nombre,
                Apellidos = adddoctor.Apellidos,
                Sexo = adddoctor.Sexo,
                Nacimiento = adddoctor.Nacimiento,
                Sangre = adddoctor.Sangre,
                Curp = adddoctor.Curp,
                Correo = adddoctor.Correo,
                Telefono = adddoctor.Telefono,
                Institucion = adddoctor.Institucion,
                Matricula = adddoctor.Matricula,
                Especialidad = adddoctor.Especialidad,
                Jerarquia = adddoctor.Jerarquia,
                Password = adddoctor.Password,
                Imagen = formFinalPath,
                Firma = formFirmaFinalPath,
                Visilbe = "Si"
            };

            //Lo enviamos al contexto y guardamos
            await teleDemoDBContext.Doctors.AddAsync(doctor);
            await teleDemoDBContext.SaveChangesAsync();
            //Mensaje para decir que si se guardo
            TempData["Message"] = "Doctor Guardado con Exito";

            //Sacamos el doctor, para sacar su respectiva id
            var doctorfinal = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Curp == doctor.Curp);
            //Registramos quien lo hizo
            var MDoctor = new MDoctor()
            {
                DoctorId = doctorfinal.Id,
                TipoModificacion = "Registro",
                UsuarioId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                UsuarioTipo = HttpContext.Session.GetString(SessionVariables.SessionKeyType),
                Fecha = DateTime.Now
            };
            await teleDemoDBContext.MDoctor.AddAsync(MDoctor);
            await teleDemoDBContext.SaveChangesAsync();

            //Terminamos y enviamos a una vista
            return RedirectToAction("Index");
        }//Fin del Metodo de Guardar Doctor


        //Metodo para poder colocar la informacion de un doctor en la vista para modificar
        [HttpGet]
        public async Task<IActionResult> modificardoctor(int id)
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
            var doctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Id == id);

            if(doctor!=null)
            {
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                    TempData["Message"] = "";
                }
                //En el caso de encontrar una id sacamos la informacion
                var viewModel = new UpdateDoctorViewModel()
                {
                    Id = doctor.Id,
                    Nombre = doctor.Nombre,
                    Apellidos = doctor.Apellidos,
                    Sexo = doctor.Sexo,
                    Sangre = doctor.Sangre,
                    Nacimiento = doctor.Nacimiento,
                    Curp = doctor.Curp,
                    Correo = doctor.Correo,
                    Telefono = doctor.Telefono,
                    Institucion = doctor.Institucion,
                    Matricula = doctor.Matricula,
                    Especialidad = doctor.Especialidad,
                    Jerarquia = doctor.Jerarquia,
                    Password = doctor.Password,
                };
                //Llevamos a la vista con la informacion
                return await Task.Run(() => View("modificardoctor", viewModel));
            }
            //En el caso de no encontrar una id asi, se reenvia al index
            ViewBag.Message = "No se encontro el Paciente";
            return RedirectToAction("Index");
        }//Fin del metodo para mostrar los datos del doctor



        //Se modifico la informacion y la mando a comprobar para guardar
        [HttpPost]
        public async Task<IActionResult> modificardoctor(UpdateDoctorViewModel model)
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

                return View("modificardoctor", model);
            }

            //Verificacion de Curp
            //Verificamos si tenemos un paciente
            var curppasiente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curppasiente != null)
            {
                ViewBag.Message = "Curp Ya registrada";
                return View("modificardoctor", model);
            }
            //Verificamos si tenemos un doctor
            var curpdoctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curpdoctor != null && curpdoctor.Id != model.Id)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("modificardoctor", model);
            }
            //Verificamos si tenemos un asistente local
            var curpasisLocal = await teleDemoDBContext.AsistenteLocal.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curpasisLocal != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("modificardoctor", model);
            }
            //Verificamos si tenemos un asistente remoto
            var curpasisremoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Curp == model.Curp);
            if (curpasisremoto != null)
            {
                ViewBag.Message = "Curp ya registrada";
                return View("modificardoctor", model);
            }

            //Llamamos al contexto para poder comparar la id
            var doctor = await teleDemoDBContext.Doctors.FindAsync(model.Id);
            //en el caso de existir esa id, asignamos la informacion
            if (doctor != null)
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
                        return View("modificardoctor", model);
                    }

                    //Ahora verificamos si tiene un peso adecuado que es Maximo 2MB
                    var formsize = formFile.Length;
                    System.Diagnostics.Debug.WriteLine(formsize);
                    if (formsize > 2000000)
                    {   //Regresamos en el caso que no suba ningun archivo
                        ViewBag.Message = "Archivo Pesado en Imagen, Maximo 2MB";
                        return View("modificarpaciente", model);
                    }

                    //Obtemenos la direccion del servidor, para meter la imagen
                    string localPath = Directory.GetCurrentDirectory();
                    localPath.Replace(@"\\", @"/");
                    //System.Diagnostics.Debug.WriteLine(localPath);

                    //Sacamos el nombre del archivo, tiene que ser diferente por cada paciente
                    var fileName = Guid.NewGuid() + ".jpg";
                    var ruta = string.Format("\\wwwroot\\images\\Doctor\\{0}", fileName);
                    System.Diagnostics.Debug.WriteLine(ruta);

                    //Combinamos la ruta del proyecto con el archivo para subirlo
                    string oPath = localPath + ruta;
                    System.Diagnostics.Debug.WriteLine(oPath);

                    //Ahora guardamos la ruta que se guardara en la base de datos
                    formFinalPath = "\\images\\Doctor\\" + fileName;

                    //Subimos el archivo a la carpeta
                    using (var stream = System.IO.File.Create(oPath))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                }//Fin del Analisis de la Imagen que subio


                //Modificacion de la Firma
                var formFirma = model.File;
                var formFinalFirmaPath = "";
                //Verificacion si es que actualizo la Imagen
                if (formFirma != null && formFirma.Length > 0)
                {
                    //Verificamos si el tipo de archivo es el correcto
                    var formtype = formFirma.ContentType;
                    //System.Diagnostics.Debug.WriteLine(formtype);
                    if (formtype != "image/jpeg")
                    {   //Regresamos en el caso que no suba ningun archivo
                        ViewBag.Message = "Formato de Archivo no Invalido para Firma";
                        return View("modificardoctor", model);
                    }

                    //Ahora verificamos si tiene un peso adecuado que es Maximo 2MB
                    var formsize = formFirma.Length;
                    System.Diagnostics.Debug.WriteLine(formsize);
                    if (formsize > 2000000)
                    {   //Regresamos en el caso que no suba ningun archivo
                        ViewBag.Message = "Archivo Pesado en Firma, Maximo 2MB";
                        return View("modificarpaciente", model);
                    }

                    //Obtemenos la direccion del servidor, para meter la imagen
                    string localPath = Directory.GetCurrentDirectory();
                    localPath.Replace(@"\\", @"/");
                    //System.Diagnostics.Debug.WriteLine(localPath);

                    //Sacamos el nombre del archivo, tiene que ser diferente por cada paciente
                    var fileName = Guid.NewGuid() + ".jpg";
                    var ruta = string.Format("\\wwwroot\\images\\Doctor\\{0}", fileName);
                    System.Diagnostics.Debug.WriteLine(ruta);

                    //Combinamos la ruta del proyecto con el archivo para subirlo
                    string oPath = localPath + ruta;
                    System.Diagnostics.Debug.WriteLine(oPath);

                    //Ahora guardamos la ruta que se guardara en la base de datos
                    formFinalFirmaPath = "\\images\\Doctor\\" + fileName;

                    //Subimos el archivo a la carpeta
                    using (var stream = System.IO.File.Create(oPath))
                    {
                        await formFirma.CopyToAsync(stream);
                    }

                }//Fin del Analisis de la Firma que subio


                //Empezamos a actualizar la informacion dada
                doctor.Nombre = model.Nombre;
                doctor.Apellidos = model.Apellidos;
                doctor.Sexo = model.Sexo;
                doctor.Nacimiento = model.Nacimiento;
                doctor.Sangre = model.Sangre;
                doctor.Curp = model.Curp;
                doctor.Telefono = model.Telefono;
                doctor.Correo = model.Correo;
                doctor.Correo = model.Institucion;
                doctor.Matricula = model.Matricula;
                doctor.Especialidad = model.Especialidad;
                doctor.Jerarquia = model.Jerarquia;
                doctor.Password = model.Password;

                //Comprobamos si hizo la actualizacion de la foto
                if (formFinalPath.Length > 5)
                {
                    doctor.Imagen = formFinalPath;
                }

                //Comprobamos si hizo la actualizacion de la firma
                if (formFinalFirmaPath.Length > 5)
                {
                    doctor.Firma = formFinalFirmaPath;
                }

                //Agregamos la modificacion a la tabla
                var Mdoctor = new MDoctor()
                {
                    DoctorId = doctor.Id,
                    TipoModificacion = "Modificacion",
                    UsuarioId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                    UsuarioTipo = HttpContext.Session.GetString(SessionVariables.SessionKeyType),
                    Fecha = DateTime.Now
                };

                //Guardamos finalmente los datos en dicho doctor
                await teleDemoDBContext.MDoctor.AddAsync(Mdoctor);
                await teleDemoDBContext.SaveChangesAsync();
                //Mensaje para decir que si se guardo
                TempData["Message"] = "Doctor Actualizado";
                //Retornamos al index
                return RedirectToAction("Index");
            }
            //No encontro id, vamos al index
            TempData["Message"] = "No se encontro al Doctor";
            return RedirectToAction("Index");
        }//Fin del metodo de actualizacion de datos de doctor



        //Metodo cuando vamos a eliminar de forma visual un doctor
        [HttpPost]
        public async Task<IActionResult> eliminardoctor(UpdateDoctorViewModel model)
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

            //Sacamos la id del doctor del contexto
            var doctor = await teleDemoDBContext.Doctors.FindAsync(model.Id);
            //Vemos si esta vacia
            if (doctor != null)
            {
                //Asignamos que no se vea ese paciente
                doctor.Visilbe = "No";

                //Agregamos la eliminacion en la tabla
                var Mdoctor = new MDoctor()
                {
                    DoctorId = doctor.Id,
                    TipoModificacion = "Eliminacion",
                    UsuarioId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                    UsuarioTipo = HttpContext.Session.GetString(SessionVariables.SessionKeyType),
                    Fecha = DateTime.Now
                };

                //Guardamos
                await teleDemoDBContext.MDoctor.AddAsync(Mdoctor);
                await teleDemoDBContext.SaveChangesAsync();
                //Regresamos al index
                return RedirectToAction("Index");
            }
            //En el caso que no se encontro, vamos al index
            return RedirectToAction("Index");
        }

    }//Fin del Controlador
}
 
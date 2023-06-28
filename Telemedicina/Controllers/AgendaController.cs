using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using System.Runtime.InteropServices;
using Telemedicina.Data;
using Telemedicina.Models.Agenda;
using Telemedicina.Models.AsistenteLocalFormularios;
using Telemedicina.Models.Dominio;
using Telemedicina.Models.Receta;

namespace Telemedicina.Controllers
{

    public class AgendaController : Controller
    {
        //Poder utilizar el contexto de la base de datos
        private readonly TeleDemoDBContext teleDemoDBContext;


        public AgendaController(TeleDemoDBContext teleDemoDBContext)
        {
            this.teleDemoDBContext = teleDemoDBContext;
        }

        //--------------------------------------- Citas Temporales Asistente Remoto --------------------------------

        //Metodo para cancelar y regresar a la vista principal
        [HttpPost]
        public async Task<IActionResult> back()
        {
            return RedirectToAction("Index");
        }

        //Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistRemoto")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/login");
            }

            //Recepcion si es que existe el mensaje por registrar un asistenteLocal
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }
            var id = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId));
            var citastemp = await teleDemoDBContext.CitaTemp.Where(x => x.AsistenteRemotoId == id).ToListAsync();
            //Acomodamos las citas con el formato a presentar
            List<Models.Agenda.CitaTemp> lista = new List<Models.Agenda.CitaTemp>();
            //Ahora vamos a llenar los los campos
            foreach (var citastempRow in citastemp)
            {
                //Creamos objeto de la lista
                Models.Agenda.CitaTemp CitaTemporalDatos = new Models.Agenda.CitaTemp();

                //Consulta para unir la id de localidad de la tabla Complejo con Localidad
                var Complejo = await teleDemoDBContext.Complejo.FirstOrDefaultAsync(x => x.Id == citastempRow.ComplejoId);
                var Paciente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Id == citastempRow.PacienteId);

                //Unir los datos
                CitaTemporalDatos.Estado = citastempRow.Estado;
                CitaTemporalDatos.FechaCreacion = citastempRow.FechaCreacion;
                CitaTemporalDatos.Fecha = citastempRow.Fecha;
                CitaTemporalDatos.NombrePaciente = Paciente.Nombre + " " + Paciente.Apellidos;
                CitaTemporalDatos.Comentarios = citastempRow.Comentarios;
                CitaTemporalDatos.NombreComplejo = Complejo.Nombre;

                lista.Add(CitaTemporalDatos);
            }
            //Ahora si mandamos a la vista de Index
            return View(lista);
        }//Metodo Index de la vista de citas temporales


        //Enviamos a la vista para solicitar uan citatemporal
        [HttpGet]
        public async Task<IActionResult> CitaTD()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistRemoto")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/login");
            }

            //Recepcion si es que existe del mensaje de error
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }
            //Acomodamos lo que vamos a enviar
            Models.Agenda.CitaTemp citatemp = new Models.Agenda.CitaTemp();
            //Sacamos la id del Asistente Remoto
            var idAsistR = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId));
            //Buscamos a que Complejo esta asignado
            var ComplejoAsignado = await teleDemoDBContext.ComplejoAsistR.FirstOrDefaultAsync(x => x.AsistRId == idAsistR);
            //Buscamos los datos de ese complejo para la localidad
            if (ComplejoAsignado != null)
            {
                var Complejo = await teleDemoDBContext.Complejo.FirstOrDefaultAsync(x => x.Id == ComplejoAsignado.ComplejoId);
                if (Complejo != null)
                {
                    //Sacamos la localidad donde esta dicho complejo
                    var Localidad = await teleDemoDBContext.Localidad.FirstOrDefaultAsync(x => x.Id == Complejo.IdLocalidad);
                    if (Localidad != null)
                    {
                        //Ahora clasificamos que pacientes son de dicha localidad y los agregamos al modelo
                        citatemp.ComplejoId = Complejo.Id;
                        citatemp.Pacientes = await teleDemoDBContext.Pacientes.Where(x => x.Localidad == Localidad.Nombre).ToListAsync();
                        return View(citatemp);
                    }
                    else
                    {
                        //El el caso de que no se encontro dicha Id, lo reenviamos a la vista de Complejo
                        ViewBag.Message = "Localidad Vinculada a Complejo No encontrado";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    //El el caso de que no se encontro dicha Id, lo reenviamos a la vista de Complejo
                    ViewBag.Message = "Complejo Vinculado No encontrado";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                //El el caso de que no se encontro dicha Id, lo reenviamos a la vista de Complejo
                ViewBag.Message = "Asistente Remoto no Asigando a un Complejo";
                return RedirectToAction("Index");
            }

            //El el caso de que no se encontro dicha Id, lo reenviamos a la vista de Complejo
            ViewBag.Message = "Informacion o Asignacion insuficiente para crear citas";
            return RedirectToAction("Index");
        }//Fin del metodo para llevar a la vista de CitaTD(Agregar)


        //Vista donde se registra la localidad colcoada en el formulario
        [HttpPost]
        public async Task<IActionResult> CitaTD(Models.Agenda.CitaTemp citaTemp)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistRemoto")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/login");
            }

            //Verificamos si tiene algun error y en el caso regresar y enviar error
            if (!ModelState.IsValid && ModelState.ErrorCount != 0)
            {
                var idAsistR = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId));
                //Buscamos a que Complejo esta asignado
                var ComplejoAsignado = await teleDemoDBContext.ComplejoAsistR.FirstOrDefaultAsync(x => x.AsistRId == idAsistR);
                //Buscamos los datos de ese complejo para la localidad
                var Complejo = await teleDemoDBContext.Complejo.FirstOrDefaultAsync(x => x.Id == ComplejoAsignado.ComplejoId);
                //Sacamos la localidad donde esta dicho complejo
                var Localidad = await teleDemoDBContext.Localidad.FirstOrDefaultAsync(x => x.Id == Complejo.IdLocalidad);
                //Ahora clasificamos que pacientes son de dicha localidad y los agregamos al modelo
                citaTemp.ComplejoId = Complejo.Id;
                citaTemp.Pacientes = await teleDemoDBContext.Pacientes.Where(x => x.Localidad == Localidad.Nombre).ToListAsync();
                return View("CitaTD", citaTemp);
            }

            var citaTemporal = new Models.Dominio.CitaTemp()
            {
                AsistenteRemotoId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                PacienteId = citaTemp.PacienteId,
                ComplejoId = citaTemp.ComplejoId,
                Fecha = citaTemp.Fecha,
                Comentarios = citaTemp.Comentarios,
                FechaCreacion = DateTime.Now,
                Estado = "Pendiente"
            };

            //Hacemos el movimiento en la base de datos
            await teleDemoDBContext.CitaTemp.AddAsync(citaTemporal);
            await teleDemoDBContext.SaveChangesAsync();
            //Mensaje para decir que se guardo
            TempData["Message"] = "Creacion de Cita Solicitada con Exito";

            //Terminamos y enviamos a una vista
            return RedirectToAction("Index");
        }//Fin del metodo de registrar una cita temporal


        //Metodo para cancelar y regresar a la vista principal
        [HttpPost]
        public async Task<IActionResult> back2()
        {
            return RedirectToAction("Citas");
        }

        //--------------------------------------- Citas Finales Asistente Local --------------------------------
        //Index
        [HttpGet]
        public async Task<IActionResult> Citas()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                TempData["Message"] = "Acceso Restringido";
                return Redirect("/Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                TempData["Message"] = "Acceso Restringido";
                return Redirect("/Home/Index");
            }

            //
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }
            //Lista de citas temporales a utilizar despues
            List<Models.Agenda.CitaTemp> lista = new List<Models.Agenda.CitaTemp>();
            //Sacamos la id para buscar a que localidad esta vinculado
            var id = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId));
            var localidad = await teleDemoDBContext.LocalAsistente.Where(x => x.AsistenteLID == id).ToListAsync();
            //Ahora por cada localidad vinculado, vamos a sacar cada complejo que tenga esa localidad
            foreach (var localidades in localidad)
            {
                //Por cada complejo vamos a sacar las citas temporales que se hayan registrado en cada complejo
                var complejo = await teleDemoDBContext.Complejo.Where(x => x.IdLocalidad == localidades.LocalidadID).ToListAsync();
                foreach (var complejos in complejo)
                {
                    //Sacamos todas las citas temporales y ahora las tenemos que guardar en una lista que presentaremos en la vista
                    var citasT = await teleDemoDBContext.CitaTemp.Where(x => x.ComplejoId == complejos.Id).ToListAsync();
                    foreach (var cita in citasT)
                    {
                        if (cita.Estado == "Pendiente")
                        {
                            //Objeto de Cita temporal a guardar
                            Models.Agenda.CitaTemp CitaTemporalDatos = new Models.Agenda.CitaTemp();

                            //Consulta para unir la id de localidad de la tabla Complejo con Localidad
                            var Complejo = await teleDemoDBContext.Complejo.FirstOrDefaultAsync(x => x.Id == cita.ComplejoId);
                            var Paciente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Id == cita.PacienteId);

                            //Unir los datos
                            CitaTemporalDatos.Id = cita.Id;
                            CitaTemporalDatos.Estado = cita.Estado;
                            CitaTemporalDatos.FechaCreacion = cita.FechaCreacion;
                            CitaTemporalDatos.Fecha = cita.Fecha;
                            CitaTemporalDatos.NombrePaciente = Paciente.Nombre + " " + Paciente.Apellidos;
                            CitaTemporalDatos.Comentarios = cita.Comentarios;
                            CitaTemporalDatos.NombreComplejo = Complejo.Nombre;

                            lista.Add(CitaTemporalDatos);
                        }
                    }//Fin del foreach de citaT
                }//Fin del foreach de complejo
            }//Fin del foreach de localidad

            //Ahora si mandamos a la vista de Index
            return View(lista);
        }//Metodo Index de la vista de citas temporales


        //Metodo para poder colocar la informacion de un cita temporal para revisarla
        [HttpGet]
        public async Task<IActionResult> CitasM(int id)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/login");
            }

            //Llamamos al contexto para poder comparar la id
            var citaTemporal = await teleDemoDBContext.CitaTemp.FirstOrDefaultAsync(x => x.Id == id);

            if (citaTemporal != null)
            {
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                    TempData["Message"] = "";
                }
                //Datos para llenar el objeto a enviar
                List<AgendaDoctor> AgendaDoctoresFinal = new List<AgendaDoctor>();
                List<Doctor> doctors = new List<Doctor>();
                //AsistenteRemoto Datos
                var asistenteRemoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Id == citaTemporal.AsistenteRemotoId);
                //Paciente Datos
                var paciente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Id == citaTemporal.PacienteId);
                //Complejo Datos
                var complejo = await teleDemoDBContext.Complejo.FirstOrDefaultAsync(x => x.Id == citaTemporal.ComplejoId);
                //Lista de Doctores a ese complejo
                var listaComplejoDoctor = await teleDemoDBContext.ComplejoDoctor.Where(x => x.ComplejoId == citaTemporal.ComplejoId).ToListAsync();
                foreach (var row in listaComplejoDoctor)
                {
                    //Lista de citas temporales a utilizar despues
                    AgendaDoctor AgendaDoctor = new AgendaDoctor();
                    //
                    List<AgendaDoctorLista> agendadoctorlistafinal = new List<AgendaDoctorLista>();
                    //Sacamos los datos del doctor
                    var doctorDatos = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Id == row.DoctorId);
                    AgendaDoctor.NombreDoctor = doctorDatos.Nombre + " " + doctorDatos.Apellidos;
                    //Buscamos sus citas
                    var doctorCitasLista = await teleDemoDBContext.CitaFinal.Where(x => x.DoctorId == doctorDatos.Id).OrderBy(x => x.Fecha).ToListAsync();
                    foreach (var cita in doctorCitasLista)
                    {
                        //Objeto donde meteremos la info de la cita
                        AgendaDoctorLista agendaDoctorLista = new AgendaDoctorLista();
                        //Sacamos info de cada cita
                        var pacientedoctor = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Id == cita.PacienteId);
                        var complejodoctor = await teleDemoDBContext.Complejo.FirstOrDefaultAsync(x => x.Id == cita.ComplejoId);
                        //guardamos en el objeto
                        agendaDoctorLista.Hora = cita.Hora;
                        agendaDoctorLista.Fecha = cita.Fecha;
                        agendaDoctorLista.PacienteNombre = pacientedoctor.Nombre + " " + pacientedoctor.Apellidos;
                        agendaDoctorLista.ComplejoNombre = complejodoctor.Nombre;
                        //Metemos a la lista
                        agendadoctorlistafinal.Add(agendaDoctorLista);
                    }//Fin del foreach de doctorCitasLista
                    //Asignamos lista en AngedaDoctor
                    AgendaDoctor.Lista = agendadoctorlistafinal;
                    //Lo metemos a la lista de CitaFinalF
                    AgendaDoctoresFinal.Add(AgendaDoctor);
                }//Fin del foreach de ListaComplejoDoctor

                //Sacamos la lista de los doctores que puede seleccioar
                var doctorcomplejo = await teleDemoDBContext.ComplejoDoctor.Where(x => x.ComplejoId == citaTemporal.ComplejoId).ToListAsync();
                foreach (var row in doctorcomplejo)
                {
                    Doctor doctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Id == row.DoctorId);
                    doctors.Add(doctor);
                }//Fin del foreach de doctorescomplejo

                //En el caso de encontrar una id sacamos la informacion
                var viewModel = new CitaFinalF()
                {
                    Id = citaTemporal.Id,
                    AsistenteRemotoId = citaTemporal.AsistenteRemotoId,
                    NombreAsistenteRemoto = asistenteRemoto.Nombre + " " + asistenteRemoto.Apellidos,
                    PacienteId = citaTemporal.PacienteId,
                    NombrePaciente = paciente.Nombre + " " + paciente.Apellidos,
                    ComplejoId = citaTemporal.ComplejoId,
                    NombreComplejo = complejo.Nombre,
                    Comentarios = citaTemporal.Comentarios,
                    Fecha = citaTemporal.Fecha,
                    Doctores = doctors,
                    AgendaDoctores = AgendaDoctoresFinal
                };//Fin del if de null asistentelocal
                //Llevamos a la vista con la informacion
                return await Task.Run(() => View("CitasM", viewModel));
            }
            //En el caso de no encontrar una id asi, se reenvia al index
            ViewBag.Message = "No se encontro el Asistente Local";
            return RedirectToAction("Citas");
        }//Fin del metodo para mostrar los datos del asistente local


        //Metodo para aceptar una cita temporal y hacer una cita Final
        [HttpPost]
        public async Task<IActionResult> CitasM(CitaFinalF model)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/login");
            }

            //Verificamos si tiene algun error y en el caso regresar y enviar error
            if (!ModelState.IsValid && ModelState.ErrorCount != 0)
            {
                ViewBag.Message = "Error en los datos";
                //var errors = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();
                List<AgendaDoctor> AgendaDoctoresFinal = new List<AgendaDoctor>();
                List<Doctor> doctors = new List<Doctor>();
                //AsistenteRemoto Datos
                var asistenteRemoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Id == model.AsistenteRemotoId);
                //Paciente Datos
                var paciente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Id == model.PacienteId);
                //Complejo Datos
                var complejo = await teleDemoDBContext.Complejo.FirstOrDefaultAsync(x => x.Id == model.ComplejoId);
                //Lista de Doctores a ese complejo
                var listaComplejoDoctor = await teleDemoDBContext.ComplejoDoctor.Where(x => x.ComplejoId == model.ComplejoId).ToListAsync();
                foreach (var row in listaComplejoDoctor)
                {
                    //Lista de citas temporales a utilizar despues
                    AgendaDoctor AgendaDoctor = new AgendaDoctor();
                    //
                    List<AgendaDoctorLista> agendadoctorlistafinal = new List<AgendaDoctorLista>();
                    //Sacamos los datos del doctor
                    var doctorDatos = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Id == row.DoctorId);
                    AgendaDoctor.NombreDoctor = doctorDatos.Nombre + " " + doctorDatos.Apellidos;
                    //Buscamos sus citas
                    var doctorCitasLista = await teleDemoDBContext.CitaFinal.Where(x => x.DoctorId == doctorDatos.Id).ToListAsync();
                    foreach (var cita in doctorCitasLista)
                    {
                        //Objeto donde meteremos la info de la cita
                        AgendaDoctorLista agendaDoctorLista = new AgendaDoctorLista();
                        //Sacamos info de cada cita
                        var pacientedoctor = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Id == cita.PacienteId);
                        var complejodoctor = await teleDemoDBContext.Complejo.FirstOrDefaultAsync(x => x.Id == cita.ComplejoId);
                        //guardamos en el objeto
                        agendaDoctorLista.Hora = cita.Hora;
                        agendaDoctorLista.Fecha = cita.Fecha;
                        agendaDoctorLista.PacienteNombre = pacientedoctor.Nombre + " " + pacientedoctor.Apellidos;
                        agendaDoctorLista.ComplejoNombre = complejodoctor.Nombre;
                        //Metemos a la lista
                        agendadoctorlistafinal.Add(agendaDoctorLista);
                    }//Fin del foreach de doctorCitasLista
                    //Asignamos lista en AngedaDoctor
                    AgendaDoctor.Lista = agendadoctorlistafinal;
                    //Lo metemos a la lista de CitaFinalF
                    AgendaDoctoresFinal.Add(AgendaDoctor);
                }//Fin del foreach de ListaComplejoDoctor

                //Sacamos la lista de los doctores que puede seleccioar
                var doctorcomplejo = await teleDemoDBContext.ComplejoDoctor.Where(x => x.ComplejoId == model.ComplejoId).ToListAsync();
                foreach (var row in doctorcomplejo)
                {
                    Doctor doctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Id == row.DoctorId);
                    doctors.Add(doctor);
                }//Fin del foreach de doctorescomplejo

                //Metemos las dos listas
                model.Doctores = doctors;
                model.AgendaDoctores = AgendaDoctoresFinal;
                //Regresamos
                return View("CitasM", model);
            }
            string room = Guid.NewGuid().ToString();
            //Creamos la cita Final
            Models.Dominio.CitaFinal citaFinal = new Models.Dominio.CitaFinal()
            {
                AsistenteRemotoId = model.AsistenteRemotoId,
                AsistenteLocalId = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)),
                DoctorId = model.DoctorId,
                PacienteId = model.PacienteId,
                ComplejoId = model.ComplejoId,
                Fecha = model.Fecha,
                Hora = model.Hora,
                Comentarios = model.Comentarios,
                Estado = "Pendiente",
                roomid = room
            };

            //Guardamos la nueva cita
            await teleDemoDBContext.CitaFinal.AddAsync(citaFinal);
            //Modificamos la cita temporal para decir que ya esta
            var citatemporal = await teleDemoDBContext.CitaTemp.FindAsync(model.Id);
            if (citatemporal != null)
            {
                citatemporal.Estado = "Aceptada";
            }
            //Guardamos
            await teleDemoDBContext.SaveChangesAsync();
            //Mensaje
            TempData["Message"] = "Cita Final Agendada";
            return RedirectToAction("Citas");

        }//Fin del metodo de Guardar cita final

        //Metodo para rechazar una cita temporal
        [HttpPost]
        public async Task<IActionResult> CitasMRe(CitaFinalF model)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistLocal")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/login");
            }
            //Buscamos y guardamos un no en la cita
            var citatemporal = await teleDemoDBContext.CitaTemp.FindAsync(model.Id);
            if (citatemporal != null)
            {
                citatemporal.Estado = "Rechazada";
                TempData["Message"] = "Cita Temporal Rechazada";
            }
            else
            {
                TempData["Message"] = "Cita Temporal no encontrada";
            }
            //Guardamos
            await teleDemoDBContext.SaveChangesAsync();
            //Mensaje
            return RedirectToAction("Citas");

        }

        //--------------------------------------- Ver Agenda Doctor --------------------------------

        //Agenda Doctor
        [HttpGet]
        public async Task<IActionResult> AgendaDt()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "Doctor")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/login");
            }

            //Recepcion si es que existe el mensaje por registrar un asistenteLocal
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }

            var id = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId));
            var citas = await teleDemoDBContext.CitaFinal.Where(x => x.DoctorId == id && x.Fecha >= DateTime.Now && x.Estado == "Pendiente").OrderBy(x => x.Fecha).ToListAsync();

            //Acomodamos las citas con el formato a presentar
            List<Models.Agenda.CitaFinalF> listadeCitas = new List<Models.Agenda.CitaFinalF>();
            //Ahora vamos a llenar los los campos
            foreach (var citasrow in citas)
            {
                //Creamos objeto de la lista
                Models.Agenda.CitaFinalF citadatos = new Models.Agenda.CitaFinalF();

                //Consulta para unir la id de localidad de la tabla Complejo con Localidad
                var Complejo = await teleDemoDBContext.Complejo.FirstOrDefaultAsync(x => x.Id == citasrow.ComplejoId);
                var Paciente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Id == citasrow.PacienteId);
                var AsistenteRemoto = await teleDemoDBContext.AsistenteRemotos.FirstOrDefaultAsync(x => x.Id == citasrow.AsistenteRemotoId);

                //Unir los datos
                citadatos.Id = citasrow.Id;
                citadatos.NombreAsistenteRemoto = AsistenteRemoto.Nombre + " " + AsistenteRemoto.Apellidos;
                citadatos.NombrePaciente = Paciente.Nombre + " " + Paciente.Apellidos;
                citadatos.NombreComplejo = Complejo.Nombre;
                citadatos.Fecha = citasrow.Fecha;
                citadatos.Hora = citasrow.Hora;
                citadatos.Estado = citasrow.Estado;
                citadatos.Comentarios = citasrow.Comentarios;
                citadatos.roomid = citasrow.roomid;
                listadeCitas.Add(citadatos);
            }
            //Ahora si mandamos a la vista de Index
            return View(listadeCitas);
        }//Metodo Index de la vista de citas temporales

        [HttpPost]
        [Route("Agenda/Room/Finalizar")]
        public async Task<IActionResult> Finalizar(CitaEnd model)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "Doctor")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/login");
            }
            var room = model.roomId.ToString();
            var cita = await teleDemoDBContext.CitaFinal.Where(x => x.roomid == room).FirstOrDefaultAsync();
            //Vemos si esta vacia
            if (cita != null)
            {
                //Asignamos que no se vea ese asistente local
                cita.Estado = "Finalizada";

                //Guardamos
                await teleDemoDBContext.SaveChangesAsync();
                //Regresamos al index
                return RedirectToAction("AgendaDt");
            }
            //En el caso que no se encontro, vamos al index
            return RedirectToAction("AgendaDt");
        }

        //--------------------------------------- Ver Agenda Asistente Remoto --------------------------------

        //Agenda Asistente Remoto
        [HttpGet]
        public async Task<IActionResult> AgendaAR()
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/Index");
            }
            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype != "AsistRemoto")
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/login");
            }

            //Recepcion si es que existe el mensaje por registrar un asistenteLocal
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = "";
            }

            var id = Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId));
            var citas = await teleDemoDBContext.CitaFinal.Where(x => x.AsistenteRemotoId == id && x.Fecha >= DateTime.Now && x.Estado == "Pendiente").OrderBy(x => x.Fecha).ToListAsync();

            //Acomodamos las citas con el formato a presentar
            List<Models.Agenda.CitaFinalF> listadeCitas = new List<Models.Agenda.CitaFinalF>();
            //Ahora vamos a llenar los los campos
            foreach (var citasrow in citas)
            {
                //Creamos objeto de la lista
                Models.Agenda.CitaFinalF citadatos = new Models.Agenda.CitaFinalF();

                //Consulta para unir la id de localidad de la tabla Complejo con Localidad
                var Complejo = await teleDemoDBContext.Complejo.FirstOrDefaultAsync(x => x.Id == citasrow.ComplejoId);
                var Paciente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Id == citasrow.PacienteId);
                var Doctor = await teleDemoDBContext.Doctors.FirstOrDefaultAsync(x => x.Id == citasrow.DoctorId);

                //Unir los datos
                citadatos.Id = citasrow.Id;
                citadatos.NombreDoctor = Doctor.Nombre + " " + Doctor.Apellidos;
                citadatos.NombrePaciente = Paciente.Nombre + " " + Paciente.Apellidos;
                citadatos.Fecha = citasrow.Fecha;
                citadatos.Hora = citasrow.Hora;
                citadatos.Estado = citasrow.Estado;
                citadatos.Comentarios = citasrow.Comentarios;
                citadatos.roomid = citasrow.roomid;
                listadeCitas.Add(citadatos);
            }
            //Ahora si mandamos a la vista de Index
            return View(listadeCitas);
        }//Metodo Index de la vista de citas temporales


        //--------------------------------------- Entrar a la Video Call --------------------------------

        [HttpGet]
        [Route("Agenda/Room/{roomId}")]
        public async Task<IActionResult> Room(string roomId)
        {
            //Comprobacion si inicio session
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyType)))
            {
                //No hay datos
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/Index");
            }

            //Asignacion de id de cuarto
            ViewBag.roomId = roomId;

            //Comprobacion si tiene el permiso
            var usertype = HttpContext.Session.GetString(SessionVariables.SessionKeyType);
            if (usertype == "Doctor")
            {
                var cita = await teleDemoDBContext.CitaFinal.FirstOrDefaultAsync(x => x.roomid == roomId);
                if (cita.DoctorId != Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)))
                {
                    //No es el Doctor
                    HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                    return Redirect("/Home/login");
                }   
                //Si es Doctor
                ViewBag.rango = "D";
                //Creamos el objeto a enviar
                CitaEnd citaEnd = new CitaEnd();
                //Sacamos la info del paciente
                var paciente = await teleDemoDBContext.Pacientes.FirstOrDefaultAsync(x => x.Id == cita.PacienteId);
                citaEnd.PacienteName = paciente.Nombre + " " + paciente.Apellidos;
                citaEnd.PacienteSex = paciente.Sexo;
                citaEnd.PaciOcupa = paciente.Ocupacion;
                citaEnd.PaciHere = paciente.Hereditarios;
                //Sacamos la edad del paciente
                DateTime now = DateTime.Today;
                int edad = DateTime.Today.Year - paciente.Nacimiento.Year;
                if (DateTime.Today < paciente.Nacimiento.AddYears(edad))
                {
                    --edad;
                }
                citaEnd.PaciEdad = "" + edad;
                //Sacamos la info del motivo de la cita
                citaEnd.MotivoCita = cita.Comentarios;

                //Ahora buscamos el historial
                List<Historial> historial = new List<Historial>();
                //Buscamos las citas finalizadas que tenga esa persona
                var citasfinalizadas = await teleDemoDBContext.CitaFinal.Where(x => x.PacienteId == cita.PacienteId && x.Estado == "Finalizada").ToListAsync();
                foreach (var cc in citasfinalizadas)
                {
                    Historial histo = new Historial();
                    //Sacamos fecha
                    histo.fecha = cc.Fecha;
                    //Sacamos el diagnostico
                    var diag = await teleDemoDBContext.Diagnostico.FirstOrDefaultAsync(x => x.CitaId == cc.Id);
                    if(diag.DiagnosticoTexto == null)
                    {
                        diag.DiagnosticoTexto = "No se registro Diagnostico";
                    }
                    histo.diagnostico = diag.DiagnosticoTexto;
                    //Sacamos las recetas
                    List<RecetaH> Receta = new List<RecetaH>();
                    var receta = await teleDemoDBContext.Receta.Where(x => x.CitaId == cc.Id).ToListAsync();
                    if (receta != null)
                    {
                        foreach (var rec in receta)
                        {
                        //Guardamos la prescripcion
                        RecetaH recetaTemp = new RecetaH();
                        recetaTemp.prescripcion = rec.prescripcion;
                        //Sacamos los medicamentos
                        List<RecetaItems> rlista = new List<RecetaItems>();
                        var recetaitems = await teleDemoDBContext.RecetaLista.Where(x => x.recetaId == rec.Id).ToListAsync();
                        foreach (var item in recetaitems)
                        {
                            RecetaItems rlist = new RecetaItems();
                            //Se guarda la info de medicamento
                            rlist.Dosis = item.Dosis;
                            rlist.Fecuencia = item.frecuencia;
                            rlist.Medicamento = item.Medicamento;
                            //Se guarda en la lista de medicamentos
                            rlista.Add(rlist);
                        }
                        
                        //Se guarda en la recetatemp
                        recetaTemp.Items = rlista;
                        Receta.Add(recetaTemp);
                        }
                        //Fin ciclo rec in receta
                        histo.Receta = Receta;
                    }
                    
                    //Fin ciclo rec in receta
                    historial.Add(histo);

                }//Fin ciclo cc in citasfinalizadas
                //guardamos ese historial en la cita final
                citaEnd.Historial = historial;

                return View(citaEnd);
            }
            else if (usertype == "AsistRemoto")
            {
                var cita = await teleDemoDBContext.CitaFinal.FirstOrDefaultAsync(x => x.roomid == roomId);
                if (cita.AsistenteRemotoId != Convert.ToInt32(HttpContext.Session.GetString(SessionVariables.SessionKeyId)))
                {
                    //No es el Asistente Remoto
                    HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                    return Redirect("/Home/login");
                }
                //Si es Asistente Remoto
                ViewBag.rango = "A";
            }
            else
            {
                //Tiene el rango que no es
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Acceso Restringido");
                return Redirect("/Home/login");
            }

            
            return View();
        }


        // ------------------------------------ Ver Receta ---------------------

        public async Task<IActionResult> rcpdf(int id)
        {
            //Creamos el objeto que vamos a mostrar
            Recetashow RecetaFinal = new Recetashow();

            //Sacamos la informacion de la receta que accede
            var receta = await teleDemoDBContext.Receta.Where(x => x.Id == id ).FirstOrDefaultAsync();
            if (receta == null)
            {
                HttpContext.Session.SetString(SessionVariables.SessionMessage, "Receta No encontrada");
                return Redirect("/Home/login");
            }

            //Colocamos la fecha y prescripcion
            RecetaFinal.fecha = receta.fecha;
            RecetaFinal.Prescripcion = receta.prescripcion;

            //Sacamos info de Doc,Paciente y Complejo de la Cita de esa receta
            var Cita = await teleDemoDBContext.CitaFinal.Where(x => x.Id == receta.CitaId).FirstOrDefaultAsync();

            //Doctor
            var Doctor = await teleDemoDBContext.Doctors.Where(x => x.Id == Cita.DoctorId).FirstOrDefaultAsync();
            RecetaFinal.DocNombre = Doctor.Nombre + " " + Doctor.Apellidos;
            RecetaFinal.DocCelula = Doctor.Matricula;
            RecetaFinal.DocEscuela = Doctor.Institucion;
            RecetaFinal.DocFirma = Doctor.Firma;
            RecetaFinal.DocRango = Doctor.Especialidad;

            //Paciente
            var Paciente = await teleDemoDBContext.Pacientes.Where(x => x.Id == Cita.PacienteId).FirstOrDefaultAsync();
            RecetaFinal.PacNombre = Paciente.Nombre + " " + Paciente.Apellidos;
            //Sacamos la edad del paciente
            DateTime now = DateTime.Today;
            int edad = DateTime.Today.Year - Paciente.Nacimiento.Year;
            if (DateTime.Today < Paciente.Nacimiento.AddYears(edad))
            {
                --edad;
            }
            RecetaFinal.PacEdad = "" + edad;

            //Complejo
            var Complejo = await teleDemoDBContext.Complejo.Where(x => x.Id == Cita.ComplejoId).FirstOrDefaultAsync();
            RecetaFinal.ModuloNombre = Complejo.Nombre;
            RecetaFinal.ModuloUbicacion = Complejo.Ubicacion;

            //Medicamentos
            List<RecetaItems> listaMedicamentos = new List<RecetaItems>();
            var recetaLista = await teleDemoDBContext.RecetaLista.Where(x => x.recetaId == receta.Id).ToListAsync();
            //Obtenemos todos los medicamnetos
            foreach (var item in recetaLista)
            {
                RecetaItems Medicamento = new RecetaItems();
                //Sacamos los datos de lista
                Medicamento.Medicamento = item.Medicamento;
                Medicamento.Dosis = item.Dosis;
                Medicamento.Fecuencia = item.frecuencia;

                listaMedicamentos.Add(Medicamento);
            }
            //Agregamos la lista
            RecetaFinal.Items = listaMedicamentos;


            //return View(RecetaFinal);
            ///*
            return new ViewAsPdf("rcpdf", RecetaFinal)
            {
                FileName = "Receta.pdf",
                PageMargins = { Left = 20, Bottom = 20, Right = 20, Top = 20 },
            };
            //*/
        }
    }
}

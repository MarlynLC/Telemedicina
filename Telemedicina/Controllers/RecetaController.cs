using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

using System;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json;
using Telemedicina.Data;
using Microsoft.EntityFrameworkCore;
using Telemedicina.Models.Dominio;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Telemedicina.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class RecetaController : Controller
    {
        //Poder utilizar el contexto de la base de datos
        private readonly TeleDemoDBContext teleDemoDBContext;


        public RecetaController(TeleDemoDBContext teleDemoDBContext)
        {
            this.teleDemoDBContext = teleDemoDBContext;
        }

        //Metodo para crear/guardar diagnostico
        [HttpPost]
        [EnableCors("MyPolicy")]
        public async Task<IActionResult> receta()
        {
            //Sacamos la informacion del json
            using var reader = new StreamReader(Request.Body);
            var body = reader.ReadToEndAsync().Result;
            var json = JsonConvert.DeserializeObject<JObject>(body);
            //Verificacion si esta vacio
            if (json["press"] == null) return BadRequest();
            //Verificacion si esta vacio
            if (json["recetas"] == null) return BadRequest();
            //Receta principal
            //Empezamos sacando la id de room
            var idroom = json["id"].ToString();
            if (idroom == null) return BadRequest();
            //Buscamos la cita con esa id de room
            var cita = await teleDemoDBContext.CitaFinal.FirstOrDefaultAsync(x => x.roomid.Equals(idroom));
            //Si no encontramos nada regresamos
            if (cita == null) return BadRequest();
            //Creamos la receta
            var receta = new Receta()
            {
                CitaId = cita.Id,
                fecha = DateTime.Now,
                prescripcion = json["press"].ToString()
            };
            //Hacemos el movimiento en la base de datos
            await teleDemoDBContext.Receta.AddAsync(receta);
            await teleDemoDBContext.SaveChangesAsync();
            //Ahora creamos la lista de medicamentos de la receta

            //Obtenemos la id de la receta
            var recetaId = await teleDemoDBContext.Receta.OrderBy(x => x.fecha).LastOrDefaultAsync(x => x.CitaId == cita.Id);
            if (recetaId == null) return BadRequest();
            //Vemos cuantos medicamentos llegaron
            foreach (var recetaobj in json["recetas"])
            {
                //Creamos la lista
                var lista = new RecetaLista
                {
                    recetaId = recetaId.Id,
                    Dosis = recetaobj["dosis"].ToString(),
                    Medicamento = recetaobj["nombre_medicamento"].ToString(),
                    frecuencia = recetaobj["frecuencia"].ToString(),
                };
                //Agregamos a la base
                await teleDemoDBContext.RecetaLista.AddAsync(lista);
            }
            //Guardamos todo
            await teleDemoDBContext.SaveChangesAsync();

            return Json(body);
        }

        //Metodo para crear/guardar diagnostico
        [HttpPost]
        [EnableCors("MyPolicy")]
        [Route("diag")]
        public async Task<IActionResult> diagnostico()
        {
            //Sacamos la informacion del json
            using var reader = new StreamReader(Request.Body);
            var body = reader.ReadToEndAsync().Result;
            var json = JsonConvert.DeserializeObject<JObject>(body);
            
            //Verificacion si esta vacio
            if (json["diagnostico"] == null) return BadRequest();
            //Sacamos la id
            var idroom = json["id"].ToString();
            if (idroom == null) return BadRequest();
            //Buscamos la cita con esa id de room
            var cita = await teleDemoDBContext.CitaFinal.FirstOrDefaultAsync(x => x.roomid == idroom);
            //Si no encontramos nada regresamos
            if (cita == null) return BadRequest();
            var diagtemp = await teleDemoDBContext.Diagnostico.FirstOrDefaultAsync(x => x.CitaId == cita.Id);
            //checamos si existe un diagnostico a esa cita
            if (diagtemp == null) {
                //Si no existe uno lo creamos
                var diagnew = new Diagnostico
                {
                    CitaId = cita.Id,
                    DiagnosticoTexto = json["diagnostico"].ToString(),
                    peso = json["pesocorporal"].ToString(),
                    presion = json["presionarterial"].ToString(),
                    ritmo = json["ritmocardiaco"].ToString(),
                    temp = json["temperatura"].ToString(),
                    fecha = DateTime.Now,
                };
                //subimos a diagnostico
                await teleDemoDBContext.Diagnostico.AddAsync(diagnew);
                //Guardamos cambios
                await teleDemoDBContext.SaveChangesAsync();
            } else
            {
                //Ahora si existe ya un dignostico, se actualiza el existente
                diagtemp.DiagnosticoTexto = json["diagnostico"].ToString();
                diagtemp.peso = json["pesocorporal"].ToString();
                diagtemp.presion = json["presionarterial"].ToString();
                diagtemp.ritmo = json["ritmocardiaco"].ToString();
                diagtemp.temp = json["temperatura"].ToString();
                //Guardamos cambios
                await teleDemoDBContext.SaveChangesAsync();
            }

            return Json(body);
        }

        //Metodo para decir si hay recetas de una reunion
        [HttpPost]
        [EnableCors("MyPolicy")]
        [Route("hayrece")]
        public async Task<IActionResult> hayreceta()
        {   
            //Sacamos la informacion del json
            using var reader = new StreamReader(Request.Body);
            var body = reader.ReadToEndAsync().Result;
            var json = JsonConvert.DeserializeObject<JObject>(body);

            //Verificacion si esta vacio
            if (json["id"] == null) return BadRequest();
            //Sacamos la id
            var idroom = json["id"].ToString();
            //Buscamos la cita con esa id de room
            var cita = await teleDemoDBContext.CitaFinal.FirstOrDefaultAsync(x => x.roomid == idroom);
            //Si no encontramos nada regresamos
            if (cita == null) return BadRequest();
            //Treamos todas las recetas esa cita
            var recetas = await teleDemoDBContext.Receta.Where(x => x.CitaId == cita.Id).ToListAsync();
            //Comprabamos si es no tienes recetas
            if (recetas.Count == 0) return BadRequest();
            //Si tiene recetas, entonces hay que preparar el link
            string[] links = new string[recetas.Count];
            int a = 0;
            foreach (var link in recetas)
            {
                var idreceta = link.Id;
                string aa = "https://localhost:7051/Agenda/rcpdf/" + idreceta;
                links[a] = aa;
                a++;
            }

            return Json(links);
        }

    }
}

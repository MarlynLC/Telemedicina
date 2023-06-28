using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telemedicina.Models.Dominio
{
    public class Paciente
    {
        //Clase del Paciente, esta definido las variables que contiene,
        //para la rapida configuracion de la base de datos
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
        public string? Sexo { get; set; }
        public string? Sangre { get; set; }
        public DateTime Nacimiento { get; set; }
        public string? Curp { get; set; }
        public string? Domicilio { get; set; }
        public string? Localidad { get; set;}
        public string? Municipio { get; set; }
        public string? Estado { get; set;}
        public string? CodigoPostal { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Escolaridad { get; set; }
        public string? Familiares { get; set; }
        public string? Ocupacion { get; set; }
        public string? Hereditarios { get; set; }
        public string? Visilbe { get; set; }
        public string? Imagen { get; set; }

        [NotMapped]
        public IFormFile? File { get; set; }
    }
}

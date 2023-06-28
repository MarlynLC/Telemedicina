﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Telemedicina.Models.Dominio
{
    public class AsistenteRemoto
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
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Comentarios { get; set; }
        public string? Password { get; set; }
        public string? Imagen { get; set; }
        public string? Visilbe { get; set; }
        public DateTime FechaIngreso { get; set; }

        [NotMapped]
        public IFormFile? File { get; set; }

    }
}

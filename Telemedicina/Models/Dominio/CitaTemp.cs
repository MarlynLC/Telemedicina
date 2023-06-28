namespace Telemedicina.Models.Dominio
{
    public class CitaTemp
    {
        //Clase para referencia a la tabla de CitaTemp de la base de datos,
        //no modificar, vinculada con base de datos
        public int Id { get; set; }
        public int AsistenteRemotoId { get; set; }
        public int PacienteId { get; set; }
        public int ComplejoId { get; set; }
        public DateTime Fecha { get; set; }
        public string? Comentarios { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? Estado { get; set; }
    }
}

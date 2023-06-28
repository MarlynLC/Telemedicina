namespace Telemedicina.Models.Dominio
{
    public class Complejo
    {
        //Datos de la tabla Entidad para la base de datos
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Ubicacion { get; set;}
        public int IdLocalidad { get; set; }

    }
}

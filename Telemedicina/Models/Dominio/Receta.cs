namespace Telemedicina.Models.Dominio
{
    public class Receta
    {
        public int Id { get; set; }
        public int CitaId { get; set; }
        public DateTime fecha { get; set; }
        public string? prescripcion { get; set; }
    }
}

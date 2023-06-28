namespace Telemedicina.Models.Dominio
{
    public class RecetaLista
    {
        public int Id { get; set; }
        public int recetaId { get; set; }
        public string? Medicamento { get; set; }
        public string? Dosis { get; set; }
        public string? frecuencia { get; set; }

    }
}

namespace Telemedicina.Models.Dominio
{
    public class MAsistLocal
    {
        public int Id { get; set; }
        public int AsistenteLocalId { get; set; }
        public string? TipoModificacion { get; set; }
        public int UsuarioId { get; set; }
        public string? UsuarioTipo { get; set; }
        public DateTime Fecha { get; set; }
    }
}

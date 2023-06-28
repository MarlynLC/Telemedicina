namespace Telemedicina.Models.Dominio
{
    public class MAsistRemoto
    {
        public int Id { get; set; }
        public int AsistenteRemotoId { get; set; }
        public string? TipoModificacion { get; set; }
        public int UsuarioId { get; set; }
        public string? UsuarioTipo { get; set; }
        public DateTime Fecha { get; set; }
    }
}

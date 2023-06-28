//Libreria para la manipulacion de plantillas/Base de datos
using Microsoft.EntityFrameworkCore;
using Telemedicina.Models.Dominio;

namespace Telemedicina.Data
{
    // Agregamos el DbContext a la clase, para la manupulacion de vistas/base de datos
    public class TeleDemoDBContext : DbContext
    {
        //Creamos el constructor de la clase
        public TeleDemoDBContext(DbContextOptions options) : base(options)
        {

        }
        //Referencia a la tabla de Paciente.
        public DbSet<Paciente> Pacientes { get; set; }

        //Referencia a la tabla de Modificaciones Paciente
        public DbSet<MPaciente> MPaciente { get; set; }

        //Referencia a la tabla de Doctor
        public DbSet<Doctor> Doctors { get; set; }

        //Referencia a la tabla de Modificaciones Doctor
        public DbSet<MDoctor> MDoctor { get; set; }

        //Referencia a la tabla AsistenteLocal
        public DbSet<AsistenteLocal> AsistenteLocal { get; set; }

        //Referencia a la tabla de Modificaciones AsistLocal
        public DbSet<MAsistLocal> MAsistLocal { get; set; }

        //Referencia a la tabla AsistenteRemoto
        public DbSet<AsistenteRemoto> AsistenteRemotos { get; set; }

        //Referencia a la tabla de Modificaciones AsistRemoto
        public DbSet<MAsistRemoto> MAsistRemoto { get; set; }

        //Referencia a la tabla de Localidades
        public DbSet<Localidad> Localidad { get; set; }

        //Referencia a la tabla de Complejo
        public DbSet<Complejo> Complejo { get; set; }

        //Referencia a la tabla de ComplejoDoctor
        public DbSet<ComplejoDoctor> ComplejoDoctor { get; set; }
        //Referencia a la tabla de ComplejoAsistR
        public DbSet<ComplejoAsistR> ComplejoAsistR { get; set; }
        //Referencia a la tabla de CitasTemporaes
        public DbSet<CitaTemp> CitaTemp { get; set; }
        //Referencia a la tabla de Localidades con AsistentesLocales
        public DbSet<LocalAsistente> LocalAsistente { get; set; }
        //Referencia a la tabla de CitasFinales
        public DbSet<CitaFinal> CitaFinal { get; set; }
        //Referencia a la tabla de Diagnostico
        public DbSet<Diagnostico> Diagnostico { get; set; }
        //Referencia a la tabla de Receta
        public DbSet<Receta> Receta { get; set; }
        //Referencia a la tabla de RecetaLista
        public DbSet<RecetaLista> RecetaLista { get; set; }
    }
}

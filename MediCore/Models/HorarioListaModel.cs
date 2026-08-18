using System;

namespace MediCore.Models
{
    public class HorarioListaModel
    {
        public int IdHorario { get; set; }
        public int IdDoctor { get; set; }
        public string NombreDoctor { get; set; }
        public byte DiaSemana { get; set; }
        public string NombreDia { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public int DuracionCitaMin { get; set; }
        public string Estado { get; set; }
    }
}
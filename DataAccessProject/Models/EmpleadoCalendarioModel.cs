using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models
{
    public class EmpleadoCalendarioModel
    {
        public DateTime Fecha { get; set; }
        public string DiaSemana { get; set; }

        public int? fila { get; set; }
        public int? SDia { get; set; }
        public int IdTurno { get;set; }

        public string NomTurno { get; set; }
        public int IdHorario { get; set; }

        public string NomHorario { get; set; }
        public string Observacion { get; set; }


    }
}

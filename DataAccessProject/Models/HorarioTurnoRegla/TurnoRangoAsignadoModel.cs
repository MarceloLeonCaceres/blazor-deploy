using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.HorarioTurnoRegla
{
    public class TurnoRangoAsignadoModel
    {
        public int IdTurno { get; set; }
        public string NomTurno { get; set; }
        public DateTime FechaIni { get; set; }
        public DateTime FechaFin { get; set; }

        public TurnoRangoAsignadoModel()
        {
            FechaIni = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            FechaFin = new DateTime(DateTime.Now.Year + 5, 12, 31);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.HorarioTurnoRegla
{
    public class HorarioChartModel
    {
        public int sDia { get; set; }
        public TimeSpan Inicio { get; set; }
        public TimeSpan Fin { get; set; }
        public int idHorario { get; set; }
        public int ordenHorario { get; set; }
    }
}

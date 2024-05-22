using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.HorarioTurnoRegla
{
    public class RaHoraExtraModel
    {
        public int? id_RegHoraExtra { get; set; }
        public string Des_RegHoraExtra { get; set; }
        public bool Overtime { get; set; }
        public bool RegisterOT { get; set; }
        public bool HorarioRotativo { get; set; }
        public bool FueraDeHorario { get; set; }
        public bool HEDiaLaborable { get; set; }

    }
}

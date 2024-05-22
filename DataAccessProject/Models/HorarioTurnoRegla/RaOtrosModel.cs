using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.HorarioTurnoRegla
{
    public class RaOtrosModel
    {
        public int? id_RegOtros { get; set; }
        public string Des_RegOtros { get; set; }
        public bool AtrasoGracia { get; set; }
        public bool CobraMulta { get; set; }
        public int HOLIDAY { get; set; }
        public string EnFeriado { get; set; }

    }
}

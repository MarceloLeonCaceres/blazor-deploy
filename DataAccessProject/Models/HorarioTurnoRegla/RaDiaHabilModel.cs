using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.HorarioTurnoRegla
{
    public class RaDiaHabilModel
    {
        public int? id_RegDiaHabil { get; set; }
        public string Des_RegDiaHabil { get; set; }
        public bool dia0 { get; set; }
        public bool dia1 { get; set; }
        public bool dia2 { get; set; }
        public bool dia3 { get; set; }
        public bool dia4 { get; set; }
        public bool dia5 { get; set; }
        public bool dia6 { get; set; }
        public bool DiaHabilRotativo { get; set; }

    }
}

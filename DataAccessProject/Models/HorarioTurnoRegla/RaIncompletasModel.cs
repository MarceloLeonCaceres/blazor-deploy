using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.HorarioTurnoRegla
{
    public class RaIncompletasModel
    {
        // Tabla RegAsistencia
        // (Marcaciones Incompletas)
        public int? id_RegAsistencia { get; set; }
        public string Des_RegAsistencia { get; set; }
        public int JMaximaT { get; set; }
        public int JminimaT { get; set; }
        public int Intervalo { get; set; }
        public int NoTIngreso { get; set; }
        public int NoTSalida { get; set; }
        public int? MultaIngreso { get; set; }
        public int? MultaSalida { get; set; }
        public string SinIngreso { get; set; }
        public string SinSalida { get; set; }

    }
}

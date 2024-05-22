using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Vacaciones
{
    public class ConfigVacationModel
    {
        public int idContrato { get; set; }
        public string contrato { get; set; }
        public double dVacPeriodo { get; set; }
        public double dVacHabiles { get; set; }
        public double dVacAdicional{ get; set; }
        public int aPartir { get; set; }
        public int dMaxAcumulable { get; set; }
        public int bPeriodoCarga { get; set; }
        public int bDiaCarga { get; set; }
        public int eTiempoCumplido { get; set; }
        public double fDescuentoV { get; set; }
        public double fDescuentoCV { get; set; }
        public bool bAtrasosCV { get; set; }
        public bool bSalidasCV { get; set; }
        public bool bAusenciasCV { get; set; }
        public bool bExcesoLunchCV { get; set; }
        public bool bDescontarDH { get; set; }

        public ConfigVacationModel()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.HorarioTurnoRegla
{
    public class RaSobreTiempoModel
    {
        public int? id_RegSobretiempo { get; set; }
        public string Des_RegSobretiempo { get; set; }
        public int Tarde { get; set; }
        public int Temprano { get; set; }
        public int? MinTarde { get; set; }
        public int? MinTemprano { get; set; }
        public string SaleTarde { get; set; }
        public string LlegaTemprano { get; set; }
    }
}

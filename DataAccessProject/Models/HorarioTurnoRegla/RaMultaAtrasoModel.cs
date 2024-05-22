using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.HorarioTurnoRegla
{
    public class RaMultaAtrasoModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public double? Menos15 { get; set; }
        public double? Entre15_30 { get; set; }
        public double? Mas30 { get; set; }
    }
}

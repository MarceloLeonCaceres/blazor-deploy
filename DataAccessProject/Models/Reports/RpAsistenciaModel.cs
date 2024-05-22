using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Reports
{
    public class RpAsistenciaModel
    {
        public int UserId { get; set; }
        public string NumCA { get; set; }   
        public string Empleado { get; set; }
        public string Cedula { get; set; }
        public string Departamento { get; set; }
        public string Cargo { get; set; }
        public string Dia { get; set; }
        public DateTime Fecha { get; set; }
        public int CodH { get; set; }

        public string Horario { get; set; }
        public string HoraEntrada { get; set; }
        public string HoraSalida { get; set; }
        public string RegEntrada { get; set; }
        public string RegSalida { get; set; }
        public string SalidaAlmuerzo { get; set; }
        public string RegresoAlmuerzo { get; set; }
        public DateTime? TiempoAlmuerzo { get; set; }
        public DateTime? ExcesoAlmuerzo { get; set; }
        public DateTime? Atrasos { get; set; }
        public DateTime? SalidasTemprano { get; set; }
        public double? Ausente { get; set; }
        public string? Permiso { get; set; }
        public string? MotivoPermiso { get; set; }
        public DateTime? HNormal { get; set; }
        public DateTime? H_25 { get; set; }
        public DateTime? H_50 { get; set; }
        public DateTime? H_100 { get; set; }
        public DateTime? DiaLibre { get; set; }
        public DateTime? TPermisoTrab { get; set; }
        public DateTime? TPermisoNoTrab { get; set; }
        public double? TotalHE100 { get; set; }
        public DateTime? TTrabajado { get; set; }
        public DateTime? TAsistido { get; set; }
        public DateTime? TNoCumplido { get; set; }
        public float? USD_25 { get; set; }
        public float? USD_50 { get; set; }
        public float? USD_100 { get; set; }
        public float? USD_Total { get; set; }
        public float? MultaAtrasos { get; set; }
        public float? MultaAusencias { get; set; }
        public double? DiaTrabajado { get; set; }        
        public string Autorizado { get; set; }
        public bool? AprobadoHE50 { get; set; }
        public bool? AprobadoHE100 { get; set; }
        public DateTime? TAprobadoHE50 { get; set; }
        public DateTime? TAprobadoHE100 { get; set; }
        public DateTime? TAprobadoDiaLibre { get; set; }

        public float? AUSD_50 { get; set; }
        public float? AUSD_100 { get; set; }
        public float? AUSD_Total { get; set; }
        public string Motivo { get; set; }


    }
}

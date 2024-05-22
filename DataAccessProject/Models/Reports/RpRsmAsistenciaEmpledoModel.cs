namespace DataAccessLibrary.Models.Reports
{
    public class RpRsmAsistenciaEmpledoModel
    {
        public int UserId { get; set; }
        public string NumCA { get; set; }
        public string Empleado { get; set; }
        public string Cedula { get; set; }
        public string Departamento { get; set; }
        public string Cargo { get; set; }
        public double? DiasLaborables { get; set; }
        public double? DiasTrabajados { get; set; }
        public double? DiasAusente { get; set; }
        public int? NumAtrasos { get; set; }
        public int? NumSalidasTemprano { get; set; }
        public double? Atrasos { get; set; }
        public double? SalidasTemprano { get; set; }    
        public double? Almuerzo { get; set; }        
        public double? AnticipoAlmuerzo { get; set; }
        public double? AtrasoAlmuerzo { get; set; }
        public double? ExcesoAlmuerzo { get; set; }
        public string? HNormal { get; set; }
        
        public string? H_25 { get; set; }
        public string? H_50 { get; set; }
        public string? H_100 { get; set; }
        public string? DiaLibre { get; set; }
        public double? TotalHE100 { get; set; }
        public int? NumPermisos { get; set; }

        public string? TPermisoTrab { get; set; }
        public string? TPermisoNoTrab { get; set; }
        
        public string? TTrabajado { get; set; }
        public string? TAsistido { get; set; }
        public string? TNoCumplido { get; set; }
        public float? USD_25 { get; set; }
        public float? USD_50 { get; set; }
        public float? USD_100 { get; set; }
        public float? USD_Total { get; set; }
        public float? MultaAtrasos { get; set; }
        public float? MultaAusencias { get; set; }        
                
        public string? TAprobadoHE50 { get; set; }
        public string? TAprobadoHE100 { get; set; }
        public string? TAprobadoDiaLibre { get; set; }

        public float? AUSD_50 { get; set; }
        public float? AUSD_100 { get; set; }
        public float? AUSD_Total { get; set; }        
    }
}

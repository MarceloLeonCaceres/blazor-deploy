namespace DataAccessLibrary.Models.Reports
{
    public class ChartVariosEmpleadosModel : ChartDiasEmpleadoModel
    {
        public int UserId { get; set; }
        public string NomEmpleado { get; set; }
        public string LabelLaborables { get; set; }
        public string LabelTrabajados { get; set; }
        public string LabelAusencias { get; set; }
        public string LabelAtrasos { get; set; }
        public string LabelSalidasTemprano { get; set; }
        public string LabelExcesosLunch { get; set; }
        public string LabelPermisos { get; set; }

        public ChartVariosEmpleadosModel(int userid, string nomEmpleado, float laborables, float trabajados, float ausencias, float atrasos, float salidasTemplano,
            float excesoLunch, float permisos) : base(laborables, trabajados, ausencias, atrasos, salidasTemplano,
             excesoLunch, permisos)
        {
            UserId = userid;
            NomEmpleado = nomEmpleado;
            LabelLaborables = laborables.ToString();
            LabelTrabajados = trabajados.ToString();
            LabelAusencias = ausencias.ToString();
            LabelAtrasos = atrasos.ToString();
            LabelSalidasTemprano = salidasTemplano.ToString();
            LabelExcesosLunch = excesoLunch.ToString();
            LabelPermisos = permisos.ToString();
        }
        public ChartVariosEmpleadosModel()
        {
        }

    }
}

namespace DataAccessLibrary.Models.Reports
{
    public class DataChartEmpleado
    {
        public string NomEmpleado { get; set; }
        public double NumeroDias { get; set; }
        public string DataLabelMappingName { get; set; }
        public string color { get; set; }

        public DataChartEmpleado(string nomEmpleado, double numeroDias)
        {
            NomEmpleado = nomEmpleado;
            NumeroDias = numeroDias;
            DataLabelMappingName = numeroDias.ToString();
        }

        public DataChartEmpleado(string nomEmpleado, double numeroDias, string color)
        {
            NomEmpleado = nomEmpleado;
            NumeroDias = numeroDias;
            DataLabelMappingName = numeroDias.ToString();
            this.color = color;
        }

        public DataChartEmpleado()
        {
        }
    }
}

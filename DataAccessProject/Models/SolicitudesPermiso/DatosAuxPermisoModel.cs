namespace DataAccessLibrary.Models.SolicitudesPermiso
{
    public class DatosAuxPermisoModel
    {
        public int UserId { get; set; }
        public int IdSolicitud { get; set; }
        public string NumCA { get; set; }
        public string NomEmpleado { get; set; }
        public string CorreoEmpleado { get; set; }
        public int IdSupervisor1 { get; set; }
        public int IdSupervisor2 { get; set; }
        public string NombreS1 { get; set; }
        public string NombreS2 { get; set; }
        public string CorreoS1 { get; set; }
        public string CorreoS2 { get; set; }
        public int Estado { get; set; }
        public int respuesta1 { get; set; }
        public int? respuesta2 { get; set; }
    }
}

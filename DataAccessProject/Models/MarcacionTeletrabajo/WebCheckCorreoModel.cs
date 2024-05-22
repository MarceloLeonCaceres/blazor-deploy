using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.MarcacionTeletrabajo
{
    public class WebCheckCorreoModel
    {
        public int UserId { get; set; }
        public string NomEmpleado { get; set; }
        public string CorreoEmpleado { get; set; }
        public int IdSupervisor { get; set; }
        public string NomSupervisor { get; set; }
        public string CorreoSupervisor { get; set; }
    }
}

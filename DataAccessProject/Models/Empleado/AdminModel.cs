using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models
{
    public class AdminModel : EmpleadoBaseModel
    {
        public int OTAdmin { get; set; }
        public string OTPrivAdmin { get; set; }
        public string OTPassword { get; set; }
        public string Username { get; set; }

        public bool[] Vis { get; set; }
        public bool esAdministrador { get; set; }
        public bool esAprobador { get; set; }
        public bool esSupervisor3 { get; set; }
        public bool esSupervisorXL { get; set; }
        public bool activo { get; set; }

        public AdminModel(string sTemporal)
        {
            if (sTemporal.ToLower() == "temporal")
            {
                UserId = 0;
                Badge = "0";
                NombreEmp = "Admin temporal";      
                activo = false;
            }
            else if(sTemporal.ToLower() == "master")
            {
                UserId = -1;
                Badge = "-1";
                NombreEmp = "Master Admin";
                activo=true;
            }

            DeptId = 1;
            Departamento = null;

            OTAdmin = 3;
            OTPrivAdmin = null;
            OTPassword = null;
            Username = null;

            esAdministrador = true;
            esAprobador = true;
            esSupervisor3 = true;
            esSupervisorXL = true;            
        }

        public AdminModel()
        {
        }
    }    
        

}

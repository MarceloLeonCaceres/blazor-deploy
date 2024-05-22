using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class UserinfoModel : AdminModel
    {
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Solo números")]
        public string Cedula { get; set; }  // ssn
        public int gender { get; set; }  // gender
        public string nomGenero { get; set; }  // gender
        public string Cargo { get; set; }   // Title
        public string Celular { get; set; } // pager
        
        
        public int idCentroCostos { get; set; }    // CONTRATO.idCentroCostos
        public string nomCentroCostos { get; set; }    // CENTRO_COSTOS.nomCentroCostos
        public int idContrato { get; set; }    // CONTRATO.idContrato
        public string nomContrato { get; set; }    // CONTRATO.nomContrato
        public DateTime? FechaNacimiento { get; set; }   // BirthDay
        public DateTime? FechaEmpleo { get; set; }   // HiredDay
        public DateTime? FechaSalida { get; set; }   
        public string Direccion { get; set; }       // street
        public int idCiudad { get; set; }
        public string nomCiudad { get; set; }          // CIUDAD.nomCiudad
        public string TelOficina { get; set; }      // OPhone
        public string CodigoEmpleado { get; set; }  // CardNo
        public string CorreoOficina { get; set; }
        public string CorreoPersonal { get; set; }
        public byte[] PhotoB { get; set; }
        public string Notes { get; set; }

        // public int IdTipoEmpleado { get; set; }     // ya no se usa, vino de ProperTime pero fue reemplazado por esAdministrador, esAprobador, esSupervisor3, esSupervisorXl, 
        public bool esAdministrador { get; set; }
        public bool esAprobador { get; set; }
        public bool esSupervisor3 { get; set; }
        public bool esSupervisorXl { get; set; }
        public int idSupervisor1 { get; set; }
        public string Supervisor1 { get; set; }
        public int idSupervisor2 { get; set; }
        public string Supervisor2 { get; set; }
        public int idSupervisor3 { get; set; }
        public string Supervisor3 { get; set; }
        public int? CargasFamiliares { get; set; }
        public int idEstadoCivil { get; set; }     
        public string EstadoCivil { get; set; }     // nomEstadoCivil
        public int idTipoSangre { get; set; }
        public string TipoSangre { get; set; }
        public int idGrupoSalarial { get; set; }
        public string nomGrupoSalarial { get; set; }
        // public bool activo { get; set; }

        public int RegAsistencia { get; set; }
        public int RegSobretiempo { get; set; }
        public int RegHoraExtra { get; set; }
        public int RegDiaHabil { get; set; }
        public int RegOtros { get; set; }
        public int RegMultaAtraso { get; set; }


        public UserinfoModel()
        {
        }

        public UserinfoModel(string badge)
        {
            Badge = badge;
            NombreEmp = badge;
        }

        public UserinfoModel(string badge, UserinfoModel userinfoModel)
        {
            Badge = badge;
            NombreEmp = badge;
            DeptId = userinfoModel.DeptId;
            idCiudad = userinfoModel.idCiudad;
            nomContrato = userinfoModel.nomContrato;            
            OTAdmin = 0;
            esAdministrador = false;
            esAprobador = false;
            esSupervisor3 = false;
            esSupervisorXl = false;
            nomCentroCostos = userinfoModel.nomCentroCostos;            
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.DataAccessLayer
{

    public class ConstAsignacionEmpleados
    {
        public string colEnUser { get; set; }
        public string nombreTabla { get; set; }
        public string colIdTabla { get; set; }
        public string colDescripcionTabla { get; set; }

        ConstAsignacionEmpleados()
        {
        }

        ConstAsignacionEmpleados(string _colEnUser, string _nombreTabla, string _colIdTabla, string _colDescriptionTabla)
        {
            colEnUser = _colEnUser;
            nombreTabla = _nombreTabla;
            colIdTabla = _colIdTabla;
            colDescripcionTabla = _colDescriptionTabla;
        }

        public static ConstAsignacionEmpleados ParametersAllocation(string sVariable)
        {
            ConstAsignacionEmpleados parametros = new ConstAsignacionEmpleados();
            switch (sVariable)
            {
                case "RA Incompletas":
                    parametros.colEnUser = "RegAsistencia";
                    parametros.nombreTabla = "RegAsistencia";
                    parametros.colIdTabla = "id_RegAsistencia";
                    parametros.colDescripcionTabla = "Des_RegAsistencia";
                    break;
                case "RA Sobretiempo":
                    parametros.colEnUser = "RegSobretiempo";
                    parametros.nombreTabla = "RegSobretiempo";
                    parametros.colIdTabla = "id_RegSobretiempo";
                    parametros.colDescripcionTabla = "Des_RegSobretiempo";
                    break;
                case "RA DiasHabiles":
                    parametros.colEnUser = "RegDiaHabil";
                    parametros.nombreTabla = "RegDiaHabil";
                    parametros.colIdTabla = "id_RegDiaHabil";
                    parametros.colDescripcionTabla = "Des_RegDiaHabil";
                    break;
                case "RA HoraExtra":
                    parametros.colEnUser = "RegHoraExtra";
                    parametros.nombreTabla = "RegHoraExtra";
                    parametros.colIdTabla = "id_RegHoraExtra";
                    parametros.colDescripcionTabla = "Des_RegHoraExtra";
                    break;
                case "RA Otros":
                    parametros.colEnUser = "RegOtros";
                    parametros.nombreTabla = "RegOtros";
                    parametros.colIdTabla = "id_RegOtros";
                    parametros.colDescripcionTabla = "Des_RegOtros";
                    break;
                case "RA Multa Atrasos":
                    parametros.colEnUser = "RegMultaAtraso";
                    parametros.nombreTabla = "RegMultaAtraso";
                    parametros.colIdTabla = "Id";
                    parametros.colDescripcionTabla = "Name";
                    break;
                case "A_Departamento":
                    parametros.colEnUser = "DefaultDeptId";
                    parametros.nombreTabla = "Departments";
                    parametros.colIdTabla = "DeptId";
                    parametros.colDescripcionTabla = "DeptName";
                    break;
                case "A_Ciudad":
                    parametros.colEnUser = "idCiudad";
                    parametros.nombreTabla = "Ciudad";
                    parametros.colIdTabla = "idCiudad";
                    parametros.colDescripcionTabla = "nomCiudad";
                    break;
                case "A_CentroCostos":
                    parametros.colEnUser = "idCentroCostos";
                    parametros.nombreTabla = "Centro_Costos";
                    parametros.colIdTabla = "idCentroCostos";
                    parametros.colDescripcionTabla = "nomCentroCostos";
                    break;
                case "A_Contrato":
                    parametros.colEnUser = "idContrato";
                    parametros.nombreTabla = "Contrato";
                    parametros.colIdTabla = "idContrato";
                    parametros.colDescripcionTabla = "nomContrato";
                    break;
                case "A_GrupoSalarial":
                    parametros.colEnUser = "idGrupoSalarial";
                    parametros.nombreTabla = "GrupoSalarial";
                    parametros.colIdTabla = "idGrupo";
                    parametros.colDescripcionTabla = "nomGrupo";
                    break;
            }
            return parametros;

        }

    }

}
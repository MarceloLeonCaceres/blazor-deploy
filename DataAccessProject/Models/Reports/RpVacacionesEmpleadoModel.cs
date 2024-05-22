using DataAccessLibrary.Models.Vacaciones;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Reports
{
    public class RpVacacionesEmpleadoModel : EmpleadoContratoVacacionModel
    {

        public List<FilaCalculoVacacionModel> FilasV { get; set; }

        public RpVacacionesEmpleadoModel(int userId, string numCA, string nomEmpleado, int idContrato, string nomContrato, DateTime fechaEmpleo)
        {
            UserId = userId;
            NumCA = numCA;
            NomEmpleado = nomEmpleado;
            IdContrato = idContrato;
            NomContrato = nomContrato;
            this.FechaEmpleo = fechaEmpleo;
            FilasV = null;
        }

        public RpVacacionesEmpleadoModel(EmpleadoContratoVacacionModel empContFempleo)
        {
            UserId = empContFempleo.UserId;
            NumCA = empContFempleo.NumCA;
            NomEmpleado = empContFempleo.NomEmpleado;
            IdContrato = empContFempleo.IdContrato;
            NomContrato = empContFempleo.NomContrato;
            FechaEmpleo = empContFempleo.FechaEmpleo;
            FilasV = null;
        }
    }

}

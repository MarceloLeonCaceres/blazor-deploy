using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataAccessLibrary.Models.FeriadoLogTipoPermiso
{
    public class TipoPermisoModel
    {
        // Tabla LeaveClass

        public int? LeaveId { get; set; }

        [Required(ErrorMessage = "Se necesita un nombre")]
        [StringLength(40, ErrorMessage = "El nombre puede tener hasta 40 caracteres")]
        public string LeaveName { get; set; }

        [Required(ErrorMessage = "Se necesita un código (iniciales)")]
        [StringLength(4, ErrorMessage = "El código puede tener hasta 4 caracteres")]
        public string ReportSymbol { get; set; }
        public int idCategoria { get; set; }
        public string categoria { get; set; }

        public int idSeleccionable { get; set; }
        public bool seleccionable { get; set; }
        // public TipoSeleccion Seleccionable { get; set; }
        //public int Classify { get; set; }
        //// 0    No trabajado (JUSTIFICADO)
        //// 1    Vacaciones
        //// 2    Cargo a Vacaciones
        //// 128  Trabajado
        //public int Seleccionable { get; set; }        

        public TipoPermisoModel(string nombre)
        {
            LeaveName = nombre;
        }
        public TipoPermisoModel()
        {

        }
    }




}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataAccessLibrary.Models
{
    public class CentroCostosModel
    {
        public int? IdCentroCostos { get; set; }
        [Required(ErrorMessage = "Se necesita un nombre")]
        [StringLength(50, ErrorMessage = "El nombre puede tener hasta 50 caracteres")]
        public string NomCentroCostos { get; set; }


        public CentroCostosModel(string nombre)
        {
            NomCentroCostos = nombre;
        }

        public CentroCostosModel()
        {
        }
    }
}

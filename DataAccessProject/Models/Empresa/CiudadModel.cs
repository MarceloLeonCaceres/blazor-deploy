using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataAccessLibrary.Models
{
    public class CiudadModel
    {
        public int? IdCiudad { get; set; }

        [Required(ErrorMessage = "Se necesita un nombre")]
        [StringLength(50, ErrorMessage = "El nombre puede tener hasta 50 caracteres")]
        public string NomCiudad { get; set; }


        public CiudadModel(string nombre)
        {
            NomCiudad = nombre;
        }

        public CiudadModel()
        {
        }
    }
}

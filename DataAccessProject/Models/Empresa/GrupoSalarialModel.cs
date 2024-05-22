using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataAccessLibrary.Models.Empresa
{
    public class GrupoSalarialModel
    {
        public int? IdGrupo { get; set; }
        [Required(ErrorMessage = "Se necesita un nombre")]
        [StringLength(50, ErrorMessage = "El nombre puede tener hasta 50 caracteres")]
        public string NomGrupo { get; set; }
        public bool Automatico { get; set; }
        public double Sueldo { get; set; }
        public double Bono { get; set; }
        public double HE50 { get; set; }
        public double HE100 { get; set; }
        public double JN25 { get; set; }
        public double DescuentoHora { get; set; }
        public double DescuentoDia { get; set; }


        public GrupoSalarialModel(string nombre)
        {
            NomGrupo = nombre;
        }

        public GrupoSalarialModel()
        {

        }

    }
}

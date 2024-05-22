using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.FeriadoLogTipoPermiso
{
    public class TipoCategoriaPermisoModel : EnumModel
    {
        public int Classify { get; set; }

        public TipoCategoriaPermisoModel(int codigo, string descripcion, int classify) : base(codigo, descripcion)
        {
            Classify = classify;
        }

        public TipoCategoriaPermisoModel()
        {   
        }

    }
}

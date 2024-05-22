using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models
{
    public class EnumModel
    {
        public int id { get; set; }
        public string descripcion { get; set; }

        public EnumModel()
        {
        }

        public EnumModel(int codigo, string descripcion)
        {
            this.id = codigo;
            this.descripcion = descripcion;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models
{
    public class EmpresaModel
    {
        public int Id { get; set; }
        
        public string Nombre { get; set; }
        public string Ruc { get; set; }
        public byte[] Logo { get; set; }
        public int IdCiudad { get; set; }
        public int Dia1Mes { get; set; }
        public string Direccion { get; set; }
        public EmpresaModel()
        {
        }

    }
}

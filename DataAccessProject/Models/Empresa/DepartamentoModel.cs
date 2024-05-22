using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models
{
    public class DepartamentoModel
    {
        public int? DeptId { get; set; }
        public string DeptName { get; set; }
        public int? SupDeptId { get; set; }
        public bool ConHijos { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Reports
{
    public class ChartDiasEmpleadoModel
    {
        public float Laborables { get; set; }
        public float Trabajados { get; set; }
        public float Ausencias { get; set; }
        public float Atrasos { get; set; }
        public float SalidasTemprano { get; set; }
        public float ExcesosLunch { get; set; }
        public float Permisos { get; set; }
        public ChartDiasEmpleadoModel(float laborables, float trabajados, float ausencias, float atrasos, float salidasTemplano,
            float excesosLunch, float permisos)
        {
            Laborables = laborables;
            Trabajados = trabajados;
            Ausencias = ausencias;
            Atrasos = atrasos;
            SalidasTemprano = salidasTemplano;
            ExcesosLunch = excesosLunch;
            Permisos = permisos;
        }
        public ChartDiasEmpleadoModel()
        {
        }
    }
}

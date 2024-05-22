using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using DataAccessLibrary.Models.Seguridad;
using System;

namespace DataAccessLibrary.DataAccessLayer.Seguridad
{
    public class ValidNumEmpleadosDAL
    {
        public IConfiguration Configuration;

        public ValidNumEmpleadosDAL(IConfiguration configuration)
        {
            Configuration = configuration; 
        }

        private string CUENTA_Empleados_Activos = @"SELECT Count(*) FROM USERINFO WHERE DefaultDeptId > 0;";
        private string LEE_Empleados_Licenciados = @"SELECT PARAVALUE FROM ProperParam WHERE PARANAME = 'NumUsers';";
        private string SELECT_MAXIMO_BADGENUMBER = "SELECT MAX(cast(badgenumber as BIGINT)) + 1 from USERINFO";

        public ValidNumEmpleadosModel GetValidacionNumEmpleados()
        {

            int conteoLicenciados;
            int ConteoEmpleadosActivos;
            long nextBadgenumber;

            DateTime Fecha;
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    string numLicenciados = db.QueryFirstOrDefault<string>(LEE_Empleados_Licenciados, null, commandType: CommandType.Text);
                    conteoLicenciados = int.Parse(UCommon.Cripto.Desencripta(numLicenciados));
                    ConteoEmpleadosActivos = db.QueryFirstOrDefault<int>(CUENTA_Empleados_Activos, null, commandType: CommandType.Text);
                    nextBadgenumber = db.QueryFirstOrDefault<long>(SELECT_MAXIMO_BADGENUMBER, null, commandType: CommandType.Text);                    

                    ValidNumEmpleadosModel validacion = new ValidNumEmpleadosModel(ConteoEmpleadosActivos, conteoLicenciados, nextBadgenumber);
                    return validacion;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}

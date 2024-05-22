using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.Seguridad;
using System;
using System.Text;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using System.IO;
using System.Globalization;

namespace DataAccessLibrary.DataAccessLayer.Seguridad
{
    public class ValidacionBasicaDAL
    {
        public IConfiguration Configuration;

        public ValidacionBasicaDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private string SELECT_FechaCaducidad = @"SELECT PARAVALUE as sFechaCoded FROM PROPERPARAM WHERE PARANAME = 'vfCoded'";
        private string CUENTA_Admin = @"SELECT Count(*) FROM USERINFO WHERE OTAdmin > 0;";
        private string CUENTA_Empleados_Activos = @"SELECT Count(*) FROM USERINFO WHERE DefaultDeptId > 0;";


        public async Task<SeguridadEmpleAdminFecha> GetValidacionSeguridades()
        {
            string sFecha;
            int conteoAdmin;
            int conteoEmpleados;

            DateTime Fecha;
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();
                    sFecha = db.QueryFirstOrDefault<string>(SELECT_FechaCaducidad, null, commandType: CommandType.Text);
                    conteoAdmin = db.QueryFirstOrDefault<int>(CUENTA_Admin, null, commandType: CommandType.Text);
                    conteoEmpleados = db.QueryFirstOrDefault<int>(CUENTA_Empleados_Activos, null, commandType: CommandType.Text);
                    string fechaDesencriptada = UCommon.Cripto.DesEncriptaFechaVigencia(sFecha);
                    Fecha = DateTime.ParseExact(fechaDesencriptada, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    SeguridadEmpleAdminFecha validacion = new SeguridadEmpleAdminFecha(conteoEmpleados, conteoAdmin, Fecha);
                    
                    return validacion;
                }
                catch (Exception ex)
                {
                    string docPath = Configuration.GetSection("LogErrores").Value;

                    using (StreamWriter outputFile = new StreamWriter(File.Open(Path.Combine(docPath, "Logs.txt"), FileMode.Append)))
                    {
                        await outputFile.WriteLineAsync(DateTime.Now.ToString());
                        await outputFile.WriteLineAsync($"Message: {ex.Message}");
                        await outputFile.WriteLineAsync($"Source: {ex.Source}");
                        await outputFile.WriteLineAsync(ex.StackTrace.ToString());
                        await outputFile.WriteLineAsync("\n");
                    }

                    return null;
                }
            }
        }

        public async Task LogInsertInicioSesion(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Inicio de Sesión";
            InsertLog insertLog = new InsertLog(log);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }
    }

    
}

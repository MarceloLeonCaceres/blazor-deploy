using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer
{
    public class EmpleadoAsignacionDAL
    {
        public IConfiguration Configuration;       

        public EmpleadoAsignacionDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public List<EmpleadoAsignacionModel> GetEmpleadosAsignacion(string csLstUsers, string variableAsignada)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql;
                try
                {
                    ConstAsignacionEmpleados tabla = ConstAsignacionEmpleados.ParametersAllocation(variableAsignada);
                    if (tabla == null)
                    {
                        return null;
                    }                        
                    sql = "SELECT userid, badgenumber as Badge, U.Name as NombreEmp, DefaultDeptId as DeptId, U." +
            tabla.colEnUser + " AS idParametroAsignado, Variable." + tabla.colDescripcionTabla + " AS nombreParametro \n" +
            "FROM userinfo U LEFT JOIN " + tabla.nombreTabla + " AS Variable ON U." + tabla.colEnUser + " = Variable." + tabla.colIdTabla + 
            " WHERE U.Userid in (" + csLstUsers + ");";
                    db.Open();
                    
                    IEnumerable<EmpleadoAsignacionModel> resultado = db.Query<EmpleadoAsignacionModel>(sql, null, commandType: CommandType.Text);
                    return resultado.ToList();
                }
                catch (System.Data.SyntaxErrorException error)
                {
                    throw error;
                }
                catch (System.Exception ex)
                {
                    // throw ex;
                    return null;
                }
            }
        }

        public async Task SaveEmpleadosAsignacionAsync(string variableAsignada, List<int> usersList, string idRegla)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    ConstAsignacionEmpleados tabla = ConstAsignacionEmpleados.ParametersAllocation(variableAsignada);
                    if (tabla == null)
                    {
                        return;
                    }
                    var parametros = new DynamicParameters();                    
                    parametros.Add("IdRegla", idRegla, DbType.String);
                    string sql = @"UPDATE Userinfo set " + tabla.colEnUser + @" = @IdRegla 
                                    WHERE UserId in (" + string.Join(", ", usersList) + ");";
                    
                    if (tabla.nombreTabla == "RegHoraExtra")
                    {
                        sql += "\n" + @"update U
			set U.OVERTIME = RHE.Overtime, u.RegisterOT = RHE.RegisterOT
			from USERINFO U inner join RegHoraExtra RHE ON U.RegHoraExtra = RHE.Id_RegHoraExtra";
                    }
                    else if (tabla.nombreTabla == "RegOtros")
                    {
                        sql += "\n" + @"UPDATE U
			SET U.holiday = RO.holiday
			FROM USERINFO U inner join RegOtros RO ON U.RegOtros = RO.Id_RegOtros";
                    }

                    db.Open();                    
                    await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<EnumModel> GetVariableAsignarAsync(string variableAsignada)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    ConstAsignacionEmpleados tabla = ConstAsignacionEmpleados.ParametersAllocation(variableAsignada);
                        if (tabla == null)
                        {
                            return null;
                        }
                        string sql = "SELECT " + tabla.colIdTabla + " as id, " + 
                        tabla.colDescripcionTabla + " as descripcion " +
                                    "FROM " + tabla.nombreTabla;
                    db.Open();

                    IEnumerable<EnumModel> resultado = db.Query<EnumModel>(sql, null, commandType: CommandType.Text);
                    return resultado.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

    }
}

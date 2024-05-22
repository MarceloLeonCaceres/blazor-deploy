using Dapper;
using DataAccessLibrary.Models;
using DataAccessLibrary.Models.Reports;

using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.ReportsDAL
{
    public class MarcacionManualDAL
    {
        public IConfiguration Configuration;

        public MarcacionManualDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }      

        public int CuentaMarcacionManual(int userId, DateTime checkTime)
        {
            var parametros = new DynamicParameters();
            parametros.Add("UserId", userId, DbType.Int32);
            parametros.Add("CheckTime", checkTime, DbType.DateTime);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = "SELECT COUNT(*) FROM CHECKEXACT WHERE Userid = @UserId AND CHECKTIME = @CheckTime;";
                db.Open();
                try
                {
                    int result = db.ExecuteScalar<int>(sql, parametros, commandType: CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public int CuentaMarcacionesManualesRepetidas(string csvUsersId, DateTime checkTime)
        {
            var parametros = new DynamicParameters();
            parametros.Add("CheckTime", checkTime, DbType.DateTime);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = "SELECT COUNT(*) FROM CHECKEXACT WHERE Userid in ( " + csvUsersId + @" ) AND CHECKTIME = @CheckTime;";
                db.Open();
                try
                {
                    int result = db.ExecuteScalar<int>(sql, parametros, commandType: CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public int InsertaMarcacionManual(MarcacionManualModel mmModel)
        {
            var parametros = new DynamicParameters();
            parametros.Add("UserId", mmModel.UserId, DbType.Int32);
            parametros.Add("CheckTime", mmModel.Checktime, DbType.DateTime);
            parametros.Add("Tipo", mmModel.CheckType, DbType.String);
            parametros.Add("Yuyin", mmModel.Yuyin, DbType.String);
            parametros.Add("ModifyBy", mmModel.ModifiedBy, DbType.String);
            parametros.Add("Fecha", DateTime.Now, DbType.DateTime);

            string sqlExact = @"INSERT INTO [dbo].[CheckEXACT]
           ([USERID], [CHECKTIME], [CHECKTYPE], [ISADD]
           ,[YUYIN]
           ,[ISMODIFY],[ISDELETE],[INCOUNT],[ISCOUNT]
           ,[MODIFYBY],[DATE])
     VALUES
           (@UserId, @CheckTime, @Tipo, 1
           ,@Yuyin
           ,0 ,0 ,0 ,0
           ,@ModifyBy, @Fecha );";
            string sqlInOut = @"INSERT INTO [dbo].[CheckInOut]
           ([USERID], [CHECKTIME], [CHECKTYPE], VerifyCode)
     VALUES
           (@UserId, @CheckTime, @Tipo, 7)";
           

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
           
                db.Open();
                try
                {
                    db.Execute(sqlExact, parametros, commandType: CommandType.Text);
                    int result = db.Execute(sqlInOut, parametros, commandType: CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }


        public int InsertaMarcacionesMasivas(List<MarcacionManualModel> lstMarcaciones)
        {
            int contador = 0;
            foreach(MarcacionManualModel marcacion in lstMarcaciones)
            {
                contador += InsertaMarcacionManual(marcacion);
            }
            return contador;
        }

    }
}

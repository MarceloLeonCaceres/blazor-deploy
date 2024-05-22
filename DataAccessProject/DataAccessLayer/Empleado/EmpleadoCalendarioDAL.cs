using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DataAccessLibrary.DataAccessLayer
{
    public class EmpleadoCalendarioDAL
    {
        public IConfiguration Configuration;

        public EmpleadoCalendarioDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<EmpleadoCalendarioModel>> GetCalendarioAsync(string userid, DateTime fDesde, DateTime fHasta)
        {                       

            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("SDesde", fDesde.ToString("dd/MM/yyyy"), DbType.String);
            parametros.Add("SHasta", fHasta.ToString("dd/MM/yyyy"), DbType.String);
            parametros.Add("UserId", userid, DbType.String);

            List<string> lista = new List<string>();

            string sql1 = @"SET DATEFORMAT DMY;
                SET DATEFIRST 1;
                Create table #BorrarTablaG0 (USERID INT NOT NULL , Empleado VARCHAR(60) NULL , Fecha date NOT NULL, diaSemana VARCHAR(10) NULL , fila INT Null);";

            string sql2 = @"DECLARE @dia as date, @fila as integer 
                SET @dia = @Sdesde
                set @fila = 1 
                WHILE @dia <= @Shasta 
                BEGIN INSERT INTO #BorrarTablaG0 
	                SELECT userid, [name], @dia, DATENAME(dw, @dia), @fila 
	                FROM userinfo 
	                WHERE Userid = @UserId
	                SET @dia = dateadd(d,1,@dia) 
	                SET @fila = @fila +1 
                END;";

            string sql3 = @"SELECT G0.*, (case NUM_RUN.UNITS when 1 then datepart(dw, G0.fecha) when 2 then day(g0.fecha) -1 else (datediff(day, USER_OF_RUN.STARTDATE, G0.fecha) ) % NUM_RUN.CYCLE end)  as SDia, USER_OF_RUN.NUM_OF_RUN_ID, NUM_RUN.NAME, USER_OF_RUN.STARTDATE, USER_OF_RUN.ENDDATE, NUM_RUN.CYCLE AS ciclosTurno, NUM_RUN.UNITS AS medidaTurno,  case NUM_RUN.UNITS when '1' then '1' when '0' then 'D' when '2' then 'M' else null end as Dia 
                INTO #BorrarTablaG1 
                FROM #BorrarTablaG0 as G0 LEFT JOIN USER_OF_RUN ON  G0.USERID = USER_OF_RUN.USERID and G0.Fecha between USER_OF_RUN.STARTDATE and USER_OF_RUN.ENDDATE 
                                          LEFT JOIN NUM_RUN ON USER_OF_RUN.NUM_OF_RUN_ID = NUM_RUN.NUM_RUNID;";

            string sql4 = @"SELECT G1.*, SchClass.SchClassId, SchClass.SchName, cast(SchClass.StartTime as time) as Inicio, cast(SchClass.EndTime as time) as Fin, '                    ' as Observacion
                INTO #BorrarTablaG2 
                FROM #BorrarTablaG1 AS G1 LEFT JOIN NUM_RUN_DEIL ON (G1.NUM_OF_RUN_ID = NUM_RUN_DEIL.NUM_RUNID) AND (G1.SDia = NUM_RUN_DEIL.SDAYS) 
						                  LEFT JOIN SchClass ON NUM_RUN_DEIL.SCHCLASSID = SchClass.schClassid;";
                // -- ALTER TABLE #BorrarTablaG2 Add Observacion varchar(20) NULL;";

            // Quita feriados
            string sql5 = @"UPDATE G2 
                SET G2.SCHClassID = NULL, Observacion = 'Feriado'
                FROM #BorrarTablaG2 as G2  INNER JOIN HOLIDAYS  ON G2.Fecha = HOLIDAYS.STARTTIME INNER JOIN  USERINFO ON G2.USERID = USERINFO.USERID 
                WHERE userinfo.holiday=1;";
            // Pone Horarios Ocasionales
            // Elimina el día siguiente en horarios de 24 horas
            string sql6 = @"DELETE FROM G2 
	            FROM #BorrarTablaG2 as G2 inner join USER_TEMP_SCH on G2.userid = USER_TEMP_SCH.USERID and Cast( dateadd(d,1,USER_TEMP_SCH.cometime) as DATE)=G2.fecha inner join SchClass ON USER_TEMP_SCH.schClassId = SchClass.schClassid 
	            WHERE SchClass.Tipo >= 24;";

            // Elimina el día propio en cualquier horario
            sql6 += "DELETE FROM G2 FROM #BorrarTablaG2 AS G2 inner join USER_TEMP_SCH on G2.userid = USER_TEMP_SCH.USERID and CAST(USER_TEMP_SCH.cometime as DATE)=G2.fecha;";

            // Inserta horarios ocasionales
            string sql7 = @"INSERT INTO #BorrarTablaG2 
                (Userid, Empleado, Fecha, diaSemana, fila, Name, SCHCLASSID, schName, Inicio, Fin, Observacion) 
                SELECT tg0.*, '--', USER_TEMP_SCH.SCHCLASSID, SchClass.schName, SchClass.StartTime, SchClass.EndTime, 'Ocasional' 
                FROM #BorrarTablaG0 AS tG0 INNER JOIN USER_TEMP_SCH on tG0.userid = USER_TEMP_SCH.USERID and CAST(USER_TEMP_SCH.cometime as DATE)=tG0.fecha 
				     inner join SchClass ON USER_TEMP_SCH.schClassId = SchClass.schClassid;";

            // Retorna el calendario del empleado
            string sql8 = @"SELECT Fecha, diaSemana, fila, SDia, NUM_OF_RUN_ID as IdTurno, [NAME] as NomTurno, SchClassId as IdHorario, SchName as NomHorario, Observacion
                FROM #BorrarTablaG2 Order by Fecha, Inicio;";

            lista.Add(sql1);
            lista.Add(sql2);
            lista.Add(sql3);
            lista.Add(sql4);
            lista.Add(sql5);
            lista.Add(sql6);
            lista.Add(sql7);
            lista.Add(sql8);

            string sqlTotal = string.Join("\n", lista);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    
                    IEnumerable<EmpleadoCalendarioModel> calendarios = await db.QueryAsync<EmpleadoCalendarioModel>(sqlTotal, parametros, commandType: CommandType.Text);
                    return calendarios.ToList();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

    }
}

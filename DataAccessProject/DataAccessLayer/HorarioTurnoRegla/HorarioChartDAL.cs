using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using DataAccessLibrary.Models.HorarioTurnoRegla;

namespace DataAccessLibrary.DataAccessLayer.HorarioTurnoRegla
{
    public class HorarioChartDAL
    {
        public IConfiguration Configuration;

        public HorarioChartDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public const string sqlTurno = @"With Horarios As (
            SELECT case T.UNITS when 1 then SDAYS when 0 then sdays + 1 else sdays + 1 end as sdays, 
            cast(H.STARTTIME as time) as Inicio, cast(H.ENDTIME as time) as Fin, H.schclassID, case T.UNITS when 1 then T.CYCLE * 7 when 0 then T.CYCLE * 1 else 31 end as filas 
            FROM num_run_deil INNER JOIN NUM_RUN T on NUM_RUN_DEIL.NUM_RUNID = T.NUM_RUNID 
                 INNER JOIN SCHCLASS H ON NUM_RUN_DEIL.SCHCLASSID = H.SCHCLASSID
            WHERE num_run_deil.NUM_RUNID=@codTurno
            UNION 
            SELECT case when num_run_deil.SDAYS = case T.UNITS when 1 then T.CYCLE * 7 when 0 then CYCLE - 1 else 31 end then 1 else (SDAYS + 1) % (case T.UNITS when 1 then T.CYCLE * 7 when 0 then T.CYCLE * 1 else 31 end) + (case T.UNITS when 1 then 0 when 0 then 1 else 0 end) end,  
            '00:00' AS Inicio, cast(H.ENDTIME as time) AS Fin , H.schclassID, case T.UNITS when 1 then T.CYCLE * 7 when 0 then T.CYCLE * 1 else 31 end as filas 
            FROM num_run_deil INNER JOIN NUM_RUN T on NUM_RUN_DEIL.NUM_RUNID = T.NUM_RUNID 
                 INNER JOIN SCHCLASS H ON NUM_RUN_DEIL.SCHCLASSID = H.SCHCLASSID
            WHERE num_run_deil.NUM_RUNID=@codTurno AND H.STARTTIME > H.ENDTIME         
            )
            Select SDAYS as sDia, Inicio, Fin, SCHCLASSID as idHorario, ROW_NUMBER() over(Partition by Sdays order by sdays, Inicio) as OrdenHorario
            From Horarios
            Order By SDAYS desc, Inicio;";

        public async Task<List<HorarioChartModel>> RetornaHorariosTurno(int codTurno)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("CodTurno", codTurno, DbType.Int16);

                db.Open();
                IEnumerable<HorarioChartModel> result = await db.QueryAsync<HorarioChartModel>(sqlTurno, parametros, commandType: CommandType.Text);
                return result.ToList();
            }
        }

        public async Task<List<HorarioChartModel>> RetornaHorariosEmpDesdeHasta(int idUsuario, DateTime fDesde, DateTime fHasta)
        {
            string sqlEmpleadoRango1 = @"IF OBJECT_ID('#BorrarTablaG0" + "', 'U') IS NOT NULL \n Drop Table #BorrarTablaG0" + ";" + "\n" + "IF OBJECT_ID('#BorrarTablaG1', 'U') IS NOT NULL \n Drop Table #BorrarTablaG1;" + "\n" + "IF OBJECT_ID('#BorrarTablaG2', 'U') IS NOT NULL \n Drop Table #BorrarTablaG2" + ";";
            string sqlEmpleadoRango2 = @"Create table #BorrarTablaG0" + " " + "(USERID INT NOT NULL , " + "Empleado VARCHAR(60) NULL , " + "Fecha date NOT NULL, " + "diaSemana VARCHAR(10) NULL , " + "fila INT Null)";
            string sqlEmpleadoRango3 = @"SET DATEFORMAT DMY;" + "\n" + "SET DATEFIRST 1;" + "\n" + "DECLARE @dia as date, @fila as integer;\n" + "SET @dia = '" + fDesde.ToShortDateString() + "';\n" + "set @fila = 1;" + "WHILE @dia <= '" + fHasta.ToShortDateString() + "' " + "BEGIN " + "INSERT INTO #BorrarTablaG0" + " " + "SELECT userid, name, @dia, DATENAME(dw, @dia), @fila " + "FROM userinfo " + "WHERE Userid=" + idUsuario.ToString() + " " + "SET @dia = dateadd(d,1,@dia) SET @fila = @fila +1 " + "END";
            string sqlEmpleadoRango4 = @"SET DATEFORMAT DMY;" + "\n" + "SET DATEFIRST 1;" + "\n" + "SELECT G0.*, " + "(case NUM_RUN.UNITS " + "when 1 then datepart(dw, G0.fecha) " + "when 2 then day(g0.fecha) -1 " + "else (datediff(day, USER_OF_RUN.STARTDATE, G0.fecha) ) % NUM_RUN.CYCLE end)  as SDia, " + "USER_OF_RUN.NUM_OF_RUN_ID, NUM_RUN.NAME, USER_OF_RUN.STARTDATE, USER_OF_RUN.ENDDATE, " + "NUM_RUN.CYCLE AS ciclosTurno, NUM_RUN.UNITS AS medidaTurno,  " + "case NUM_RUN.UNITS when '1' then '1' when '0' then 'D' when '2' then 'M' else null end as Dia " + "INTO #BorrarTablaG1" + " " + "FROM #BorrarTablaG0" + " as G0 LEFT JOIN USER_OF_RUN ON  " + "G0.USERID = USER_OF_RUN.USERID and G0.Fecha between USER_OF_RUN.STARTDATE and USER_OF_RUN.ENDDATE " + "INNER JOIN " + "NUM_RUN ON USER_OF_RUN.NUM_OF_RUN_ID = NUM_RUN.NUM_RUNID " + "WHERE (USER_OF_RUN.USERID = " + idUsuario.ToString() + ");";
            string sqlEmpleadoRango5 = @"SET DATEFORMAT DMY;" + "\n" + "SET DATEFIRST 1;" + "\n" + "SELECT G1.*, " + "SchClass.SchClassId, SchClass.SchName, cast(SchClass.StartTime as time) as Inicio, cast(SchClass.EndTime as time) as Fin " + "\n" + "INTO #BorrarTablaG2" + " " + "\n" + "FROM #BorrarTablaG1" + " AS G1 LEFT JOIN NUM_RUN_DEIL ON " + "(G1.NUM_OF_RUN_ID = NUM_RUN_DEIL.NUM_RUNID) AND (G1.SDia = NUM_RUN_DEIL.SDAYS) LEFT JOIN SchClass ON " + "NUM_RUN_DEIL.SCHCLASSID = SchClass.schClassid";

            string poneFeriados = @"SET DATEFORMAT DMY;" + "\n" + "SET DATEFIRST 1;" + "\n" + "UPDATE G2 " + "SET G2.SCHClassID = NULL " + "FROM " + "#BorrarTablaG2" + " as G2  INNER JOIN HOLIDAYS  " + "ON G2.Fecha = HOLIDAYS.STARTTIME INNER JOIN  " + "USERINFO ON G2.USERID = USERINFO.USERID " + "where userinfo.holiday=1;";
            string poneTurnosTemporales = @"SET DATEFORMAT DMY;" + "\n" + "SET DATEFIRST 1;" + "\n" + "DELETE FROM G2 " + "FROM #BorrarTablaG2" + " as G2 inner join USER_TEMP_SCH on G2.userid = USER_TEMP_SCH.USERID " + "and Cast( dateadd(d,1,USER_TEMP_SCH.cometime) as DATE)=G2.fecha inner join SchClass ON " + "USER_TEMP_SCH.schClassId = SchClass.schClassid " + "WHERE SchClass.Tipo >= 24;";
            string eliminaDiaPropioEnCualquierHorario = @"SET DATEFORMAT DMY;" + "\n" + "SET DATEFIRST 1;" + "\n" + "DELETE FROM G2 " + "FROM #BorrarTablaG2" + " AS G2 inner join USER_TEMP_SCH on G2.userid = USER_TEMP_SCH.USERID " + "and CAST(USER_TEMP_SCH.cometime as DATE)=G2.fecha;";
            string insertaHorariosOcasionales = @"SET DATEFORMAT DMY;" + "\n" + "SET DATEFIRST 1;" + "\n" + "INSERT INTO #BorrarTablaG2" + " " + "\n" + "(Userid, Empleado, Fecha, diaSemana, fila, Name, SCHCLASSID, schName, Inicio, Fin) " + "\n" + "SELECT tg0.*, 'HO', USER_TEMP_SCH.SCHCLASSID, SchClass.schName, SchClass.StartTime, SchClass.EndTime " + "\n" + "FROM #BorrarTablaG0" + " AS tG0 INNER JOIN USER_TEMP_SCH on tG0.userid = USER_TEMP_SCH.USERID and " + "CAST(USER_TEMP_SCH.cometime as DATE)=tG0.fecha inner join SchClass ON " + "USER_TEMP_SCH.schClassId = SchClass.schClassid;";

            string selectHorariosEmp = @"SET DATEFORMAT DMY;
                SET DATEFIRST 1;
                With Horarios as ( 
                SELECT UserId, Empleado, Fecha, diaSemana, fila as SDia, Num_of_run_id, Name, SchClassId, schName, Inicio, Fin 
                FROM #BorrarTablaG2 WHERE not SchClassId is null UNION ( 
                SELECT UserId, Empleado, DateAdd(d, 1, Fecha), DATENAME(dw, DateAdd(d, 1, Fecha)), fila+1, Num_of_run_id, Name, SchClassId, schName, '00:00', Fin 
                FROM #BorrarTablaG2 WHERE Inicio > Fin AND fecha < '" + fHasta.ToString("dd/MM/yyyy") + @"' ) UNION ( 
                SELECT UserId, Empleado, DateAdd(d, 1, Fecha), DATENAME(dw, DateAdd(d, 1, Fecha)), fila+1, Num_of_run_id, Name, SchClass.SchClassId, SchClass.schName, '00:00', Inicio 
                FROM #BorrarTablaG2 INNER JOIN schclass  on #BorrarTablaG2.schclassid = SchClass.schClassid WHERE SchClass.Tipo >=24 AND fecha < '" + fHasta.ToString("dd/MM/yyyy") + @"' ) 
                ) 
                Select *, ROW_NUMBER() over(Partition by fecha order by fecha, Inicio) as OrdenHorario
                From Horarios 
                ORDER BY fecha Desc, Inicio";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("IdUsuario", idUsuario, DbType.Int16);
                parametros.Add("FDesde", fDesde, DbType.DateTime);
                parametros.Add("FHasta", fHasta, DbType.DateTime);

                db.Open();
                db.Execute(sqlEmpleadoRango1, null, commandType: CommandType.Text);
                db.Execute(sqlEmpleadoRango2, null, commandType: CommandType.Text);
                db.Execute(sqlEmpleadoRango3, parametros, commandType: CommandType.Text);
                db.Execute(sqlEmpleadoRango4, null, commandType: CommandType.Text);
                db.Execute(sqlEmpleadoRango5, null, commandType: CommandType.Text);
                db.Execute(poneFeriados, null, commandType: CommandType.Text);
                db.Execute(poneTurnosTemporales, null, commandType: CommandType.Text);
                db.Execute(eliminaDiaPropioEnCualquierHorario, null, commandType: CommandType.Text);
                db.Execute(insertaHorariosOcasionales, null, commandType: CommandType.Text);
                IEnumerable<HorarioChartModel> result = await db.QueryAsync<HorarioChartModel>(selectHorariosEmp, null, commandType: CommandType.Text);
                return result.ToList();
            }
        }
    }
}

using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.Vacaciones;
using System;
using System.Text;
using DataAccessLibrary.Models.Reports;

namespace DataAccessLibrary.DataAccessLayer.Vacaciones
{
    public class FilaCalculoVacacionDAL
    {
        public IConfiguration Configuration;        

        public FilaCalculoVacacionDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public const string SELECT_ULTIMOCORTE = @"WITH UltimoCorte AS
(
SELECT C.UserId, max(FechaCorte) as Fecha, u.idContrato
FROM [Vacations.Cortes] C inner join userinfo U on c.UserId = U.USERID
where C.UserId = @UserId
GROUP BY C.UserId, U.idContrato 
)
";

        public const string SELECT_CORTE = SELECT_ULTIMOCORTE + @"SELECT C.UserId, C.FechaCorte AS Fecha, DiasAgregados as VacGanadas, 'Corte de Vacaciones' AS NombrePermiso
FROM [Vacations.Cortes] C INNER JOIN UltimoCorte UC ON C.USERID = UC.Userid AND C.FechaCorte = UC.Fecha";
        
        public const string SELECT_PERMISOS = SELECT_ULTIMOCORTE + @"SELECT P.USERID, P.STARTSPECDAY AS InicioP, P.ENDSPECDAY AS FinP, P.DATEID, P.YUANYING AS Motivo, 
    CAST(P.STARTSPECDAY AS DATE) AS Fecha, DATENAME(WEEKDAY, p.STARTSPECDAY) as Dia, TP.LeaveName as NombrePermiso, TP.Classify as TipoVacacion
FROM (USER_SPEDAY P inner join UltimoCorte UC ON P.USERID = UC.Userid ) INNER JOIN LeaveClass TP ON TP.LeaveId = P.DATEID
WHERE STARTSPECDAY > UC.Fecha AND tp.Classify in (1, 2) AND ENDSPECDAY < @fHasta
ORDER BY STARTSPECDAY;";

        public const string SELECT_DESCUENTOS = SELECT_ULTIMOCORTE + @"SELECT P.USERID, P.STARTSPECDAY AS InicioP, P.ENDSPECDAY AS FinP, P.DATEID, P.YUANYING AS Motivo, 
    CAST(P.STARTSPECDAY AS DATE) AS Fecha, DATENAME(WEEKDAY, p.STARTSPECDAY) as Dia, 'Descuento ' + P.YUANYING as NombrePermiso, 2 as TipoVacacion
FROM (AtrasoSalidaTemp P inner join UltimoCorte UC ON P.USERID = UC.Userid inner join [Vacations.Parameters] VP on UC.idContrato = vp.idContrato )
WHERE STARTSPECDAY > UC.Fecha  AND ENDSPECDAY < @fHasta AND ( 
(DATEID = '1114' and vp.bAusenciasCV = 'true') or (DATEID = '1112' and vp.bSalidasCV = 'true') or (DATEID = '1111' and vp.bAtrasosCV = 'true') 
)
ORDER BY STARTSPECDAY;";

        public const string SELECT_DATOS_CONTRATO = @"SELECT userid, Badgenumber as NumCA, [name] as NomEmpleado, 
                    USERINFO.idContrato, nomContrato, cast(HIREDDAY as date) as FechaEmpleo
FROM USERINFO left join CONTRATO on USERINFO.idContrato = CONTRATO.idContrato
WHERE userid = @UserId";

        public string Select_Datos_Empleado(string csLstUsers)
        {
            return @"SELECT userid, Badgenumber as NumCA, [name] as NomEmpleado, USERINFO.idContrato, nomContrato, 
                    cast(HIREDDAY as date) as FechaEmpleo
FROM USERINFO inner join CONTRATO on USERINFO.idContrato = CONTRATO.idContrato
WHERE userid in ( " + csLstUsers  + " ) AND Not HIREDDAY Is Null";
        }


        public List<RpVacacionesEmpleadoModel> GetObjCalculoVacacionesEmpleado(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {

            List<RpVacacionesEmpleadoModel> LstEmpleadosVacaciones = new List<RpVacacionesEmpleadoModel>();
            List<EmpleadoContratoVacacionModel> LstEmpleados;
            string sqlDatosEmpleados = Select_Datos_Empleado(csLstUsers);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {                
                db.Open();
                try
                {
                    IEnumerable<EmpleadoContratoVacacionModel> empleados =  db.Query<RpVacacionesEmpleadoModel>(sqlDatosEmpleados, null, commandType: CommandType.Text);
                    if (empleados.Count() == 0)
                    {
                        return null;
                    }
                    LstEmpleados = empleados.ToList();
                    foreach (RpVacacionesEmpleadoModel empleado in LstEmpleados)
                    {
                        DynamicParameters parametros = new DynamicParameters();
                        parametros.Add("UserId", empleado.UserId, DbType.String);
                        parametros.Add("fDesde", fDesde, DbType.DateTime);
                        parametros.Add("fHasta", fHasta, DbType.DateTime);

                        IEnumerable<FilaCalculoVacacionModel> corte = db.Query<FilaCalculoVacacionModel>(SELECT_CORTE, parametros, commandType: CommandType.Text);
                        if (corte.Count() == 0)
                        {
                            continue;
                        }
                        RpVacacionesEmpleadoModel vacaciones1Empleado = new RpVacacionesEmpleadoModel(empleado);

                        vacaciones1Empleado.FilasV = corte.ToList();

                        IEnumerable<FilaCalculoVacacionModel> filasVacacion = db.Query<FilaCalculoVacacionModel>(SELECT_PERMISOS, parametros, commandType: CommandType.Text);
                        vacaciones1Empleado.FilasV.AddRange(filasVacacion);                        

                        IEnumerable<FilaCalculoVacacionModel> filasDescuento = db.Query<FilaCalculoVacacionModel>(SELECT_DESCUENTOS, parametros, commandType: CommandType.Text);
                        vacaciones1Empleado.FilasV.AddRange(filasDescuento);

                        LstEmpleadosVacaciones.Add(vacaciones1Empleado);
                    }

                    return LstEmpleadosVacaciones;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<DateTime> GetFechaUltimoCorte(int userid)
        {
            DateTime fechaUltimoCorte;
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("UserId", userid, DbType.Int32);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                fechaUltimoCorte = (DateTime) await db.ExecuteScalarAsync("select Max(FechaCorte) from [Vacations.Cortes] where userid = @UserId", parametros, commandType: CommandType.Text);
            }
            return fechaUltimoCorte;
        }

    }
}

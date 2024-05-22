using System;

namespace DataAccessLibrary.QueriesStrategy
{
    internal class AmbosDispositivos : ITipoDispositivos
    {
        public string SELECT_MARCACIONES(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            string sql = @"WITH UnionChecks AS (
SELECT U.UserId, U.Badgenumber as NumCA, U.[Name] as Empleado, U.DEFAULTDEPTID, CHECKTIME as FechaHora,  
		(case M.CHECKTYPE when 'I' then 'Automático' when 'O' then 'Salida' else 'Otro' end ) as Estado, R.sn, R.MachineAlias as Reloj, 
		U.SSN as Cedula, NULL AS Verificacion
FROM ((MACHINES R INNER JOIN CHECKINOUT M ON R.sn = M.SN )
		INNER JOIN USERINFO U ON M.USERID = U.USERID)
WHERE U.Userid in (" + csLstUsers + @")
AND CheckTime >= '" + fDesde.ToString("dd/MM/yyyy") + "' AND CheckTime < '" + fHasta.ToString("dd/MM/yyyy") + @"'
UNION
SELECT U.UserId, U.Badgenumber as NumCA, U.[Name] as Empleado, U.DEFAULTDEPTID, CHECKTIME as FechaHora,  
		(case M.CHECKTYPE when 'I' then 'Automático' when 'O' then 'Salida' else 'Otro' end ) as Estado, null, NULL, 
		U.SSN as Cedula, 'Marcación Manual' as Verificacion
FROM (CHECKINOUT M INNER JOIN USERINFO U ON M.USERID = U.USERID)
WHERE U.Userid in (" + csLstUsers + @")
AND CheckTime >= '" + fDesde.ToString("dd/MM/yyyy") + "' AND CheckTime < '" + fHasta.ToString("dd/MM/yyyy") + @"'
AND M.VERIFYCODE = 7
UNION
SELECT U.UserId, U.Badgenumber as NumCA, U.[Name] as Empleado, U.DEFAULTDEPTID, AttTime, 
		(case M_Push.Status when 255 then 'Automático' when 0 then 'Salida' else 'Otro' end ) as Estado, R_Push.DevSN, R_Push.DevName as Reloj, 
		U.SSN as Cedula, NULL as Verificacion
FROM (([Push_BioSmart].[dbo].[Device] R_Push INNER JOIN CheckInOut_P M_Push ON R_Push.DevSN = M_Push.DeviceID )
		INNER JOIN USERINFO U ON M_Push.PIN = U.Badgenumber)
WHERE U.Userid in (" + csLstUsers + @")
AND M_Push.AttTime >= '" + fDesde.ToString("dd/MM/yyyy") + "' AND M_Push.AttTime < '" + fHasta.ToString("dd/MM/yyyy") + @"'
)
SELECT Userid, NumCA, Empleado, D.DEPTNAME as Departamento, sn, Reloj, Estado, Cedula, 
case DATEPART(DW, FechaHora) when 1 then 'Lunes' when 2 then 'Martes' when 3 then 'Miércoles' when 4 then 'Jueves' when 5 then 'Viernes' when 6 then 'Sábado' when 7 then 'domingo' end as Dia, 
CONVERT(date,FechaHora) as Fecha, DATETIMEFROMPARTS(2000,1,1, datepart(hour, FechaHora), datepart(minute,FechaHora), datepart(second, FechaHora), 0) as Hora, Verificacion
FROM UnionChecks INNER JOIN DEPARTMENTS D ON UnionChecks.DEFAULTDEPTID = D.DEPTID
ORDER BY Empleado, NumCA, FechaHora;";
            return sql;
        }
        public string SELECT_MARCACIONES_MRL(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            string sql = @"WITH UnionChecks AS (
    SELECT U.SSN as Cedula, CHECKTIME as FechaHora,  
    (case M.CHECKTYPE when 'I' then '1' when 'O' then '1' else M.CHECKTYPE end ) as Estado
FROM ((MACHINES R INNER JOIN CHECKINOUT M ON R.sn = M.SN )
		INNER JOIN USERINFO U ON M.USERID = U.USERID)
WHERE U.Userid in (" + csLstUsers + @")
AND CheckTime >= '" + fDesde.ToString("dd/MM/yyyy") + "' AND CheckTime < '" + fHasta.ToString("dd/MM/yyyy") + @"'
UNION
SELECT U.SSN as Cedula, AttTime, 
		M_Push.Status  as Estado
FROM (([Push_BioSmart].[dbo].[Device] R_Push INNER JOIN CheckInOut_P M_Push ON R_Push.DevSN = M_Push.DeviceID )
		INNER JOIN USERINFO U ON M_Push.PIN = U.Badgenumber)
WHERE U.Userid in (" + csLstUsers + @")
AND M_Push.AttTime >= '" + fDesde.ToString("dd/MM/yyyy") + "' AND M_Push.AttTime < '" + fHasta.ToString("dd/MM/yyyy") + @"'
)
SELECT  Cedula, Estado, convert(varchar(19), FechaHora, 120) as FechaHora
FROM UnionChecks 
ORDER BY FechaHora;";
            return sql;
        }
    }
}

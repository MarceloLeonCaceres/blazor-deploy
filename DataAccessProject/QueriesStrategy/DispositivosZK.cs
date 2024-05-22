using System;

namespace DataAccessLibrary.QueriesStrategy
{
    internal class DispositivosZK : ITipoDispositivos
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

            string sql = @"SELECT  U.SSN as Cedula, 
(case M.CHECKTYPE when 'I' then '1' when 'O' then '1' else M.CHECKTYPE end ) as Estado, 
convert(varchar(19), CHECKTIME, 120) as FechaHora
FROM ((MACHINES R INNER JOIN CHECKINOUT M ON R.sn = M.SN )
		INNER JOIN USERINFO U ON M.USERID = U.USERID)
WHERE U.Userid in (" + csLstUsers + @")
AND CheckTime >= '" + fDesde.ToString("dd/MM/yyyy") + "' AND CheckTime < '" + fHasta.ToString("dd/MM/yyyy") + @"'
ORDER BY FechaHora;";
            return sql;
        }
    }
}

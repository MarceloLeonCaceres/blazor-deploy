using System;

namespace DataAccessLibrary.QueriesStrategy
{
    internal interface ITipoDispositivos
    {
        string SELECT_MARCACIONES(string csLstUsers, DateTime fDesde, DateTime fHasta);
        string SELECT_MARCACIONES_MRL(string csLstUsers, DateTime fDesde, DateTime fHasta);


        //public const string SELECT_MARCACIONES = @"SET dateformat dmy;
        //    SELECT U.Badgenumber as NumCA, U.Name as Empleado, DEPARTMENTS.DEPTNAME as Departamento, " +
        //    "case DATEPART(DW, M.checktime) when 1 then 'Lunes' when 2 then 'Martes' when 3 then 'Miércoles' when 4 then 'Jueves' when 5 then 'Viernes' when 6 then 'Sábado' when 7 then 'domingo' end as Dia, " + "\n" +
        //    "CONVERT(date,checktime) as Fecha, " +
        //    "DATETIMEFROMPARTS(2000,1,1, datepart(hour, checkTime), datepart(minute,checktime), datepart(second, checktime), 0) as Hora, " +
        //    "(case M.CHECKTYPE when 'I' then 'Automático' when 'O' then 'Salida' else 'Inválida' end ) as Estado, " +
        //    "ISNULL(VerifyType.Type, 'Otro') AS Verificacion, " +
        //    "M.SENSORID as RelojID, MACHINES.MachineAlias as Reloj, " + "\n" +
        //    "U.SSN as Cedula, U.TiTle as Cargo, nomCiudad as Ciudad, CENTRO_COSTOS.nomCentroCostos as CentroCostos, U.UserId  " + "\n" +
        //    @"FROM ((DEPARTMENTS INNER JOIN ((Checkinout AS M LEFT JOIN MACHINES ON M.SensorID = Machines.MachineNumber) 
        //                                    INNER JOIN USERINFO AS U ON M.USERid = U.USERID)
        //                                    ON DEPARTMENTS.DEPTID = U.DEFAULTDEPTID )
        //                                    LEFT JOIN CENTRO_COSTOS on U.idCentroCostos = CENTRO_COSTOS.idCentroCostos
        //                                    LEFT JOIN CIUDAD ON U.idCiudad = CIUDAD.idCiudad )
        //                                    LEFT JOIN VerifyType oN M.VERIFYCODE = VerifyType.id " + "\n";

    }
}

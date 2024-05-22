using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Text;

using DataAccessLibrary.DataAccessLayer.Empresa;
using DataAccessLibrary.Models.Empresa;

namespace DataAccessLibrary.DataAccessLayer.Calculos
{
    public class CalculoAsistenciaDAL
    {
        public static IConfiguration Configuration;


        public CalculoAsistenciaDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static TimeSpan vHor_Nor, vJor_Noc, vHE50, vHE100;
        public static TimeSpan vTTrabajado, vDescanso;
        public static TimeSpan turno_ant_aux, turno_act_aux;

        public int Temprano, Tarde;

        public static TimeSpan OchoHoras = new TimeSpan(8, 0, 0);
        public static readonly TimeSpan SeisAM = new TimeSpan(6, 0, 0);
        public static readonly TimeSpan SietePM = new TimeSpan(19, 0, 0);
        public static readonly TimeSpan OchoAM = new TimeSpan(8, 0, 0);
        public static readonly TimeSpan CuatroAM = new TimeSpan(4, 0, 0);

        private static string consultaXL = "";

        private static RedondeoModel redondeoModel;
        private static int iCharsHora = 5;

        public async Task CalculaAsistencia(DateTime fDesde, DateTime fHasta, string csvUsersSelected)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("FDesde", fDesde, DbType.DateTime);
            parametros.Add("FHasta", fHasta, DbType.DateTime);
            
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                foreach (List<string> proceso in ProcesosIniciales(fDesde, fHasta, csvUsersSelected))
                {
                    foreach (string query in proceso)
                    {
                        try
                        {
                            if (query.Contains("INSERT INTO [Tabla0]") )
                            {
                                await db.ExecuteAsync(query, parametros, commandType: CommandType.Text);
                            }
                            else
                            {
                                await db.ExecuteAsync(query, parametros, commandType: CommandType.Text);
                            }                            
                        }
                        catch (Exception ex)
                        {   
                            throw ex;
                        }
                    }
                }
                db.Close();
                DistribuyeHoras();
                db.Open();
                foreach (List<string> proceso in ProcesosFinales(fDesde, fHasta, csvUsersSelected))
                {
                    foreach (string query in proceso)
                    {
                        try
                        {
                            if (query.Contains("USDTotal = case when USDTotal <> 0 ") )
                            {
                                await db.ExecuteAsync(query, parametros, commandType: CommandType.Text);
                            }
                            else
                            {
                                await db.ExecuteAsync(query, parametros, commandType: CommandType.Text);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
        }

        public string RelojesValidos()
        {
            string sql1 = "Select id, Serial as descripcion From Dispositivos where Uso = 1 or Uso = 3;";
            IEnumerable<EnumModel> listaEncriptada;

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                listaEncriptada = db.Query<EnumModel>(sql1);
            }
            if(listaEncriptada == null || listaEncriptada.Count() == 0)
            {
                throw new ArgumentNullException("No hay relojes biométricos registrados.");
            }
            List<EnumModel> lista = listaEncriptada.ToList();
            List<string> relojesDesencriptados = new List<string>();
            foreach (EnumModel relojEncripted in lista)
            {
                relojesDesencriptados.Add("'" + UCommon.Cripto.DesencriptarBIO(relojEncripted.descripcion) + "'");
            }
            return string.Join(",", relojesDesencriptados);
        }

        private RedondeoModel GetRedondeoModel()
        {
            RedondeoDAL redondeoDAL = new RedondeoDAL(Configuration);
            RedondeoModel redondeo = redondeoDAL.GetRedondeoModel();            
            return redondeo;
        }


        #region Lista de Procesos
        private List<List<string>> ProcesosIniciales(DateTime f1, DateTime f2, string csvUsers)
        {
            List<List<string>> procesos = new List<List<string>>();

            string sRelojes = RelojesValidos();
            
            redondeoModel = GetRedondeoModel();

            procesos.Add(EliminaTablasTemporales());            

            procesos.Add(Tabla0(f1, f2, csvUsers));
            procesos.Add(TablaM(f1, f2, sRelojes, csvUsers));
            procesos.Add(LlenaTablasAux());
            //procesos.Add(EligeRotativos());
            //procesos.Add(ProcesaRotativosElegidos());
            procesos.Add(LlenaTablaHEL());
            procesos.Add(LlenaTablaXL());
            procesos.Add(CargaVariablesOT());
            procesos.Add(PoneAsistenciaPuntualidad());
            procesos.Add(PoneAtrasosConCargoVacacion(f1, f2, csvUsers));
            procesos.Add(PonePermisosHora2020(f1, f2, csvUsers));
            procesos.Add(PreparaCalculosHE());            

            return procesos;
        }

        private List<List<string>> ProcesosFinales(DateTime f1, DateTime f2, string csvUsers)
        {
            List<List<string>> procesos = new List<List<string>>();

            

            procesos.Add(Calcula_Horas_Extras());
            procesos.Add(QuitaTiempoAlmuerzo());
            procesos.Add(PoneTiempoPermisosAcumulados());
            procesos.Add(QuitaPermisosHora2020());
            // procesos.Add(DistribuyeHorasEnPermiso());
            if(redondeoModel.RedondeoA != -1)
            {
                procesos.Add(Redondea_Horas_Extras(redondeoModel));
            }            
            procesos.Add(Valora_Horas_Extras());
            procesos.Add(LlenaReporte(f1, f2, csvUsers));
            procesos.Add(LlenaResumenEmpleado());
            procesos.Add(PoneDescuentosPrestamos(f1, f2));
            procesos.Add(LlenaResumenDepartamento());

            return procesos;
        }

        private static List<string> Tabla0(DateTime f1, DateTime f2, string csvUsers)
        {
            List<string> lstTabla0 = new List<string>();

            //string s1 = @"Create table Tabla0
            //(idTipoH INT NOT NULL DEFAULT 0, 
            //DEPTID INT NULL , 
            //DEPTNombre VARCHAR(200) NULL, 
            //USERID INT NOT NULL , 
            //dFecha datetime NOT NULL, 
            //ATT INT NULL)";

            string sqlInsert = @"INSERT INTO [Tabla0] (userID, DeptID, ATT, dFecha) 
                SELECT userID, DEFAULTDEPTID, ATT, ";
            string sqlFromWhere = @"FROM USERINFO WHERE USERINFO.DEFAULTDEPTID > 0";
            sqlFromWhere = csvUsers == "" ? sqlFromWhere += ";" : sqlFromWhere += " AND USERID in (" + csvUsers + ");";


            StringBuilder s0 = new StringBuilder();
            for (int i = 0; i <= (f2 - f1).TotalDays; i++)
            {
                s0.Append(sqlInsert + "Cast('" + f1.AddDays(i).ToString("dd/MM/yyyy") + @"' as date) as dFecha " + sqlFromWhere);                
            }

            // lstTabla0.Add(s1);
            lstTabla0.Add(s0.ToString());

            return lstTabla0;
        }

        private static List<string> TablaM(DateTime f1, DateTime f2, string relojes, string csvUsers)
        {
            List<string> lstTablaM = new List<string>();
            f1 = f1.AddDays(-1).AddHours(21);
            f2 = f2.AddDays(1).AddHours(10);

            string formatoRedondeado = redondeoModel.RedondeoHoraMinSeg == 0 ? "Cast(CheckTime as smalldatetime)" : "CheckTime";

            string s1 = @"INSERT INTO TablaM
            SELECT Userid, " + formatoRedondeado + @", 'I' AS CheckType, VerifyCode, Sensorid, WorkCode, Machines.MachineAlias 
            FROM [Checkinout] LEFT JOIN Machines ON CheckInOut.SensorID = Machines.MachineNumber 
            WHERE ( [CheckInOut].sn in (" + relojes + @") or (CheckInOut.SN IS NULL OR Verifycode=7 or CheckInOut.sn = 'Web') )
            AND 
            (checktime between '" + f1.ToString("dd/MM/yyyy HH:mm:ss") + "' and '" + f2.ToString("dd/MM/yyyy HH:mm:ss") + "')\n";

            if(csvUsers != "")
            {
                s1 += "AND USERID in (" + csvUsers + ");";
            }

            formatoRedondeado = redondeoModel.RedondeoHoraMinSeg == 0 ? "Cast(AttTime as smalldatetime)" : "AttTime";

            // Toma las marcaciones Push
            string s2 = @"INSERT INTO [TablaM]
            (UserId, CheckTime, CheckType, VerifyCode, SensorId, WorkCode, MachineAlias)
            SELECT U.USERID, " + formatoRedondeado + @", 'I', verify, right(DeviceID, 3), 0, DeviceID
            FROM checkinout_P t inner join USERINFO U on t.PIN = U.Badgenumber
            WHERE DeviceID in (" + relojes +
            " ) \n and AttTime between '" + f1.ToString("dd/MM/yyyy HH:mm:ss") + "' and '" + f2.ToString("dd/MM/yyyy HH:mm:ss") + "'\n";

            if (csvUsers != "")
            {
                s2 += "AND USERID in (" + csvUsers + ");";
            }

            lstTablaM.Add(s1);
            lstTablaM.Add(s2);

            if (redondeoModel.RedondeoHoraMinSeg == 0)
            {
                iCharsHora = 5;
                string s3 = @"UPDATE [TablaM] SET CheckTime = Cast(CheckTime as smalldatetime);";
                lstTablaM.Add(s3);
            }
            else
            {
                iCharsHora = 8;
            }
            
            return lstTablaM;
        }

        private static List<string> LlenaTablasAux()
        {
            List<string> lstTablaAux = new List<string>();
            string s1 = @"UPDATE UserInfo SET idContrato=3 WHERE idContrato IS NULL or idContrato < 1 or idContrato > 5;
UPDATE USERINFO SET Overtime = RHE.Overtime, RegisterOT = RHE.RegisterOT 
    FROM USERINFO U INNER JOIN RegHoraExtra RHE ON u.RegHoraExtra = RHE.Id_RegHoraExtra;
UPDATE USERINFO SET HOLIDAY = RO.Holiday 
    FROM USERINFO U INNER JOIN RegOtros RO ON u.RegOtros = RO.Id_RegOtros;";

            // Llena Tabla 2
            string s2 = @"SELECT T0.DEPTID, T0.USERID, T0.dFecha, USER_OF_RUN.NUM_OF_RUN_ID, NUM_RUN.NAME AS NomTurno, NUM_RUN.STARTDATE AS iniTurno, 
                      NUM_RUN.CYCLE AS ciclosTurno, NUM_RUN.UNITS AS medidaTurno, 
                      (case NUM_RUN.UNITS 
            when 1 then datepart(dw, T0.dfecha) 
            when 2 then day(T0.dfecha) -1 
            else (datediff(day, USER_OF_RUN.STARTDATE, T0.dfecha) ) % NUM_RUN.CYCLE end)  as SDia, 
            case NUM_RUN.UNITS when '1' then '1' when '0' then 'D' when '2' then 'M' else null end as Dia 
            INTO [Tabla2] 
            FROM         NUM_RUN RIGHT OUTER JOIN 
                      USER_OF_RUN ON NUM_RUN.NUM_RUNID = USER_OF_RUN.NUM_OF_RUN_ID RIGHT OUTER JOIN 
                      Tabla0 AS T0 ON USER_OF_RUN.STARTDATE <= T0.dFecha AND USER_OF_RUN.ENDDATE >= T0.dFecha AND USER_OF_RUN.USERID = T0.USERID 
            ORDER BY T0.USERID, T0.dFecha";

            // Llena Tabla 3
            string s3 = @"SELECT 0 As idTipoH, T2.DEPTID, T2.USERID, T2.dFecha, T2.NUM_OF_RUN_ID as NUM_RUNID, T2.NomTurno, T2.iniTurno, T2.ciclosTurno, T2.medidaTurno, T2.SDia, T2.Dia, NUM_RUN_DEIL.SCHCLASSID 
            INTO Tabla3 
            FROM Tabla2 as T2 LEFT JOIN NUM_RUN_DEIL 
            ON (T2.NUM_OF_RUN_ID = NUM_RUN_DEIL.NUM_RUNID) AND (T2.SDia = NUM_RUN_DEIL.SDAYS);";

            // Completa horarios
            string s4 = @"UPDATE SCHCLASS SET Tipo = case  
            when [CheckInTIME1] >[StartTIME] then 1 
            when [StartTIME] >[CheckInTIME2] then 2 
            when [CheckInTIME2]>[CheckOutTIME1] then 3 
            when [CheckOutTIME1] >[EndTIME] then 4 
            when [EndTIME] >[CheckOutTIME2] then 5 
            else 0 
            end;";

            string s5 = "UPDATE SCHCLASS SET WorkMins=0 WHERE (WorkMins is null);";

            // Esta parte se repetirá en Tabla R
            // Quita días hábiles de los Feriados()
            string s6 = @"UPDATE T3 
            SET T3.schclassid = NULL 
            FROM 
            Tabla3 T3 INNER JOIN HOLIDAYS  
            ON T3.dFecha = HOLIDAYS.STARTTIME INNER JOIN 
            USERINFO ON T3.USERID = USERINFO.USERID 
            where userinfo.holiday=-1 AND (HOLIDAYS.idCiudad = USERINFO.idCiudad OR HOLIDAYS.idCiudad=0);";
            // Fin modificación por 3 tipos de feriados

            // Call Horarios Ocasionales()
            // Elimina el día siguiente en horarios de 24 horas
            string s7 = @"DELETE FROM T3 
            FROM Tabla3 AS T3 INNER JOIN user_temp_sch 
            ON T3.USERID = [USER_TEMP_SCH].USERID and  
            cast(T3.dFecha as DATE) = cast(dateadd (d,1,[USER_TEMP_SCH].COMETIME) as DATE) 
            INNER JOIN SchClass ON 
            USER_TEMP_SCH.schClassId = SchClass.schClassid 
            WHERE SchClass.Tipo >= 24;";

            // Elimina el día propio en cualquier horario
            string s8 = @"DELETE FROM T3 
            FROM Tabla3 AS T3, user_temp_sch 
            WHERE T3.USERID = [USER_TEMP_SCH].USERID and  
            cast(T3.dFecha as DATE) = cast([USER_TEMP_SCH].COMETIME as DATE);";

            // Inserta los horarios ocasionales
            string s9 = @"INSERT INTO Tabla3
            (idTipoh, USERID, dFecha, SCHClassID) 
            SELECT Type, USER_TEMP_SCH.USERID, CAST(USER_TEMP_SCH.COMETIME AS DATE), USER_TEMP_SCH.SCHCLASSID 
            FROM Tabla2 T2 INNER JOIN USER_TEMP_SCH ON 
            T2.USERID = [USER_TEMP_SCH].USERID and
            cast(T2.dFecha as DATE) = cast([USER_TEMP_SCH].COMETIME as DATE)";

            string s10 = "DELETE FROM Tabla3 where idTipoH = -1;";

            // Llena tabla R
            // Inserta los horarios rotativos en los días que NO hay un turno fijo asignado
            string s11 = @"SELECT T3.idTipoH, T3.DEPTID, T3.USERID,  
            T3.dFecha, datename(dw,T3.dfecha) AS Dia, T3.NUM_RUNID,   
            SCHCLASS.SCHCLASSID, SCHCLASS.SCHNAME, T3.dFecha + 2 + SCHCLASS.STARTTIME as Entrada,   
            case when (SCHCLASS.Tipo=5 OR SCHCLASS.Tipo<=1) then (T3.dFecha + 2 + SCHCLASS.ENDTIME) else  
            (T3.dFecha + 3 + SCHCLASS.ENDTIME) end as Salida,   
            SCHCLASS.LATEMINUTES, SCHCLASS.EARLYMINUTES, SCHCLASS.CHECKIN, SCHCLASS.CHECKOUT,  
            (case when SCHCLASS.Tipo=1 then (T3.dFecha + 1 + SCHCLASS.CHECKINTIME1) else  
            (T3.dFecha + 2 + SCHCLASS.CHECKINTIME1) end ) as iEntrada,  
            case when SCHCLASS.Tipo=2 then T3.dFecha + 3 + SCHCLASS.CHECKINTIME2 else T3.dFecha + 2 + SCHCLASS.CHECKINTIME2 end as fEntrada,   
            case when SCHCLASS.Tipo=2 OR SCHCLASS.Tipo=3 then T3.dFecha + 3 + SCHCLASS.CHECKOUTTIME1 else  
            T3.dFecha + 2 + SCHCLASS.CHECKOUTTIME1 end as iSalida,  
            case when SCHCLASS.Tipo>=2 then T3.dFecha + 3 + SCHCLASS.CHECKOUTTIME2 else T3.dFecha + 2 + SCHCLASS.CHECKOUTTIME2 end as fSalida,   
            SCHCLASS.WorkDay, cast(SCHCLASS.WorkMins/(24*60) AS SmallDateTime) as Almuerzo, AutoBind as descuentaAlmuerzo  
            INTO [TablaR]  
            FROM (Tabla3 AS T3 INNER JOIN UserUsedSClasses ON T3.USERID = UserUsedSClasses.UserId) INNER JOIN SCHCLASS ON UserUsedSClasses.SchId = SCHCLASS.SCHCLASSID   
            WHERE (((T3.SCHCLASSID) Is Null));";

            // Pone Almuerzo
            string s12 = "UPDATE TablaR set Almuerzo='00:00:00' WHERE Almuerzo=0;";

            // Call LlenaTablaRInOut()

            // Registros de Entrada   TablaRIn
            string s13 = @"SELECT TR.DEPTID, TR.USERID, TR.dFecha, TR.Dia, TR.NUM_RUNID, TR.schClassid, TR.schName, TR.Entrada, TR.Salida, TR.iEntrada, TR.fEntrada, 
            Min(TablaM.checktime) AS RegEntrada, TR.LateMinutes, TR.EarlyMinutes, TR.WorkDay, TR.Almuerzo, TR.descuentaAlmuerzo 
            INTO TablaRIn 
            FROM TablaM RIGHT JOIN TablaR AS TR 
            ON (TablaM.USERID = TR.USERID and (TablaM.checktime between [TR].[iEntrada] and [TR].[fEntrada] )) 
            GROUP BY TR.DEPTID, TR.USERID, TR.dFecha, TR.Dia, TR.NUM_RUNID, TR.schClassid, TR.schName, TR.Entrada, TR.Salida, TR.iEntrada, TR.fEntrada, TR.LateMinutes, TR.EarlyMinutes, TR.WorkDay, TR.Almuerzo, TR.descuentaAlmuerzo;";

            // RegSalida              TablaROut
            // 'Salida con Botón Salida <> I
            string s14 = @"SELECT TR.idTipoH, TR.USERID, TR.dFecha, TR.schClassid, TR.schName, TR.iSalida, TR.fSalida, 
            Max(TablaM.checktime) AS RegSalida 
            INTO TablaROut1 
            FROM TablaM RIGHT JOIN TablaR AS TR 
            ON (TablaM.USERID = TR.USERID and (TablaM.checktime between [TR].[iSalida] and [TR].[fSalida]))
            WHERE TablaM.checkType<>'I' AND TablaM.checkType<>'X' 
            GROUP BY TR.idTipoH, TR.USERID, TR.dFecha, TR.schClassid, TR.schName, TR.iSalida, TR.fSalida;";

            // Salida con Botón Entrada I
            string s15 = @"SELECT TR.idTipoH, TR.USERID, TR.dFecha, TR.schClassid, TR.schName, TR.iSalida, TR.fSalida, 
            Max(TablaM.checktime) AS RegSalida 
            INTO TablaROut2 
            FROM TablaM RIGHT JOIN TablaR AS TR 
            ON (TablaM.USERID = TR.USERID and (TablaM.checktime between [TR].[iSalida] and [TR].[fSalida])) 
            GROUP BY TR.idTipoH, TR.USERID, TR.dFecha, TR.schClassid, TR.schName, TR.iSalida, TR.fSalida;";

            // Únión Salidas, tiene preferencia botón Salida sobre botón Entrada
            string s16 = @"SELECT TablaROut2.idTipoH, TablaROut2.USERID, TablaROut2.dFecha, TablaROut2.schClassid, TablaROut2.schName, TablaROut2.iSalida, TablaROut2.fSalida, 
            CASE WHEN (TablaROut1.RegSalida) IS NULL THEN TablaROut2.RegSalida ELSE TablaROut1.RegSalida END AS RegSalida, 
            CASE WHEN (TablaROut1.RegSalida) IS NULL THEN 'I' ELSE 'O' END AS CheckType 
            INTO TablaROut 
            FROM TablaROut1  RIGHT JOIN TablaROut2 
            ON (TablaROut1.USERID=TablaROut2.USERID AND TablaROut1.dFecha=TablaROut2.dFecha AND TablaROut1.schClassid=TablaROut2.schClassid);";

            // Entrada y Salida       TablaRInOut
            string s17 = @"SELECT RIn.DEPTID, RIn.USERID, RIn.dFecha, RIn.Dia, RIn.NUM_RUNID, RIn.schClassid, RIn.schName, RIn.Entrada, RIn.Salida, RIn.iEntrada, RIn.fEntrada, RIn.RegEntrada, 
            ROut.iSalida, ROut.fSalida, ROut.RegSalida, RIn.LateMinutes, RIn.EarlyMinutes, RIn.WorkDay, RIn.Almuerzo, RIn.descuentaAlmuerzo, 0 as RegDiaHabil, ROut.idTipoH 
            INTO TablaRInOut 
            FROM TablaRIn as RIn INNER JOIN TablaROut as ROut
            ON (RIn.schClassid = ROut.schClassid) AND (RIn.dFecha = ROut.dFecha) AND (RIn.USERID = ROut.USERID);";


            string s18 = "DELETE from TablaRInOut where ((Regsalida is null) AND (RegEntrada is null));";

            string s19 = "ALTER TABLE TablaRInOut ADD overtime INT;";

            string s20 = @"Update TablaRInOut 
            SET TablaRInOut.overtime = userinfo.overtime 
            FROM TablaRInOut INNER JOIN userinfo on TablaRInOut.userid=userinfo.userid;";

            // Call LlenaTablaF()
            string s21 = @"SELECT T3.idTipoH, T3.DEPTID, T3.USERID, T3.dFecha, T3.Dia, T3.NUM_RUNID,  
            H.SCHCLASSID, H.SCHNAME, 
            cast(T3.dFecha + 2 + H.STARTTIME as smalldatetime) as Entrada,  
            CASE WHEN (H.Tipo=5 OR H.Tipo<=1) THEN cast(T3.dFecha + 2 + H.ENDTIME as smalldatetime) 
            ELSE cast(T3.dFecha + 3 + H.ENDTIME as smalldatetime) END as Salida,  
            H.LATEMINUTES, H.EARLYMINUTES, H.CHECKIN, H.CHECKOUT, 
            CASE WHEN H.Tipo=1 THEN CAST(T3.dFecha + 1 + H.CHECKINTIME1 AS SMAlldatetime) 
            WHEN H.Tipo>=24 AND CHECKINTIME1 > STARTTIME THEN cast(T3.dFecha + 1 + CHECKINTIME1 as smalldatetime) 
            WHEN H.Tipo>=24 AND CHECKINTIME1 < STARTTIME THEN cast(T3.dFecha + 2 + CHECKINTIME1 as smalldatetime) 
            ELSE cast(T3.dFecha + 2 + H.CHECKINTIME1 as smalldatetime) END as iEntrada,  
            CASE WHEN H.Tipo=2 THEN cast(T3.dFecha + 3 + H.CHECKINTIME2 as smalldatetime) 
            WHEN H.Tipo>=24 AND CHECKINTIME2 > STARTTIME THEN cast(T3.dFecha + 2 + CHECKINTIME2 as smalldatetime) 
            WHEN H.Tipo>=24 AND CHECKINTIME2 < STARTTIME THEN cast(T3.dFecha + 3 + CHECKINTIME2 as smalldatetime) 
            ELSE cast(T3.dFecha + 2 + H.CHECKINTIME2 as smalldatetime) END as fEntrada,  
            CASE WHEN (H.Tipo=2 OR H.Tipo=3) THEN cast(T3.dFecha + 3 + H.CHECKOUTTIME1 as smalldatetime) 
            WHEN H.Tipo>=24 AND CHECKINTIME2 > CHECKOUTTIME1 THEN cast(T3.dFecha + 3 + CHECKOUTTIME1 as smalldatetime) 
            WHEN H.Tipo>=24 AND CHECKINTIME2 < CHECKOUTTIME1 THEN cast(T3.dFecha + 2 + CHECKOUTTIME1 as smalldatetime) 
            ELSE cast(T3.dFecha + 2 + H.CHECKOUTTIME1 as smalldatetime) END as iSalida,  
            CASE WHEN H.Tipo>=2 THEN cast(T3.dFecha + 3 + H.CHECKOUTTIME2 as smalldatetime) 
            ELSE cast(T3.dFecha + 2 + H.CHECKOUTTIME2 as smalldatetime) END as fSalida,  
            H.WorkDay, cast(H.WorkMins/(60*24) as SmallDateTime) as Almuerzo, AutoBind as descuentaAlmuerzo 
            INTO [TablaF] 
            FROM (Tabla3 as T3 INNER JOIN SCHCLASS AS H ON T3.schClassid = H.SCHCLASSID);";

            // Pone Almuerzo
            string s22 = @"UPDATE TablaF set Almuerzo='00:00:00' WHERE Almuerzo=0 AND Almuerzo < Salida - Entrada;";

            // Call LlenaTablaFInOut()      
            // 'RegEntrada              TablaFIn
            string s23 = @"SELECT F.DEPTID, F.USERID, F.dFecha, F.Dia, F.NUM_RUNID, F.schClassid, F.schName, F.Entrada, F.Salida, F.iEntrada, F.fEntrada, Min(MIn.checktime) AS RegEntrada, F.LateMinutes, F.EarlyMinutes, F.WorkDay, F.Almuerzo, F.descuentaAlmuerzo 
            INTO TablaFIn 
            FROM TablaM as MIn RIGHT JOIN TablaF as F 
            ON (MIn.USERID = F.USERID and (MIn.checktime between [F].[iEntrada] and [F].[fEntrada] ) AND (MIn.CheckType <> 'X') ) 
            GROUP BY F.DEPTID, F.USERID, F.dFecha, F.Dia, F.NUM_RUNID, F.schClassid, F.schName, F.Entrada, F.Salida, F.iEntrada, F.fEntrada, F.LateMinutes, F.EarlyMinutes, F.WorkDay, F.Almuerzo, F.descuentaAlmuerzo;";

            // RegSalida              TablaFOut
            string s24 = @"SELECT F.idTipoH, F.USERID, F.dFecha, F.NUM_RUNID, F.schClassid, F.iSalida, F.fSalida, Max(MOut.checktime) AS RegSalida 
            INTO TablaFOut 
            FROM TablaM as MOut RIGHT JOIN TablaF as F 
            ON (MOut.USERID = F.USERID and (MOut.checktime between [F].[iSalida] and [F].[fSalida]) AND (MOut.CheckType <> 'X') ) 
            GROUP BY F.idTipoH, F.USERID, F.dFecha, F.NUM_RUNID, F.schClassid, F.iSalida, F.fSalida;";

            // Entrada y Salida       TablaFInOut
            string s25 = @"SELECT FIn.DEPTID, FIn.USERID, FIn.dFecha, FIn.Dia, FIn.NUM_RUNID, FIn.schClassid, FIn.schName, FIn.Entrada, FIn.Salida, FIn.iEntrada, FIn.fEntrada, FIn.RegEntrada, FOut.iSalida, FOut.fSalida, FOut.RegSalida, FIn.LateMinutes, FIn.EarlyMinutes, FIn.WorkDay, FIn.Almuerzo, FIn.descuentaAlmuerzo, FOut.idTipoH 
            INTO TablaFInOut 
            FROM TablaFIn as FIn INNER JOIN TablaFOut as FOut 
            ON (FIn.schClassid = FOut.schClassid) AND (FIn.dFecha = FOut.dFecha) AND (FIn.USERID = FOut.USERID);";

            lstTablaAux.Add(s1);
            lstTablaAux.Add(s2);
            lstTablaAux.Add(s3);
            lstTablaAux.Add(s4);
            lstTablaAux.Add(s5);
            lstTablaAux.Add(s6);
            lstTablaAux.Add(s7);
            lstTablaAux.Add(s8);
            lstTablaAux.Add(s9);
            lstTablaAux.Add(s10);
            lstTablaAux.Add(s11);
            lstTablaAux.Add(s12);
            lstTablaAux.Add(s13);
            lstTablaAux.Add(s14);
            lstTablaAux.Add(s15);
            lstTablaAux.Add(s16);
            lstTablaAux.Add(s17);
            lstTablaAux.Add(s18);
            lstTablaAux.Add(s19);
            lstTablaAux.Add(s20);
            lstTablaAux.Add(s21);
            lstTablaAux.Add(s22);
            lstTablaAux.Add(s23);
            lstTablaAux.Add(s24);
            lstTablaAux.Add(s25);

            return lstTablaAux;
        }

        private static List<string> EligeRotativos()
        {
            List<string> lstEligeRotativos = new List<string>();

            string s1 = @"";
            string s2 = @"";
            string s3 = @"";
            string s4 = @"";
            string s5 = @"";
            string s6 = @"";
            string s7 = @"";
            string s8 = @"";
            string s9 = @"";
            string s10 = @"";
            string s11 = @"";
            string s12 = @"";
            string s13 = @"";
            string s14 = @"";
            string s15 = @"";
            string s16 = @"";
            string s17 = @"";
            string s18 = @"";

            lstEligeRotativos.Add(s1);
            lstEligeRotativos.Add(s2);
            lstEligeRotativos.Add(s3);
            lstEligeRotativos.Add(s4);
            lstEligeRotativos.Add(s5);
            lstEligeRotativos.Add(s6);
            lstEligeRotativos.Add(s7);
            lstEligeRotativos.Add(s8);
            lstEligeRotativos.Add(s9);
            lstEligeRotativos.Add(s10);
            lstEligeRotativos.Add(s11);
            lstEligeRotativos.Add(s12);
            lstEligeRotativos.Add(s13);
            lstEligeRotativos.Add(s14);
            lstEligeRotativos.Add(s15);
            lstEligeRotativos.Add(s16);
            lstEligeRotativos.Add(s17);
            lstEligeRotativos.Add(s18);

            return lstEligeRotativos;
        }

        private static List<string> ProcesaRotativosElegidos()
        {
            List<string> lstProcesaRotativos = new List<string>();

            string s1 = @"";
            string s2 = @"";
            string s3 = @"";
            string s4 = @"";
            string s5 = @"";
            string s6 = @"";
            string s7 = @"";
            string s8 = @"";
            string s9 = @"";
            string s10 = @"";
            string s11 = @"";
            string s12 = @"";
            string s13 = @"";
            string s14 = @"";
            string s15 = @"";
            string s16 = @"";
            string s17 = @"";
            string s18 = @"";

            lstProcesaRotativos.Add(s1);
            lstProcesaRotativos.Add(s2);
            lstProcesaRotativos.Add(s3);
            lstProcesaRotativos.Add(s4);
            lstProcesaRotativos.Add(s5);
            lstProcesaRotativos.Add(s6);
            lstProcesaRotativos.Add(s7);
            lstProcesaRotativos.Add(s8);
            lstProcesaRotativos.Add(s9);
            lstProcesaRotativos.Add(s10);
            lstProcesaRotativos.Add(s11);
            lstProcesaRotativos.Add(s12);
            lstProcesaRotativos.Add(s13);
            lstProcesaRotativos.Add(s14);
            lstProcesaRotativos.Add(s15);
            lstProcesaRotativos.Add(s16);
            lstProcesaRotativos.Add(s17);
            lstProcesaRotativos.Add(s18);

            return lstProcesaRotativos;
        }

        private static List<string> LlenaTablaHEL()
        {
            List<string> lstTablaHEL = new List<string>();

            //    'Tabla HEL: Sin Turno, Con Turno pero en día de Descanso y SIN Horarios Rotativos pero con FOT
            //'Crea tabla HEL     Tabla 3 x UserUsedClasses
            //'En esta tabla sabemos que el RegisterOT = 1
            string s1 = @"SELECT T3.USERID, T3.dFecha, T3.NUM_RUNID, T3.SchClassID  
            Into HEL
            FROM (Tabla3 as T3 LEFT JOIN UserUsedSClasses ON T3.USERID = UserUsedSClasses.UserId) INNER JOIN USERINFO ON T3.USERID = USERINFO.USERID 
            WHERE (((UserUsedSClasses.UserId) Is Null) AND ((T3.schClassId) Is Null or T3.SCHClassID=-1) AND ((USERINFO.RegisterOT)=1));";

            //    'La tabla HELTemporal tiene las marcaciones de los empleados cuando no tienen turno fijo ni rotativo pero tienen FOT
            //'y cuando teniendo turno fijo, no tienen rotativos pero tienen FOT
            string s2 = @"Create table HELTemporal
            (OverTime INT NULL , 
            DeptID INT NULL,  
            DeptNombre VarChar(200) NULL,  
            UserID INT NULL,  
             dFecha smalldatetime null,  
             TDia INT NULL,  
             NomDia Varchar(12) NULL,  
             RegEntrada datetime null,  
             RegSalida datetime null,  
             CuentaDeCHECKTIME INT NULL,  
             TTrabajado datetime NULL,  
             TAsistencia datetime NULL,  
             HE50 datetime NULL,  
             Descanso datetime NULL,  
             HorarioNormal datetime NULL,  
             RegDiaHabil INT NULL,  
             RegHoraExtra INT NULL,  
             TipoHEL INT NULL,  
             FueraDeHorario INT,  
             NUM_RUNID INT NULL DEFAULT 0,  
             schClassid INT NULL);";

            //    'TipoHEL = 0 los días de descanso del empleado cuando tiene turno fijo y no tiene horario rotativo
            //'TipoHEL = 1 cuando el empleado no tiene turno fijo ni horario rotativo
            //'TipoHEL = 2 el TIEMPO que el empleado no tiene horario asignado, puede ser FOT DENTRO DEL DIA en que hay turnos fijos asignados

            //'Pone RegEntrada en Tabla HEL IN Temporal   
            string s3 = @"WITH helBase as ( 
            Select HEL.*, F.RegSalida as SalidaAnterior 
              FROM HEL LEFT JOIN TablaFInOut F 
            on hel.userid = F.USERID and hel.dfecha = cast(f.RegSalida as date)  
            ) 
            select helBase.*, MIN(M.CheckTime) as RegEntrada 
            INTO  helIN 
            from helBase LEFT join TablaM as M 
            ON helBase.userid = M.Userid AND convert(varchar(10), helBase.dFecha,110) = convert(varchar(10), M.checktime, 110) 
            AND M.CheckTime > isnull(helBase.SalidaAnterior, cast(helBase.dFecha as date))
            group by helBase.userid, helBase.dfecha, helBase.num_runid, helBase.schclassid, helBase.SalidaAnterior;";

            //'Pone RegSalida en Tabla HELTemporal   
            string s4 = @"INSERT INTO HELTemporal (UserId, dFecha, NUM_RUNID, SchClassID, RegEntrada, RegSalida, TipoHEL ) 
            SELECT HIN.USERID, HIN.dFecha, HIN.NUM_RUNID, HIN.SchClassID, HIN.RegEntrada, 
            Max(TM.CHECKTIME) AS RegSalida, NULL as TipoHEL 
            FROM helIN AS HIN LEFT JOIN TablaM AS TM ON HIN.USERID = TM.USERid 
            AND convert(varchar(10), HIN.dFecha,110) = convert(varchar(10), TM.checktime, 110) 
            AND TM.CheckTime > isnull(HIN.RegEntrada, cast(HIN.dFecha as date)) 
            GROUP BY HIN.USERID, HIN.dFecha, HIN.NUM_RUNID, HIN.SchClassID, HIN.RegEntrada, HIN.SalidaAnterior;";

            string s5 = @"UPDATE HELTemporal
            SET RegDiaHabil    = U.RegDiaHabil,
				RegHoraExtra   = U.RegHoraExtra
            from HELTemporal inner join Userinfo U on HELTemporal.UserID = U.USERID";


            //    @"UPDATE HELtemp 
            // SET Overtime = userinfo.Overtime, RegDiaHabil = Userinfo.RegDiaHabil, deptid = Userinfo.defaultDeptId 
            // FROM HELTemporal HELtemp INNER JOIN USERINFO ON HELtemp.userid = Userinfo.userid;";

            string s6 = @"UPDATE HELTemporal
SET Overtime    = RegHoraExtra.Overtime,
FueraDeHorario = RegHoraExtra.FueraDeHorario
from HELTemporal inner join RegHoraExtra on HELTemporal.RegHoraExtra = RegHoraExtra.Id_RegHoraExtra";

            string s7 = @"DELETE FROM HELTemporal
            WHERE RegEntrada IS NULL or RegSalida IS NULL;";

            string s8 = @"UPDATE HELTemporal Set TAsistencia = RegSalida - RegEntrada,
TDia = datepart(dw,dFecha) % 7, NomDia=datename(dw,dFecha);";

            string s9 = @"UPDATE HELTemporal Set TTrabajado = TAsistencia;";

            //    'TipoHEL = 0 cuando el empleado tiene turno fijo pero es día de descanso y no tiene horario rotativo
            //'TipoHEL = 1 cuando el empleado no tiene turno fijo ni horario rotativo
            //'TipoHEL = 2 el TIEMPO que el empleado no tiene horario asignado, puede ser FOT DENTRO DEL DIA en que hay turnos fijos asignados
            //' 
            //'Nuevo TipoHEL
            //'TipoHEL = 0: Todo el día es HE, (es día de descanso cuando no hay turno fijo) ó (es día sin horario fijo de turno fijo y DSH no es HE)
            //'TipoHEL = 1: HE es TTrabajado menos 8:00:00, (es día hábil cuando no hay turno fijo) ó (es día sin horario fijo de turno fijo y DSH ES HE)
            //'Esta parte es cuando NO tiene turno fijo (ni horario rotativo)
            //'TDia=1 = día Hábil, TDía=0 = día de descanso
            string s10 = @"UPDATE HELTemporal
            Set TipoHEL = Case TDia 
             When 0 Then RegDiaHabil.dia0 
             When 1 Then RegDiaHabil.dia1 
             When 2 Then RegDiaHabil.dia2 
             When 3 Then RegDiaHabil.dia3 
             When 4 Then RegDiaHabil.dia4 
             When 5 Then RegDiaHabil.dia5 
             When 6 Then RegDiaHabil.dia6 End  
             --,   FueraDeHorario = RegDiaHabil.FueraDeHorario 
            FROM HELTemporal INNER JOIN RegDiaHabil On 
            HELTemporal.RegDiaHabil = RegDiaHabil.Id_RegDiaHabil 
            WHERE NUM_RUNID Is null;";

            //'Aplica en el DSH el FueraDeHorario, Si FueraDeHorario then 0 else 1
            string s11 = @"UPDATE HELTemporal Set TipoHEL=Case RegDiaHabil.FueraDeHorario When '1' THEN '0' ELSE '1' END 
            FROM HELTemporal INNER JOIN RegDiaHabil ON 
            HELTemporal.RegDiaHabil = RegDiaHabil.Id_RegDiaHabil
            WHERE not NUM_RUNID IS NULL;";

            //'DSH es HE, cuando TipoHEL = 0
            string s12 = @"UPDATE HELTemporal SET Descanso = TTrabajado where Overtime = 1 and TipoHEL = 0;";

            //'DSH es día hábil, cuando TipoHEL = 1
            string s13 = @"UPDATE HELTemporal SET HorarioNormal = TTrabajado where TipoHEL = 1;";

            //'DSH es Horario Normal, cuando el empleado no gana HE
            string s14 = @"UPDATE HELTemporal SET HorarioNormal = TTrabajado where overtime = 0;";

            //'Calcula HE cuando es el exceso de las 8:00
            string s15 = @"UPDATE HELTemporal SET HE50 = TTrabajado - '08:30:00', HorarioNormal = '08:30:00' 
            WHERE Overtime = 1 AND TipoHEL = 1 and TTrabajado > '08:30:00';";

            lstTablaHEL.Add(s1);
            lstTablaHEL.Add(s2);
            lstTablaHEL.Add(s3);
            lstTablaHEL.Add(s4);
            lstTablaHEL.Add(s5);
            lstTablaHEL.Add(s6);
            lstTablaHEL.Add(s7);
            lstTablaHEL.Add(s8);
            lstTablaHEL.Add(s9);
            lstTablaHEL.Add(s10);
            lstTablaHEL.Add(s11);
            lstTablaHEL.Add(s12);
            lstTablaHEL.Add(s13);
            lstTablaHEL.Add(s14);
            lstTablaHEL.Add(s15);

            return lstTablaHEL;
        }

        private static List<string> LlenaTablaXL()
        {
            List<string> lstTablaXL = new List<string>();

            //    'Crea la tabla XL
            //'La tabla XL tiene toda la información del reporte de Asistencia
            string s1 = @"Create table TablaXL   
            (Departamento VARCHAR(200) NULL ,  
            Cargo VARCHAR(50) NULL ,  
            DeptID INT NULL ,  
            UserID INT NOT NULL ,  
            Empleado VARCHAR(60) NULL ,  
            NumCA VARCHAR(10) NULL ,  
            Sueldo FLOAT null ,  
            Cedula varchar(20) null ,  
            dFecha smalldatetime NOT NULL ,  
            Dia VARCHAR(10) NULL ,  
            Horario VARCHAR(25) NULL ,  
            Entrada smalldatetime NULL ,  
            Salida smalldatetime NULL ,  
            iE smalldatetime NULL ,  
            fE smalldatetime NULL ,  
            iSalida smalldatetime NULL ,  
            fS smalldatetime NULL ,  
            DiaNormal FLOAT NULL ,  
            JornadaTrabajo FLOAT NULL ,  
            codTurno INT NULL,  
            codH INT NOT NULL ,  
            MinGE INT NULL ,  
            MinGS INT NULL ,  
            descuentaAlmuerzo INT NULL, ";

            s1 += @"jornada SmallDateTime NULL , 
                tiempoBDD datetime NULL,  
                marca_entrada datetime NULL,  
                marca_salida datetime NULL,  
                diaEnteroCHE INT NULL,  
                turnoEnteroCHE INT NULL,  
                checkIn int NULL ,  
                checkOut int NULL ,  
                regENT datetime NULL ,  
                regSAL datetime NULL ,  
                Atraso datetime NULL ,  
                SalidaTemprano datetime NULL ,  
                numAtrasos int NULL ,  
                numSalidasTemprano int NULL ,  
                iEA datetime NULL ,  
                fEA datetime NULL ,  
                iA datetime NULL ,  
                fA datetime NULL ,  
                regIngALM datetime NULL ,  
                regsalALM datetime NULL ,  
                TiempoAlmuerzo datetime NULL ,  
                AnticipoAlmuerzo datetime NULL ,  
                AtrasoAlmuerzo datetime NULL ,  
                ExcesoAlmuerzo datetime NULL , ";
            s1 += @"Permiso varchar(40) NULL ,  
                MotivoPermiso varchar(50) NULL ,  
                IniPermiso smalldatetime NULL ,  
                FinPermiso smalldatetime NULL ,  
                TiempoPermisoT smalldatetime NULL ,  
                TiempoPermisoNT smalldatetime NULL ,  
                ClasePermiso VarChar(50) NULL ,  
                TipoPermiso VarChar(2) NULL , ";
            s1 += @"IdContrato INT NULL, 
                Contrato VarChar(30) NULL ,  
                InicioTrabajo datetime NULL ,  
                FinTrabajo datetime NULL ,  
                Ausente FLOAT NULL ,  
                DiaTrabajado FLOAT NULL ,  
                HorarioNormal datetime NULL ,  
                JornadaNocturna datetime NULL ,  
                HE50 datetime NULL ,  
                HE100 datetime NULL ,  
                Descanso datetime NULL ,  
                TiempoTrabajado datetime NULL ,  
                TiempoAsistencia datetime NULL ,  
                USDNormal FLOAT NULL ,  
                USDNocturna FLOAT NULL ,  
                USD50 FLOAT NULL ,  
                USD100 FLOAT NULL ,  
                MultaAtrasos FLOAT NULL ,  
                MultaAusencias FLOAT NULL ,  
                USDTotal FLOAT NULL ,  
                idTipoh INT NULL,  
                Marcaciones INT NULL ,  
                Almuerzo datetime NULL ,  
                TotalNoLaborado datetime NULL ,  
                TiempoNoCumplido datetime NULL ,  
                ATT INT NULL ,  
                OverTime INT NULL ,  
                Holiday INT NULL ,  
                RegisterOT INT NULL) ";

            //    'idTipoH = 3 para HEL, No Calcula Atrasos, Todo el día es HE ó HE50 = TTrabajado - 8h00
            //'idTipoH = 0 para Horarios Fijos de turnos fijos, 
            //'            Días Hábiles de horarios rotativos sin Turnos fijos, y horarios rotativos con Turnos Fijos pero Rotativo NO es HE
            //'            cuando Calcula Atrasos; y Calcula JN, HE50, HE100
            //'idTipoH = 1 para Días Descanso de horarios rotativos sin Turnos fijos, y horarios rotativos con Turnos Fijos y Rotativo ES HE
            //'            cuando Calcula Atrasos: y Todo el Horario es HE

            //'Ingresa tabla F InOut
            string s2 = @"INSERT INTO [TablaXL ]  
            (DEPTID, USERID, dFecha, Dia, codH,  
            Horario, Entrada, Salida, iE, fE, iSalida,  
            fS, RegENT, RegSAL, MinGE, MinGS, DiaNormal, Almuerzo, descuentaAlmuerzo, idTipoh) 
              
            SELECT DEPTID, USERID, dFecha, Dia, SchClassid,  
            schName, Entrada, Salida, iEntrada, fEntrada, iSalida,  
            fSalida, RegEntrada, RegSalida, LateMinutes, EarlyMinutes, WorkDay, Almuerzo, descuentaAlmuerzo, idTipoH  
            FROM TablaFInOut ;";

            //'Ingresa tabla R InOut
            string s3 = @"INSERT INTO [TablaXL ] (DEPTID, USERID, dFecha, Dia, codH,  
            Horario, Entrada, Salida, iE, fE, iSalida,  
            fS, RegENT, RegSAL, MinGE, MinGS, DiaNormal, Almuerzo, descuentaAlmuerzo, idTipoh) 
              
            SELECT DEPTID, USERID, dFecha, Dia, SchClassid,  
            schName, Entrada, Salida, iEntrada, fEntrada, iSalida,  
            fSalida, RegEntrada, RegSalida, LateMinutes, EarlyMinutes, WorkDay, Almuerzo, descuentaAlmuerzo, idTipoH  
            FROM TablaRInOut ;";

            //    'Modificación Amcor con Feriados de 3 tipos y por Ciudad
            //'Mantiene Horario, pero calcula al 100% en los Feriados
            string s4 = @"UPDATE XL   
            SET XL.idTipoh = 1   
            FROM  
            TablaXL  XL INNER JOIN HOLIDAYS    
            ON XL.dFecha = HOLIDAYS.STARTTIME INNER JOIN   
            USERINFO ON XL.USERID = USERINFO.USERID   
            where userinfo.holiday=1 AND (HOLIDAYS.idCiudad = USERINFO.idCiudad OR HOLIDAYS.idCiudad=0);";

            //    'Mantener el Horario Normal en Feriado es no hacer ninguna modificación

            //'Ingresa HEL
            //'Añade a TablaXL, la tabla HEL Temporal junto con el TiempoTrabajado u Tiempo Extra. De Codigo del Trabajo
            string s5 = @"INSERT INTO TablaXL  ( Departamento, DEPTID, USERID, NumCA, Cedula, Empleado, dFecha,  
            Sueldo, regENT, regSAL, TiempoTrabajado, TiempoAsistencia, Dia, HorarioNormal, HE50, Descanso, DiaTrabajado, codTurno, codH, idTipoh )   
            SELECT DEPARTMENTS.DEPTNAME, HELT.DEPTID, HELT.USERID, USERINFO.Badgenumber,  
            USERINFO.SSN, USERINFO.Name, HELT.dFecha, USERINFO.sueldo, HELT.RegEntrada, HELT.RegSalida,  
            HELT.TTrabajado, HELT.TAsistencia, HELT.NomDia, HELT.HorarioNormal, HELT.HE50,  
            HELT.Descanso, 1 AS Expr1, HELT.NUM_RUNID, -1 as codH, 3 AS idTipoH   
            FROM (HELTemporal  HELT LEFT JOIN DEPARTMENTS ON HELT.DEPTID = DEPARTMENTS.DEPTID) INNER JOIN   
            USERINFO ON HELT.USERID = USERINFO.USERID;";

            //'Ingresa Ausencias en Días Hábiles Obligatorios con Horarios Rotativos
            string s6 = @"IF OBJECT_ID('TAusencias ', 'U') IS NOT NULL  
            INSERT INTO [TablaXL ] (DEPTID, USERID, dFecha, Dia, codH, DiaNormal)  
            SELECT Ausencias.DEPTID, Ausencias.USERID, Ausencias.dFecha, Ausencias.Dia, 3, 1  
            FROM TAusencias  as Ausencias LEFT join TablaXL  as XL   
            on Ausencias.userid= xl.UserID and Ausencias.dfecha = xl.dFecha   
            WHERE TDia = 1;";

            //    'Pasa el dato de Tipo de Contrato (LOSEP o Código de Trabajo) y OverTime
            //'Personalización para el IESS DDI, lo normal es:
            //'strSQLServer = "UPDATE TablaXL SET Contrato=CASE left(userinfo.gender,4) WHEN 'LOSE' THEN 'LOSEP' WHEN 'CODI' THEN 'CodigoT' else 'Otro' END
            string s7 = @"UPDATE TablaXL   
            SET idContrato= USERINFO.idContrato, Contrato = nomContrato,  
            overtime = USERINFO.overtime  
            FROM TablaXL  INNER JOIN USERINFO ON TablaXL .UserId=Userinfo.Userid 
			INNER JOIN CONTRATO ON USERINFO.idContrato = CONTRATO.idContrato;";

            //'Pone el nombre del día
            string s8 = @"UPDATE TablaXL  set Dia=datename(dw,dfecha), diaEnteroCHE = datepart(dw,dfecha)";

            lstTablaXL.Add(s1);
            lstTablaXL.Add(s2);
            lstTablaXL.Add(s3);
            lstTablaXL.Add(s4);
            lstTablaXL.Add(s5);
            lstTablaXL.Add(s6);
            lstTablaXL.Add(s7);
            lstTablaXL.Add(s8);

            return lstTablaXL;
        }

        private static List<string> CargaVariablesOT()
        {
            List<string> lstCargaVariablesOT = new List<string>();

            string s1 = @"ALTER TABLE TablaXL Add
             JMaximaT Int,  
             JminimaT Int,  
             Intervalo Int,  
             NoTIngreso Int,  
             NoTSalida Int,  
             MultaIngreso Int,  
             MultaSalida Int,  
             Tarde Int,  
             Temprano int,  
             MinTarde int,  
             MinTemprano int,  
             AtrasoGracia INT,  
             CobraMulta INT,  
             ValorMulta Float;";

            string s2 = @"UPDATE TablaXL   
             SET JMaximaT = RegAsistencia.JMaximaT,  
             JminimaT =RegAsistencia.JminimaT,  
             Intervalo =RegAsistencia.Intervalo,  
             NoTIngreso =RegAsistencia.NoTIngreso,  
             NoTSalida =RegAsistencia.NoTSalida ,  
             MultaIngreso =RegAsistencia.MultaIngreso ,  
             MultaSalida =RegAsistencia.MultaSalida ,  
             Tarde =RegSobreTiempo.Tarde,  
             Temprano =RegSobreTiempo.Temprano,  
             MinTarde =RegSobreTiempo.MinTarde,  
             MinTemprano =RegSobreTiempo.MinTemprano,  
             AtrasoGracia =RegOtros.AtrasoGracia,  
             CobraMulta =RegOtros.CobraMulta,  
             ValorMulta=RegOtros.ValorMulta 
             FROM TablaXL  INNER JOIN  
                   USERINFO ON TablaXL .UserID = USERINFO.USERID LEFT OUTER JOIN  
                    RegOtros ON USERINFO.RegOtros = RegOtros.Id_RegOtros LEFT OUTER JOIN  
                   RegAsistencia ON USERINFO.RegAsistencia = RegAsistencia.id_RegAsistencia LEFT OUTER JOIN  
                   RegSobreTiempo ON USERINFO.RegSobreTiempo = RegSobreTiempo.id_RegSobreTiempo;";

            string s3 = @"UPDATE TablaXL  
             SET JMaximaT = 1200,  
             JminimaT =120,  
             Intervalo =5,  
             NoTIngreso =0,  
             NoTSalida =0 ,  
             MultaIngreso =60 ,  
             MultaSalida =60  
             WHERE NoTIngreso is null;";

            string s4 = @"UPDATE TablaXL   
             SET Tarde =9,  
             Temprano =5,  
             MinTarde =60,  
             MinTemprano =60  
             WHERE Tarde is null;";

            string s5 = @"UPDATE TablaXL   
             SET AtrasoGracia =1,  
             CobraMulta =0,  
             ValorMulta=0  
             where AtrasoGracia is null;";

            lstCargaVariablesOT.Add(s1);
            lstCargaVariablesOT.Add(s2);
            lstCargaVariablesOT.Add(s3);
            lstCargaVariablesOT.Add(s4);
            lstCargaVariablesOT.Add(s5);

            return lstCargaVariablesOT;
        }

        private static List<string> PoneAsistenciaPuntualidad()
        {
            List<string> lstAsistenciaPuntualidad = new List<string>();

            //' Pone Ausente: Si no hay registro de entrada ni registro salida 
            string s1 = @"UPDATE TablaXL  SET Ausente = diaNormal  
            WHERE ((RegENT Is Null) and (RegSAL Is NULL));";

            #region No marcó Ingreso
            //'Case 2              'Se considera ausente
            string s2 = @"UPDATE TablaXL  SET InicioTrabajo = NULL, Ausente = diaNormal  
            WHERE (RegENT Is Null) AND (Ausente Is Null) AND NoTIngreso=2;";

            //'Case 0              'Se considera Entrada Puntual
            string s3 = @"UPDATE TablaXL  SET InicioTrabajo = Entrada  
            WHERE (RegENT Is Null) AND (Ausente Is Null) AND NoTIngreso = 0;";

            //    'Case Else           'Se considera atrasado con MultaIngreso
            //' Pone Ausente: Si no hay registro de Entrada y la multa por no marcar supera la salida o registro salida
            string s4 = @"UPDATE TablaXL  SET Ausente = diaNormal  
            WHERE ( (ausente Is Null) and (RegENT Is Null)  
            AND (entrada+((cast(MultaIngreso as float) / 1440)) > salida or (entrada+((cast(MultaIngreso as float) / 1440)) > regsal)))
             AND NoTIngreso = 1; ";

            //' Pone MultaIngreso
            string s5 = @"UPDATE TablaXL  SET Atraso=((cast(MultaIngreso as float) / 1440)),  
            InicioTrabajo= case when (Entrada+((cast(MultaIngreso as float) / 1440)) ) > salida or  
            (entrada+((cast(MultaIngreso as float) / 1440)) ) > regsal then  
            null else entrada+((cast(MultaIngreso as float) / 1440)) end  
            WHERE (RegENT Is Null) AND (Ausente Is Null) AND NoTIngreso=1;";

            #endregion
            #region Marcó Entrada Temprano
            //    'Case Temprano
            //'Case 1, 2, 3, 4     'Horas extras o Jornada noche
            string s6 = @"UPDATE TablaXL  SET InicioTrabajo= 
            CASE WHEN ( RegENT+((CAST(MinTemprano as FLOAT)/ 1440)) < Entrada) THEN RegEnt ELSE Entrada END  
            WHERE (not (RegENT Is Null) AND (Entrada >= RegENT) AND ( (Contrato Is Null) or Contrato<>'LOSEP') and idTipoh<>3  
            and Temprano <> 5);";

            //'Case 5              'Se considera entrada puntual
            string s7 = @"UPDATE TablaXL  SET InicioTrabajo = Entrada  
            WHERE ( not (RegENT is null) AND (Entrada >= RegENT) AND ( (Contrato is null) or Contrato<>'LOSEP') and idTipoh<>3  
            and Temprano = 5);";

            //'Marcó Entrada Temprano para LOSEP = Horas ExtraOrdinarias del 60% que se almacenan en HE50
            string s8 = @"UPDATE TablaXL  SET InicioTrabajo = Entrada,  
            HE50 = CASE WHEN convert(smalldatetime, convert(varchar(8),RegENT, 108), 108)<= '06:00:00'   
            then convert(smalldatetime, convert(varchar(8),RegENT, 108), 108)-'06:00:00' ELSE null END  
            WHERE (not (RegENT Is Null) AND (RegENT <= Entrada ) AND Contrato='LOSEP'  and idTipoh<>3 );";

            #endregion
            #region Marcó Entrada Atrasado
            //    ' depende de si está en período de Gracia (minGE) y tiene AtrasoGracia
            //'
            //' Tiene AtrasoGracia, pero No está en período de Gracia
            string s9 = @"UPDATE TablaXL  SET InicioTrabajo = RegENT, 
            Atraso = case when(regEnt > entrada + cast(minge as float) / 1440) THEN RegENT-Entrada ELSE Null END
            WHERE(NOT([RegENT] IS NULL) AND(([Entrada]) < [RegENT]) AND regEnt > entrada + cast(minge as float) / 1440  and idTipoh<>3
            AND AtrasoGracia = 1); ";

            //' Tiene AtrasoGracia y está en período de Gracia
            string s10 = @"UPDATE TablaXL  SET InicioTrabajo = Entrada,  
            Atraso = Null  
            WHERE (  NOT ([RegENT] IS NULL) AND (([Entrada]) < [RegENT]) AND regEnt <= entrada+cast(minge as float)/1440 and idTipoh<>3  
            AND AtrasoGracia=1);";

            //' No tiene AtrasoGracia
            string s11 = @"UPDATE TablaXL  SET InicioTrabajo = RegENT,  
            Atraso = CASE WHEN regEnt > entrada+cast(minge as float)/1440 THEN RegENT-Entrada ELSE Null END  
            WHERE ( NOT ([RegENT] IS NULL) AND (( [Entrada]) < [RegENT]) and idTipoh<>3  
            AND AtrasoGracia=0);";
            #endregion
            #region No Marcó Salida
            //    'Select Case NoTSalida
            //'Case 2              'Se considera ausente
            string s12 = @"UPDATE TablaXL  SET FinTrabajo = NULL, Ausente = diaNormal WHERE (RegSAL Is NULL) AND NoTSalida=2;";

            //'Case 0              'Se considera Salida Puntual
            string s13 = @"UPDATE TablaXL  SET FinTrabajo = Salida WHERE (RegSAL Is NULL) AND NoTSalida = 0;";

            //    'Case Else (NoTSalida = 1)          'Se considera SalidaTemprano con MultaSalida
            //' Pone Ausente: Si no hay registro de Salida y la multa por no marcar supera la entrada o registro de entrada
            string s14 = @"UPDATE TablaXL  SET Ausente = diaNormal  
            WHERE ( (ausente Is Null) and (RegSAL Is Null)  
            AND (Salida-((cast(MultaSalida as float) / 1440)) < Entrada or (Salida-((cast(MultaSalida as float) / 1440)) < RegENT)))
            AND NoTSalida = 1; ";

            //' Pone MultaSalida
            string s15 = @"UPDATE TablaXL  SET SalidaTemprano = ((cast(MultaSalida as float) / 1440)) ,  
            FinTrabajo = CASE when (Salida- ((cast(MultaSalida as float) / 1440)) ) < Entrada or (Salida- ((cast(MultaIngreso as float) / 1440)) ) < RegENT  
            THEN null ELSE Salida- ((cast(MultaSalida as float) / 1440)) END  
            WHERE (RegSAL Is NULL) AND (Ausente Is NULL) AND NoTSalida=1;";
            #endregion
            #region Marcó Salida Tarde
            //    'Case Tarde
            //'Case 6, 7, 8, 9     'Horas extras o Jornada noche
            string s16 = @"UPDATE TablaXL  SET FinTrabajo = 
            CASE WHEN ((Salida+ ((CAST(MinTarde AS FLOAT)/ 1440)) ) > RegSAL) THEN Salida ELSE RegSAL END  
            WHERE (not (RegSAL Is NULL) AND (Salida < RegSAL) and idTipoh<>3  
            and Tarde <> 10);";

            //'Case 10              'Se considera Salida puntual
            string s17 = @"UPDATE TablaXL  SET FinTrabajo = Salida  
            WHERE (not (RegSAL IS NULL) AND (Salida < RegSAL) and idTipoh<>3  
            and Tarde = 10);";
            #endregion

            //'Marcó salida temprano
            //    ' depende de minutos de gracia salida (minGS)
            string s18 = @"UPDATE TablaXL  SET FinTrabajo = RegSAL,  
            SalidaTemprano=CASE WHEN Salida - RegSAL>mings/1440 THEN Salida-RegSAL ELSE null END  
            WHERE (not (RegSAL IS NULL) AND (Salida >= RegSAL) and idTipoh<>3 ) ;";

            //'Pone Tiempo de Asistencia
            string s19 = @"UPDATE TablaXL  SET marca_entrada = 
            CASE WHEN (RegEnt IS NULL) THEN Iniciotrabajo ELSE RegEnt END,  
            marca_salida = CASE WHEN (RegSal Is NULL) then fintrabajo else RegSal end  
            Where (Ausente IS NULL) and idTipoh<>3;";

            lstAsistenciaPuntualidad.Add(s1);
            lstAsistenciaPuntualidad.Add(s2);
            lstAsistenciaPuntualidad.Add(s3);
            lstAsistenciaPuntualidad.Add(s4);
            lstAsistenciaPuntualidad.Add(s5);
            lstAsistenciaPuntualidad.Add(s6);
            lstAsistenciaPuntualidad.Add(s7);
            lstAsistenciaPuntualidad.Add(s8);
            lstAsistenciaPuntualidad.Add(s9);
            lstAsistenciaPuntualidad.Add(s10);
            lstAsistenciaPuntualidad.Add(s11);
            lstAsistenciaPuntualidad.Add(s12);
            lstAsistenciaPuntualidad.Add(s13);
            lstAsistenciaPuntualidad.Add(s14);
            lstAsistenciaPuntualidad.Add(s15);
            lstAsistenciaPuntualidad.Add(s16);
            lstAsistenciaPuntualidad.Add(s17);
            lstAsistenciaPuntualidad.Add(s18);
            lstAsistenciaPuntualidad.Add(s19);

            return lstAsistenciaPuntualidad;
        }
        private static List<string> PoneAtrasosConCargoVacacion(DateTime f1, DateTime f2, string csvUsers)
        {
            List<string> listCV = new List<string>();
            
            string s1 = @"delete from  AtrasoSalidaTemp
            WHERE STARTSPECDAY >= '" + f1.ToString("dd/MM/yyyy HH:mm") + "' And STARTSPECDAY < '" + f2.AddDays(1).ToString("dd/MM/yyyy HH:mm") + @"'
            AND USERID in (" + csvUsers + ");";

            string s2 = @"INSERT INTO [AtrasoSalidaTemp]
            Select UserID, Entrada, Entrada + Atraso, '1111', 'Atraso', dFecha
            From TablaXL XL inner join [Vacations.Parameters] PV on XL.idcontrato = PV.idContrato
            Where Not Atraso Is NULL AND PV.bAtrasosCV = 1;";

            string s3 = @"INSERT INTO [AtrasoSalidaTemp]
            Select UserID, Salida - SalidaTemprano , Salida, '1112', 'Salida Anticipada', dFecha
            From TablaXL XL inner join [Vacations.Parameters] PV on XL.idcontrato = PV.idContrato
            Where Not SalidaTemprano Is NULL AND PV.bSalidasCV = 1;";

            string s4 = @"INSERT INTO [AtrasoSalidaTemp]
            Select UserID, iSalida, iSalida + ExcesoAlmuerzo, '1113', 'Exceso Almuerzo', dFecha
            From TablaXL XL inner join [Vacations.Parameters] PV on XL.idcontrato = PV.idContrato
            Where Not ExcesoAlmuerzo Is NULL AND PV.bExcesoLunchCV = 1;";

            string s5 = @"INSERT INTO [AtrasoSalidaTemp]
            Select UserID, Entrada, Salida, '1114', 'Ausencia injustificada', dFecha
            From TablaXL XL inner join [Vacations.Parameters] PV on XL.idcontrato = PV.idContrato
            Where Ausente > 0 AND pv.bAusenciasCV = 1;";

            listCV.Add(s1);
            listCV.Add(s2);
            listCV.Add(s3);
            listCV.Add(s4);
            listCV.Add(s5);
            return listCV;
        }

        private static List<string> PonePermisosHora2020(DateTime f1, DateTime f2, string csvUsers)
        {
            List<string> lstPonePermisos2020 = new List<string>();

            //'Crea la tabla #PermisosH,
            string s1 = @"SELECT Per.USERID, cast(Per.STARTSPECDAY as smalldatetime) as STARTSPECDAY, cast(Per.ENDSPECDAY as smalldatetime) as ENDSPECDAY, cast((Per.ENDSPECDAY - Per.STARTSPECDAY) as time)as Tiempo, Per.YUANYING,  
            LEAVECLASS.LEAVENAME, LEAVECLASS.classify, LEAVECLASS.[ReportSymbol] as TipoPermiso,  
            XL.MinGE, XL.MinGS, cast(XL.dFecha as Date) as dFecha, XL.Entrada, XL.Salida, 
            XL.Empleado, XL.Departamento, XL.codH, XL.regENT, XL.regSAL  
            INTO PermisosH_Temp
            FROM TablaXL  as XL INNER JOIN (LEAVECLASS RIGHT JOIN USER_SPEDAY AS Per ON LEAVECLASS.LEAVEID = Per.DATEID) 
            ON XL.UserID = Per.USERID  
            WHERE (  
            ( (Per.STARTSPECDAY >= '" + f1.ToString("dd/MM/yyyy HH:mm") + "' And Per.STARTSPECDAY <  '" + f2.AddDays(1).ToString("dd/MM/yyyy HH:mm") + @"') OR 
              (Per.ENDSPECDAY   >  '" + f1.ToString("dd/MM/yyyy HH:mm") + "' And Per.ENDSPECDAY   <= '" + f2.AddDays(1).ToString("dd/MM/yyyy HH:mm") + @"')  ) AND  
            ( (XL.[entrada] >= [Per].[STARTSPECDAY] And XL.[entrada] < [Per].[ENDSPECDAY]) OR
              (XL.[salida]  >  [Per].[STARTSPECDAY] And XL.[salida] <= [Per].[ENDSPECDAY]) OR 
              ([Per].[STARTSPECDAY] >= XL.[entrada] And [Per].[STARTSPECDAY] < XL.[salida]) OR 
              ([Per].[ENDSPECDAY]   >  XL.[entrada] And [Per].[ENDSPECDAY]  <= XL.[salida]) )
            )  
            ORDER BY LEAVECLASS.LEAVENAME, Per.STARTSPECDAY;";

            //'Crea la tabla PermisosH,
            string s2 = @"WITH Totales As  
            (select P2.USERID, CAST(P2.STARTSPECDAY AS DATE) as Fecha, count(P2.startspecday) as Conteo, P2.codH, P2.TipoPermiso
            from PermisosH_Temp   P2  
            group by cast(P2.STARTSPECDAY as date), P2.USERID, P2.codH, P2.TipoPermiso)  
            SELECT ROW_NUMBER() over ( partition by P1.userid, cast(P1.STARTSPECDAY as date) order by P1.startspecday) as Orden, Conteo, P1.*  
            INTO PermisosH   
            FROM PermisosH_Temp   P1 INNER JOIN Totales  
            ON P1.USERID = Totales.USERID AND CAST(P1.STARTSPECDAY AS DATE) = Totales.Fecha and P1.codH = Totales.codH  
            ORDER BY P1.USERID;";

            //'Va a poner permiso en un día que no hay horario fijo u ocasional asignado
            string s3 = @"INSERT INTO PermisosH  (Userid, StartSpecday, EndSpecDay, Tiempo, YuanYing, LeaveName, Classify, dFecha, Empleado, Departamento, codH) 
            SELECT Per.USERID, Per.STARTSPECDAY, Per.ENDSPECDAY, Per.ENDSPECDAY - Per.STARTSPECDAY, Per.YUANYING, 
            LEAVECLASS.LEAVENAME, LEAVECLASS.classify, 
            XL.dFecha, XL.Empleado, XL.Departamento, xl.codH  
            FROM TablaXL  as XL INNER JOIN (LEAVECLASS RIGHT JOIN USER_SPEDAY AS Per ON LEAVECLASS.LEAVEID = Per.DATEID) ON XL.UserID = Per.USERID AND xl.dFecha = cast(Per.STARTSPECDAY as date)  
            WHERE 
            (Per.STARTSPECDAY >= '" + f1.ToString("dd/MM/yyyy HH:mm") + "' And Per.STARTSPECDAY <= '" + f2.AddDays(1).ToString("dd/MM/yyyy HH:mm") + @"') AND XL.Horario is null  
            ORDER BY LEAVECLASS.LEAVENAME, Per.STARTSPECDAY;";

            //    ' Pone Nombre de Permiso,  quita Ausente en el día,  y pone ClasePermiso
            //' Caso Cuando Hay Solo 1 Permiso en el Horario 
            string s4 = @"UPDATE XL  
            SET XL.Ausente=NULL, XL.Permiso=PerH.LeaveName, XL.MotivoPermiso=LEFT(PerH.YuanYing, 49), XL.ClasePermiso=PerH.classify  
            From PermisosH  AS PerH INNER JOIN TablaXL  AS XL  
            ON (XL.dFecha = PerH.dFecha) AND (PerH.codH = XL.codH) AND (PerH.USERID = XL.UserID)";

            //    ' Pone Nombre de Permiso y quita Ausente en el día
            //' Caso Cuando Hay Más de 1 Permiso en el Horario  
            string s5 = @"With PermisosAcum as 
            ( 
                 select userid, dFecha, codH  
                 FROM [PermisosH ] 
                 group by userid, dFecha, codH having count(*) > 1 
            ) 
            UPDATE XL  
            SET XL.Permiso='Varios Permisos', XL.MotivoPermiso='Varios Permisos', XL.ClasePermiso=-1 
            FROM PermisosAcum AS PerAcum INNER JOIN TablaXL  AS XL  
            ON (XL.dFecha = PerAcum.dFecha) AND (PerAcum.codH = XL.codH) AND (PerAcum.USERID = XL.UserID);";

            //    '
            //'SIEMPRE Considera que es permiso trabajado,
            //'FINALMENTE Quita el tiempo de permiso cuando ha sido permiso NO TRABAJADO
            //'Pone Inicio Trabajo = Entrada, Atraso = null, cuando el Permiso abarca la Entrada
            string s6 = @"UPDATE XL  
            SET XL.InicioTrabajo=XL.Entrada, XL.Atraso=NULL  
            FROM PermisosH  AS PerH INNER JOIN TablaXL  AS XL 
            ON (XL.dFecha = PerH.dFecha) AND (PerH.codH = XL.codH) AND (PerH.USERID = XL.UserID)  
            WHERE [PerH].[STARTSPECDAY]<=[PerH].[Entrada] and [PerH].[Entrada]<[PerH].[ENDSPECDAY];";

            //Pone RegEnt = IniPermiso cuando el Permiso NO abarca la Entrada pero no hay InicioTrabajo o InicioTrabajo > IniPermiso
            string s7 = @"UPDATE XL  
            SET XL.InicioTrabajo=[PerH].[STARTSPECDAY], XL.Atraso=[PerH].[STARTSPECDAY] - XL.Entrada  
            FROM PermisosH  AS PerH INNER JOIN TablaXL  AS XL 
            ON (XL.dFecha = PerH.dFecha) AND (PerH.codH = XL.codH) AND (PerH.USERID = XL.UserID)  
            WHERE [PerH].[Entrada]<[PerH].[STARTSPECDAY] and [PerH].[STARTSPECDAY]<[PerH].[Salida] and  
            (XL.InicioTrabajo is NULL or XL.InicioTrabajo > [PerH].[STARTSPECDAY]) AND [PerH].Orden =1;";

            //'Pone RegSAL = Salida cuando el Permiso abarca la Salida (Independiente de marcacion de Entrada)
            string s8 = @"UPDATE XL  
            SET XL.FinTrabajo=XL.Salida, XL.SalidaTemprano=NULL  
            FROM PermisosH  AS PerH INNER JOIN TablaXL  AS XL 
            ON (XL.dFecha = PerH.dFecha) AND (PerH.codH = XL.codH) AND (PerH.USERID = XL.UserID)  
            WHERE [PerH].[STARTSPECDAY]<[PerH].[Salida] and [PerH].[Salida]<=[PerH].[ENDSPECDAY]";

            //    Pone RegSAL = FinPermiso cuando el Permiso NO abarca la Salida(Independiente de marcacion de Entrada)
            //' y no hay RegSalida
            string s9 = @"With PerH as (
                SELECT userId, dFecha, codH, Entrada, Salida, Orden, Conteo, max(EndSpecDay) AS EndSpecDay
                FROM PermisosH 
                Group by userId, dFecha, codH, Entrada, Salida, Orden, Conteo
                )
             UPDATE XL  
            SET XL.FinTrabajo=[PerH].[ENDSPECDAY], XL.SalidaTemprano = XL.Salida - [PerH].[ENDSPECDAY]  
            FROM PerH INNER JOIN TablaXL  AS XL 
            ON (XL.dFecha = PerH.dFecha) AND (PerH.codH = XL.codH) AND (PerH.USERID = XL.UserID)  
            WHERE [PerH].[Entrada]<[PerH].[ENDSPECDAY] AND [PerH].[ENDSPECDAY]<[PerH].[Salida] and  
            (XL.FinTrabajo is NULL or XL.FinTrabajo < [PerH].[ENDSPECDAY])  
            AND [PerH].Orden = [PerH].Conteo;";

            lstPonePermisos2020.Add(s1);
            lstPonePermisos2020.Add(s2);
            lstPonePermisos2020.Add(s3);
            lstPonePermisos2020.Add(s4);
            lstPonePermisos2020.Add(s5);
            lstPonePermisos2020.Add(s6);
            lstPonePermisos2020.Add(s7);
            lstPonePermisos2020.Add(s8);
            lstPonePermisos2020.Add(s9);


            return lstPonePermisos2020;
        }

        private static List<string> PreparaCalculosHE()
        {
            List<string> lstPreparaCalculosHE = new List<string>();

            //    '*****************************************************************************************************************************************************
            //'Para Cálculo del Almuerzo en una sola fila
            //'En la tabla XL 
            //'el campo Almuerzo se refiere al tiempo que el empleado DISPONE para almorzar, es el tiempo de almuerzo que consta en el HORARIO.
            //'el campo TiempoAlmuerzo se refiere al tiempo que el empleado realmente se TOMA para almorzar, se calcula como RegSalALM - RegIngALM
            //'
            string s1 = @"UPDATE TablaXL   
                     SET Marcaciones = registros.conteo  
               FROM ( SELECT XL.UserID, XL.dFecha, XL.codH, COUNT(TablaM.CHECKTIME) AS CONTEO  
              FROM TablaXL  as XL inner join TablaM  TablaM  
              on XL.UserID = TablaM.USERID AND TablaM.CHECKTIME BETWEEN fE and iSalida  
              GROUP BY XL.UserID, XL.dFecha, XL.codH  
               ) registros  
                     WHERE(TablaXL .UserID = registros.UserID And TablaXL .dFecha = registros.dFecha)  
              and TablaXL .codH = registros.codH;";

            string s2 = @"UPDATE TablaXL   
                     SET regIngALM = registros.Primera  
               FROM ( SELECT XL.UserID, XL.dFecha, XL.codH, MIN(TablaM.CHECKTIME) AS Primera  
              FROM TablaXL  as XL inner join TablaM  TablaM  
              on XL.UserID = TablaM.USERID AND TablaM.CHECKTIME BETWEEN fE and iSalida  
              GROUP BY XL.UserID, XL.dFecha, XL.codH  
               ) registros  
                     WHERE(TablaXL .UserID = registros.UserID And TablaXL .dFecha = registros.dFecha)  
              and TablaXL .codH = registros.codH AND Marcaciones >=2;";

            string s3 = @"UPDATE TablaXL   
                     SET regSalALM = registros.Ultima  
               FROM ( SELECT XL.UserID, XL.dFecha, XL.codH, Max(TablaM.CHECKTIME) AS Ultima  
              FROM TablaXL  as XL inner join TablaM  TablaM  
              on XL.UserID = TablaM.USERID AND TablaM.CHECKTIME BETWEEN fE and iSalida  
              GROUP BY XL.UserID, XL.dFecha, XL.codH  
               ) registros  
                     WHERE(TablaXL .UserID = registros.UserID And TablaXL .dFecha = registros.dFecha)  
              and TablaXL .codH = registros.codH AND Marcaciones >=2;";

            string s4 = @"UPDATE TablaXL   
                    SET TiempoAlmuerzo = regSalAlm - regIngAlm;";

            //    'Calcula el Tiempo de Almuerzo
            //'Revisar WHERE ... AND idTipoH<>3
            string s5 = @"UPDATE TablaXL   
            SET TiempoAlmuerzo=Almuerzo  
            WHERE ((TiempoAlmuerzo Is Null) or (Almuerzo > TiempoAlmuerzo)) AND (Almuerzo > 0) and (Ausente Is Null) and idTipoh<>3 AND descuentaAlmuerzo=1;";

            string s6 = @"UPDATE TablaXL   
                    SET ExcesoAlmuerzo = TiempoAlmuerzo - Almuerzo where TiempoAlmuerzo > Almuerzo;";

            //    ' '' ''        'Asistencia = "RegSalida - RegEntrada"
            //' '' ''        'Tiempo de Trabajo = FinTrabajo - IniTrabajo
            //' '' ''        'Revisar si todavía se usa Jornada de Trabajo = HoraSalida - HoraEntrada

            string s7 = @"UPDATE TablaXL  SET TiempoAsistencia=marca_salida- marca_entrada,  
     TiempoTrabajado=FinTrabajo-InicioTrabajo  
     Where (Ausente Is NULL) and idTipoh<>3;"; //        'Calcula el tiempo de ASISTENCIA a la empresa

            //    '        'Asistencia = "RegSalida - RegEntrada"
            //'        'Tiempo de Trabajo = FinTrabajo - IniTrabajo
            //'        'Revisar si todavía se usa Jornada de Trabajo = HoraSalida - HoraEntrada
            string s8 = @"UPDATE TablaXL  SET jornada = CASE WHEN Almuerzo=0 THEN salida-entrada ELSE (salida-entrada)-Almuerzo END  
            WHERE idTipoh<>3;";

            string s9 = @"UPDATE TablaXL  SET tiempoBDD=fintrabajo-iniciotrabajo, DiaTrabajado=diaNormal  
            Where (Ausente is null) AND idTipoh<>3 AND fintrabajo>iniciotrabajo;";

            //    'Aquí empieza a configurar TurnoEnteroCHE
            //'Ve si 2 HORARIO del mismo día se los considera como una sola jornada
            //'' '' ''Ve si 2 HORARIO del mismo día se los considera como una sola jornada
            //'' '' ''turnoenteroCHE = 0 único turno del día
            //'' '' ''turnoenteroCHE = 1 primer turno del día
            //'' '' ''turnoenteroCHE = 3 es un turno intermedio, es decir, Hay un turno en la fila anterior y hay otro turno en la fila siguiente en el mismo día
            //'' '' ''turnoenteroCHE = 2 es el último turno del mismo día de la fila anterior

            // Check int HORARIO_x_Dia != 0
            // Empieza if(HORARIO_x_Dia == 0

            string s9_1 = @"IF OBJECT_ID('TurnoTemp', 'U')     IS NOT NULL DROP Table TurnoTemp ;";

            // 'En esta parte se elimino WHERE and [primera_vez].[overtime]=1
            string s10 = @"SELECT (SELECT COUNT(*)  
                     FROM [TablaXL ] AS [segunda_vez]  
                     Where [segunda_vez].[userid] = [primera_vez].[userid]  
                     and [segunda_vez].[dfecha] = [primera_vez].[dfecha]  
                     and [segunda_vez].[entrada] <= [primera_vez].[entrada]  
                     GROUP BY [segunda_vez].[userid], [segunda_vez].[dfecha]  
                     ) AS Contador, [primera_vez].userid, [primera_vez].dfecha, entrada  
             INTO TurnoTemp   
             FROM TablaXL  AS primera_vez  
             ORDER BY [primera_vez].[userid], [primera_vez].[dfecha], entrada;";
            // '"WHERE (([primera_vez].ausente is null) and [primera_vez].[overtime]=1 )  _

            // 'En esta parte se elimino WHERE AND overtime=1
            string s11 = @"UPDATE TablaXL   
                 set TablaXL .turnoenteroche=turnotemp .Contador  
                 From TablaXL  inner join turnotemp  on TablaXL .userid=turnotemp .userid  
                 and TablaXL .dfecha=turnotemp .dfecha  
                 and TablaXL .entrada=turnotemp .entrada;";
            // '"WHERE ((ausente is null) AND overtime=1 );"

            string s12 = @"DROP Table TurnoTemp ;";

            //
            // Esta linea no se la ejecuta ?!
            // string s13 = @"UPDATE TablaXL  set turnoenteroCHE=0 WHERE overtime=0";
            // Finaliza if(HORARIO_x_Dia == 0

            //    '0 único turno del día
            //'1 primer turno del día
            //'3 es un turno intermedio, es decir, Hay un turno en la fila anterior y hay otro turno en la fila siguiente en el mismo día
            //'2 es el último turno del mismo día de la fila anterior
            string s13 = @"SELECT userid, dfecha, count(*) as Cuenta  
             INTO CuentaTemp   
             from TablaXL  where overtime=1 group by userid, dfecha;";

            string s14 = @"UPDATE TablaXL   
             SET TablaXL .turnoEnteroCHE=CASE WHEN CuentaTemp .cuenta=1 THEN 0 ELSE  
             CASE WHEN TablaXL .turnoenteroche=1 THEN 1 ELSE  
             CASE WHEN TablaXL .turnoenteroche=CuentaTemp .cuenta THEN 2 ELSE 3 END END END  
            FROM TablaXL  inner join CuentaTemp   
             ON TablaXL .userid=CuentaTemp .userid and TablaXL .dfecha=CuentaTemp .dfecha  
              WHERE idTipoh<>3";

            string s15 = @"DROP TABLE CuentaTemp ;";

            string s16 = @"ALTER TABLE TablaXL  add primary key (userid, dFecha, codh);";

            //'Aquí termina de configurar TurnoEnteroCHE

            string s17 = @"UPDATE TablaXL  set turnoenteroCHE=4 where TiempoAlmuerzo>0;";


            lstPreparaCalculosHE.Add(s1);
            lstPreparaCalculosHE.Add(s2);
            lstPreparaCalculosHE.Add(s3);
            lstPreparaCalculosHE.Add(s4);
            lstPreparaCalculosHE.Add(s5);
            lstPreparaCalculosHE.Add(s6);
            lstPreparaCalculosHE.Add(s7);
            lstPreparaCalculosHE.Add(s8);
            lstPreparaCalculosHE.Add(s9);
            lstPreparaCalculosHE.Add(s9_1);
            lstPreparaCalculosHE.Add(s10);
            lstPreparaCalculosHE.Add(s11);
            lstPreparaCalculosHE.Add(s12);
            lstPreparaCalculosHE.Add(s13);
            lstPreparaCalculosHE.Add(s14);
            lstPreparaCalculosHE.Add(s15);
            lstPreparaCalculosHE.Add(s16);
            lstPreparaCalculosHE.Add(s17);

            return lstPreparaCalculosHE;
        }

        private static List<string> Calcula_Horas_Extras()
        {

            List<string> lstCalculaHorasExtras = new List<string>();
            
            // Aquí DistribuyeHoras()            
            

            // Segunda Parte:
            string s1 = @"UPDATE TablaXL  SET tiempotrabajado=tiempotrabajado-(HE50-'04:00:00'), HE50='04:00:00' 
                where HE50>'04:00:00' and IdContrato=2";

            //    'Horario Rotativo: idTipoH=1
            //'HEL: idTipoH=3

            //'Calcula_Horas_Extras cuando
            //'Horario Rotativo (idTipoH=1), es tiempo extra
            string s2 = @"Update TablaXL  SET tiempotrabajado=FinTrabajo-InicioTrabajo-Almuerzo  
            WHERE (Ausente is null) AND (Permiso is NULL) AND idTipoH=1;";

            //    'Revisar WHERE ... AND diaEnteroCHE=0 
            //'Esta parte es cuando los horarios rotativos son de tiempo extra.
            //'Este caso se está tomando al 100%, como día de descanso cuando el empleado gana HE (overtime=1)
            //'No depende del día hábil, que queda solo cuando el empleado no tiene ningún turno fijo ni horario rotativo
            //'idTipoH = 3 es para los feriados con día libre, pero el muy trabajador ha ido a trabajar																								  
            string s3 = @"Update TablaXL  SET Descanso=tiempotrabajado, HorarioNormal=NULL  
            WHERE (Ausente is null) AND (Permiso is NULL) and overtime=1 AND (idTipoH=1 or idTipoH=3);";

            //    'Esta parte es cuando los horarios rotativos son de tiempo extra.
            //'Pero el empleado NO gana HE (overtime=0)
            string s4 = @"Update TablaXL  SET HorarioNormal=tiempotrabajado  
            WHERE (Ausente is null) AND (Permiso is NULL) AND idTipoH=1 and overtime=0;";


            string s5 = @"IF OBJECT_ID('RegDiaHabilVertical', 'U') IS NOT NULL TRUNCATE Table RegDiaHabilVertical;";
                //CREATE TABLE RegDiaHabilVertical 
                //(id_RegDiaHabil int not null, Dia int not null, FinSemana int not null default 0)";

            string s6 = @"INSERT INTO RegDiaHabilVertical  
            select id_regdiaHabil, 1, case when dia1 = 1 and dia2 = 0 then 1 else 0 end  
            from RegDiaHabil; 
            INSERT INTO RegDiaHabilVertical  
            select id_regdiaHabil, 2, case when dia2 = 1 and dia3 = 0 then 1 else 0 end  
            from RegDiaHabil; 
            INSERT INTO RegDiaHabilVertical  
            select id_regdiaHabil, 3, case when dia3 = 1 and dia4 = 0 then 1 else 0 end  
            from RegDiaHabil; 
            INSERT INTO RegDiaHabilVertical  
            select id_regdiaHabil, 4, case when dia4 = 1 and dia5 = 0 then 1 else 0 end  
            from RegDiaHabil; 
            INSERT INTO RegDiaHabilVertical  
            select id_regdiaHabil, 5, case when dia5 = 1 and dia6 = 0 then 1 else 0 end  
            from RegDiaHabil; 
            INSERT INTO RegDiaHabilVertical  
            select id_regdiaHabil, 6, case when dia6 = 1 and dia0 = 0 then 1 else 0 end  
            from RegDiaHabil; 
            INSERT INTO RegDiaHabilVertical  
            select id_regdiaHabil, 0, case when dia0 = 1 and dia1 = 0 then 1 else 0 end  
            from RegDiaHabil;";

            //'En CheckIn pone si el día es víspera de un día de descanso o nó
            string s7 = @"update xl  
            SET xl.checkIn = RegDiaHabilVertical.finsemana  
            from TablaXL  as xl inner join userinfo on xl.userid = userinfo.userid  
            inner join RegDiaHabilVertical on userinfo.regdiahabil = RegDiaHabilVertical.id_RegDiaHabil   
            and xl.diaEnteroCHE = RegDiaHabilVertical.Dia";

            // 'En CheckOut pone si La Hora Extra en el día de descanso con inicio en Día Hábil se le considera de Descanso o en Día Hábil
            string s8 = @"UPDATE xl  
            SET checkOut = RegSobreTiempo.HEDiaLaborable  
            FROM (TablaXL  AS xl INNER JOIN USERINFO ON xl.UserID = USERINFO.USERID)  
            INNER JOIN RegSobreTiempo ON USERINFO.RegSobreTiempo = RegSobreTiempo.HEDiaLaborable;";

            string s9 = @"update TablaXL   
            SET HE100 = HE100 + (FinTrabajo - dateadd(HOUR,6,DATEADD(DAY, 1, dFecha))),  
            HE50 = HE50 - (FinTrabajo - DateAdd(Hour, 6, DateAdd(Day, 1, dFecha)))  
            where FinTrabajo > dateadd(HOUR,6,DATEADD(DAY, 1, dFecha)) AND HE50 > 0 AND HE100 > 0 AND checkIn =1 AND CheckOut=1;";

            if(consultaXL != "")
            {
                lstCalculaHorasExtras.Add(consultaXL);
            }

            lstCalculaHorasExtras.Add(s1);
            lstCalculaHorasExtras.Add(s2);
            lstCalculaHorasExtras.Add(s3);
            lstCalculaHorasExtras.Add(s4);
            lstCalculaHorasExtras.Add(s5);
            lstCalculaHorasExtras.Add(s6);
            lstCalculaHorasExtras.Add(s7);
            lstCalculaHorasExtras.Add(s8);
            lstCalculaHorasExtras.Add(s9);

            return lstCalculaHorasExtras;
        }

        private static List<string> QuitaTiempoAlmuerzo()
        {
            List<string> lstQuitaTiempoAlmuerzo = new List<string>();

            //    'Quita el tiempo de almuerzo del tiempo trabajado
            //'Revisar WHERE ... AND idTipoH<>3
            string s1 = @"Update TablaXL   
            SET tiempotrabajado=CASE WHEN FinTrabajo-InicioTrabajo-almuerzo<0 THEN null ELSE FinTrabajo-InicioTrabajo-almuerzo END  
            WHERE Almuerzo > 0 and (Ausente Is Null) and (idTipoH=0 or idTipoH=1) AND (TiempoAlmuerzo Is Null) and (descuentaAlmuerzo=1);";

            string s2 = @"Update TablaXL   
            SET tiempotrabajado=CASE WHEN FinTrabajo-InicioTrabajo-TiempoAlmuerzo<0 THEN null ELSE FinTrabajo-InicioTrabajo-TiempoAlmuerzo END  
            WHERE TiempoAlmuerzo > 0 and (Ausente Is Null) and (idTipoH=0 or idTipoH=1) AND (Not TiempoAlmuerzo Is Null);";

            //    'Quita el tiempo de almuerzo del tiempo trabajado, hay 3 casos: 
            //'Caso 1 cuando Trabaja en el día (JornadaNocturna=0 y HorarioNormal > 0)
            //'Es INDEPENDIENTE del Tipo de contrato (Losep o código Trabajo)
            //'REVISAR Si DEPENDE del tipo de contrato, LOSEP o Código del Trabajo, Código del Trabajo por si el empleado trabajó en horario nocturno
            string s3 = @"Update TablaXL   
            SET HorarioNormal=CASE WHEN HorarioNormal-almuerzo > 0 THEN HorarioNormal-almuerzo ELSE 0 END  
            WHERE (Almuerzo> 0 and (Ausente is null) AND (HorarioNormal > 0) and (JornadaNocturna=0) and (idTipoH=0 or idTipoH=1)  AND (TiempoAlmuerzo Is Null) and (descuentaAlmuerzo=1) );";

            string s4 = @"Update TablaXL   
            SET HorarioNormal=CASE WHEN HorarioNormal-TiempoAlmuerzo > 0 THEN HorarioNormal-TiempoAlmuerzo ELSE 0 END  
            WHERE (TiempoAlmuerzo> 0 and (Ausente is null) AND (HorarioNormal > 0) and (JornadaNocturna=0) and (idTipoH=0 or idTipoH=1)  AND (Not TiempoAlmuerzo Is Null) );";

            //    'Caso 2 y cuando Trabaja en la noche(JornadaNocturna > 0 y HorarioNormal = 0)
            //'REVISAR and Contrato<>'LOSEP' 
            string s5 = @"Update TablaXL  SET JornadaNocturna=CASE WHEN JornadaNocturna-almuerzo > 0 THEN JornadaNocturna-almuerzo ELSE 0 END  
            WHERE ((Almuerzo > 0) and (Ausente is null) and (HorarioNormal = 0) and (JornadaNocturna>0) and (idTipoH=0 or idTipoH=1) AND (TiempoAlmuerzo Is Null) and (descuentaAlmuerzo=1) );";

            string s6 = @"Update TablaXL  SET JornadaNocturna=CASE WHEN JornadaNocturna-TiempoAlmuerzo > 0 THEN JornadaNocturna-TiempoAlmuerzo ELSE 0 END  
            WHERE ((TiempoAlmuerzo > 0) and (Ausente is null) and (HorarioNormal = 0) and (JornadaNocturna>0) and (idTipoH=0 or idTipoH=1) AND (Not TiempoAlmuerzo Is Null)  );";

            //    'Caso 3 y cuando trabaja en el día y en la noche pero la jornada nocturna no alcanza a cubrir el Horario Normal
            //'cuando JornadaNocturna>0 y HorarioNormal > 0,
            string s7 = @"Update TablaXL  SET HorarioNormal= 
            CASE WHEN JornadaNocturna>cast(almuerzo as smalldatetime) THEN HorarioNormal ELSE  
            CASE WHEN JornadaNocturna+HorarioNormal>cast(almuerzo as smalldatetime) THEN JornadaNocturna+HorarioNormal-cast(almuerzo as smalldatetime) ELSE 0 END END,  
            JornadaNocturna= 
            CASE WHEN JornadaNocturna>cast(almuerzo as smalldatetime) THEN JornadaNocturna-cast(almuerzo as smalldatetime) ELSE 0 END  
            WHERE ( (Almuerzo > 0) and (Ausente is null) and (HorarioNormal > 0) and (JornadaNocturna>0) and (idTipoH=0 or idTipoH=1) AND (TiempoAlmuerzo Is Null) and (descuentaAlmuerzo=1) );";

            string s8 = @"Update TablaXL  SET HorarioNormal= 
            CASE WHEN HorarioNormal >= JornadaNocturna AND HorarioNormal > cast(TiempoAlmuerzo as smalldatetime) THEN HorarioNormal-cast(almuerzo as smalldatetime)  
            WHEN HorarioNormal >= JornadaNocturna AND HorarioNormal < cast(TiempoAlmuerzo as smalldatetime) THEN 0  
            WHEN HorarioNormal < JornadaNocturna AND JornadaNocturna < cast(TiempoAlmuerzo as smalldatetime) THEN 0 ELSE HorarioNormal END,  
            JornadaNocturna= 
            CASE WHEN HorarioNormal < JornadaNocturna AND JornadaNocturna > cast(TiempoAlmuerzo as smalldatetime) THEN JornadaNocturna-cast(almuerzo as smalldatetime)  
            WHEN HorarioNormal >= JornadaNocturna AND HorarioNormal < cast(TiempoAlmuerzo as smalldatetime) THEN 0  
            WHEN HorarioNormal < JornadaNocturna AND JornadaNocturna < cast(TiempoAlmuerzo as smalldatetime) THEN 0 ELSE JornadaNocturna END  
            WHERE ( (TiempoAlmuerzo > 0) and (Ausente is null) and (HorarioNormal > 0) and (JornadaNocturna>0) and (idTipoH=0 or idTipoH=1) AND (Not TiempoAlmuerzo Is Null) );";

            ////    '**************************************************************************************************************************************************
            ////'Se aumenta where Contrato=LOSEP
            ////'Se necesita otro update de HorarioNormal para LOSEP, porque ya no depende de JornadaNocturna=0, porque en LOSEP JornadaNocturna es HE25%
            ////'Revisar lo que pasa cuando contrato = 'LOSEP' o 'Código del Trabajo'
            ////'**************************************************************************************************************************************************
            //string s9 = @"Update TablaXL  SET HorarioNormal = CASE WHEN HorarioNormal>almuerzo  THEN HorarioNormal-almuerzo ELSE 0 END  
            //WHERE ((Almuerzo > 0)  and (Ausente is null) and (HorarioNormal > 0) and Contrato='LOSEP' and (idTipoH=0 or idTipoH=1) );";

            ////    'Quita el tiempo de almuerzo de la Jornada Nocturna
            ////'Es INDEPENDIENTE del Tipo de contrato (Losep, Código Trabajo u Otro)
            //string s10 = @"Update TablaXL  SET JornadaNocturna = CASE WHEN JornadaNocturna>almuerzo THEN JornadaNocturna-almuerzo ELSE 0 END  
            //WHERE ((Almuerzo > 0)  and (Ausente is null) and (HorarioNormal = 0) and (JornadaNocturna> 0) AND Contrato<>'LOSEP' and (idTipoH=0 or idTipoH=1) );";

            ////'Trabaja en el día y en la noche, revisar si depende del tipo de contrato
            //string s11 = @"Update TablaXL  SET HorarioNormal = CASE WHEN JornadaNocturna>almuerzo THEN HorarioNormal ELSE  
            //CASE WHEN JornadaNocturna+HorarioNormal>almuerzo THEN JornadaNocturna+HorarioNormal-almuerzo ELSE 0 END END,  
            //JornadaNocturna=CASE WHEN JornadaNocturna>almuerzo THEN JornadaNocturna-almuerzo ELSE 0 END  
            //WHERE ( (Almuerzo > 0) and (Ausente is null) and (HorarioNormal > 0) and (JornadaNocturna>0) and Contrato<>'LOSEP' and (idTipoH=0 or idTipoH=1) );";

            //'Pone TiempoTrabajado
            string s9 = @"Update TablaXL  SET tiempotrabajado=CASE WHEN Descanso=0 THEN HorarioNormal+JornadaNocturna+HE50+HE100 ELSE descanso END  
            WHERE Almuerzo > 0 and (Ausente IS NULL) and (idTipoH=0 or idTipoH=1);";


            lstQuitaTiempoAlmuerzo.Add(s1);
            lstQuitaTiempoAlmuerzo.Add(s2);
            lstQuitaTiempoAlmuerzo.Add(s3);
            lstQuitaTiempoAlmuerzo.Add(s4);
            lstQuitaTiempoAlmuerzo.Add(s5);
            lstQuitaTiempoAlmuerzo.Add(s6);
            lstQuitaTiempoAlmuerzo.Add(s7);
            lstQuitaTiempoAlmuerzo.Add(s8);
            lstQuitaTiempoAlmuerzo.Add(s9);
            //lstQuitaTiempoAlmuerzo.Add(s10);
            //lstQuitaTiempoAlmuerzo.Add(s11);
            //lstQuitaTiempoAlmuerzo.Add(s12);

            return lstQuitaTiempoAlmuerzo;
        }

        private static List<string> PoneTiempoPermisosAcumulados()
        {
            List<string> lstPermisosAcumulados = new List<string>();

            //    ' classify  Tipo de Permiso
            //'   0       Justificado (Originalmente era No Trabajado, y en las siguientes versiones recibirá un trato diferente, pero por ahora, contendrá a Vacaciones y Cargo a Vacaciones
            //'   1       Vacaciones
            //'   2       Cargo a Vacaciones
            //'   128     Trabajado
            string s1 = @"With PermisosAcumNT as
            (
             select userid, dFecha, codH, sum(cast((endspecday - startspecday) as float)) as TiempoPermisos
             FROM [PermisosH] 
             where classify <> 128 
             group by userid, dFecha, codH
            )
            UPDATE XL 
            SET XL.TiempoPermisoNT = PerAcum.TiempoPermisos
            FROM PermisosAcumNT AS PerAcum INNER JOIN TablaXL AS XL
            ON (XL.dFecha = PerAcum.dFecha) AND (PerAcum.codH = XL.codH) AND (PerAcum.USERID = XL.UserID);";

            string s2 = @"UPDATE [TablaXL] 
            SET TiempoPermisoNT = '08:00'
            WHERE TiempoPermisoNT >= '07:59';";

            string s3 = @"With PermisosAcumT as
            (
             select userid, dFecha, codH, sum(cast((endspecday - startspecday) as float)) as TiempoPermisos
             FROM [PermisosH] 
             where classify = 128
             group by userid, dFecha, codH
            )
            UPDATE XL 
            SET XL.TiempoPermisoT = PerAcum.TiempoPermisos
            FROM PermisosAcumT AS PerAcum INNER JOIN TablaXL AS XL
            ON (XL.dFecha = PerAcum.dFecha) AND (PerAcum.codH = XL.codH) AND (PerAcum.USERID = XL.UserID);";

            string s4 = @"UPDATE [TablaXL] 
            SET TiempoPermisoT = '08:00'
            WHERE TiempoPermisoT >= '07:59';";


            lstPermisosAcumulados.Add(s1);
            lstPermisosAcumulados.Add(s2);
            lstPermisosAcumulados.Add(s3);
            lstPermisosAcumulados.Add(s4);

            return lstPermisosAcumulados;
        }

        private static List<string> QuitaPermisosHora2020()
        {
            List<string> lstQuitaPermisos = new List<string>();

            //    '
            //'   Quita tiempo Permiso, Pone Tiempo Trabajo = Tiempo Trabajado - TiempoPermiso NT, 
            //'   Pone HorarioNormal = HorarioNormal - TiempoPermiso NT)
            //'   Permiso NT = [classify] <> 128
            //'
            //'   classify  Tipo de Permiso
            //'   0       Justificado (Originalmente era No Trabajado, y en las siguientes versiones recibirá un trato diferente, pero por ahora, contendrá a Vacaciones y Cargo a Vacaciones
            //'   1       Vacaciones
            //'   2       Cargo a Vacaciones
            //'   128     Trabajado
            //'   Cuando el permiso No Trabajado No cubre las 8 horas
            string s1 = @"UPDATE TablaXL 
            SET TiempoTrabajado = case when TiempoTrabajado > 0  and TiempoPermisoNT < TiempoTrabajado then TiempoTrabajado - TiempoPermisoNT 
                                        when TiempoTrabajado > 0  and TiempoPermisoNT > TiempoTrabajado then 0 end
            where not TiempoPermisoNT is null and TiempoPermisoNT < '08:00'; ";

            //        '   Cuando el permiso No Trabajado Cubre las 8 horas
            //'   No hay tiempo trabajado, ni normal, ni nocturna, ni nada,
            string s2 = @"UPDATE TablaXL 
            SET HE50 =NULL, HE100 = Null, HorarioNormal = Null, JornadaNocturna = Null, TiempoTrabajado = Null, Descanso = Null , TiempoAlmuerzo = Null
            where Not TiempoPermisoNT Is null And TiempoPermisoNT >= '08:00'; ";

            //'Pone Tiempo TotalNoLaborado
            string s3 = @"UPDATE TablaXL 
            SET TotalNoLaborado=cast( case when Atraso is NULL then '0' else cast(Atraso as float) end + 
            case when SalidaTemprano is NULL then '0' else cast(SalidaTemprano as float) end + 
            case when ExcesoAlmuerzo is NULL then '0' else cast(ExcesoAlmuerzo as float) end as datetime) 
            WHERE not Ausente is NULL;";

            string s4 = @"UPDATE TablaXL 
            SET TotalNoLaborado = jornada 
            WHERE Ausente = 1;";

            //    'Pone TiempoNoCumplido
            //'El Tiempo No Cumplido es la duración de jornada menos tiempo trabajado, cuando tiempo trabajado es menor que la jornada
            //'Tiempo No Laborado es la suma de atrasos, salidas temprano y exceso almuerzos
            //'La diferencia entre estos 2 es que si alguien llega atrasado pero se queda después de su hora de salida, puede tener
            //'Tiempo no cumplido = 0 y Tiempo NO laborado > 0
            string s5 = @"update TablaXL 
            SET TiempoNoCumplido = Jornada - TiempoTrabajado 
            where Jornada > TiempoTrabajado;";


            lstQuitaPermisos.Add(s1);
            lstQuitaPermisos.Add(s2);
            lstQuitaPermisos.Add(s3);
            lstQuitaPermisos.Add(s4);
            lstQuitaPermisos.Add(s5);

            return lstQuitaPermisos;
        }

        private static List<string> DistribuyeHorasEnPermiso()
        {
            List<string> lstDistribuyeHorasEnPermiso = new List<string>();

            string s1 = @"";
            string s2 = @"";
            string s3 = @"";
            string s4 = @"";
            string s5 = @"";
            string s6 = @"";
            string s7 = @"";
            string s8 = @"";
            string s9 = @"";
            string s10 = @"";
            string s11 = @"";
            string s12 = @"";
            string s13 = @"";
            string s14 = @"";
            string s15 = @"";
            string s16 = @"";
            string s17 = @"";
            string s18 = @"";

            lstDistribuyeHorasEnPermiso.Add(s1);
            lstDistribuyeHorasEnPermiso.Add(s2);
            lstDistribuyeHorasEnPermiso.Add(s3);
            lstDistribuyeHorasEnPermiso.Add(s4);
            lstDistribuyeHorasEnPermiso.Add(s5);
            lstDistribuyeHorasEnPermiso.Add(s6);
            lstDistribuyeHorasEnPermiso.Add(s7);
            lstDistribuyeHorasEnPermiso.Add(s8);
            lstDistribuyeHorasEnPermiso.Add(s9);
            lstDistribuyeHorasEnPermiso.Add(s10);
            lstDistribuyeHorasEnPermiso.Add(s11);
            lstDistribuyeHorasEnPermiso.Add(s12);
            lstDistribuyeHorasEnPermiso.Add(s13);
            lstDistribuyeHorasEnPermiso.Add(s14);
            lstDistribuyeHorasEnPermiso.Add(s15);
            lstDistribuyeHorasEnPermiso.Add(s16);
            lstDistribuyeHorasEnPermiso.Add(s17);
            lstDistribuyeHorasEnPermiso.Add(s18);

            return lstDistribuyeHorasEnPermiso;
        }

        private static List<string> Redondea_Horas_Extras(RedondeoModel redondeoModel)
        {
            List<string> lstRedondeaHE = new List<string>();

            string s1 = @"";

            if (redondeoModel.RedondeoDir == 1 && redondeoModel.RedondeoA == 1)
                    {
                s1 = @"SET HE50 = DATEADD(hour, DATEDIFF(hour, 0, DATEADD(mi, 60, HE50)), 0),  
                HE100 = DATEADD(hour, DATEDIFF(hour, 0, DATEADD(mi, 60, HE100)), 0), 
                Descanso = DATEADD(hour, DATEDIFF(hour, 0, DATEADD(mi, 60, Descanso)), 0);"; // --as ArribaHoras
                }            
            else if( redondeoModel.RedondeoDir == 1 && redondeoModel.RedondeoA == 2)
                    {
                s1 = @"SET HE50 = DATEADD(mi, ((DATEDIFF(MI, 0, HE50) / 30)+1)*30, 0), 
                HE100 = DATEADD(mi, ((DATEDIFF(MI, 0, HE100) / 30)+1)*30, 0), 
                Descanso = DATEADD(mi, ((DATEDIFF(MI, 0, Descanso) / 30)+1)*30, 0);"; // --as ArribaMediasHoras, 
            }
            else if ( redondeoModel.RedondeoDir == 1 && redondeoModel.RedondeoA == 3 )
            {
                s1 = @"SET HE50 = DATEADD(mi, DATEDIFF(mi, 0, DATEADD(s, 60, HE50)), 0), 
                HE100 = DATEADD(mi, DATEDIFF(mi, 0, DATEADD(s, 60, HE100)), 0), 
                Descanso = DATEADD(mi, DATEDIFF(mi, 0, DATEADD(s, 60, Descanso)), 0);"; // --as ArribaMinutos,
            }
            else if ( redondeoModel.RedondeoDir == 0 && redondeoModel.RedondeoA == 1 )
            {
                s1 = @"SET HE50 = DATEADD(hour, DATEDIFF(hour, 0, DATEADD(mi, 30, HE50)), 0), 
                HE100 = DATEADD(hour, DATEDIFF(hour, 0, DATEADD(mi, 30, HE100)), 0), 
                Descanso = DATEADD(hour, DATEDIFF(hour, 0, DATEADD(mi, 30, Descanso)), 0);"; // --as CercanoHoras,
            }
            else if ( redondeoModel.RedondeoDir == 0 && redondeoModel.RedondeoA == 2 )
            {
                s1 = @"SET HE50 = DATEADD(mi, (DATEDIFF(MI, 0, DATEADD(mi,15,HE50)) / 30)*30, 0), 
                HE100 = DATEADD(mi, (DATEDIFF(MI, 0, DATEADD(mi,15,HE100)) / 30)*30, 0), 
                Descanso = DATEADD(mi, (DATEDIFF(MI, 0, DATEADD(mi,15,Descanso)) / 30)*30, 0);"; // --as CercanoMediasHoras, 
                    }
            else if ( redondeoModel.RedondeoDir == 0 && redondeoModel.RedondeoA == 3 )
            {
                s1 = @"SET HE50 = DATEADD(mi, DATEDIFF(mi, 0, DATEADD(s, 30, HE50)), 0), 
                HE100 = DATEADD(mi, DATEDIFF(mi, 0, DATEADD(s, 30, HE100)), 0), 
                Descanso = DATEADD(mi, DATEDIFF(mi, 0, DATEADD(s, 30, Descanso)), 0);"; // --as CercanoMinutos,
                    }
            else if ( redondeoModel.RedondeoDir == -1 && redondeoModel.RedondeoA == 1 )
            {
                s1 = @"SET HE50 = DATEADD(hour, DATEDIFF(hour, 0, HE50), 0), 
                HE100 = DATEADD(hour, DATEDIFF(hour, 0, HE100), 0), 
                Descanso = DATEADD(hour, DATEDIFF(hour, 0, Descanso), 0);"; // --as AbajoHoras,
                    }
            else if ( redondeoModel.RedondeoDir == -1 && redondeoModel.RedondeoA == 2 )
            {
                s1 = @"SET HE50 = DATEADD(mi, (DATEDIFF(MI, 0, HE50) / 30)*30, 0), 
                HE100 = DATEADD(mi, (DATEDIFF(MI, 0, HE100) / 30)*30, 0), 
                Descanso = DATEADD(mi, (DATEDIFF(MI, 0, Descanso) / 30)*30, 0);"; // --as AbajoMediasHoras, 
                    }
            else if ( redondeoModel.RedondeoDir == -1 && redondeoModel.RedondeoA == 3 )
            {
                s1 = @"SET HE50 = DATEADD(mi, DATEDIFF(mi, 0, HE50), 0), 
                HE100 = DATEADD(mi, DATEDIFF(mi, 0, HE100), 0), 
                Descanso = DATEADD(mi, DATEDIFF(mi, 0, Descanso), 0);"; // --as AbajoMinutos,         
            }   
          
            if(s1 != "")
            {
                s1 = "Update TablaXL " + s1;
                lstRedondeaHE.Add(s1);
            }            

            return lstRedondeaHE;
        }
        private static List<string> Valora_Horas_Extras()
        {
            List<string> lstValoraHE = new List<string>();

            //'Primero cuenta los atrasos y las Salidas Tempranas
            string s1 = @"Update TablaXL SET numAtrasos= CASE WHEN Atraso Is NULL THEN 0 ELSE 1 END, 
            numSalidasTemprano= CASE WHEN SalidaTemprano Is NULL THEN 0 ELSE 1 END;";

            string s2 = @"Update XL  
            SET XL.USDNocturna = CAST(XL.JornadaNocturna AS FLOAT) * 24 * GS.JN25, 
            XL.USD50 = CAST(XL.HE50 AS FLOAT) * 24 * GS.HE50, 
            XL.USD100 = (CAST( ISNULL(XL.HE100,0) AS FLOAT) + CAST(ISNULL(XL.Descanso,0) AS FLOAT)) * 24 * GS.HE100  
            FROM TablaXL AS XL inner join userinfo U on XL.userid = U.userid 
            inner join GrupoSalarial GS on U.idGrupoSalarial = GS.idGrupo;";

            string s3 = @"Update XL  
            SET MultaAtrasos = 
            case when Atraso <= '1900-01-01 00:15:00' then Mu.menos15 * GS.Sueldo / 100 
			             when Atraso > '1900-01-01 00:30:00' then Mu.mas30 * GS.Sueldo / 100 
			             else Mu.entre15_30 * GS.Sueldo / 100 end
            FROM TablaXL AS XL inner join userinfo U on XL.userid = U.userid 
                        inner join GrupoSalarial GS on U.idGrupoSalarial = GS.idGrupo
			            inner join RegMultaAtraso Mu on U.RegMultaAtraso = Mu.Id
            where Atraso > 0;";

            string s4 = @"Update TablaXL 
            SET USDTotal = isnull(USD50, 0) + isnull(USD100, 0) + isnull(USDNocturna, 0) - isnull(MultaAtrasos, 0) - isnull(MultaAusencias, 0);";

            string s5 = @"Update TablaXL SET 
            USDTotal = case when USDTotal <> 0 THEN USDTotal else null END,
            USDNocturna = case when USDNocturna > 0 THEN USDNocturna else null END,
            USD50 = case when USD50 > 0 THEN USD50 else null END,
            USD100 = case when USD100 > 0 THEN USD100 else null END";

            string s6 = @"Update TablaXL 
            SET JornadaNocturna = case when JornadaNocturna > 0 then JornadaNocturna else null end,
	        HE50 = case when HE50 > 0 then HE50 else null end,
	        HE100 = case when HE100 > 0 then HE100 else null end,
	        Descanso = case when Descanso > 0 then Descanso else null end;";

            lstValoraHE.Add(s1);
            lstValoraHE.Add(s2);
            lstValoraHE.Add(s3);
            lstValoraHE.Add(s4);
            lstValoraHE.Add(s5);
            lstValoraHE.Add(s6);

            return lstValoraHE;
        }
        private static List<string> LlenaReporte(DateTime f1, DateTime f2, string csvUsers)
        {
            List<string> lstReporte = new List<string>();
            // int iCharsHora = 5;

            string s1 = "DELETE From rptAsistencia Where UserId in (" + csvUsers + ") AND Fecha between '" + f1.ToString("dd/MM/yyyy") + "' AND '" +  f2.ToString("dd/MM/yyyy") + @"';
            IF OBJECT_ID('rptAsistencia5', 'U')     IS NOT NULL Drop Table rptAsistencia5;
            IF OBJECT_ID('RptRsmAsistencia', 'U')   IS NOT NULL Drop Table RptRsmAsistencia;
            IF OBJECT_ID('RptRsmDepartamento', 'U') IS NOT NULL Drop Table RptRsmDepartamento;";

            string s2 = @"UPDATE XL 
            SET XL.Departamento = departments.deptname, Cargo = userinfo.title, XL.Contrato = CONTRATO.nomContrato, 
            XL.Dia = case DATEPART(DW, XL.dfecha) when 1 then 'Lunes' when 2 then 'Martes' when 3 then 'Miércoles' when 4 then 'Jueves'   when 5 then 'Viernes' when 6 then 'Sábado' when 7 then 'domingo' end,
            XL.Empleado = userinfo.name, XL.NumCA = userinfo.badgenumber, XL.Sueldo = userinfo.sueldo, XL.Cedula = userinfo.ssn
            FROM departments INNER JOIN (TablaXL as XL INNER JOIN userinfo
            ON XL.userid = userinfo.userid) LEFT join CONTRATO
            on userinfo.idContrato = CONTRATO.idContrato ON departments.deptid = userinfo.defaultdeptid;";

            //'Grid Reporte rptAsistencia desde Tabla XL
            string s3 = @"INSERT INTO [dbo].[rptAsistencia]
( [UserId], [NumCA], [Empleado], [Cedula], [Departamento], [Cargo]
,[Fecha],[Dia]
,[codh],[Horario],[HoraEntrada],[HoraSalida]
,[RegEntrada],[RegSalida]
,[SalidaAlmuerzo],[RegresoAlmuerzo],[TiempoAlmuerzo],[ExcesoAlmuerzo]
,[Atrasos],[SalidasTemprano]
,[DiaNormal],[DiaTrabajado],[DiaAusente]
,[HNormal],[Suplem 25%],[ExtraOrd],[Extra 100%],[DiaLibre],[TotalHE100]
,[Permiso],[MotivoPermiso],[TPermisoTrab],[TPermisoNoTrab]
,[TTrabajado],[TAsistido],[TNoCumplido]
,[USD Suplem],[USD ExOrd],[USD 100%],[USD Total]
,[MultaAtrasos],[MultaAusencias]
,[AprobadoHE50],[AprobadoHE100],[T Aprobado HE50],[T Aprobado HE100],[T Aprobado DiaLibre]
,[A USD 50%],[A USD 100%],[A USD TOTAL]
,[Motivo],[Autorizado] )
            SELECT xl.UserId, xl.NumCA, xl.Empleado, xl.Cedula, xl.Departamento, xl.Cargo,  
            cast(xl.dFecha as Date) as Fecha, xl.Dia, 
            xl.codh, xl.Horario, 
            CAST( xl.Entrada As time(0))  as HoraEntrada,  
            CAST( xl.Salida As time(0))   as HoraSalida,  
            CAST( xl.regENT As time(0)) as RegEntrada, CAST( xl.regSAL As time(0)) as RegSalida,  
            CAST( xl.regIngALM As time(0)) as SalidaAlmuerzo, CAST( xl.regsalALM As time(0)) as RegresoAlmuerzo, 
            xl.TiempoAlmuerzo, xl.ExcesoAlmuerzo, 
            xl.Atraso as Atrasos,  xl.SalidaTemprano As SalidasTemprano, 
            xl.DiaNormal, xl.DiaTrabajado, xl.Ausente As DiaAusente, 
            xl.HorarioNormal As HNormal,  xl.JornadaNocturna As  [Suplem 25%], xl.HE50 As [ExtraOrd],  xl.HE100 As [Extra 100%], 
            xl.Descanso As DiaLibre,              
            (isNull(cast(cast(xl.HE100 as datetime) as float), 0)  + isNull(cast(cast(xl.Descanso as datetime) as float), 0) 
	            ) * 24 AS TotalHE100,
            xl.Permiso, xl.MotivoPermiso, xl.TiempoPermisoT As TPermisoTrab, xl.TiempoPermisoNT As TPermisoNoTrab, 
            xl.TiempoTrabajado As TTrabajado, xl.TiempoAsistencia As TAsistido, xl.TiempoNoCumplido As TNoCumplido, 
            convert(money, xl.USDNocturna) as [USD Suplem],  
            convert(money, xl.USD50) as [USD ExOrd], convert(money, xl.USD100) as [USD 100%],  
            convert(money, xl.USDTotal) as [USD Total], 
            convert(money, xl.MultaAtrasos) as MultaAtrasos, 
            convert(money, xl.MultaAusencias) as MultaAusencias,                         
            he.AprobadoHE50, he.AprobadoHE100, 
            he.TAHE50 As [T Aprobado HE50], he.TAHE100 As [T Aprobado HE100], he.TADD As [T Aprobado DiaLibre], 
            he.AUsd50 As [A USD 50%], he.AUsd100 as [A USD 100%], he.AUsdtotal as [A USD TOTAL], 
            he.Motivo, he.Autorizado 
            FROM TablaXL xl left outer join AprobacionHE he on he.userid=xl.userid and he.codh=xl.codh and he.fecha=xl.dfecha 
            ORDER BY xl.Empleado, xl.NumCA, xl.dFecha, xl.Entrada;";

            string s4 = @"UPDATE rptAsistencia 
            set codh = -2 
            from rptAsistencia as rpt inner join SchClass on rpt.codh = schclass.schClassid 
            where Tipo>=24";

            string s5 = @"WITH T as 
            ( 
            SELECT UserID, turnoEnteroCHE, NumCA, Empleado, Dia, cast(dFecha as Date) as Fecha, Permiso, regENT, regSAL, Atraso, SalidaTemprano,  
            HorarioNormal, JornadaNocturna, HE50, HE100, Descanso, TiempoTrabajado, TiempoAsistencia  
            FROM TablaXL 
            WHERE turnoEnteroCHE = 2 
            ) 
            SELECT M.UserID, M.turnoEnteroCHE, M.NumCA, M.Empleado, M.Dia, cast(M.dFecha as Date) as Fecha, 
            'Dos Jornadas' as Horario, coalesce(T.Permiso, M.Permiso) as Permiso, convert(char(8), M.regENT, 108) as RegEnt, convert(char(8), M.regSAL , 108) as regSAL, 
            T.turnoEnteroCHE as turnoEnteroCHE2, 
             convert(char(8), T.regENT, 108) as RegEntrada2, convert(char(8), T.regSAL, 108) as RegSalida2, 
             isnull(M.Atraso,0) + isnull(T.Atraso,0) as Atraso, isnull(M.SalidaTemprano,0) + isnull(T.SalidaTemprano,0) as SalidaTemprano, 
             isnull(M.HorarioNormal,0) + isnull(T.HorarioNormal,0) as HorarioNormal, isnull(M.JornadaNocturna,0)+ isnull(T.JornadaNocturna,0) as JornadaNocturna,  
             isnull(M.HE50,0) + isnull(T.HE50,0) as HE50, isnull(M.HE100,0) + isnull(T.HE100,0) AS HE100, 
             isnull(M.Descanso,0) + isnull(T.Descanso,0) as Descanso, isnull(M.TiempoTrabajado,0) +  isnull(T.TiempoTrabajado,0) as TiempoTrabajado, 
             isnull(M.TiempoAsistencia,0) + isnull(T.TiempoAsistencia,0) as TiempoAsistencia 
             INTO rptAsistencia5 
            FROM TablaXL as M JOIN T ON M.UserID = T.userid and M.dFecha = t.Fecha 
            WHERE M.turnoEnteroCHE = 1 
            UNION 
            SELECT UserID, turnoEnteroCHE, NumCA, Empleado, Dia, cast(dFecha as Date) as Fecha, Horario, Permiso, convert(char(8), regENT, 108) as RegEnt, NULL, NULL, NULL, convert(char(8), regSAL , 108) as regSAL, 
             Atraso, SalidaTemprano, HorarioNormal, JornadaNocturna, HE50, HE100, Descanso, TiempoTrabajado, TiempoAsistencia 
            FROM TablaXL --WHERE turnoEnteroCHE is null; 
            WHERE turnoEnteroCHE =0 or turnoEnteroCHE is null or turnoEnteroCHE = 4 
            ORDER BY Empleado, Fecha;";


            lstReporte.Add(s1);
            lstReporte.Add(s2);
            lstReporte.Add(s3);
            lstReporte.Add(s4);
            lstReporte.Add(s5);


            return lstReporte;
        }
        private static List<string> LlenaResumenEmpleado()
        {
            List<string> lstResumenEmpleado = new List<string>();

            string s1 = @"CREATE table RptRsmAsistencia (" +
            "Foto Image NULL, " +
            "Departamento VARCHAR(200) NULL, " +
            "Cargo VARCHAR(50) NULL, " +
            "Empleado VARCHAR(60) NULL, " +
            "NumCA VARCHAR(10) NULL, " +
            "DiasLaborables FLOAT NULL, " +
            "DiasTrabajados FLOAT NULL, " +
            "Atrasos FLOAT NULL, " +
            "Ausencias FLOAT NULL, " +
            "TPermisoTrab FLOAT NULL, " +
            "TPermisoNoTrab FLOAT NULL, " +
            "[Salidas Temprano] FLOAT NULL, " +
            "numAtrasos INT NULL, " +
            "numSalidasTemprano INT NULL, " +
            "Almuerzo FLOAT NULL, " +
            "[Anticipo Almuerzo] FLOAT NULL, " +
            "[Atrasos Almuerzo] FLOAT NULL, " +
            "[Exceso Almuerzo] FLOAT NULL, " +
            "[HorarioNormal] FLOAT NULL, " +
            "[Jornada Nocturna] FLOAT NULL, " +
            "[HE 50%] FLOAT NULL, " +
            "[HE 100%] FLOAT NULL, " +
            "HH_Atrasos INT NULL, " +
            "mm_Atrasos INT NULL, " +
            "ss_Atrasos INT NULL, " +
            "HH_SalidasTemprano INT NULL, " +
            "mm_SalidasTemprano INT NULL, " +
            "ss_SalidasTemprano INT NULL, " +
            "HH_Normales INT NULL, " +
            "mm_Normales INT NULL, " +
            "ss_Normales INT NULL, " +
            "HH_Nocturnas INT NULL, " +
            "mm_Nocturnas INT NULL, " +
            "ss_Nocturnas INT NULL, " +
            "HH_HE50 INT NULL, " +
            "mm_HE50 INT NULL, " +
            "ss_HE50 INT NULL, " +
            "HH_HE100 INT NULL, " +
            "mm_HE100 INT NULL, " +
            "ss_HE100 INT NULL, " +
            "HH_Feriado INT NULL, " +
            "mm_Feriado INT NULL, " +
            "ss_Feriado INT NULL, " +
            "HH_PermisoT INT NULL, " +
            "mm_PermisoT INT NULL, " +
            "HH_PermisoNoT INT NULL, " +
            "mm_PermisoNoT INT NULL, " +
            "HH_TiempoTrabajado INT NULL, " +
            "mm_TiempoTrabajado INT NULL, " +
            "ss_TiempoTrabajado INT NULL, " +
            "[Atrasos (HH:mm)] varchar(10) NULL, [Salidas Temprano (HH:mm)] varchar(10) NULL, " +
            "[Normales (HH:mm)] varchar(10) NULL, [Nocturnas (HH:mm)] varchar(10) NULL, [Extra 50% (HH:mm)] varchar(10) NULL, [Extra 100% (HH:mm)] varchar(10) NULL," +
            "[Feriado (HH:mm)] varchar(10) NULL, [Tiempo PermisoT (HH:mm)] varchar(10) NULL, [Tiempo PermisoNoT (HH:mm)] varchar(10) NULL, [Tiempo Trabajado (HH:mm)] varchar(10) NULL, " +
            "[Aprobado HE 50%] FLOAT NULL, " +
            "[Aprobado HE 100%] FLOAT NULL, " +
            "[Aprobado Dias Descanso] FLOAT NULL, " +
            "[Total No Laborado] FLOAT NULL, " +
            "[Dias Descanso] FLOAT NULL, " +
            "[Tiempo Trabajado] FLOAT NULL, " +
            "[Tiempo Asistencia] FLOAT NULL, " +
            "[Tiempo No Cumplido] FLOAT NULL, " +
            "[USD Nocturna] FLOAT NULL, " +
            "[USD 50%] FLOAT NULL, " +
            "[USD 100%] FLOAT NULL, " +
            "[A USD 50%] FLOAT NULL, " +
            "[A USD 100%] FLOAT NULL, " +
            "MultaAtrasos FLOAT NULL, " +
            "MultaAusencias FLOAT NULL, " +
                "DescuentoAnticipo FLOAT NULL, " +
                "DescuentoPrestamo FLOAT NULL, " +
            "[USD Total] FLOAT NULL, " +
            "[A USD Total] FLOAT NULL, " +
            "Sueldo FLOAT NULL, " +
                "userId INT NOT NULL, " +
            "Cedula VARCHAR(11) NULL, " +
            "Contrato Varchar(25) NULL);";

            string s2 = @"INSERT INTO RptRsmAsistencia ( Departamento, Cargo, Empleado, NumCA, DiasLaborables, DiasTrabajados, Ausencias, Atrasos, " +
            "[Salidas Temprano], numAtrasos, numSalidasTemprano, Almuerzo, [Exceso Almuerzo], TPermisoTrab, TPermisoNoTrab, HorarioNormal, [Jornada Nocturna], [HE 50%], [HE 100%], " +
            "HH_Atrasos, mm_Atrasos, ss_Atrasos, HH_SalidasTemprano, mm_SalidasTemprano, ss_SalidasTemprano, " +
            "HH_Normales, mm_Normales, ss_Normales, HH_Nocturnas, mm_Nocturnas, ss_Nocturnas, HH_HE50, mm_HE50, ss_HE50, HH_HE100, mm_HE100, ss_HE100, " +
            "HH_Feriado, mm_Feriado, ss_Feriado, HH_PermisoT, mm_PermisoT, HH_PermisoNoT, mm_PermisoNoT, HH_TiempoTrabajado, mm_TiempoTrabajado, ss_TiempoTrabajado, " +
            "[Aprobado HE 50%],[Aprobado HE 100%],[Aprobado Dias Descanso], [Dias Descanso], " +
            "[Tiempo Trabajado], [Tiempo Asistencia], [Tiempo No Cumplido], [USD Nocturna], [USD 50%], [USD 100%], [USD Total], MultaAtrasos, MultaAusencias, " +
            "[A USD 50%], [A USD 100%], [A USD Total], Sueldo, userId, Cedula, Contrato)  " +
            "SELECT xl.Departamento, xl.Cargo, xl.Empleado, xl.NumCA,  " +
            "Sum(cast(xl.DiaNormal as float)) AS SumaDeDiaNormal, Sum(cast(xl.DiaTrabajado as float)) AS SumaDeDiaTrabajado,  " +
            "Sum(cast(xl.Ausente as float)) AS SumaDeAusente, Sum(cast(xl.Atraso as float)) AS SumaDeAtraso,  " +
            "Sum(cast(xl.SalidaTemprano as float)) AS SumaDeSalidaTemprano, " +
            "Sum(numAtrasos) AS CuentaDeAtrasos, Sum(numSalidasTemprano) AS CuentaDeSalidasTemprano, " +
            "Sum(cast(xl.TiempoAlmuerzo as float)) AS Almuerzo, " +
            "Sum(cast(xl.ExcesoAlmuerzo as float)) AS [Exceso Almuerzo], " +
            "Sum(cast(xl.TiempoPermisoT as float)) AS TPermisoTrab, " +
            "Sum(cast(xl.TiempoPermisoNT as float)) AS TPermisoNoTrab, Sum(cast(xl.HorarioNormal as float)) AS SumaDeHorarioNormal,  " +
            "Sum(cast(xl.JornadaNocturna as float)) AS SumaDeJornadaNocturna, Sum(cast(xl.HE50 as float)) AS SumaDeHE50,  " +
            "Sum(cast(xl.HE100 as float)) AS SumaDeHE100, " +
            "sum(datepart(Hour, Atraso)), sum(datepart(Minute, Atraso)), sum(datepart(Second, Atraso)), sum(datepart(Hour, SalidaTemprano)), sum(datepart(Minute, SalidaTemprano)), sum(datepart(Second, SalidaTemprano)), " +
            "sum(datepart(Hour, HorarioNormal)), sum(datepart(Minute, HorarioNormal)), sum(datepart(Second, HorarioNormal)), sum(datepart(Hour, JornadaNocturna)), sum(datepart(Minute, JornadaNocturna)), sum(datepart(Second, JornadaNocturna)), " +
            "sum(datepart(Hour, HE50)), sum(datepart(Minute, HE50)), sum(datepart(Second, HE50)), sum(datepart(Hour, HE100)), sum(datepart(Minute, HE100)), sum(datepart(Second, HE100)), " +
            "sum(datepart(Hour, Descanso)), sum(datepart(Minute, Descanso)), sum(datepart(Second, Descanso)), sum(datepart(Hour, TiempoPermisoT)), sum(datepart(Minute, TiempoPermisoT)), " +
            "sum(datepart(Hour, TiempoPermisoNT)), sum(datepart(Minute, TiempoPermisoNT)), sum(datepart(Hour, TiempoTrabajado)), sum(datepart(Minute, TiempoTrabajado)), sum(datepart(Second, TiempoTrabajado)), " +
            "Sum(cast(he.tahe50 as float)) AS SumaDeAHE50, Sum(cast(he.tahe100 as float)) AS SumaDeAHE100, Sum(cast(he.tadd as float)) AS SumaDeAHEDD, Sum(cast(xl.Descanso as float)) AS SumaDeDescanso, " +
            "Sum(cast(xl.TiempoTrabajado as float)) AS SumaDeTiempoTrabajado,  " +
            "Sum(cast(xl.TiempoAsistencia as float)) AS SumaDeTiempoAsistencia, " +
            "Sum(cast(xl.TiempoNoCumplido as float)) AS SumaDeTiempoNoCumplido, " +
            "Sum(xl.USDNocturna) AS SumaDeUSDNocturna,  " +
            "Sum(xl.USD50) AS SumaDeUSD50, Sum(xl.USD100) AS SumaDeUSD100, Sum(xl.USDTotal) AS SumaDeUSDTotal,  " +
            "Sum(xl.MultaAtrasos) AS SumaDeMultaAtrasos,  " +
            "Sum(xl.MultaAusencias) AS SumaDeMultaAusencias,  " +
            "Sum(HE.AUSD50) AS SumaAUSD50, Sum(HE.AUSD100) AS SumaAUSD100, Sum(HE.AUSDTotal) AS SumaAUSDTotal,  " +
            "xl.Sueldo, xl.UserID, substring(xl.Cedula,1,10), xl.Contrato " +
            "FROM TablaXL xl left outer join AprobacionHE he on he.userid=xl.userid and he.codh=xl.codh and he.fecha=xl.dfecha " +
            "GROUP BY xl.Departamento, xl.Cargo, xl.Empleado, xl.NumCA, xl.UserID, xl.Sueldo, xl.Cedula, xl.Contrato  " +
            "ORDER BY xl.Empleado, xl.NumCA;";

            //'La línea siguiente es para la LOSEP, pone 60 en JornadaNocturna (Suplementarias) y pone 60 en (HE50 +EH100) cuando se pasan de las 60 horas
            string s3 = @"UPDATE RptRsmAsistencia SET [Jornada Nocturna]=CASE WHEN [Jornada Nocturna]>(60/24) THEN (60/24) ELSE [Jornada Nocturna] END, " +
            "[HE 50%]=CASE WHEN [HE 100%]<(60/24) THEN (60/24)-[HE 100%] ELSE 0 END, " +
            "[HE 100%]=CASE WHEN [HE 100%]<(60/24) THEN [HE 100%] ELSE (60/24) END " +
            "WHERE Contrato='LOSEP' AND [HE 50%]+[HE 100%]>(60/24);";

            //'Pone la foto
            string s4 = @"UPDATE rsmAsistencia " +
            "SET Foto = Userinfo.PhotoB " +
            "FROM RptRsmAsistencia as rsmAsistencia Inner Join Userinfo " +
            "ON rsmAsistencia.NumCA = Userinfo.badgenumber;";

            //'Pone el sueldo
            string s5 = @"UPDATE rsmAsistencia " +
            "SET Sueldo = GS.Sueldo " +
            "FROM RptRsmAsistencia as rsmAsistencia Inner Join Userinfo " +
            "ON rsmAsistencia.NumCA = Userinfo.badgenumber " +
            "INNER JOIN GrupoSalarial GS ON Userinfo.idGrupoOcupacional = GS.idGrupo;";

            //'Esta linea totaliza el tiempo trabajado + el tiempo de permisos trabajados
            string s6 = @"UPDATE RptRsmAsistencia SET [Tiempo Trabajado]=[Tiempo Trabajado]+TPermisoTrab WHERE not (TPermisoTrab is null);";

            //'La línea siguiente es para sumar los Segundos acumulados que se hacen Minutos, a los minutos que ya habian
            string s7 = @"UPDATE RptRsmAsistencia SET " +
            @"mm_Atrasos = mm_Atrasos + (ss_Atrasos / 60), mm_SalidasTemprano = mm_SalidasTemprano + (ss_SalidasTemprano / 60), 
            mm_Normales = mm_Normales + (ss_Normales / 60), mm_Nocturnas = mm_Nocturnas + (ss_Nocturnas / 60), 
            mm_HE50 = mm_HE50 + (ss_HE50 / 60), mm_HE100 = mm_HE100 + (ss_HE100 / 60), 
            mm_Feriado = mm_Feriado + (ss_Feriado / 60), mm_TiempoTrabajado = mm_TiempoTrabajado + (ss_TiempoTrabajado / 60); ";

            string s8 = @"UPDATE RptRsmAsistencia SET " +
            @"HH_Atrasos = HH_Atrasos + (mm_atrasos / 60), ss_Atrasos = ss_Atrasos % 60,
            HH_SalidasTemprano = HH_SalidasTemprano + (mm_SalidasTemprano / 60), ss_SalidasTemprano = ss_SalidasTemprano % 60,
            HH_Normales = HH_Normales + (mm_Normales / 60), ss_Normales = ss_Normales % 60,
            HH_Nocturnas = HH_Nocturnas + (mm_Nocturnas / 60), ss_Nocturnas = ss_Nocturnas % 60,
            HH_HE50 = HH_HE50 + (mm_HE50 / 60), ss_HE50 = ss_HE50 % 60,
            HH_HE100 = HH_HE100 + (mm_HE100 / 60), ss_HE100 = ss_HE100 % 60,
            HH_Feriado = HH_Feriado + (mm_Feriado / 60), ss_Feriado = ss_Feriado % 60,
            HH_PermisoT = HH_PermisoT + (mm_PermisoT / 60), 
            HH_PermisoNoT = HH_PermisoNoT + (mm_PermisoNoT / 60), 
            HH_TiempoTrabajado = HH_TiempoTrabajado + (mm_TiempoTrabajado / 60), ss_TiempoTrabajado = ss_TiempoTrabajado % 60; ";

            string s9 = @"UPDATE RptRsmAsistencia SET " +
            @"mm_atrasos = (mm_atrasos % 60), 
            mm_SalidasTemprano = (mm_SalidasTemprano % 60), 
            mm_Normales = (mm_Normales % 60), 
            mm_Nocturnas = (mm_Nocturnas % 60), 
            mm_HE50 = (mm_HE50 % 60), 
            mm_HE100 = (mm_HE100 % 60), 
            mm_Feriado = (mm_Feriado % 60), 
            mm_PermisoT = (mm_PermisoT % 60), 
            mm_PermisoNoT = (mm_PermisoNoT % 60), 
            mm_TiempoTrabajado = (mm_TiempoTrabajado % 60); ";

            string s10 = @"UPDATE RptRsmAsistencia SET " +
            @"[Tiempo PermisoT (HH:mm)] = null
            where [Tiempo PermisoT (HH:mm)] = '::00'";

            string s11 = @"UPDATE RptRsmAsistencia SET " +
            @"[Tiempo PermisoNoT (HH:mm)] = null
            where [Tiempo PermisoNoT (HH:mm)] = '::00'";


            lstResumenEmpleado.Add(s1);
            lstResumenEmpleado.Add(s2);
            lstResumenEmpleado.Add(s3);
            lstResumenEmpleado.Add(s4);
            lstResumenEmpleado.Add(s5);
            lstResumenEmpleado.Add(s6);
            lstResumenEmpleado.Add(s7);
            lstResumenEmpleado.Add(s8);
            lstResumenEmpleado.Add(s9);
            lstResumenEmpleado.Add(s10);
            lstResumenEmpleado.Add(s11);


            return lstResumenEmpleado;
        }
        private static List<string> PoneDescuentosPrestamos(DateTime f1, DateTime f2)
        {
            List<string> lstDescuentosPrestamos = new List<string>();

            string s1 = @"with Pagos  as (
            select idUsuario, idTipoPrestamo, fechaPago, count(*) as cuotas, SUM(C.Monto) as montoTotal
            from Prestamos P inner
            join CuotasPrestamo C on P.idPrestamo = C.idPrestamo
            where (fechaPago between '" + f1.ToString("dd/MM/yyyy") + "' and '" + f2.ToString("dd/MM/yyyy") + @"')             
            group by idUsuario, idTipoPrestamo, fechaPago
            ) " +
        @"UPDATE RptRsmAsistencia  
        SET [DescuentoAnticipo] = montoTotal
        FROM RptRsmAsistencia  Rsm INNER JOIN Pagos 
        ON Rsm.UserID = Pagos.idUsuario
        WHERE idTipoPrestamo = 0; ";

            string s2 = @"with Pagos  as (
            select idUsuario, idTipoPrestamo, fechaPago, count(*) as cuotas, SUM(C.Monto) as montoTotal
            from Prestamos P inner
            join CuotasPrestamo C on P.idPrestamo = C.idPrestamo
            where (fechaPago between '" + f1.ToString("dd/MM/yyyy") + "' and '" + f2.ToString("dd/MM/yyyy") + @"')             
            group by idUsuario, idTipoPrestamo, fechaPago
            ) " +
        @"UPDATE RptRsmAsistencia  
        SET[DescuentoPrestamo] = montoTotal
        FROM RptRsmAsistencia  Rsm INNER JOIN Pagos  
        ON Rsm.UserID = Pagos.idUsuario
        WHERE idTipoPrestamo = 1; ";

            string s3 = @"UPDATE RptRsmAsistencia  
            SET[USD Total] = isnull([USD Nocturna], 0) +isnull([USD 50%], 0) +
            isnull([USD 100%], 0) -isnull(MultaAtrasos, 0) - isnull(MultaAusencias, 0) -
            isnull([DescuentoAnticipo], 0) -isnull([DescuentoPrestamo], 0); ";


            lstDescuentosPrestamos.Add(s1);
            lstDescuentosPrestamos.Add(s2);
            lstDescuentosPrestamos.Add(s3);


            return lstDescuentosPrestamos;
        }
        private static List<string> LlenaResumenDepartamento()
        {
            List<string> lstRsmDepartamentos = new List<string>();

            string s1 = @"CREATE table RptRsmDepartamento  (" +
            "Departamento VARCHAR(200) NULL, " +
            "Empleados INT NULL, " +
            "DiasLaborables FLOAT NULL, " +
            "DiasTrabajados FLOAT NULL, " +
            "Atrasos FLOAT NULL, " +
            "Ausencias FLOAT NULL, " +
            "TPermisoTrab FLOAT NULL, " +
            "TPermisoNoTrab FLOAT NULL, " +
            "[Salidas Temprano] FLOAT NULL, " +
            "numAtrasos INT NULL, " +
            "numSalidasTemprano INT NULL, " +
            "Almuerzo FLOAT NULL, " +
            "[Anticipo Almuerzo] FLOAT NULL, " +
            "[Atrasos Almuerzo] FLOAT NULL, " +
            "[Exceso Almuerzo] FLOAT NULL, " +
            "[HorarioNormal] FLOAT NULL, " +
            "[Jornada Nocturna] FLOAT NULL, " +
            "[HE 50%] FLOAT NULL, " +
            "[HE 100%] FLOAT NULL, " +
            "[Aprobado HE 50%] FLOAT NULL, " +
            "[Aprobado HE 100%] FLOAT NULL, " +
            "[Aprobado Dias Descanso] FLOAT NULL, " +
            "[Total No Laborado] FLOAT NULL, " +
            "[Dias Descanso] FLOAT NULL, " +
            "[Tiempo Trabajado] FLOAT NULL, " +
            "[Tiempo Asistencia] FLOAT NULL, " +
            "[Tiempo No Cumplido] FLOAT NULL, " +
            "[USD Nocturna] FLOAT NULL, " +
            "[USD 50%] FLOAT NULL, " +
            "[USD 100%] FLOAT NULL, " +
            "[A USD 50%] FLOAT NULL, " +
            "[A USD 100%] FLOAT NULL, " +
            "MultaAtrasos FLOAT NULL, " +
            "MultaAusencias FLOAT NULL, " +
            "[USD Total] FLOAT NULL, " +
            "[A USD Total] FLOAT NULL)";

            string s2 = @"INSERT INTO RptRsmDepartamento  ( Departamento, Empleados, DiasLaborables, DiasTrabajados, Ausencias, Atrasos, " +
            "[Salidas Temprano], numAtrasos, numSalidasTemprano, Almuerzo, [Anticipo Almuerzo], [Atrasos Almuerzo], [Exceso Almuerzo], " +
            "TPermisoTrab, TPermisoNoTrab, HorarioNormal, [Jornada Nocturna], [HE 50%], [HE 100%],[Aprobado HE 50%],[Aprobado HE 100%],[Aprobado Dias Descanso], [Dias Descanso], " +
            "[Tiempo Trabajado], [Tiempo Asistencia], [Tiempo No Cumplido], [USD Nocturna], [USD 50%], [USD 100%], [USD Total], MultaAtrasos, MultaAusencias, " +
            "[A USD 50%], [A USD 100%], [A USD Total])  " +
            "SELECT Departamento, Count(Empleado), SUM(DiasLaborables) , SUM(DiasTrabajados), SUM(Ausencias), SUM(Atrasos), " +
            "SUM([Salidas Temprano]), SUM(numAtrasos), SUM(numSalidasTemprano), SUM(Almuerzo), SUM([Anticipo Almuerzo]), SUM([Atrasos Almuerzo]), SUM([Exceso Almuerzo]), " +
            "SUM(TPermisoTrab), SUM(TPermisoNoTrab), SUM(HorarioNormal), SUM([Jornada Nocturna]), " +
            "SUM([HE 50%]), SUM([HE 100%]), SUM([Aprobado HE 50%]),  " +
            "SUM([Aprobado HE 100%]), SUM([Aprobado Dias Descanso]), SUM([Dias Descanso]), " +
            "SUM([Tiempo Trabajado]), SUM([Tiempo Asistencia]), SUM([Tiempo No Cumplido]), SUM([USD Nocturna]), SUM([USD 50%]), SUM([USD 100%]), SUM([USD Total]), " +
            "SUM(MultaAtrasos), SUM(MultaAusencias), SUM([A USD 50%]), SUM([A USD 100%]), SUM([A USD Total]) " +
            "FROM RptRsmAsistencia  " +
            "GROUP BY Departamento " +
            "ORDER BY Departamento;";



            lstRsmDepartamentos.Add(s1);
            lstRsmDepartamentos.Add(s2);


            return lstRsmDepartamentos;
        }
        private static List<string> EliminaTablasTemporales()
        {
            List<string> tablasEliminadas = new List<string>();
            StringBuilder sb = new StringBuilder();
                        
            sb.Append("IF OBJECT_ID('Tabla0', 'U') IS NOT NULL  TRUNCATE Table Tabla0;\n");
            sb.Append( "IF OBJECT_ID('TablaM', 'U') IS NOT NULL  TRUNCATE Table TablaM;\n");
            sb.Append( "IF OBJECT_ID('Tabla2', 'U') IS NOT NULL  Drop Table Tabla2;\n");
            sb.Append( "IF OBJECT_ID('Tabla3', 'U') IS NOT NULL  Drop Table Tabla3;\n");
            sb.Append( "IF OBJECT_ID('TablaR', 'U') IS NOT NULL  Drop Table TablaR;\n");
            sb.Append( "IF OBJECT_ID('TablaRIn', 'U') IS NOT NULL  Drop Table TablaRIn;\n");
            sb.Append( "IF OBJECT_ID('TablaROut', 'U') IS NOT NULL  Drop Table TablaROut;\n");
            sb.Append( "IF OBJECT_ID('TablaROut1', 'U') IS NOT NULL  Drop Table TablaROut1;\n");
            sb.Append( "IF OBJECT_ID('TablaROut2', 'U') IS NOT NULL  Drop Table TablaROut2;\n");
            sb.Append( "IF OBJECT_ID('TablaRInOut', 'U') IS NOT NULL  Drop Table TablaRInOut;\n");
            sb.Append( "IF OBJECT_ID('TablaF', 'U') IS NOT NULL  Drop Table TablaF;\n");
            sb.Append( "IF OBJECT_ID('TablaFIn', 'U') IS NOT NULL  Drop Table TablaFIn;\n");
            sb.Append( "IF OBJECT_ID('TablaFOut', 'U') IS NOT NULL  Drop Table TablaFOut;\n");
            sb.Append( "IF OBJECT_ID('TablaFInOut', 'U') IS NOT NULL  Drop Table TablaFInOut;\n");
            sb.Append( "IF OBJECT_ID('HEL', 'U') IS NOT NULL  Drop Table HEL;\n");
            sb.Append( "IF OBJECT_ID('helIN', 'U') IS NOT NULL  Drop Table helIN;\n");
            sb.Append( "IF OBJECT_ID('HELTemporal', 'U') IS NOT NULL  Drop Table HELTemporal;\n");
            sb.Append( "IF OBJECT_ID('TablaXL', 'U') IS NOT NULL  Drop Table TablaXL;\n");
            sb.Append( "IF OBJECT_ID('PermisosH', 'U') IS NOT NULL  Drop Table PermisosH;\n");
            sb.Append( "IF OBJECT_ID('PermisosH_Temp', 'U') IS NOT NULL  Drop Table PermisosH_Temp;\n");
            // sb.Append("IF OBJECT_ID('RegDiaHabilVertical', 'U') IS NOT NULL  Drop Table RegDiaHabilVertical;\n");

            tablasEliminadas.Add(sb.ToString());

            return tablasEliminadas;
        }
        #endregion


        public static void DistribuyeHoras()
        {
            string strSQLServer = @"SELECT userid, dfecha, dia, tiempoBDD, horario, codh, overtime, 
            InicioTrabajo, Entrada, Temprano, Contrato, FinTrabajo, Tarde, Salida, 
            turnoEnteroCHE, Almuerzo, descuentaAlmuerzo, IsNull(TiempoAlmuerzo, 0) as TiempoAlmuerzo, HorarioNormal, JornadaNocturna, HE50, HE100, Descanso
                FROM TablaXL
            WHERE (ausente is null and idTipoh=0 and NOT tiempoBDD is null) 
            ORDER BY userid, dFecha, Entrada;";
            DataTable dTauxTemp = new DataTable();

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                SqlCommand comando = (SqlCommand)db.CreateCommand();
                dataAdapter.SelectCommand = comando;
                comando.CommandText = strSQLServer;
                dataAdapter.Fill(dTauxTemp);                
            }            
                                             

            //'Dim tiempo As Double, tiempo_extra As Double, turno_ant As Double
            TimeSpan tiempo, tiempo_extra = new();
            DateTime entrada_aux, entrada_aux2;
            TimeSpan aux_temprano, aux_tarde;
            int Temprano, Tarde;
            TimeSpan turno_ant_aux = new(), turno_act_aux = new();

            DateTime f0 = new DateTime(1900, 1, 1);
            TimeSpan h0 = new TimeSpan();

            StringBuilder sbConsultas = new StringBuilder();
            int i = 0;

            for (i = 0; i < dTauxTemp.Rows.Count; i++)
            {

                // 'Revisar porque debe haber una mejor forma y así evitar este if
                tiempo = ((DateTime)dTauxTemp.Rows[i]["tiempobdd"]).TimeOfDay;

                vHor_Nor = new TimeSpan();
                vJor_Noc = new TimeSpan();
                vHE50 = new TimeSpan();
                vHE100 = new TimeSpan();
                vTTrabajado = new TimeSpan();
                vDescanso = new TimeSpan();

                //'Antes
                //'if TrabajaDia(dTauxTemp.Rows[i]("diaenteroche")) = -1 And dTauxTemp.Rows[i]("overtime") = 1 {
                //'Ahora
                if ((int)dTauxTemp.Rows[i]["overtime"] == 1)       // 'Se le calcula HorasExtras, Tiene horario fijo y/o Rotativo, No importa el dia
                {
                    //'Inicializa la entrada auxiliar
                    entrada_aux = (DateTime)dTauxTemp.Rows[i]["InicioTrabajo"];    //'Entrada

                    //' "Quita" el tiempo destinado a horas extras "obligatorias", _
                    //'Por LLEGAR TEMPRANO
                    aux_temprano = (DateTime)(dTauxTemp.Rows[i]["Entrada"]) - (DateTime)(dTauxTemp.Rows[i]["InicioTrabajo"]);
                    Temprano = (int)dTauxTemp.Rows[i]["Temprano"];

                    if (aux_temprano.TotalDays > 0)          //'Llegó temprano      En realidad es mayor
                    {
                        switch (Temprano)
                        {
                            case 1:
                            case 2:
                                tiempo = tiempo - aux_temprano;
                                //'entrada_aux = CDate(dTauxTemp.Rows[i]["Entrada"])
                                if((string)dTauxTemp.Rows[i]["Contrato"] == "Losep")
                                {
                                    vHE50 = (TimeSpan)dTauxTemp.Rows[i]["HE50"];
                                    Distribuye_HE_LOSEP(ref entrada_aux, ref aux_temprano);
                                }
                                else
                                {
                                    Distribuye_Horas_Extras_vb6(ref entrada_aux, ref aux_temprano);
                                }
                                break;
                            case 4:
                            case 5:
                                //' Trabaja con la hora de entrada y sigue el proceso usual
                                aux_temprano = new TimeSpan();
                                break;
                        }
                    }
                    else    //'aux_temprano <= 0      Llegó atrasado
                    { 
                        aux_temprano = new TimeSpan();
                    }

                    //' "Quita" el tiempo destinado a horas extras "obligatorias", _
                    //'Por SALIR TARDE
                    aux_tarde = (DateTime)(dTauxTemp.Rows[i]["FinTrabajo"]) - (DateTime)(dTauxTemp.Rows[i]["Salida"]);
                    Tarde = (int)dTauxTemp.Rows[i]["Tarde"];

                    if (aux_tarde.TotalDays > 0)           // 'Salió tarde
                    {
                        switch (Tarde)
                        {
                            case 6:
                            case 7:
                                tiempo = tiempo - aux_tarde;
                                entrada_aux2 = (DateTime)dTauxTemp.Rows[i]["Salida"];
                                if ((string)dTauxTemp.Rows[i]["Contrato"] == "Losep")
                                {
                                    Distribuye_HE_LOSEP(ref entrada_aux2, ref aux_tarde);
                                }
                                else
                                {
                                    Distribuye_Horas_Extras_vb6(ref entrada_aux2, ref aux_tarde);
                                }
                                break;
                            case 9:
                            case 10:
                                //' Trabaja con la hora de salida y sigue el proceso usual
                                aux_tarde = new TimeSpan();
                                break;
                        }
                    }
                    else                    //'aux_tarde <= 0     Salió temprano
                    { 
                        aux_tarde = new TimeSpan();
                    }
                    //'Hasta aquí llegan las instrucciones adicionales por la llegada temprana o salida tarde

                    if ((int)dTauxTemp.Rows[i]["turnoenteroche"] == 1)
                    {
                        turno_ant_aux = new TimeSpan();
                    }

                    //'Es obligatorio poner 0 en duración de jornada (min) en la configuración del horario cuando no hay tiempo de almuerzo
                    //'turnoenteroCHE=0 único turno del día
                    //'turnoenteroCHE=1 primer turno del día
                    //'turnoenteroCHE=3 es un turno intermedio, es decir, Hay un turno en la fila anterior y hay otro turno en la fila siguiente en el mismo día
                    //'turnoenteroCHE=2 es el último turno del mismo día de la fila anterior
                    if ((int)dTauxTemp.Rows[i]["turnoenteroche"] == 0 && ((DateTime)dTauxTemp.Rows[i]["Almuerzo"]).TimeOfDay > h0 && (int)dTauxTemp.Rows[i]["descuentaAlmuerzo"] == 1)
                    {
                        OchoHoras = (DateTime)(dTauxTemp.Rows[i]["Salida"]) - (DateTime)(dTauxTemp.Rows[i]["Entrada"]) > OchoAM ?
                                        OchoAM.Add(((DateTime)(dTauxTemp.Rows[i]["Almuerzo"])).TimeOfDay) :
                                        (DateTime)(dTauxTemp.Rows[i]["Salida"]) - (DateTime)(dTauxTemp.Rows[i]["Entrada"]);
                    }
                    else if ((int)dTauxTemp.Rows[i]["turnoenteroche"] == 0 && ((DateTime)dTauxTemp.Rows[i]["Almuerzo"]).TimeOfDay > h0 && (int)dTauxTemp.Rows[i]["descuentaAlmuerzo"] == 0)
                    {
                        OchoHoras = (DateTime)(dTauxTemp.Rows[i]["Salida"]) - (DateTime)(dTauxTemp.Rows[i]["Entrada"]) > OchoAM ?
                                          OchoAM : (DateTime)(dTauxTemp.Rows[i]["Salida"]) - (DateTime)(dTauxTemp.Rows[i]["Entrada"]);
                    }
                    else if ((int)dTauxTemp.Rows[i]["turnoenteroche"] == 4)
                    {
                        OchoHoras = (DateTime)(dTauxTemp.Rows[i]["Salida"]) - (DateTime)(dTauxTemp.Rows[i]["Entrada"]) > OchoAM ?
                                        OchoAM.Add(((DateTime)dTauxTemp.Rows[i]["TiempoAlmuerzo"]).TimeOfDay) :
                                        OchoAM;
                    }
                    else
                    {
                        OchoHoras = OchoAM;
                    }

                    //'tiempo es la variable que se necesitaba como turno_ant_aux para el día siguiente
                    if ((int)dTauxTemp.Rows[i]["turnoenteroche"] == 1 || (int)dTauxTemp.Rows[i]["turnoenteroche"] == 3)
                    {
                        turno_act_aux = turno_ant_aux;
                        turno_ant_aux = turno_ant_aux + tiempo;
                    }

                    //'Recontra REVISAR los ticks
                    //'
                    //'if dTauxTemp.Rows[i]("idTipoH") = 2 Then tiempo = IIf(-dTauxTemp.Rows[i]("TiempoPermiso").add(tiempo) > #1/1/1900#, tiempo - dTauxTemp.Rows[i]("TiempoPermiso"), 0)
                    //'if dTauxTemp.Rows[i]("idTipoH") = 2 Then tiempo = IIf(-dTauxTemp.Rows[i]("TiempoPermiso").add(tiempo) > #12/30/1899#, tiempo - dTauxTemp.Rows[i]("TiempoPermiso"), 0)
                    //'if rsAux!idTipoH = 2 Then tiempo = IIf(tiempo - rsAux!TiempoPermiso > 0, tiempo - rsAux!TiempoPermiso, 0)
                    //'
                    //'FIN Recontra REVISAR los ticks

                    //' Calcula el tiempo extra y el tiempo de las primeras 8 horas
                    if ((int)dTauxTemp.Rows[i]["turnoenteroche"] == 0 || (int)dTauxTemp.Rows[i]["turnoenteroche"] == 1 || (int)dTauxTemp.Rows[i]["turnoenteroche"] == 4)
                    {
                        if (tiempo.TotalDays > OchoHoras.TotalDays)
                        {
                            tiempo_extra = tiempo - OchoHoras;
                            tiempo = tiempo - tiempo_extra;
                        }
                        else
                        {
                            tiempo_extra = new TimeSpan();
                        }
                    }
                    else if ((int)dTauxTemp.Rows[i]["turnoenteroche"] == 2)   //'Es el último turno del día
                    {
                        if (turno_ant_aux.TotalDays > OchoHoras.TotalDays)   //'En los turnos anteriores ya se pasó de las 8 horas
                        {
                            tiempo_extra = tiempo;           //'todo el turno 2 o 3 son horas extras
                            tiempo = new TimeSpan();
                        }
                        else if (tiempo > OchoHoras - turno_ant_aux)   //'CON el turno 1 se pasa de las 8 horas
                        {
                            tiempo_extra = tiempo - OchoHoras + turno_ant_aux;
                            tiempo = tiempo - tiempo_extra;
                        }
                        else        //'Entre los 2 turnos no completan las 8 horas
                        {
                            tiempo_extra = new TimeSpan();
                        }
                        turno_ant_aux = new TimeSpan();   //'Inicializa turno_ant_aux para el caso en el empleado no se presente al primer turno
                    }
                    else if ((int)dTauxTemp.Rows[i]["turnoenteroche"] == 3)   //'Es el último turno del día
                    {
                        if (turno_act_aux > OchoHoras)    //'En los turnos anteriores ya se pasó de las 8 horas
                        {
                            tiempo_extra = tiempo;           //'todo el turno 2 o 3 son horas extras
                            tiempo = new TimeSpan();
                        }
                        else if (tiempo > OchoHoras - turno_act_aux)  //'CON el turno 1 se pasa de las 8 horas
                        {
                            tiempo_extra = tiempo - OchoHoras + turno_act_aux;
                            tiempo = tiempo - tiempo_extra;
                        }
                        else        // 'Entre los 2 turnos no completan las 8 horas
                        {
                            tiempo_extra = new TimeSpan();
                        }

                    }

                    //'Distribuye el tiempo de las primeras 8 horas y luego Distribuye el tiempo extra
                    if (dTauxTemp.Rows[i]["Contrato"] != null)
                    {
                        if ((string)dTauxTemp.Rows[i]["Contrato"] == "Losep")
                        {
                            if ((dTauxTemp.Rows[i]["HE50"] is DBNull) == false)
                            {
                                vHE50 = (TimeSpan)dTauxTemp.Rows[i]["HE50"] - h0;
                            }
                            entrada_aux = Distribuye_Horas_Habiles_LOSEP(ref entrada_aux, ref tiempo); //'Toma la nueva entrada_aux
                            if (tiempo_extra.TotalDays > 0)
                            {
                                Distribuye_HE_LOSEP(ref entrada_aux, ref tiempo_extra);
                            }
                        }
                        else
                        {
                            entrada_aux = Distribuye_Horas_Habiles_vb6(ref entrada_aux, ref tiempo);     //'Toma la nueva entrada_aux
                            if (tiempo_extra.TotalDays > 0)
                                Distribuye_Horas_Extras_vb6(ref entrada_aux, ref tiempo_extra);
                        }
                    }
                    else
                    {
                        entrada_aux = Distribuye_Horas_Habiles_vb6(ref entrada_aux, ref tiempo); //'Toma la nueva entrada_aux
                        if (tiempo_extra.TotalDays > 0)
                        {
                            Distribuye_Horas_Extras_vb6(ref entrada_aux, ref tiempo_extra);
                        }
                    }


                    //'Distribuye el tiempo extra
                    //'If tiempo_extra.TotalDays > 0 Then Call Distribuye_HE_LOSEP(entrada_aux, tiempo_extra) 'Call Distribuye_Horas_Extras_vb6(entrada_aux, tiempo_extra)

                    vTTrabajado = vHor_Nor + vJor_Noc + vHE50 + vHE100;
                }
                else
                {
                    //'Caso 1:
                    //'No se le calcula HorasExtras (overtime=0), Tiene horario fijo y/o Rotativo, No importa el dia, pero se le calcula HNormal y JNocturna

                    //'Inicializa la entrada auxiliar
                    entrada_aux = (DateTime)(dTauxTemp.Rows[i]["InicioTrabajo"]);    //'Entrada

                    //'Distribuye el tiempo de trabajo entre Horario Normal y Jornada Nocturna
                    //'En esta parte no se calculan Horas Extras porque overtime=0
                    //'entrada_aux = Distribuye_Horas_Habiles_vb6(entrada_aux, tiempo) 'Toma la nueva entrada_aux
                    if (dTauxTemp.Rows[i]["Contrato"] != null)
                    {
                        if ((string)dTauxTemp.Rows[i]["Contrato"] == "Losep")
                        {
                            entrada_aux = Distribuye_Horas_Habiles_LOSEP(ref entrada_aux, ref tiempo); //'Toma la nueva entrada_aux
                        }
                        else
                        {
                            entrada_aux = Distribuye_Horas_Habiles_vb6(ref entrada_aux, ref tiempo); //'Toma la nueva entrada_aux
                        }
                    }
                    else
                    {
                        entrada_aux = Distribuye_Horas_Habiles_vb6(ref entrada_aux, ref tiempo); //'Toma la nueva entrada_aux
                    }

                    vTTrabajado = vHor_Nor + vJor_Noc;

                }

                    // Final_De_While:

                    strSQLServer = "UPDATE TablaXL SET HE50='" + f0.Add(vHE50).ToString("dd/MM/yyyy HH:mm:ss") +
                    "', HE100='" + f0.Add(vHE100).ToString("dd/MM/yyyy HH:mm:ss") +
                    "', HorarioNormal='" + f0.Add(vHor_Nor).ToString("dd/MM/yyyy HH:mm:ss") +
                    "', JornadaNocturna='" + f0.Add(vJor_Noc).ToString("dd/MM/yyyy HH:mm:ss") +
                    "', TiempoTrabajado='" + f0.Add(vTTrabajado).ToString("dd/MM/yyyy HH:mm:ss") +
                    "', Descanso='" + f0.Add(vDescanso).ToString("dd/MM/yyyy HH:mm:ss") +
                    "' WHERE userID=" + dTauxTemp.Rows[i]["userid"] +
                        " and codH=" + dTauxTemp.Rows[i]["codH"] +
                        " and dFecha='" + ((DateTime)dTauxTemp.Rows[i]["dFecha"]).ToString("dd/MM/yyyy") + "';\n";

                //'comando.CommandText = strSQLServer
                //'comando.ExecuteNonQuery()

                sbConsultas.Append(strSQLServer);
            }
            
            consultaXL = sbConsultas.ToString() != "" ? sbConsultas.ToString() : "";            
            
        }   
    

        private static void Distribuye_HE_LOSEP(ref DateTime entrada_aux, ref TimeSpan tiempo_extra)
        {           
            
            DateTime fecha = new DateTime(entrada_aux.Year, entrada_aux.Month, entrada_aux.Day);

            // ' Distribuye el tiempo extra
            while (tiempo_extra.TotalDays > 0)
            {
                if(entrada_aux < fecha+SeisAM) // 'Termina jornada en la madrugada, 60%
                {
                    if(entrada_aux.Add(tiempo_extra) < fecha.AddDays(1).Add(SeisAM))    // 'Termina jornada en la madrugada, pero en la LOSEP son del 60% y la variable se llama vHE50
                    {
                        vHE50 += tiempo_extra;
                        entrada_aux = entrada_aux.Add(tiempo_extra);
                        tiempo_extra = new TimeSpan();
                    }
                    else        // ' Sigue jornada en el dia, ahora es horario normal
                    {
                        vHor_Nor += (entrada_aux.Date - fecha.AddDays(1)) + vHor_Nor.Add(SeisAM);
                        entrada_aux = entrada_aux.Date.Add(SeisAM);
                    }
                }
                else            // ' Horas del 25%
                {
                    if(entrada_aux + tiempo_extra < fecha.AddDays(1))           // ' Termina jornada en el mismo día
                    {
                        vJor_Noc += tiempo_extra;
                        entrada_aux += tiempo_extra;
                        tiempo_extra = new TimeSpan();
                    }
                    else                                                        // ' Sigue jornada en la madrugada del dia siguiente
                    {
                        vHE50 += (fecha.AddDays(1) - entrada_aux);
                        tiempo_extra -= (fecha.AddDays(1) - entrada_aux);
                        entrada_aux = fecha;
                    }
                }
            }

            if(vJor_Noc > CuatroAM)
            {
                vJor_Noc = CuatroAM;
            }
            
        }
        private static void Distribuye_Horas_Extras_vb6(ref DateTime entrada_aux, ref TimeSpan tiempo_extra)
        {           

           DateTime fecha = new DateTime(entrada_aux.Year, entrada_aux.Month, entrada_aux.Day);

            // ' Distribuye el tiempo extra
            while (tiempo_extra.TotalDays > 0)
            {
                if (entrada_aux < fecha + SeisAM)           // 'Horas del 100%
                {
                    if (entrada_aux.Add(tiempo_extra) < fecha.AddDays(1).Add(SeisAM))    // 'Termina jornada en la madrugada,
                    {
                        vHE100 += tiempo_extra;
                        entrada_aux = entrada_aux.Add(tiempo_extra);
                        tiempo_extra = new TimeSpan();
                    }
                    else        // ' Sigue jornada en el dia, 
                    {
                        vHE100 = vHE100.Add(SeisAM) - (entrada_aux.Date - fecha);
                        tiempo_extra = entrada_aux.Add(tiempo_extra) - fecha.Add(SeisAM);
                        
                        entrada_aux = fecha.Add(SeisAM);
                    }
                }
                else            // ' Horas del 50%
                {
                    if (entrada_aux + tiempo_extra < fecha.AddDays(1))           // ' Termina jornada en el mismo día
                    {
                        vHE50 += tiempo_extra;
                        entrada_aux += tiempo_extra;
                        tiempo_extra = new TimeSpan();
                    }
                    else                                                 // ' Sigue jornada en la madrugada del dia siguiente
                    {
                        vHE50 += (fecha.AddDays(1) - entrada_aux);
                        tiempo_extra -= (fecha.AddDays(1) - entrada_aux);
                        entrada_aux = fecha;
                    }
                }
            }            
        }
        private static DateTime Distribuye_Horas_Habiles_LOSEP(ref DateTime entrada_aux, ref TimeSpan tiempo)
        {
            DateTime fecha = new DateTime(entrada_aux.Year, entrada_aux.Month, entrada_aux.Day);

            // Distribuye el tiempo de las primeras 8 horas
            vHor_Nor += tiempo;
            entrada_aux += tiempo;
            tiempo = new TimeSpan();
            return entrada_aux;
        }
        private static DateTime Distribuye_Horas_Habiles_vb6(ref DateTime entrada_aux, ref TimeSpan tiempo)
        {
            DateTime dfecha;

            while(tiempo.TotalDays > 0)
            {
                dfecha = entrada_aux.Date;
                if(entrada_aux < dfecha.Add(SeisAM))    // Jornada nocturna (madrugada)
                {
                    if(entrada_aux + tiempo < dfecha + SeisAM)                // Termina jornada en la madrugada del día siguiente
                    {
                        vJor_Noc += tiempo;
                        entrada_aux += tiempo;
                        tiempo = new TimeSpan();
                    }
                    else                // Sigue jornada en la mañana
                    {
                        vJor_Noc += (dfecha.Add(SeisAM) - entrada_aux);
                        tiempo -= (dfecha.Add(SeisAM) - entrada_aux);
                        entrada_aux = dfecha.Add(SeisAM);
                    }
                }
                else if(entrada_aux < dfecha.Add(SietePM))      // Horario Normal
                {
                    if(entrada_aux + tiempo < dfecha.Add(SietePM))                               // Termina jornada en el día
                    {
                        vHor_Nor += tiempo;
                        entrada_aux += tiempo;
                        tiempo = new TimeSpan();
                    }
                    else                                // Sigue jornada en la noche
                    {
                        vHor_Nor += SietePM + (dfecha - entrada_aux);
                        //tiempo -= SietePM + (entrada_aux - dfecha);
                        tiempo -= SietePM;
                        tiempo += (entrada_aux - dfecha);
                        entrada_aux = dfecha.Add(SietePM);
                    }
                }
                else                    // Jornada nocturna (Noche)
                {
                    if(entrada_aux + tiempo < dfecha.AddDays(1))       // Termina jornada en la noche
                    {
                        vJor_Noc += tiempo;
                        entrada_aux += tiempo;
                        tiempo = new TimeSpan();
                    }
                    else        // Sigue jornada en la madrugada
                    {
                        vJor_Noc = dfecha.AddDays(1).Add(vJor_Noc) - entrada_aux;
                        tiempo = entrada_aux.Add(tiempo) - dfecha.AddDays(1);
                        entrada_aux = dfecha.AddDays(1);
                    }
                }
            }
            return entrada_aux;
        }
        
    }
}

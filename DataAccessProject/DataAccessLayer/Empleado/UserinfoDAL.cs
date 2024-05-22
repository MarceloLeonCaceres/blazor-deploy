using Dapper;
using DataAccessLibrary.Models;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace DataAccessLibrary.DataAccessLayer
{
    public class UserinfoDAL
    {
        public static IConfiguration Configuration;

        public const string SELECT_LISTA_COLUMNAS = @"SELECT
U.UserId, U.Badgenumber as Badge, U.Name as NombreEmp, U.SSN as Cedula, 
U.Gender, Genero.nomGenero,
U.Title as Cargo, U.Pager as Celular, 
DeptId, D.DeptName as Departamento, SupDeptId as DeptPadre,
U.idCentroCostos, CENTRO_COSTOS.nomCentroCostos, U.idContrato, Contrato.nomContrato, 
U.idGrupoSalarial, GrupoSalarial.nomGrupo as nomGrupoSalarial,  U.idCiudad, CIUDAD.nomCiudad,
CAST(U.Birthday as date) as [FechaNacimiento], CAST(U.HiredDay as date) as [FechaEmpleo], U.FechaSalida as [FechaSalida], 
U.Street as [Direccion], U.ATT as activo,
u.OTAdmin, U.OTPrivAdmin, U.OTPassword,  U.Username, 
U.esAdministrador, U.esAprobador, U.esSupervisor3, U.esSupervisorXl,
U.OPhone as [TelOficina], U.CardNo as CodigoEmpleado, U.CorreoOficina, U.PHOTOB, U.Notes,             
U.CargasFamiliares, U.idEstadoCivil, U.IdTipoSangre,
U.RegAsistencia, U.RegSobretiempo, U.RegDiaHabil, U.RegHoraExtra, U.RegOtros, U.RegMultaAtraso 
";


        public const string FROM_DEPTOS = @"FROM((((( DEPARTMENTS D INNER JOIN USERINFO AS U ON U.DEFAULTDEPTID = D.DEPTID)
left join Genero on u.Gender = Genero.idGenero)
left join Ciudad on u.idCiudad = CIUDAD.idCiudad)
left join CENTRO_COSTOS on u.idCentroCostos = CENTRO_COSTOS.idCentroCostos)
left join CONTRATO on u.idContrato = CONTRATO.idContrato)
left join GrupoSalarial on u.idGrupoSalarial = GrupoSalarial.idGrupo
";

        public const string GET_SUB_DEPTOS = @"WITH SubDepartamentos (deptid, deptName, supDeptId)
    AS
    (
        SELECT deptid, deptName, 0 FROM departments WHERE deptid = @IdDepto
        UNION ALL                
	    SELECT D.deptid, D.deptName, D.supDeptId FROM departments D inner join SubDepartamentos Sub on Sub.deptid = D.SUPDEPTID
    )
";

            public const string FROM_SUB_DEPTOS = @"FROM (((((SubDepartamentos D INNER JOIN USERINFO U ON D.DEPTID = U.DEFAULTDEPTID)
left join Genero on u.Gender = Genero.idGenero)
left join Ciudad on u.idCiudad = CIUDAD.idCiudad)
left join CENTRO_COSTOS on u.idCentroCostos = CENTRO_COSTOS.idCentroCostos)
left join CONTRATO on u.idContrato = CONTRATO.idContrato)
left join GrupoSalarial on u.idGrupoSalarial = GrupoSalarial.idGrupo
";
            



        //anterior Select = @" FROM (((( USERINFO AS U INNER JOIN DEPARTMENTS D ON U.DEFAULTDEPTID = D.DEPTID ) 
        //LEFT JOIN USERINFO AS SUP1 ON U.Supervisor1 = SUP1.USERID ) 
        //LEFT JOIN USERINFO AS SUP2 ON U.Supervisor2 = SUP2.USERID ) 
        //LEFT JOIN USERINFO AS SUP3 ON U.Supervisor3 = SUP3.USERID )";

        public const string SELECT_DESACTIVADOS = @"SELECT U.UserId, U.Badgenumber as Badge, U.Name as NombreEmp, 
            U.SSN as Cedula, U.gender, U.Title as Cargo, U.Pager as Celular, 
            U.idCentroCostos, U.idContrato, U.idGrupoSalarial, U.idCiudad, 
            U.Birthday as [FechaNacimiento], U.HiredDay as [FechaEmpleo], U.FechaSalida as [FechaSalida], 
            U.Street as [Direccion], 
            u.OTAdmin, U.OTPrivAdmin, U.OTPassword, U.Username, 
            U.esAdministrador, U.esAprobador, U.esSupervisor3, U.esSupervisorXl,
            U.OPhone as [TelOficina], U.CardNo as CodigoEmpleado, U.CorreoOficina, U.PHOTOB, U.Notes,             
			SUP1.NAME AS Supervisor1, SUP2.NAME AS Supervisor2, SUP3.NAME AS Supervisor3, 
            U.CargasFamiliares, U.idEstadoCivil, U.IdTipoSangre 
 FROM ((( USERINFO AS U  
            LEFT JOIN USERINFO AS SUP1 ON U.Supervisor1 = SUP1.USERID ) 
            LEFT JOIN USERINFO AS SUP2 ON U.Supervisor2 = SUP2.USERID ) 
            LEFT JOIN USERINFO AS SUP3 ON U.Supervisor3 = SUP3.USERID ) WHERE U.DefaultDeptId = -1";

        // it's not needed Where U.DefaultDeptId <> -1 because U.DefaultDeptId must be equal to Departments.DeptId

        public const string SELECT_BASE_DATA = @"SELECT USERID, BADGENUMBER AS BADGE, [NAME] AS NombreEmp, DefaultDeptId as DeptId
    FROM USERINFO WHERE USERID = @UserId;";

        public const string SELECT_SUB_DEPTOS = @"WITH SubDepartamentos (deptid, deptName, supDeptId, ConHijos)
            AS
            (
                SELECT deptid, deptName, 0, NULL FROM departments WHERE deptid =  @IdDepto
                UNION ALL                
	            SELECT D.deptid, D.deptName, D.supDeptId, NULL FROM departments D inner join SubDepartamentos Sub on Sub.deptid = D.SUPDEPTID
            )
            SELECT deptid
            FROM  SubDepartamentos
";

        public const string ORDERED_BY_NAME = "\n Order By NombreEmp";
        public UserinfoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<UserinfoModel>> GetEmpleadosAsync(string parametros)
        {
            IEnumerable<UserinfoModel> result;
            string sql = SELECT_LISTA_COLUMNAS + FROM_DEPTOS + ORDERED_BY_NAME;
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    result = await db.QueryAsync<UserinfoModel>(sql);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<UserinfoModel> GetEmpleados(int otadmin, int idDepto, int userId)
        {
            string sql;
            DynamicParameters parametro = new DynamicParameters();
            if (otadmin == 3)
            {
                sql = SELECT_LISTA_COLUMNAS + FROM_DEPTOS + "WHERE DEFAULTDEPTID > 0 " + ORDERED_BY_NAME;                
            }
            else if (otadmin == 1 & idDepto == -1)
            {
                sql = SELECT_DESACTIVADOS;
            }
            else if(otadmin == 1)
            {
                sql = SELECT_LISTA_COLUMNAS + FROM_DEPTOS + "WHERE DEFAULTDEPTID = " + idDepto.ToString() + ORDERED_BY_NAME;
            }
            else if(otadmin == 2)
            {
                sql = GET_SUB_DEPTOS + SELECT_LISTA_COLUMNAS + FROM_SUB_DEPTOS + ORDERED_BY_NAME;                
                parametro.Add("@IdDepto", idDepto, DbType.Int16);
            }
            else
            {
                sql = SELECT_LISTA_COLUMNAS + FROM_DEPTOS + "WHERE USERID = " + userId.ToString() + ORDERED_BY_NAME; ;
            }
            
            
            IEnumerable<UserinfoModel> result;
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    if(otadmin != 2)
                    {
                        result = db.Query<UserinfoModel>(sql);
                    }
                    else
                    {
                        result = db.Query<UserinfoModel>(sql, parametro, commandType: CommandType.Text);
                    }
                    
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<UserinfoModel> TraeDesactivados()
        {
            string sql = SELECT_DESACTIVADOS + "\n " + ORDERED_BY_NAME;
            IEnumerable<UserinfoModel> result;
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    result = db.Query<UserinfoModel>(sql);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<UserinfoModel> GetEmpleadoAsync(string badge)
        {
            string sql = SELECT_LISTA_COLUMNAS + FROM_DEPTOS + "\n WHERE U.Badgenumber = @Badgenumber;";
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("Badgnumber", badge, DbType.String);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {                
                db.Open();
                try
                {
                    UserinfoModel result = (UserinfoModel)await db.QueryFirstAsync<UserinfoModel>(sql, parametros, commandType: CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public UserinfoModel GetEmpleado(string userid)
        {
            string sql = SELECT_LISTA_COLUMNAS + FROM_DEPTOS + "\n WHERE U.UserId = @UserId;";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {                
                db.Open();
                try
                {
                    UserinfoModel result = (UserinfoModel) db.QueryFirst<UserinfoModel>(sql, new { UserId = userid });
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public EmpleadoBaseModel GetBaseModel(string userid)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {                
                db.Open();
                try
                {
                    EmpleadoBaseModel result = (EmpleadoBaseModel)db.QueryFirst<EmpleadoBaseModel>(SELECT_BASE_DATA , new { UserId = userid});
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task AddUserinfoAsync(UserinfoModel userinfo)
        {
            DynamicParameters parametros = ParametrosUserinfo(userinfo);

            string sql = @"INSERT INTO [dbo].[USERINFO] ([NAME], [BADGENUMBER], [SSN]
                ,[Gender], [Pager], [CorreoOficina], [CorreoPersonal]
                , [idCiudad], [Title], [OPhone]
                ,[CardNo], [Street], BirthDay, HiredDay, FechaSalida, [Tipo], [CargasFamiliares]) " +
                      @"Values (@NombreEmpleado, @Entrada, @Salida
                    , @GraciaEntrada, @GraciaSalida, @CorreoOficina, @CorreoPersonal
                    , @IniEntrada, @FinEntrada, @IniSalida, @FinSalida
                    , @DiaTrabajo, @MinutosBreak, @DescuentaAlmuerzo
                    , @HiredDay, @DescuentaAlmuerzo, @Tipo, @CargasFamiliares
                    );";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }


        public async Task<bool> ChangePassword(string userId, string pwdEncriptado)
        {            
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("Userid", userId, DbType.String);
            parametros.Add("Pwd", pwdEncriptado, DbType.String);
            string sql = "Update USERINFO SET OTPassword = @Pwd WHERE Userid = @Userid;";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
                    return true;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }                
            }
        }

        public async Task UpdateUserinfoAsync(UserinfoModel userinfo)
        {
            DynamicParameters parametros = ParametrosUserinfo(userinfo);
            parametros.Add("UserId", userinfo.UserId, DbType.Int16);
            string sql = "Update USERINFO SET " +
@"[NAME] =              @NombreEmp                  
,[SSN] =              @Cedula
,[DefaultDeptId] =         @DeptId
,[Gender] =           @Gender
,[Pager] =            @Celular
,[CorreoOficina] =      @CorreoOficina
,[CorreoPersonal] =     @CorreoPersonal
,[idCiudad] =         @idCiudad
,[idCentroCostos] =         @idCentroCosto
,[idContrato] =         @idContrato
,[idEstadoCivil] =   @idEstadoCivil
,[idGrupoSalarial] =   @idGrupoSalarial
,[idTipoSangre] =   @idTipoSangre
,[Title]=             @Cargo
,[OPhone]=        @TelOficina
,[esAdministrador] =   @esAdministrador
,[esAprobador] =   @esAprobador
,[esSupervisor3] =   @esSupervisor3
,[esSupervisorXl] =   @esSupervisorXl
,[ATT] =           @UsuarioWebActivo
,[OTAdmin]=        @OTAdmin
,[OTPassword]=        @OTPassword
,[OTPrivAdmin]=        @OTPrivAdmin
,[Street] =       @Direccion
,BirthDay   =     @FechaNacimiento
,[HiredDay] =     @FechaEmpleo                                 
,[FechaSalida] =     @FechaSalida
,[CargasFamiliares] =     @CargasFamiliares
,[RegAsistencia] = @regAsistencia
,[RegSobretiempo] = @regSobretiempo
,[RegHoraExtra] = @regHoraExtra
,[RegDiaHabil] = @regDiaHabil
,[RegOtros] = @regOtros
,[RegMultaATraso] = @regMultaAtraso
" +
                    "WHERE USERID = @UserId";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task RemoveUserinfoAsync(int userId)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                string sql = "Delete from [USERINFO] Where UserId = @UserId;\n";
                await db.ExecuteAsync(sql, new { UserId = userId });
            }
        }

        public int CuentaUserinfo(string badge)
        {
            string sql = "Select count(*) from [USERINFO] Where Badgenumber = @Badge;\n";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();                
                return (int)db.ExecuteScalar(sql, new { Badge = badge });
            }
        }

        public void DesactivaUserinfo(string csListaUsers)
        {
            string sql = "UPDATE [USERINFO] SET DefaultDeptId = -1 Where UserId in (" + csListaUsers + ")";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();                
                db.ExecuteScalar(sql, null);
            }
        }
        
        public void EliminaUserinfo(string csListaUsers)
        {
            string sqlPush = @"IF EXISTS (SELECT * FROM sysdatabases WHERE (name = 'Push_BioSmart')) 
            BEGIN
                delete from [Push_BioSmart].[dbo].[Attlog] where PIN in
                        (select badgenumber FROM userinfo WHERE userId in (" + csListaUsers + @") );                
                delete from [Push_BioSmart].[dbo].[TmpBioData] where PIN in
                        (select badgenumber FROM userinfo WHERE userId in (" + csListaUsers + @") );
                delete from [Push_BioSmart].[dbo].[TmpBioPhoto] where PIN in
                        (select badgenumber FROM userinfo WHERE userId in (" + csListaUsers + @") );
                delete from [Push_BioSmart].[dbo].[TmpFace] where PIN in
                        (select badgenumber FROM userinfo WHERE userId in (" + csListaUsers + @") );
                delete from [Push_BioSmart].[dbo].[TmpFP] where PIN in
                        (select badgenumber FROM userinfo WHERE userId in (" + csListaUsers + @") );
                delete from [Push_BioSmart].[dbo].[TmpFVein] where PIN in
                        (select badgenumber FROM userinfo WHERE userId in (" + csListaUsers + @") );
                delete from [Push_BioSmart].[dbo].[TmpUserPic] where PIN in
                        (select badgenumber FROM userinfo WHERE userId in (" + csListaUsers + @") );
                delete from [Push_BioSmart].[dbo].[UserInfo] where PIN in
                        (select badgenumber FROM userinfo WHERE userId in (" + csListaUsers + @") );
            END; ";
            string sql = "DELETE FROM CheckInout WHERE USERID in (" + csListaUsers + @");
                DELETE FROM CheckExact WHERE USERID in (" + csListaUsers + @");
                DELETE FROM Historial WHERE USERID in (" + csListaUsers + @");
                DELETE FROM Template WHERE USERID in (" + csListaUsers + @");
                DELETE FROM UserUsedSClasses WHERE USERID in (" + csListaUsers + @");
                DELETE FROM User_Of_Run WHERE USERID in (" + csListaUsers + @");
                DELETE FROM User_Speday WHERE USERID in (" + csListaUsers + @");
                DELETE FROM User_Temp_Sch WHERE USERID in (" + csListaUsers + @");
                DELETE FROM Vacaciones WHERE USERID in (" + csListaUsers + @");
                DELETE FROM SolicitudPermiso WHERE idUsuario in (" + csListaUsers + @");
                DELETE FROM _Atrasos WHERE USERID in (" + csListaUsers + @");
                DELETE FROM _SalidasTemprano WHERE USERID in (" + csListaUsers + @");
                DELETE FROM AtrasoSalidaTemp WHERE USERID in (" + csListaUsers + @");
                DELETE FROM [USERINFO] WHERE DefaultDeptId = -1 AND UserId in (" + csListaUsers + @")";
            
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    db.ExecuteScalar(sqlPush, null);
                    db.ExecuteScalar(sql, null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }                            
            }
        }

        public void ReactivaUserinfo(string csListaUsers)
        {
            string sql = "UPDATE [USERINFO] SET DefaultDeptId = (select min(DeptId) from DEPARTMENTS) Where UserId in (" + csListaUsers + ")";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                db.ExecuteScalar(sql, null);
            }
        }

        public int InsertNewUserinfo(string badge, EmpleadoBaseModel perfil)
        {
            DynamicParameters parametrosU = new DynamicParameters();
            parametrosU.Add("Badge", badge, DbType.String);
            parametrosU.Add("DeptId", perfil.DeptId, DbType.Int16);
            parametrosU.Add("IdCiudad", 0, DbType.Int16);
            
            string sInsert = @"Insert into Userinfo
                            (Badgenumber, Name, DefaultDeptId, idCiudad)
                            VALUES
                            (@Badge, @Badge, @DeptId, @IdCiudad)";
            string sRecuperaId = "Select userid from [USERINFO] Where Badgenumber = @Badge;";

            int userId = 0;
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {                
                db.Open();      
                try
                {
                    db.ExecuteScalar(sInsert, parametrosU, commandType: CommandType.Text);
                    userId = (int)db.ExecuteScalar(sRecuperaId, new { Badge = badge });
                }
                catch(Exception ex)
                {
                    throw ex;                    
                }                
                return userId;
            }
        }

        private DynamicParameters ParametrosUserinfo(string badge, UserinfoModel userinfo)
        {
            DynamicParameters parametros = ParametrosUserinfo(userinfo);

            parametros.Add("Badge", badge, DbType.String);
            parametros.Add("NombreEmp", badge, DbType.String);
            return parametros;
        }

        private DynamicParameters ParametrosUserinfo(UserinfoModel userinfo)
        {
            DynamicParameters parametros = new DynamicParameters();

            parametros.Add("CargasFamiliares", userinfo.CargasFamiliares, DbType.Int16);
            parametros.Add("CentroCostos", userinfo.idCentroCostos, DbType.Int16);
            parametros.Add("DeptId", userinfo.DeptId, DbType.Int16);
            parametros.Add("Gender", userinfo.gender, DbType.Int16);
            parametros.Add("idCiudad", userinfo.idCiudad, DbType.Int16);
            parametros.Add("idCentroCosto", userinfo.idCentroCostos, DbType.Int16);
            parametros.Add("idContrato", userinfo.idContrato, DbType.Int16);
            parametros.Add("idEstadoCivil", userinfo.idEstadoCivil, DbType.Int16);
            parametros.Add("idGrupoSalarial", userinfo.idGrupoSalarial, DbType.Int16);
            parametros.Add("idTipoSangre", userinfo.idTipoSangre, DbType.Int16);
            // parametros.Add("TipoEmpleado", userinfo.IdTipoEmpleado, DbType.Int16);
            parametros.Add("OTAdmin", userinfo.OTAdmin, DbType.Int16);            
            
            parametros.Add("regAsistencia", userinfo.RegAsistencia, DbType.Int16);
            parametros.Add("regSobretiempo", userinfo.RegSobretiempo, DbType.Int16);
            parametros.Add("regHoraExtra", userinfo.RegHoraExtra, DbType.Int16);
            parametros.Add("regDiaHabil", userinfo.RegDiaHabil, DbType.Int16);
            parametros.Add("regOtros", userinfo.RegOtros, DbType.Int16);
            parametros.Add("regMultaAtraso", userinfo.RegMultaAtraso, DbType.Int16);

            parametros.Add("esAdministrador", userinfo.esAdministrador, DbType.Boolean);
            parametros.Add("esAprobador", userinfo.esAprobador, DbType.Boolean);
            parametros.Add("esSupervisor3", userinfo.esSupervisor3, DbType.Boolean);
            parametros.Add("esSupervisorXl", userinfo.esSupervisorXl, DbType.Boolean);

            parametros.Add("Cargo", userinfo.Cargo, DbType.String);
            parametros.Add("Cedula", userinfo.Cedula, DbType.String);
            parametros.Add("Celular", userinfo.Celular, DbType.String);
            parametros.Add("CodigoEmpleado", userinfo.CodigoEmpleado, DbType.String);                       
            parametros.Add("CorreoPersonal", userinfo.CorreoPersonal, DbType.String);
            parametros.Add("CorreoOficina", userinfo.CorreoOficina, DbType.String);
            parametros.Add("Direccion", userinfo.Direccion, DbType.String);
            parametros.Add("NombreEmp", userinfo.NombreEmp, DbType.String);
            parametros.Add("TelOficina", userinfo.TelOficina, DbType.String);
            if(userinfo.OTAdmin == 0 && !string.IsNullOrEmpty(userinfo.OTPassword))
            {
                parametros.Add("OTPassword", null, DbType.String);
            }
            else if(userinfo.OTAdmin > 0 && string.IsNullOrEmpty(userinfo.OTPassword))
            {
                parametros.Add("OTPassword", UCommon.Cripto.Encripta(userinfo.Badge), DbType.String);
            }
            else
            {
                parametros.Add("OTPassword", userinfo.OTPassword, DbType.String);
            }
            parametros.Add("OTPrivAdmin", userinfo.OTPrivAdmin, DbType.String);            
            parametros.Add("FechaNacimiento", userinfo.FechaNacimiento, DbType.DateTime);
            parametros.Add("FechaEmpleo", userinfo.FechaEmpleo, DbType.DateTime);
            parametros.Add("FechaSalida", userinfo.FechaSalida, DbType.DateTime);

            parametros.Add("UsuarioWebActivo", userinfo.activo, DbType.Boolean);

            return parametros;
        }

        public byte[] RetornaFoto(string Badge)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = "SELECT PHOTO FROM Userinfo WHERE USERID = @Badge";
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("Badge", Badge, DbType.String);
                db.Open();
                try
                {
                    byte[] foto = db.QueryFirstOrDefault<byte[]>(sql, parametros, commandType: CommandType.Text);                    
                    return foto;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }
        public async Task UpdateFotoAsync(byte[] foto, string UserId)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("Foto", foto, DbType.Binary);
            parametros.Add("Id", UserId, DbType.String);

            string sql = @"Update Userinfo Set PHOTO = @Foto WHERE USERID = @Id";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task LogUpdateFoto(SystemLogModel log)
        {
            log.LogDescr = "Modifica datos Empleado";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }


        public async Task LogInsertUserinfo(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega empleado";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                
                db.Open();                
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateUserinfo(SystemLogModel log)
        {            
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {                
                db.Open();                
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeleteUserinfo(SystemLogModel log)
        {
            log.LogDescr = "Borra empleado";
            InsertLog insertLog = new InsertLog(log);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogAsignaUserinfo(SystemLogModel log)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"SELECT stuff( 
	                ( SELECT ',' + Badgenumber from userinfo
                        WHERE UserId in ( " + log.LogDetailed + @") for xml path('') 	 ),
	                1, 1, '');";
                db.Open();
                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

    }

}

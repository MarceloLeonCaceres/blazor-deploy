using Dapper;
using DataAccessLibrary.Models.SolicitudesPermiso;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.SolicitudesPermiso
{
    public class DatosAuxPermisoDAL
    {
        public IConfiguration Configuration;

        public DatosAuxPermisoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private string SELECT_DATOS_ = @"SELECT U.USERID, U.BADGENUMBER as NumCA, U.NAME as NomEmpleado, 
CASE WHEN U.CorreoOficina is null then '' else U.CorreoOficina end as CorreoEmpleado,
U.Supervisor1 as IdSupervisor1, Sup1.NAME as NombreS1, Sup1.CorreoOficina as CorreoS1, U.Supervisor2 as IdSupervisor2, Sup2.NAME as NombreS2, Sup2.CorreoOficina as CorreoS2
From ( USERINFO U left JOIN USERINFO Sup1 on Sup1.USERID = U.Supervisor1 )
				  left JOIN USERINFO Sup2 on Sup2.USERID = U.Supervisor2
WHERE ";

        private string AUX_NUEVA = "U.USERID = @ID";
        private string AUX_EXISTENTE = "U.USERID = (SELECT IdUsuario FROM SolicitudPermiso WHERE IdSolicitud = @ID)";
        private string AUX_LISTA_IDS = "U.USERID in (SELECT IdUsuario FROM SolicitudPermiso WHERE IdSolicitud in @ID)";


        private string SELECT_CORREOS = @"SELECT SP.IdSolicitud, SP.Estado, SP.respuesta1, SP.respuesta2,
U.USERID, U.BADGENUMBER as NumCA, U.NAME as NomEmpleado, 
CASE WHEN U.CorreoOficina is null then '' else U.CorreoOficina end as CorreoEmpleado,
SP.IdSupervisor1, Sup1.NAME as NombreS1, Sup1.CorreoOficina as CorreoS1, 
SP.IdSupervisor2, Sup2.NAME as NombreS2, Sup2.CorreoOficina as CorreoS2
from (((SolicitudPermiso SP inner join Userinfo U On SP.idUsuario = U.USERID)
left JOIN USERINFO Sup1 on SP.idSupervisor1 = Sup1.USERID )
left JOIN USERINFO Sup2 on SP.idSupervisor2 = Sup2.USERID )
WHERE SP.idSolicitud IN ";

        public DatosAuxPermisoModel GetDatosAuxPermiso(string tabla, string id)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("ID", id, DbType.String);
            // // S = Solicitud nueva      A = Adjuntar archivo

            string SQL = tabla == "Existente" ? SELECT_DATOS_ + AUX_EXISTENTE : SELECT_DATOS_ + AUX_NUEVA;

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    DatosAuxPermisoModel result = db.QueryFirst<DatosAuxPermisoModel>(SQL, parametros, commandType: CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<List<DatosAuxPermisoModel>> GetDatosAuxCorreosPermisos(List<int> idsSolicitudes)
        {
            //DynamicParameters parametros = new DynamicParameters();
            //parametros.Add("ID", $"'({string.Join(",", idsSolicitudes)})'", DbType.String);
            // // S = Solicitud nueva      A = Adjuntar archivo

            string SQL = SELECT_CORREOS + "( " + string.Join(",", idsSolicitudes) + " )";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<DatosAuxPermisoModel> result = await db.QueryAsync<DatosAuxPermisoModel>(SQL, null, commandType: CommandType.Text);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}

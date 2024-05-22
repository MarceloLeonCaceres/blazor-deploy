using Dapper;
using DataAccessLibrary.DataAccessLayer.Calculos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.Biometricos
{
    public class MarcacionesPushDAL
    {
        public IConfiguration Configuration;

        public MarcacionesPushDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string sPasaMarcaciones(string relojesValidos)
        {
            return @"MERGE [Blazor].[dbo].[checkinout_P] as Destino
            USING [Push_BioSmart].[dbo].[Attlog] as Origen ON Destino.PIN = Origen.PIN and Destino.AttTime = Origen.AttTime
            WHEN NOT MATCHED BY TARGET AND Origen.DeviceID in (" + relojesValidos + @") 
                THEN
                INSERT ( [PIN], [AttTime], [Status], [Verify], [Workcode], [Reserved1], [Reserved2], [MaskFlag], [Temperature], [DeviceID] ) 
                VALUES ( Origen.[PIN], Origen.[AttTime], Origen.[Status], Origen.[Verify], Origen.[Workcode], Origen.[Reserved1], Origen.[Reserved2], Origen.[MaskFlag], Origen.[Temperature], Origen.[DeviceID] );";
        }

        public bool ExisteBasePush()
        {
            string typeDevice = Configuration.GetSection("TipoDispositivos").Value.ToLower();
            if (typeDevice == "ambos" || typeDevice == "push")
            {
                return true;
            }
            return false;
        }
        public async Task<bool> CopiaMarcacionesPushAsync(string csLstRelojesValidos)
        {
            bool resultado = false;
            CalculoAsistenciaDAL relojesValidosDAL = new CalculoAsistenciaDAL(Configuration);
            string sRelojesValidos = relojesValidosDAL.RelojesValidos();
            string sql = sPasaMarcaciones(sRelojesValidos);

            string sqlNuevosUsuarios = @"With UsuariosMarcaciones AS (
Select distinct PIN
FROM CheckInOut_P Push LEFT JOIN USERINFO u ON Push.PIN = U.Badgenumber
WHERE not Push.PIN is null and U.USERID is null
)
INSERT INTO USERINFO ( Badgenumber, [Name] )
select PIN, PIN FROM UsuariosMarcaciones;";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {

                db.Open();
                try
                {
                    await db.QueryAsync(sql, null, commandType: CommandType.Text);
                    await db.QueryAsync(sqlNuevosUsuarios, null, commandType: CommandType.Text);
                    return true;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }


    }
}

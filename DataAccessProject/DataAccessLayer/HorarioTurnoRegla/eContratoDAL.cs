using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.HorarioTurnoRegla
{
    public class eContratoDAL
    {
        public IConfiguration Configuration;

        public const string SELECT_CONTRATOS = "Select idContrato, nomContrato From Contrato";

        public eContratoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<eContratoModel>> GetContratosAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = SELECT_CONTRATOS;
                db.Open();
                IEnumerable<eContratoModel> result = await db.QueryAsync<eContratoModel>(sql);
                return result.ToList();
            }
        }

    }
}

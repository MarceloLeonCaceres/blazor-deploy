using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.Seguridad;
using System;
using System.Text;

namespace DataAccessLibrary.DataAccessLayer.Seguridad
{
    public class FechaCaducidadDAL
    {
        public IConfiguration Configuration;

        public FechaCaducidadDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private string SELECT_FechaCaducidad = @"SELECT PARAVALUE as sFechaCoded FROM PROPERPARAM WHERE PARANAME = 'vfCoded'";

        public FechaCaducidadModel GetFechaCaducidad()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    FechaCaducidadModel result = db.QueryFirstOrDefault<FechaCaducidadModel>(SELECT_FechaCaducidad, null, commandType: CommandType.Text);                    
                    return result;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

    }
}

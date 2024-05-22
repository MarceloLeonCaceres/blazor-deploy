using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.DbAccess
{
    public class SqlDataAccess : ISqlDataAccess
    {
        private readonly IConfiguration _config;

        public SqlDataAccess(IConfiguration config)
        {
            this._config = config;
        }
        public async Task<List<T>> ReadDataAsync<T, U>(string query, U parameters, string connectionStringName)
        {
            string connectionString = _config.GetConnectionString(connectionStringName);
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var rows = await connection.QueryAsync<T>(query, parameters, commandType: CommandType.Text);
                return rows.ToList();
            }
        }

        public List<T> ReadData<T, U>(string query, U parameters, string connectionStringName)
        {
            string connectionString = _config.GetConnectionString(connectionStringName);
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var rows = connection.Query<T>(query, parameters, commandType: CommandType.Text);
                return rows.ToList();
            }
        }

        public async Task SaveDataAsync<T>(string query, T parameters, string connectionStringName)
        {
            string connectionString = _config.GetConnectionString(connectionStringName);
            try
            {
                using (IDbConnection connection = new SqlConnection(connectionString))
                {
                    var rows = await connection.ExecuteAsync(query, parameters, commandType: CommandType.Text);                    
                }
            }
            catch (Exception ex)
            {
                throw;
            }            
        }

        public async Task<T> ScalarDataAsync<T, U>(string query, U parameters, string connectionStringName)
        {
            string connectionString = _config.GetConnectionString(connectionStringName);
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var dato = await connection.ExecuteScalarAsync<T>(query, parameters, commandType: CommandType.Text);
                return dato;
            }
        }

        public async Task DeleteDataAsync(string query, int id, string connectionStringName)
        {
            string connectionString = _config.GetConnectionString(connectionStringName);
            try
            {
                using (IDbConnection connection = new SqlConnection(connectionString))
                {
                    var rows = await connection.ExecuteAsync(query, new { Id = id }, commandType: CommandType.Text);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}

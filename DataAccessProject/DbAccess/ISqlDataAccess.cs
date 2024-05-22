using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.DbAccess
{
    public interface ISqlDataAccess
    {
        Task<List<T>> ReadDataAsync<T, U>(string query, U parameters, string connectionStringName);

        Task SaveDataAsync<T>(string query, T parameters, string connectionStringName);

        Task<T> ScalarDataAsync<T, U>(string query, U parameters, string connectionStringName);

        Task DeleteDataAsync(string query, int id, string connectionStringName);
        List<T> ReadData<T, U>(string query, U parameters, string connectionStringName);
    }
}

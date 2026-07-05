using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using ACI.IServices.Main.Dapper;
using ACI.Common;

namespace ACI.Service
{
    public class DapperService : IDapperService
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public DapperService(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetSection(AppSettingConstantVariables.Connection).Value;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryAsync<T>(sql, param);
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryFirstOrDefaultAsync<T>(sql, param);
            }
        }

        public async Task<int> ExecuteAsync(string sql, object param = null)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.ExecuteAsync(sql, param);
            }
        }
    }
}

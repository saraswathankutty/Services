namespace ACI.IServices.Main.Dapper
{
    public interface IDapperService
    {
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null);
        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null);
        Task<int> ExecuteAsync(string sql, object param = null);
    }
}

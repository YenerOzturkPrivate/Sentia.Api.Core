using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sentia.Api.Core.Helper
{
    public interface ISqlDbHelper
    {
        Task<int> ExecuteAsync(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);
        Task<IDataReader> ExecuteReaderAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);
        Task<T> ExecuteScalarAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);
        Task<IDbConnection> GetConnection(CancellationToken cancellationToken);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);
        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);
        Task QueryMultipleAsync(string sql, Action<SqlMapper.GridReader> map, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);
        Task<T> QuerySingleOrDefaultAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);
        Task<T> ExecuteScalarAndGetIdAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default);

    }
}

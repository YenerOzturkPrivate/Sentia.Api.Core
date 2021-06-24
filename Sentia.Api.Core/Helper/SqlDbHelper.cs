using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sentia.Api.Core.Helper
{
    public class SqlDbHelper : ISqlDbHelper
    {
        private readonly string _connectionString;

        public SqlDbHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            using (var conn = GetConnection())
            {
                var cmd = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);
                return await conn.QueryAsync<T>(cmd);
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            using (var conn = GetConnection())
            {
                var cmd = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);
                return await conn.QueryFirstOrDefaultAsync<T>(cmd);
            }
        }

        public async Task<T> QuerySingleOrDefaultAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            using (var conn = GetConnection())
            {
                var cmd = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);
                return await conn.QuerySingleOrDefaultAsync<T>(cmd);
            }
        }

        public async Task<int> ExecuteAsync(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            try
            {

                //Transactional bir işlem yapılıyorsa aynı connectiondan devam etmeli.
                if (transaction != null)
                {
                    var conn = transaction.Connection;
                    var cmd = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);
                    return await conn.ExecuteAsync(cmd);
                }
                using (var conn = GetConnection())
                {
                    var cmd = new CommandDefinition(sql, parameters, null, commandTimeout, commandType, flags, cancellationToken);
                    return await conn.ExecuteAsync(cmd);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<T> ExecuteScalarAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            //Transactional bir işlem yapılıyorsa aynı connectiondan devam etmeli.
            if (transaction != null)
            {
                var conn = transaction.Connection;
                var cmd = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);
                return await conn.ExecuteScalarAsync<T>(cmd);
            }
            using (var conn = GetConnection())
            {
                var cmd = new CommandDefinition(sql, parameters, null, commandTimeout, commandType, flags, cancellationToken);
                return await conn.ExecuteScalarAsync<T>(cmd);
            }
        }

        public async Task<T> ExecuteScalarAndGetIdAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            //Transactional bir işlem yapılıyorsa aynı connectiondan devam etmeli.
            if (transaction != null)
            {
                var conn = transaction.Connection;
                var cmd = new CommandDefinition(sql + "SELECT SCOPE_IDENTITY();", parameters, transaction, commandTimeout, commandType, flags, cancellationToken);
                return await conn.ExecuteScalarAsync<T>(cmd);
            }
            using (var conn = GetConnection())
            {
                var cmd = new CommandDefinition(sql + "SELECT SCOPE_IDENTITY();", parameters, null, commandTimeout, commandType, flags, cancellationToken);

                var result = await conn.ExecuteScalarAsync<T>(cmd);
                return result;
            }
        }

        public async Task<IDataReader> ExecuteReaderAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default)
        {
            //Transactional bir işlem yapılıyorsa aynı connectiondan devam etmeli.
            if (transaction != null)
            {
                var conn = transaction.Connection;
                var cmd = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);
                return await conn.ExecuteReaderAsync(cmd);
            }
            using (var conn = GetConnection())
            {
                var cmd = new CommandDefinition(sql, parameters, null, commandTimeout, commandType, flags, cancellationToken);
                return await conn.ExecuteReaderAsync(cmd);
            }
        }

        public async Task QueryMultipleAsync(string sql, Action<SqlMapper.GridReader> map, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered,
            CancellationToken cancellationToken = default)
        {
            using (var conn = GetConnection())
            {
                var cmd = new CommandDefinition(sql, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);
                var result = await conn.QueryMultipleAsync(cmd);
                map(result);
            }
        }

        public async Task<IDbConnection> GetConnection(CancellationToken cancellationToken)
        {
            var conn = GetConnection();
            await conn.OpenAsync(cancellationToken);
            return conn;
        }
    }
}

using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System;

namespace OoBDev.Data.Common;

public interface IDatabaseMapper
{
    IEnumerable<IDataParameter> GetCommandParameters<T>(T query);

    string GetConnectionString<TDbOptions>();
    DbConnection GetConnection<TDbOptions>();
    int? GetCommandTimeout<TDbOptions>();

    string GetStoredProcedureName<T>();
    DbCommand GetStoredProcedure<T>(DbConnection sqlConnection, T query);
    Func<DbDataReader, TResult> GetReaderMapper<TResult>(DbDataReader reader);
}

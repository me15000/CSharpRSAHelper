using System.Collections.Generic;
using System.Data.Common;

namespace Common.DB
{
    public interface IDBHelper
    {

        DbParameter CreateParameter(string name, object value);

        DbParameter CreateParameter(string name);

        DbParameter CreateParameter();

        DbParameter[] CopyParameters(DbParameter[] dbps);

        DbConnection GetConnection();

        DbCommand CreateCommand();


        T ExecuteScalarP<T>(string sql, NVCollection parameters = null);

        DbDataReader ExecuteReaderP(string sql, NVCollection parameters = null);

        int ExecuteNoneQueryP(string sql, NVCollection parameters = null);

        NVCollection GetDataP(string sql, NVCollection parameters = null);

        List<NVCollection> GetDataListP(string sql, NVCollection parameters = null);

        T ExecuteScalar<T>(string sql, params object[] parameters);

        DbDataReader ExecuteReader(string sql, params object[] parameters);

        int ExecuteNoneQuery(string sql, params object[] parameters);

        NVCollection GetData(string sql, params object[] parameters);

        List<NVCollection> GetDataList(string sql, params object[] parameters);
    }
}
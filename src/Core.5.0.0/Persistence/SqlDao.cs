using System.Data;
using System.Data.SqlClient;

namespace com.Sconit.Persistence
{
    //this is the delegate for sql helper
    public class SqlDao : ISqlDao
    {
        public string ConnectionString { get; set; }
        public string ConnectionTimeOut { get; set; }

        public int ExecuteSql(string commandText, SqlParameter[] commandParameters)
        {
            return ExecuteNonQuery(CommandType.Text, commandText, commandParameters, false);
        }

        public int ExecuteSql(string commandText, SqlParameter[] commandParameters, bool startTransaction)
        {
            return ExecuteNonQuery(CommandType.Text, commandText, commandParameters, startTransaction);
        }

        public int ExecuteStoredProcedure(string commandText, SqlParameter[] commandParameters)
        {
            return ExecuteNonQuery(CommandType.StoredProcedure, commandText, commandParameters, false);
        }

        public int ExecuteStoredProcedure(string commandText, SqlParameter[] commandParameters, bool startTransaction)
        {
            return ExecuteNonQuery(CommandType.StoredProcedure, commandText, commandParameters, startTransaction);
        }

        public DataSet GetDatasetBySql(string commandText, SqlParameter[] commandParameters)
        {
            return GetDataset(CommandType.Text, commandText, commandParameters, false);
        }

        public DataSet GetDatasetBySql(string commandText, SqlParameter[] commandParameters, bool startTransaction)
        {
            return GetDataset(CommandType.Text, commandText, commandParameters, startTransaction);
        }

        public DataSet GetDatasetByStoredProcedure(string commandText, SqlParameter[] commandParameters)
        {
            return GetDataset(CommandType.StoredProcedure, commandText, commandParameters, false);
        }

        public DataSet GetDatasetByStoredProcedure(string commandText, SqlParameter[] commandParameters, bool startTransaction)
        {
            return GetDataset(CommandType.StoredProcedure, commandText, commandParameters, startTransaction);
        }

        public SqlDataReader GetDataReaderByStoredProcedure(string commandText, SqlParameter[] commandParameters)
        {
            return GetDataReader(CommandType.StoredProcedure, commandText, commandParameters, false);
        }

        public SqlDataReader GetDataReaderByStoredProcedure(string commandText, SqlParameter[] commandParameters, bool startTransaction)
        {
            return GetDataReader(CommandType.StoredProcedure, commandText, commandParameters, startTransaction);
        }

        private int ExecuteNonQuery(CommandType commandType, string commandText, SqlParameter[] commandParameters, bool startTransaction)
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;
            int executeRecord = 0;
            try
            {
                connection = new SqlConnection(ConnectionString + ConnectionTimeOut);
                connection.Open();

                //start a transaction
                if (startTransaction)
                {
                    transaction = connection.BeginTransaction();
                    executeRecord += SqlHelper.ExecuteNonQuery(transaction, commandType, commandText, commandParameters);
                    transaction.Commit();
                }
                else
                {
                    executeRecord += SqlHelper.ExecuteNonQuery(connection, commandType, commandText, commandParameters);
                }

                return executeRecord;
            }
            catch (SqlException ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }

                throw ex;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }

        private DataSet GetDataset(CommandType commandType, string commandText, SqlParameter[] commandParameters, bool startTransaction)
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;
            DataSet executeDataSet = new DataSet();
            try
            {
                connection = new SqlConnection(ConnectionString + ConnectionTimeOut);
                connection.Open();

                if (startTransaction)
                {
                    //start a transaction
                    transaction = connection.BeginTransaction();
                    executeDataSet = SqlHelper.ExecuteDataset(transaction, commandType, commandText, commandParameters);
                    transaction.Commit();
                }
                else
                {
                    executeDataSet = SqlHelper.ExecuteDataset(connection, commandType, commandText, commandParameters);
                }

                return executeDataSet;
            }
            catch (SqlException ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }

                throw ex;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }

        private SqlDataReader GetDataReader(CommandType commandType, string commandText, SqlParameter[] commandParameters, bool startTransaction)
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;
            SqlDataReader sqlDataReader = null;
            try
            {
                connection = new SqlConnection(ConnectionString + ConnectionTimeOut);
                connection.Open();

                if (startTransaction)
                {
                    //start a transaction
                    transaction = connection.BeginTransaction();
                    sqlDataReader = SqlHelper.ExecuteReader(transaction, commandType, commandText, commandParameters);
                    transaction.Commit();
                }
                else
                {
                    sqlDataReader = SqlHelper.ExecuteReader(connection, commandType, commandText, commandParameters);
                }
                return sqlDataReader;
            }
            catch (SqlException ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }

                throw ex;
            }
        }
    }
}

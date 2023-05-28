using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace RabbitMQWorkerProject.Services
{
    public class OracleDatabaseService
    {
        private string connectionString = "Data Source=hr@//MarcinKomp.local:1521/XEPDB1;User Id=test;Password=12345";

        public OracleConnection GetConnection()
        {
            return new OracleConnection(connectionString);
        }

        public void ExecuteQuery(string query, object parameters = null)
        {
            using (OracleConnection connection = GetConnection())
            {
                connection.Open();
                connection.Execute(query, parameters);
            }
        }

        public T ExecuteScalar<T>(string query, object parameters = null)
        {
            using (OracleConnection connection = GetConnection())
            {
                connection.Open();
                return connection.ExecuteScalar<T>(query, parameters);
            }
        }

        public IEnumerable<T> ExecuteQuery<T>(string query, object parameters = null)
        {
            using (OracleConnection connection = GetConnection())
            {
                connection.Open();
                return connection.Query<T>(query, parameters);
            }
        }

        // Inne metody do obsługi bazy danych
    }
}

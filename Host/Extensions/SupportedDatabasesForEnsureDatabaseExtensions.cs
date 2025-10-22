using DbUp;
using DbUp.Engine.Output;
using Npgsql;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace ContinentExpress.TT.Api.Extensions
{
    public static class SupportedDatabasesForEnsureDatabaseExtensions
    {

        ///<Summary>
        /// Проверяет существует ли схема и создает её если она отсутствует в БД.
        ///</Summary>
        ///<param name="certificate"></param>
        ///<param name="connectionString">Строка подключения</param>
        ///<param name="logger">Средство журналирования</param>
        ///<param name="schema">Требуемая схема</param>
        ///<param name="supported">Сертификат шифрования</param>
        public static void PostgresqlSchema(this SupportedDatabasesForEnsureDatabase supported, string connectionString, string schema, IUpgradeLog logger, X509Certificate2? certificate = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, "connectionString");
            ArgumentNullException.ThrowIfNull(logger, "logger");
            NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrEmpty(npgsqlConnectionStringBuilder.Database))
            {
                throw new InvalidOperationException("The connection string does not specify a database name.");
            }

            NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder2 = new NpgsqlConnectionStringBuilder(npgsqlConnectionStringBuilder.ConnectionString);
            if (!string.IsNullOrEmpty(npgsqlConnectionStringBuilder2.Password))
            {
                npgsqlConnectionStringBuilder2.Password = "******";
            }

            logger.LogInformation("Master ConnectionString => {0}", npgsqlConnectionStringBuilder2.ConnectionString);
            using NpgsqlConnection npgsqlConnection = new NpgsqlConnection(npgsqlConnectionStringBuilder.ConnectionString);
            if (certificate != null)
            {
                npgsqlConnection.ProvideClientCertificatesCallback = (ProvideClientCertificatesCallback)Delegate.Combine(npgsqlConnection.ProvideClientCertificatesCallback, (ProvideClientCertificatesCallback)delegate (X509CertificateCollection certs)
                {
                    certs.Add(certificate);
                });
            }

            npgsqlConnection.Open();
            using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand("SELECT 1 FROM pg_namespace WHERE nspname = '" + schema + "' limit 1;", npgsqlConnection)
            {
                CommandType = CommandType.Text
            })
            {
                if (Convert.ToInt32(npgsqlCommand.ExecuteScalar()) == 1)
                {
                    return;
                }
            }

            using (NpgsqlCommand npgsqlCommand2 = new NpgsqlCommand("CREATE SCHEMA " + schema + ";", npgsqlConnection)
            {
                CommandType = CommandType.Text
            })
            {
                npgsqlCommand2.ExecuteNonQuery();
            }

            logger.LogInformation("Created schema {0}", schema);
        }
    }
}

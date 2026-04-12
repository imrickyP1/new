using Microsoft.Data.SqlClient;

namespace BAMS.Modules
{
    public static class DatabaseConnection
    {
        private static readonly string connectionString =
            "Server=MSI;Database=BAMS;Trusted_Connection=True;TrustServerCertificate=True;";

        public static string ConnectionString => connectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}

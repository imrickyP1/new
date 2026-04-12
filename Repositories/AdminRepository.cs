using BAMS.Modules;
using Microsoft.Data.SqlClient;

namespace BAMS.Repositories
{
    internal class AdminRepository
    {
        public bool Login(string username, string password)
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();

            string query = @"SELECT COUNT(*)
                             FROM Admins
                             WHERE Username=@username
                             AND Password=@password";

            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());

            conn.Open();

            int result = (int)cmd.ExecuteScalar();

            return result > 0;
        }

        public int GetAdminId(string username, string password)
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();

            string query = @"SELECT AdminID 
                             FROM Admins
                             WHERE Username=@username
                             AND Password=@password";

            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());

            conn.Open();

            object result = cmd.ExecuteScalar();

            if (result != null)
                return Convert.ToInt32(result);

            return 0;
        }
    }
}
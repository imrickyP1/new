using System.Data;
using BAMS.Modules;
using Microsoft.Data.SqlClient;

namespace BAMS.Repositories
{
    internal class UserRepository
    {
        public DataTable GetAllUsers()
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();

            string query = @"SELECT 
                            EmployeeID,
                            Name,
                            Gender,
                            Position
                            FROM Users";

            SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            return dt;
        }

        public DataTable SearchUsers(string keyword)
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();

            string query = @"SELECT 
                            EmployeeID,
                            Name,
                            Gender,
                            Position
                            FROM Users
                            WHERE Name LIKE @keyword";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            return dt;
        }

        public void AddUser(int employeeId, string name, string gender, string position)
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();

            string query = @"INSERT INTO dbo.Users
        (EmployeeID,Name,Gender,Position,DateCreated)
        VALUES
        (@EmployeeID,@Name,@Gender,@Position,GETDATE())";

            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Gender", gender);
            cmd.Parameters.AddWithValue("@Position", position);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void DeleteUser(int employeeId)
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();

            string query = "DELETE FROM Users WHERE EmployeeID = @EmployeeID";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@EmployeeID", employeeId);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public DataTable GetUserById(int employeeId)
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();

            string query = "SELECT * FROM Users WHERE EmployeeID = @EmployeeID";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@EmployeeID", employeeId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            return dt;
        }

        public void UpdateUser(int employeeId, string name, string gender, string position)
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();

            string query = @"UPDATE Users
                    SET Name=@Name,
                        Gender=@Gender,
                        Position=@Position
                    WHERE EmployeeID=@EmployeeID";

            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Gender", gender);
            cmd.Parameters.AddWithValue("@Position", position);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public bool IsAdmin(int employeeId)
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();

            string query = "SELECT IsAdmin FROM Users WHERE EmployeeID=@EmployeeID";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@EmployeeID", employeeId);

            conn.Open();

            object result = cmd.ExecuteScalar();

            if (result != null && result != DBNull.Value)
                return Convert.ToBoolean(result);

            return false;
        }
    }
}
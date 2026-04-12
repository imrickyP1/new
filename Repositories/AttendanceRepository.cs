using System;
using System.Data;
using BAMS.Modules;
using Microsoft.Data.SqlClient;

namespace BAMS.Repositories
{
    public class AttendanceRepository
    {
        private string connectionString =
            @"Data Source=MSI;
              Initial Catalog=BAMS;
              Integrated Security=True";

        public DataTable GetAttendanceLogs(DateTime from, DateTime to, string name, string role)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT 
                    a.AttendanceId,
                    a.EmployeeID,
                    u.FullName,
                    u.Position,
                    a.Day,
                    a.AM_In,
                    a.AM_Out,
                    a.PM_In,
                    a.PM_Out,
                    a.Undertime,
                    a.Overtime,
                    a.TotalHours
                FROM Attendance a
                INNER JOIN Users u ON a.EmployeeID = u.EmployeeID
                WHERE a.Day BETWEEN @From AND @To
                AND (@Name = '' OR u.FullName LIKE '%' + @Name + '%')
                AND (@Role = 'All' OR u.Position = @Role)
                ORDER BY a.Day DESC";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@From", from.Date);
                cmd.Parameters.AddWithValue("@To", to.Date);
                cmd.Parameters.AddWithValue("@Name", name ?? "");
                cmd.Parameters.AddWithValue("@Role", role ?? "All");

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                adapter.Fill(dt);

                return dt;
            }
        }

        public DataTable GetMonthlyAttendance(int month, int year)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT
                    u.FullName,
                    u.Position,
                    COUNT(a.AttendanceId) AS DaysPresent,
                    SUM(a.TotalHours) AS TotalHours,
                    SUM(a.Overtime) AS TotalOvertime,
                    SUM(a.Undertime) AS TotalUndertime
                FROM Attendance a
                INNER JOIN Users u ON a.EmployeeID = u.EmployeeID
                WHERE MONTH(a.Day) = @Month AND YEAR(a.Day) = @Year
                GROUP BY u.FullName, u.Position
                ORDER BY u.FullName";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Month", month);
                cmd.Parameters.AddWithValue("@Year", year);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                adapter.Fill(dt);

                return dt;
            }
        }


        public void UpdateComputedHours(int attendanceId, double undertime, double overtime, double totalHours)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                UPDATE Attendance
                SET Undertime = @UT,
                    Overtime = @OT,
                    TotalHours = @TH
                WHERE AttendanceId = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@UT", undertime);
                cmd.Parameters.AddWithValue("@OT", overtime);
                cmd.Parameters.AddWithValue("@TH", totalHours);
                cmd.Parameters.AddWithValue("@Id", attendanceId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void InsertAttendance(int employeeId, DateTime day,
            TimeSpan? amIn, TimeSpan? amOut,
            TimeSpan? pmIn, TimeSpan? pmOut)
        {
            string query = @"INSERT INTO Attendance
            (EmployeeID, Day, AM_In, AM_Out, PM_In, PM_Out)
            VALUES
            (@empid,@day,@amin,@amout,@pmin,@pmout)";

            using SqlConnection con = new SqlConnection(connectionString);
            using SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@empid", employeeId);
            cmd.Parameters.AddWithValue("@day", day);
            cmd.Parameters.AddWithValue("@amin", (object?)amIn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@amout", (object?)amOut ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pmin", (object?)pmIn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pmout", (object?)pmOut ?? DBNull.Value);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        public int GetTotalUsers()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Users";

                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();

                return (int)cmd.ExecuteScalar();
            }
        }

        public int GetTotalOfficials()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Users WHERE Position='Official'";

                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();

                return (int)cmd.ExecuteScalar();
            }
        }

        public int GetTotalStaff()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Users WHERE Position='Staff'";

                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();

                return (int)cmd.ExecuteScalar();
            }
        }

        public int GetTodayAttendance()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Attendance WHERE Day = CAST(GETDATE() AS DATE)";

                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();

                return (int)cmd.ExecuteScalar();
            }
        }

        public void ProcessAttendance()
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();

                string query = @"
                WITH OrderedLogs AS (
                    SELECT *,
                           ROW_NUMBER() OVER (PARTITION BY EmployeeID, CAST(TimeLog AS DATE) ORDER BY TimeLog) AS rn
                    FROM Attendance
                )
                SELECT * FROM OrderedLogs";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    var records = new Dictionary<string, List<DateTime>>();

                    while (reader.Read())
                    {
                        string empId = reader["EmployeeID"].ToString();
                        DateTime log = Convert.ToDateTime(reader["TimeLog"]);

                        string key = empId + "_" + log.Date.ToString("yyyy-MM-dd");

                        if (!records.ContainsKey(key))
                            records[key] = new List<DateTime>();

                        records[key].Add(log);
                    }

                    reader.Close();

                    foreach (var record in records)
                    {
                        var logs = record.Value.OrderBy(x => x).ToList();

                        DateTime timeIn = logs.First();
                        DateTime timeOut = logs.Count > 1 ? logs.Last() : timeIn;

                        double totalHours = (timeOut - timeIn).TotalHours;

                        int tardiness = 0;
                        TimeSpan expected = new TimeSpan(8, 0, 0);

                        if (timeIn.TimeOfDay > expected)
                            tardiness = (int)(timeIn.TimeOfDay - expected).TotalMinutes;

                        string updateQuery = @"
                        UPDATE Attendance
                        SET DateOnly = @DateOnly,
                            TimeIn = @TimeIn,
                            TimeOut = @TimeOut,
                            TotalHours = @TotalHours,
                            TardinessMinutes = @Tardiness
                        WHERE EmployeeID = @EmpID AND CAST(TimeLog AS DATE) = @DateOnly";

                        using (var updateCmd = new SqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@DateOnly", timeIn.Date);
                            updateCmd.Parameters.AddWithValue("@TimeIn", timeIn.TimeOfDay);
                            updateCmd.Parameters.AddWithValue("@TimeOut", timeOut.TimeOfDay);
                            updateCmd.Parameters.AddWithValue("@TotalHours", totalHours);
                            updateCmd.Parameters.AddWithValue("@Tardiness", tardiness);
                            updateCmd.Parameters.AddWithValue("@EmpID", record.Key.Split('_')[0]);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
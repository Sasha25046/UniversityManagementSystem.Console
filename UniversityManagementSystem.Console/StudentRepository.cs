
using System.Data;
using System.Collections.Generic;
using MySqlConnector;
using System; // Потрібен для Convert.ToInt32

namespace UniversityManagementSystem.Console
{
    public class StudentRepository : IStudentRepository
    {
        private readonly MySqlConnection _connection;

        public StudentRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public IEnumerable<Student> GetActiveStudents()
        {
            var students = new List<Student>();

            using (var command = new MySqlCommand("SELECT * FROM view_ActiveStudents", _connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        students.Add(new Student
                        {
                            StudentId = reader.GetInt32("student_id"),
                            FirstName = reader.GetString("first_name"),
                            LastName = reader.GetString("last_name"),
                            ProgramName = reader.GetString("program_name"),
                            DepartmentName = reader.GetString("department_name")
                        });
                    }
                }
            }
            return students;
        }

      
        public int GetStudentIdByUserId(int userId)
        {
            using (var command = new MySqlCommand("SELECT student_id FROM Students WHERE user_id = @UserId", _connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        public void SoftDelete(int studentId, int deleterUserId)
        {
            using (var command = new MySqlCommand("sp_SoftDeleteStudent", _connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("p_student_id", studentId);
                command.Parameters.AddWithValue("p_deleter_user_id", deleterUserId);

                command.ExecuteNonQuery();
            }
        }
    }
}
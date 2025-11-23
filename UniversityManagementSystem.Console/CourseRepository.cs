
using MySqlConnector;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UniversityManagementSystem.Console.NoSql; 

namespace UniversityManagementSystem.Console
{
    public interface ICourseRepository
    {
        void UpsertTestCourses(List<BenchmarkDataGenerator.CourseSqlData> courses, int adminUserId);
        List<int> FindByTopicSql(string topic);          
        void DeleteTestCourses(int startId);
    }

    public class CourseRepository : ICourseRepository
    {
        private readonly MySqlConnection _connection;

        public CourseRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

       
        public void UpsertTestCourses(List<BenchmarkDataGenerator.CourseSqlData> courses, int adminUserId)
        {
            foreach (var course in courses)
            {
                using (var command = new MySqlCommand("sp_UpdateCourseDetailsJson", _connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("p_course_id", course.CourseId);
                    command.Parameters.AddWithValue("p_course_name", course.Name);
                    command.Parameters.AddWithValue("p_department_id", course.DepartmentId);
                    command.Parameters.AddWithValue("p_credits", course.Credits);
                    command.Parameters.AddWithValue("p_details_json", course.DetailsJson);
                    command.Parameters.AddWithValue("p_updater_user_id", adminUserId);

                    command.ExecuteNonQuery();
                }
            }
        }


        public List<int> FindByTopicSql(string topic)
        {
            var courseIds = new List<int>();
            using (var command = new MySqlCommand("sp_FindCoursesByTopicInJson", _connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("p_search_topic", topic);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read()) { courseIds.Add(reader.GetInt32("course_id")); }
                }
            }
            return courseIds;
        }

        public void DeleteTestCourses(int startId)
        {
            using (var command = new MySqlCommand($"DELETE FROM Courses WHERE course_id >= @StartId", _connection))
            {
                command.Parameters.AddWithValue("@StartId", startId);
                command.ExecuteNonQuery();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySqlConnector;

namespace UniversityManagementSystem.Console
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly string _connectionString;
        private readonly MySqlConnection _connection;

        public IStudentRepository Students { get; }

        public UnitOfWork(string connectionString)
        {
            _connectionString = connectionString;

            _connection = new MySqlConnection(_connectionString);
            _connection.Open();

            Students = new StudentRepository(_connection);
        }

        public int RegisterNewStudent(string firstName, string lastName, string email, string passwordHash, int programId, int creatorId)
        {
            using (var command = new MySqlCommand("sp_RegisterNewStudentTransaction", _connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("p_first_name", firstName);
                command.Parameters.AddWithValue("p_last_name", lastName);
                command.Parameters.AddWithValue("p_email", email);
                command.Parameters.AddWithValue("p_password_hash", passwordHash);
                command.Parameters.AddWithValue("p_program_id", programId);
                command.Parameters.AddWithValue("p_creator_user_id", creatorId);

                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}

using System;

namespace UniversityManagementSystem.Console
{
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepository Students { get; }
        ICourseRepository Courses { get; } // ДОДАНО

        int RegisterNewStudent(
            string firstName,
            string lastName,
            string email,
            string passwordHash,
            int programId,
            int creatorId);
    }
}
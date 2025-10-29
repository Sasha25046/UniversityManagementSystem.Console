using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityManagementSystem.Console
{
    public interface IStudentRepository
    {
        IEnumerable<Student> GetActiveStudents();

        int GetStudentIdByUserId(int userId);

        void SoftDelete(int studentId, int deleterUserId);
    }
}
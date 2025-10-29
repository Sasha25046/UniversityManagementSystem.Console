using MySqlConnector;
using System;
using System.Linq;

namespace UniversityManagementSystem.Console
{
    class Program
    {
        private const string ConnectionString = "server=localhost;port=3306;database=UniversityDB;uid=root;password=windows123";
        private const int AdminUserId = 1;

        static void Main(string[] args)
        {
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;

            try
            {
                using (var uow = new UnitOfWork(ConnectionString))
                {
                    System.Console.WriteLine("1. Демонстрація UoW: Реєстрація нового студента (Транзакція)...");

                    int newUserId = uow.RegisterNewStudent(
                        "Саша", "Скор", "bohdiazh.a.new@gmail.com", 
                        "secure_hash_for_olena", programId: 2, AdminUserId);

                    System.Console.WriteLine($"   -> Успішна реєстрація. Створено UserID: {newUserId}");



                    if (newUserId > 0)
                    {
                        int actualStudentIdToDelete = uow.Students.GetStudentIdByUserId(newUserId);

                        System.Console.WriteLine("\n2. Демонстрація Repository: Отримання активних студентів (через View)...");

                        if (actualStudentIdToDelete > 0)
                        {
                            System.Console.WriteLine($"   -> Знайдено StudentID: {actualStudentIdToDelete} для видалення.");

                            System.Console.WriteLine("\n3. Демонстрація Repository: Soft Delete студента (через SP)...");

                            uow.Students.SoftDelete(studentId: actualStudentIdToDelete, AdminUserId);

                            System.Console.WriteLine("   -> Soft Delete виконано успішно.");
                            System.Console.WriteLine($"   -> ПЕРЕВІРКА: StudentID {actualStudentIdToDelete} має бути is_deleted=1.");
                        }
                        else
                        {
                            System.Console.WriteLine($"\nПОМИЛКА ЛОГІКИ: Не вдалося знайти StudentID для щойно створеного UserID: {newUserId}. Soft Delete не виконано.");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Console.WriteLine($"\nПОМИЛКА БАЗИ ДАНИХ (MySqlException): {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"\nНеочікувана помилка: {ex.Message}");
            }
            finally
            {
                System.Console.WriteLine("\nПрограма завершила роботу.");
                System.Console.ReadKey();
            }
        }
    }
}
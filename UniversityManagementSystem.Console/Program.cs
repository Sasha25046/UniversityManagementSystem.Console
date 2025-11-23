using System;
using System.Diagnostics;
using System.Linq;
using MongoDB.Driver;
using MySqlConnector;
using StackExchange.Redis;
using UniversityManagementSystem.Console.NoSql;

namespace UniversityManagementSystem.Console
{
    class Program
    {
        private const string ConnectionString = "server=localhost;port=3306;database=UniversityDB;uid=root;password=windows123";
        private const int AdminUserId = 1;

        private const string RedisHost = "redis-12766.c80.us-east-1-2.ec2.cloud.redislabs.com";
        private const int RedisPort = 12766;
        private const string RedisPassword = "IUH9TPbyk4LvGTRpATvtfrBydJFvZLDW";
        private const string RedisUser = "default";

        static void Main(string[] args)
        {
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;

            System.Console.WriteLine("--- Лабораторна робота: Гібридні бази даних (MySQL, MongoDB, Redis) ---");
            System.Console.WriteLine($"Тестування на {BenchmarkDataGenerator.COURSES_COUNT} записах.");

            IConnectionMultiplexer? redis = null;

            try
            {
                using (var uow = new UnitOfWork(ConnectionString))
                {
                    var sqlRepo = uow.Courses;
                    var noSqlRepo = new CourseDetailsNoSqlRepository();

                    System.Console.WriteLine("\n--- ЕТАП I: БЕНЧМАРК (MongoDB vs MySQL JSON) ---");
                    System.Console.WriteLine("1. Підготовка даних.");

                    sqlRepo.DeleteTestCourses(BenchmarkDataGenerator.START_COURSE_ID);
                    noSqlRepo.ClearCollection();
                    System.Console.WriteLine("-> Тестові дані очищено.");

                    var mongoData = BenchmarkDataGenerator.GenerateNoSqlData();
                    var sqlData = BenchmarkDataGenerator.GenerateSqlData();


                    var sw = Stopwatch.StartNew();
                    sqlRepo.UpsertTestCourses(sqlData, AdminUserId);
                    sw.Stop();
                    System.Console.WriteLine($"\n2. Запис MySQL (JSON): {sw.ElapsedMilliseconds} мс.");

                    sw.Restart();
                    noSqlRepo.UpsertMany(mongoData);
                    sw.Stop();
                    System.Console.WriteLine($"2. Запис MongoDB (Atlas): {sw.ElapsedMilliseconds} мс.");

                    System.Console.WriteLine("\n3.3. ТЕСТ: Пошук в масиві Topics (Multikey Index).");
                    string searchTopic = BenchmarkDataGenerator.SEARCH_TOPIC;

                    sw.Restart();
                    var sqlTopicResults = sqlRepo.FindByTopicSql(searchTopic);
                    sw.Stop();
                    System.Console.WriteLine($"  MySQL (Topic Search): {sw.ElapsedMilliseconds} мс. Знайдено: {sqlTopicResults.Count}");

                    sw.Restart();
                    var mongoTopicResults = noSqlRepo.FindByTopic(searchTopic);
                    sw.Stop();
                    System.Console.WriteLine($"  MongoDB (Topic Search): {sw.ElapsedMilliseconds} мс. Знайдено: {mongoTopicResults.Count}");

                    System.Console.WriteLine("\n--- ПОРІВНЯННЯ ШВИДКОДІЇ ЗАВЕРШЕНО ---");

                    System.Console.WriteLine("\n--- ЕТАП II: ДЕМОНСТРАЦІЯ REDIS (Key-Value) ---");
                    System.Console.WriteLine("4. Демонстрація Redis: Кешування GPA (Пункт 6)");

                    var options = new ConfigurationOptions
                    {
                        EndPoints = { { RedisHost, RedisPort } },
                        User = RedisUser,
                        Password = RedisPassword,
                        AbortOnConnectFail = false,
                        SyncTimeout = 5000
                    };

                    redis = ConnectionMultiplexer.Connect(options);
                    var gpaCache = new GpaCacheRepository(redis);

                    System.Console.WriteLine($"\n-> Базовий запит PING: {gpaCache.Ping()}");

                    int testStudentId = 1001;
                    decimal calculatedGpa = 4.75M;

                    var initialGpa = gpaCache.GetCachedGpa(testStudentId);
                    System.Console.WriteLine($"1. GPA з кешу (спочатку): {initialGpa?.ToString() ?? "NULL"}");

                    gpaCache.SetCachedGpa(testStudentId, calculatedGpa);
                    System.Console.WriteLine($"2. GPA записано в кеш (SET).");

                    var cachedGpa = gpaCache.GetCachedGpa(testStudentId);
                    System.Console.WriteLine($"3. GPA з кешу (після запису): {cachedGpa}");

                    gpaCache.DeleteCache(testStudentId);
                    System.Console.WriteLine($"4. Ключ видалено (DELETE).");

                    var afterDeleteGpa = gpaCache.GetCachedGpa(testStudentId);
                    System.Console.WriteLine($"4.1. GPA з кешу (після видалення): {afterDeleteGpa?.ToString() ?? "NULL"}");

                    System.Console.WriteLine("\n--- ДЕМОНСТРАЦІЯ УСПІШНО ЗАВЕРШЕНА ---");
                }
            }
            catch (MySqlException ex)
            {
                System.Console.WriteLine($"\nПОМИЛКА БАЗИ ДАНИХ (MySQL): Переконайтеся, що SP створено: {ex.Message}");
            }
            catch (MongoWriteException ex)
            {
                System.Console.WriteLine($"\nПОМИЛКА MONGO: Переконайтеся, що Atlas-кластер доступний: {ex.Message}");
            }
            catch (RedisConnectionException ex)
            {
                System.Console.WriteLine($"\nПОМИЛКА REDIS: Не вдалося підключитися. Переконайтеся, що облікові дані Redis Cloud коректні: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"\nНеочікувана помилка: {ex.Message}");
            }
            finally
            {
                redis?.Dispose();
                System.Console.WriteLine("\nПрограма завершила роботу. Натисніть будь-яку клавішу...");
                System.Console.ReadKey();
            }
        }
    }
}
// Шлях: UniversityManagementManagementSystem.Console/NoSql/GpaCacheRepository.cs

using StackExchange.Redis;
using System;

namespace UniversityManagementSystem.Console.NoSql
{
    public class GpaCacheRepository
    {
        // Константа підключення (використовується в Program.cs)
        // private const string REDIS_CONNECTION_STRING = "localhost:6379"; 

        private const string GPA_KEY_PREFIX = "student:gpa:";

        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);

        private readonly IDatabase _redisDb;

        public GpaCacheRepository(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        public string GetKey(int studentId)
        {
            return $"{GPA_KEY_PREFIX}{studentId}";
        }

        public bool Ping()
        {
            return _redisDb.Execute("PING").ToString() == "PONG";
        }

        public void DeleteCache(int studentId)
        {
            string key = GetKey(studentId);
            _redisDb.KeyDelete(key);
        }

        // --- ЛОГІКА КЕШУВАННЯ ---

        public decimal? GetCachedGpa(int studentId)
        {
            string key = GetKey(studentId);
            var value = _redisDb.StringGet(key);

            if (value.HasValue)
            {
                if (decimal.TryParse(value, out decimal gpa))
                {
                    return gpa;
                }
            }
            return null;
        }

        
        public void SetCachedGpa(int studentId, decimal gpa)
        {
            string key = GetKey(studentId);
            _redisDb.StringSet(key, gpa.ToString(), CacheExpiry);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text.Json;
using UniversityManagementSystem.Console.NoSql;

namespace UniversityManagementSystem.Console.NoSql
{
    public static class BenchmarkDataGenerator
    {
        private static readonly Random Rnd = new Random();

        public const int COURSES_COUNT = 10000;
        public const int START_COURSE_ID = 10000;
        public const string SEARCH_TOPIC = "AI-Fundamentals";

        public const int SEARCH_MIN_RAM = 32; 
        public const int TARGET_RAM_GB = 64;   

        private static ProgramSection CreateRandomSection(bool includeSearchTopic)
        {
            var section = new ProgramSection
            {
                Title = $"Розділ {Rnd.Next(1, 5)}: Вступ",
                DurationWeeks = Rnd.Next(1, 4),
                Topics = new List<string> { "Базові_алгоритми", "Теорія_систем", "Оцінка_ризиків" }
            };

            if (includeSearchTopic)
            {
                section.Topics.Add(SEARCH_TOPIC);
            }
            return section;
        }

        public static List<CourseDetailsDocument> GenerateNoSqlData()
        {
            var documents = new List<CourseDetailsDocument>();

            for (int i = 0; i < COURSES_COUNT; i++)
            {
                int courseId = START_COURSE_ID + i;
                bool includeTopic = (i % 10 == 0);

                int minRam = (i % 10 == 0) ? TARGET_RAM_GB : Rnd.Next(4, 32);

                documents.Add(new CourseDetailsDocument
                {
                    CourseId = courseId,
                    FullDescription = $"Детальний опис курсу #{courseId}: Сучасні тенденції в IT.",

                    MinRamGb = minRam,

                    ProgramOutline = new List<ProgramSection> {
                        CreateRandomSection(includeTopic),
                        CreateRandomSection(false)
                    },
                    ResourceLinks = new List<string> {
                        "https://docs.link/1",
                        "https://video.link/2"
                    }
                });
            }
            return documents;
        }


        public class CourseSqlData
        {
            public int CourseId { get; set; }
            public string Name { get; set; }
            public int DepartmentId { get; set; }
            public decimal Credits { get; set; }
            public string DetailsJson { get; set; }
        }

        public static List<CourseSqlData> GenerateSqlData()
        {
            var documents = GenerateNoSqlData();
            var sqlDataList = new List<CourseSqlData>();

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

            foreach (var doc in documents)
            {
                var jsonDoc = new
                {
                    full_description = doc.FullDescription,
                    program_outline = doc.ProgramOutline,
                    resource_links = doc.ResourceLinks,
                    min_ram_gb = doc.MinRamGb 
                };

                sqlDataList.Add(new CourseSqlData
                {
                    CourseId = doc.CourseId,
                    Name = $"Тестовий Курс {doc.CourseId}",
                    DepartmentId = Rnd.Next(1, 4),
                    Credits = (decimal)Rnd.Next(30, 60) / 10.0M,
                    DetailsJson = JsonSerializer.Serialize(jsonDoc, options)
                });
            }
            return sqlDataList;
        }
    }
}
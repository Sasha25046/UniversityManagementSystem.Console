using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace UniversityManagementSystem.Console.NoSql
{
    public class ProgramSection
    {
        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("duration_weeks")]
        public int DurationWeeks { get; set; }

        [BsonElement("topics")]
        public List<string> Topics { get; set; } = new List<string>();
    }

  
    public class CourseDetailsDocument
    {
        
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault] 
        public string? Id { get; set; }
       
        [BsonElement("course_id")]
        public int CourseId { get; set; }

        [BsonElement("min_ram_gb")]
        public int MinRamGb { get; set; } 

        [BsonElement("full_description")]
        public string FullDescription { get; set; } = string.Empty;

        [BsonElement("program_outline")]
        public List<ProgramSection> ProgramOutline { get; set; } = new List<ProgramSection>();

        [BsonElement("resource_links")]
        public List<string> ResourceLinks { get; set; } = new List<string>();
    }
}
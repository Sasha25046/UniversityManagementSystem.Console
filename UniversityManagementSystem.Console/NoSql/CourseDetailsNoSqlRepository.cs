
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace UniversityManagementSystem.Console.NoSql
{
    public interface ICourseDetailsNoSqlRepository
    {
        void UpsertMany(List<CourseDetailsDocument> documents);

       
        List<CourseDetailsDocument> FindByTopic(string topic);

        void ClearCollection();
    }

   
    public class CourseDetailsNoSqlRepository : ICourseDetailsNoSqlRepository
    {
        private const string CONNECTION_STRING = "mongodb://localhost:27017";
        private const string DATABASE_NAME = "UniversityNoSqlDB";
        private const string COLLECTION_NAME = "course_details";
        private readonly IMongoCollection<CourseDetailsDocument> _detailsCollection;

        public CourseDetailsNoSqlRepository()
        {
            var settings = MongoClientSettings.FromConnectionString(CONNECTION_STRING);
            var client = new MongoClient(settings);
            var database = client.GetDatabase(DATABASE_NAME);
            _detailsCollection = database.GetCollection<CourseDetailsDocument>(COLLECTION_NAME);

            var indexKeysCourseId = Builders<CourseDetailsDocument>.IndexKeys.Ascending(d => d.CourseId);
            try { _detailsCollection.Indexes.CreateOne(new CreateIndexModel<CourseDetailsDocument>(indexKeysCourseId, new CreateIndexOptions { Unique = true, Name = "Unique_CourseId_Index" })); } catch (MongoCommandException) { }

          
            var coveredIndexKeys = Builders<CourseDetailsDocument>.IndexKeys
                .Ascending(d => d.MinRamGb)
                .Ascending(d => d.CourseId); 

            try
            {
                _detailsCollection.Indexes.CreateOne(new CreateIndexModel<CourseDetailsDocument>(coveredIndexKeys, new CreateIndexOptions { Name = "RamGb_Covered_Index" }));
            }
            catch (MongoCommandException) { }

            var multikeyIndexKeys = Builders<CourseDetailsDocument>.IndexKeys.Ascending("program_outline.topics");
            try { _detailsCollection.Indexes.CreateOne(new CreateIndexModel<CourseDetailsDocument>(multikeyIndexKeys, new CreateIndexOptions { Name = "Topics_Multikey_Index" })); } catch (MongoCommandException) { }
        }

        // --- Операції Upsert та Clear (залишаються без змін) ---

        public void UpsertMany(List<CourseDetailsDocument> documents)
        {
            var models = new List<WriteModel<CourseDetailsDocument>>();
            foreach (var doc in documents)
            {
                var filter = Builders<CourseDetailsDocument>.Filter.Eq(d => d.CourseId, doc.CourseId);
                models.Add(new ReplaceOneModel<CourseDetailsDocument>(filter, doc) { IsUpsert = true });
            }
            if (models.Any())
            {
                _detailsCollection.BulkWrite(models);
            }
        }

       
        public List<CourseDetailsDocument> FindByTopic(string topic)
        {
            var filter = Builders<CourseDetailsDocument>.Filter.Eq("program_outline.topics", topic);

            var projection = Builders<CourseDetailsDocument>.Projection
                .Include(d => d.CourseId)
                .Exclude(d => d.Id);

            return _detailsCollection.Find(filter)
                .Project<CourseDetailsDocument>(projection)
                .ToList();
        }

        public void ClearCollection()
        {
            _detailsCollection.DeleteMany(Builders<CourseDetailsDocument>.Filter.Empty);
        }
    }
}
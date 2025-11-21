// Шлях: UniversityManagementSystem.Console/NoSql/CourseDetailsNoSqlRepository.cs

using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace UniversityManagementSystem.Console.NoSql
{
    public interface ICourseDetailsNoSqlRepository
    {
        void UpsertMany(List<CourseDetailsDocument> documents);
        List<CourseDetailsDocument> FindByRamRange(int minRamGb); 
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

            var indexKeys = Builders<CourseDetailsDocument>.IndexKeys.Ascending(d => d.CourseId);
            try
            {
                _detailsCollection.Indexes.CreateOne(
                    new CreateIndexModel<CourseDetailsDocument>(
                        indexKeys,
                        new CreateIndexOptions { Unique = true, Name = "Unique_CourseId_Index" }
                    )
                );
            }
            catch (MongoCommandException) { }
        }


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

       
       
        public List<CourseDetailsDocument> FindByRamRange(int minRamGb)
        {
            var filter = Builders<CourseDetailsDocument>.Filter.Gte(d => d.MinRamGb, minRamGb);

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
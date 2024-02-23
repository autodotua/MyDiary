using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyDiary.Core.Models.Converters;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace MyDiary.Core.Models
{
    internal class DiaryDbContext : DbContext
    {
        internal static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = false,
            Converters = { 
                new JsonBlockConverter(),
                new Json2DArrayConverter() 
            }
        };
        private const string CurrentVersion = "20240223";
        private const string dbName = "db.sqlite";

        private static readonly string connectionString = $"Data Source={dbName}";
        private DiaryDbContext()
        {
            Database.EnsureCreated();
        }

        public DbSet<Config> Configs { get; set; }
        public DbSet<Document> Documents { get; set; }
        public static void Migrate()
        {
            if (File.Exists(dbName))
            {
                using SqliteConnection sqlite = new SqliteConnection(connectionString);
                sqlite.Open();
                SqliteCommand command = new SqliteCommand("select Value from Configs where Key == 'Version'", sqlite);
                SqliteDataReader reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    MigrateXXXX(sqlite);
                }
                else
                {
                    reader.Read();
                    string version = reader.GetString(0);
                }
                sqlite.Close();
            }
            using var db = GetNew();
            var item = db.Configs.FirstOrDefault(p => p.Key == "Version");
            if (item == null)
            {
                db.Configs.Add(new Config("Version", CurrentVersion));
            }
            else
            {
                item.Value = CurrentVersion;
                db.Entry(item).State = EntityState.Modified;
            }
            db.SaveChanges();
            db.Dispose();
        }

        internal static DiaryDbContext GetNew()
        {
            return new DiaryDbContext();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //对于非结构化数据，采用Json的方式进行存储
            var blocksConverter = new EfJsonConverter<IList<Block>>();
            modelBuilder.Entity<Document>()
                .Property(p => p.Blocks)
                .HasConversion(blocksConverter);
        }
        private static void MigrateXXXX(SqliteConnection sqlite)
        {
            //Debug.WriteLine("数据库迁移：" + nameof(Migrate20230408));
            //Console.WriteLine("数据库迁移：" + nameof(Migrate20230408));
            //new SqliteCommand("CREATE INDEX IX_Logs_Type ON Logs (Type);", sqlite).ExecuteNonQuery();
            //new SqliteCommand("CREATE INDEX IX_Logs_Time ON Logs (Time);", sqlite).ExecuteNonQuery();
            //new SqliteCommand("CREATE INDEX IX_Logs_TaskId ON Logs (TaskId);", sqlite).ExecuteNonQuery();

            //new SqliteCommand("CREATE INDEX IX_Tasks_Type ON Tasks (Type);", sqlite).ExecuteNonQuery();
            //new SqliteCommand("CREATE INDEX IX_Tasks_CreateTime ON Tasks (CreateTime);", sqlite).ExecuteNonQuery();
            //new SqliteCommand("CREATE INDEX IX_Tasks_FinishTime ON Tasks (FinishTime);", sqlite).ExecuteNonQuery();
            //new SqliteCommand("CREATE INDEX IX_Tasks_Status ON Tasks (Status);", sqlite).ExecuteNonQuery();

            //new SqliteCommand("CREATE INDEX IX_Presets_Type ON Presets (Type);", sqlite).ExecuteNonQuery();

        }
    }
}

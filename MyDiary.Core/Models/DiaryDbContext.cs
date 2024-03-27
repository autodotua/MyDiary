using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyDiary.Models.Converters;

namespace MyDiary.Models
{
    internal class DiaryDbContext : DbContext
    {
        static DiaryDbContext()
        {
            var dataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
            {
                dataDir = Path.Combine(dataDir, nameof(MyDiary));
            }
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            dbName = Path.Combine(dataDir, "db.diary");
            connectionString = $"Data Source={dbName}";
        }

        private const string CurrentVersion = "20240223";
        private static readonly string dbName;

        private static readonly string connectionString;

        private DiaryDbContext()
        {
            Database.EnsureCreated();
        }

        public DbSet<Tag> Tags { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Binary> Binaries { get; set; }
        public DbSet<PresetStyle> PresetStyles { get; set; }

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
            var presetStyleConverter = new EfJsonConverter<TextStyle>();
            modelBuilder.Entity<Document>()
                .Property(p => p.Blocks)
                .HasConversion(blocksConverter);
            modelBuilder.Entity<PresetStyle>()
                .Property(p => p.Style)
                .HasConversion(presetStyleConverter);

            modelBuilder.Entity<Tag>().HasData(new Tag()
            {
                Id = 1,
                Name = "日记",
                TimeUnit = TimeUnit.Day
            });

            modelBuilder.Entity<PresetStyle>().HasData(new PresetStyle()
            {
                Id = 1,
                Level = 0,
            },
            new PresetStyle()
            {
                Id = 2,
                Level = 1,
                Style = new()
                {
                    Bold = true,
                    FontSize = 24,
                    Alignment = 1
                }
            },
            new PresetStyle()
            {
                Id = 3,
                Level = 2,
                Style = new()
                {
                    Bold = true,
                    FontSize = 22,
                }
            },
            new PresetStyle()
            {
                Id = 4,
                Level = 3,
                Style = new()
                {
                    Bold = true,
                    FontSize = 20,
                }
            },
            new PresetStyle()
            {
                Id = 5,
                Level = 4,
                Style = new()
                {
                    Bold = true,
                    FontSize = 18,
                }
            });
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
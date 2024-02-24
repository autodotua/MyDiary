﻿using MyDiary.Models;
using MyDiary.Models.Converters;
using System.Text.Json;

namespace MyDiary.Managers.Services
{
    public static class ConfigManager
    {
        private static readonly Dictionary<string, object> cache = new Dictionary<string, object>();

        public static T GetConfig<T>(string key, T defaultValue)
        {
            if (cache.ContainsKey(key))
            {
                return (T)cache[key];
            }
            using var db = DiaryDbContext.GetNew();
            var item = db.Configs.Where(p => p.Key == key).FirstOrDefault();
            if (item == null)
            {
                return defaultValue;
            }
            T value = Parse<T>(item.Value);
            cache.Add(key, value);

            //using Logger logger = new Logger();
            //logger.Info($"读取配置：[{key}]={value}");
            return value;
        }
        public static void SetConfig<T>(string key, T value)
        {
            if (cache.ContainsKey(key))
            {
                cache[key] = value;
            }
            using var db = DiaryDbContext.GetNew();
            var item = db.Configs.Where(p => p.Key == key).FirstOrDefault();
            if (item == null)
            {
                item = new Config()
                {
                    Key = key,
                    Value = GetString(value)
                };
                db.Configs.Add(item);
            }
            else
            {
                item.Value = GetString(value);
                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            db.SaveChanges();

            //using Logger logger = new Logger();
            //logger.Info($"写入配置：[{key}]={value}");
        }

        private static string GetString<T>(T data)
        {
            return JsonSerializer.Serialize(data,EfJsonConverter<object>.jsonOptions);
        }

        private static T Parse<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data, EfJsonConverter<object>.jsonOptions);
        }
    }
}

﻿namespace MyDiary.Models
{
    public class Config : ModelBase
    {
        public Config()
        {
        }

        public Config(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}

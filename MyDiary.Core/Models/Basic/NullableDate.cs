namespace MyDiary.Core.Models
{
    public struct NullableDate
    {
        public NullableDate()
        {
        }

        public NullableDate(int year, int? month=null, int? day=null)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        public int Year { get; init; }
        public int? Month { get; init; }
        public int? Day { get; init; }

        public DateTime ToDateTime()
        {
            if (Month.HasValue && Day.HasValue)
            {
                return new DateTime(Year, Month.Value, Day.Value);
            }
            throw new NullReferenceException("未指定的时间");
        }

        public bool IsSpecified => Month.HasValue && Day.HasValue;

        public static NullableDate Today => FromDatetime(DateTime.Today);

        public static NullableDate FromDatetime(DateTime datetime)
        {
            return new NullableDate(datetime.Year, datetime.Month, datetime.Day);
        }

        public override string ToString()
        {
            if(!Month.HasValue)
            {
                return Year.ToString();
            }
            if(!Day.HasValue)
            {
                return $"{Year}-{Month}";
            }
            return $"{Year}-{Month}-{Day}";
        }

    }
}

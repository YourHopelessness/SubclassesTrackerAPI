namespace SubclassesTracker.Api.Extensions
{
    public static class DateTimeExtensions
    {
        public static long ToUnix(this DateTime dateTime)
        {
            // Convert to UTC if it's Local
            if (dateTime.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException("DateTime.Kind должен быть Local или Utc");

            return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
        }

        public static long ToUnix(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToUnixTimeMilliseconds();
        }
    }
}

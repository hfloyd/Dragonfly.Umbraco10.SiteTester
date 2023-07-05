namespace Dragonfly.UmbracoSiteTester
{
    using System;
    using System.Linq;
    using Dragonfly.UmbracoSiteTester;

    public static class HtmlHelpers
    {
        #region Date/Time
        public enum TimezoneFormatOption
        {
            Full,
            Abbreviated
        }
        public static string FormatUtcDateTime(Config TesterConfig,DateTime dt, TimezoneFormatOption TzFormat = TimezoneFormatOption.Full, string LocalTimezone = "", string DateFormat = "ddd MMM d, yyyy", string TimeFormat = "hh:mm tt")
        {
            //var dateFormat = "ddd MMM d, yyyy";
            //var timeFormat = "hh:mm tt";
            if (LocalTimezone == "")
            {
                var configTz = TesterConfig.LocalTimezone;
                LocalTimezone = configTz != "" ? configTz : TimeZoneInfo.Local.Id;
            }
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(LocalTimezone);
            DateTime localizedDT = TimeZoneInfo.ConvertTimeFromUtc(dt, timeZone);

            var zoneFullName = timeZone.IsDaylightSavingTime(localizedDT) ? timeZone.DaylightName : timeZone.StandardName;
            var zoneAbbrevName = zoneFullName.Abbreviate();
            var zoneInfo = TzFormat == TimezoneFormatOption.Abbreviated ? zoneAbbrevName : zoneFullName;

            var stringDate = string.Format("{0} at {1} ({2})", localizedDT.ToString(DateFormat), localizedDT.ToString(TimeFormat), zoneInfo);

            return stringDate;
        }

        //TODO: Move to Dragonfly.Net
        public static string Abbreviate(this string FullString)
        {
            string abbreviation = new string(
                FullString.Split()
                    .Where(s => s.Length > 0 && char.IsLetter(s[0]) && char.IsUpper(s[0]))
                    .Take(3)
                    .Select(s => s[0])
                    .ToArray());

            return abbreviation;
        }

        #endregion

        public static string GetViewsPath(Config TesterConfig)
        {
            var path = TesterConfig.GetAppPluginsPath() + "RazorViews/";
            return path;
        }
    }
}

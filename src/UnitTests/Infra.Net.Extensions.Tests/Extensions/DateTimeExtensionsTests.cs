using System;
using System.Collections.Generic;
using Infra.Net.Extensions.Extensions;
using Xunit;
using Xunit2.Should;

namespace Infra.Net.Extensions.Tests.Extensions
{
    public class DateTimeExtensionsTests
    {
        [Theory]
        [InlineData("2017-01-01T01:00:00", "2017-01-01T02:00:00")]
        [InlineData("2017-01-01", "2017-01-02")]
        [InlineData("2017-01-01", "2017-03-01")]
        public void MustBeBefore(string aDate, string otherDate)
        {
            var firstDate = DateTime.Parse(aDate);
            var secondDate = DateTime.Parse(otherDate);

            firstDate.IsBefore(secondDate).ShouldBeTrue();
        }

        [Theory]
        [InlineData("2017-01-01T01:00:00", "2017-01-01T00:00:00")]
        [InlineData("2017-01-01", "2016-12-31")]
        [InlineData("2017-01-01", "2016-03-01")]
        [InlineData("2017-01-01", "2017-01-01")]
        public void MustNotBeBefore(string aDate, string otherDate)
        {
            var firstDate = DateTime.Parse(aDate);
            var secondDate = DateTime.Parse(otherDate);

            firstDate.IsBefore(secondDate).ShouldBeFalse();
        }

        [Theory]
        [InlineData("2017-01-01T01:00:00", "2017-01-01T00:00:00")]
        [InlineData("2017-01-01", "2016-12-31")]
        [InlineData("2017-01-01", "2016-03-01")]
        public void MustBeAfter(string aDate, string otherDate)
        {
            var firstDate = DateTime.Parse(aDate);
            var secondDate = DateTime.Parse(otherDate);

            firstDate.IsAfter(secondDate).ShouldBeTrue();
        }

        [Theory]
        [InlineData("2017-01-01T01:00:00", "2017-01-01T02:00:00")]
        [InlineData("2017-01-01", "2017-01-02")]
        [InlineData("2017-01-01", "2017-03-01")]
        [InlineData("2017-01-01", "2017-01-01")]
        public void MustNotBeAfter(string aDate, string otherDate)
        {
            var firstDate = DateTime.Parse(aDate);
            var secondDate = DateTime.Parse(otherDate);

            firstDate.IsAfter(secondDate).ShouldBeFalse();
        }

        [Theory]
        [InlineData("2017-01-01T01:00:00", "2017-01-01T00:00:00")]
        [InlineData("2017-01-21", "2017-01-01")]
        [InlineData("2017-03-12T12:33:21", "2017-03-01")]
        [InlineData("2017-02-01", "2017-02-01")]
        public void MustReturnFirstDayOfMonth(string value, string expected)
        {
            var dateValue = DateTime.Parse(value);
            var dateExpected = DateTime.Parse(expected);

            dateValue.FirstDayOfMonth().ShouldBe(dateExpected);
        }

        [Theory]
        [InlineData("2017-01-02T01:50:00", "2017-01-31T00:00:00")]
        [InlineData("2017-01-21", "2017-01-31")]
        [InlineData("2017-03-12T12:33:21", "2017-03-31")]
        [InlineData("2017-02-01", "2017-02-28")]
        public void MustReturnLastDayOfMonth(string value, string expected)
        {
            var dateValue = DateTime.Parse(value);
            var dateExpected = DateTime.Parse(expected);

            dateValue.LastDayOfMonth().ShouldBe(dateExpected);
        }

        [Theory]
        [InlineData("2017-01-02T01:50:00", "2017-02-01T00:00:00")]
        [InlineData("2017-01-21", "2017-02-01")]
        [InlineData("2017-03-12T12:33:21", "2017-04-01")]
        [InlineData("2017-02-01", "2017-03-01")]
        public void MustReturnFirstDayOfNextMonth(string value, string expected)
        {
            var dateValue = DateTime.Parse(value);
            var dateExpected = DateTime.Parse(expected);

            dateValue.FirstDayOfNextMonth().ShouldBe(dateExpected);
        }

        [Theory]
        [InlineData("2017-01-01T01:00:00", "2017-01-01T00:00:00")]
        [InlineData("2017-01-21", "2017-01-15")]
        [InlineData("2017-03-02T12:33:21", "2017-02-26")]
        [InlineData("2017-02-01", "2017-01-29")]
        public void MustReturnFirstDayOfWeek(string value, string expected)
        {
            var dateValue = DateTime.Parse(value);
            var dateExpected = DateTime.Parse(expected);

            dateValue.FirstDayOfWeek().ShouldBe(dateExpected);
        }

        [Theory]
        [InlineData("2017-01-01T01:00:00", "2017-01-07T00:00:00")]
        [InlineData("2017-01-15", "2017-01-21")]
        [InlineData("2017-03-02T12:33:21", "2017-03-04")]
        [InlineData("2017-02-28", "2017-03-04")]
        public void MustReturnLastDayOfWeek(string value, string expected)
        {
            var dateValue = DateTime.Parse(value);
            var dateExpected = DateTime.Parse(expected);

            dateValue.LastDayOfWeek().ShouldBe(dateExpected);
        }

        [Theory]
        [InlineData("2017-01-01T01:00:00", "2017-01-08T00:00:00")]
        [InlineData("2017-01-15", "2017-01-22")]
        [InlineData("2017-03-02T12:33:21", "2017-03-05")]
        [InlineData("2017-02-28", "2017-03-05")]
        public void MustReturnFirstDayOfNextWeek(string value, string expected)
        {
            var dateValue = DateTime.Parse(value);
            var dateExpected = DateTime.Parse(expected);

            dateValue.FirstDayOfNextWeek().ShouldBe(dateExpected);
        }

        [Theory]
        [InlineData("2017-01-01T01:00:00", 2)]
        [InlineData("2017-01-15", 0)]
        [InlineData("2017-03-02T12:33:21", 3)]
        [InlineData("2018-02-02T12:33:21", 10)]
        public void MustReturnNextDays(string value, int days)
        {
            var startDate = DateTime.Parse(value);

            var expectedList = new List<DateTime>();
            for (int i = 0; i <= days; i++)
                expectedList.Add(startDate.AddDays(i));

            startDate.NextDays(days).ShouldBeEqual(expectedList);
        }
        [Theory]
        [InlineData("2017-01-01T01:00:00", -2)]
        [InlineData("2017-01-15", 0)]
        [InlineData("2017-03-02T12:33:21", -3)]
        [InlineData("2018-02-02T12:33:21", -10)]
        public void MustReturnDaysBefore(string value, int days)
        {
            var startDate = DateTime.Parse(value);

            var expectedList = new List<DateTime>();
            for (int i = 0; i >= days; i--)
                expectedList.Add(startDate.AddDays(i));

            startDate.NextDays(days).ShouldBeEqual(expectedList);
        }
        [Theory]
        [InlineData("2017-01-01T01:00:00", 2)]
        [InlineData("2017-01-15", 0)]
        [InlineData("2017-03-02T12:33:21", 3)]
        [InlineData("2018-02-02T12:33:21", 10)]
        public void MustReturnNextDaysOfWeek(string value, int daysAhead)
        {
            var startDate = DateTime.Parse(value);

            var days = daysAhead;
            var expectedList = new List<DateTime>();
            for (int i = 0; i <= days; i++)
            {
                if (startDate.AddDays(i).DayOfWeek == DayOfWeek.Saturday ||
                    startDate.AddDays(i).DayOfWeek == DayOfWeek.Sunday)
                {
                    days++;
                    continue;
                }
                expectedList.Add(startDate.AddDays(i));
            }

            startDate.NextWeekDays(daysAhead).ShouldBeEqual(expectedList);
        }
        [Theory]
        [InlineData("2017-01-01T01:00:00", 2)]
        [InlineData("2017-01-15", 0)]
        [InlineData("2017-03-02T12:33:21", 3)]
        [InlineData("2018-02-02T12:33:21", 10)]
        public void MustReturnNextDaysOfWeekStraight(string value, int daysAhead)
        {
            var startDate = DateTime.Parse(value);

            var days = daysAhead;
            var expectedList = new List<DateTime>();
            for (int i = 0; i <= days; i++)
            {
                if (startDate.AddDays(i).DayOfWeek == DayOfWeek.Saturday ||
                    startDate.AddDays(i).DayOfWeek == DayOfWeek.Sunday)
                {
                    days++;
                    continue;
                }
                expectedList.Add(startDate.AddDays(i));
            }

            startDate.NextWeekDays(daysAhead, null, false).ShouldBeEqual(expectedList);
        }
        [Theory]
        [InlineData("2017-01-01T01:00:00", -2)]
        [InlineData("2017-01-15", -1)]
        [InlineData("2017-03-02T12:33:21", -3)]
        [InlineData("2018-02-02T12:33:21", -10)]
        public void MustReturnDaysBeforeOfWeek(string value, int daysAhead)
        {
            var startDate = DateTime.Parse(value);

            var days = daysAhead;
            var expectedList = new List<DateTime>();
            for (int i = 0; i >= days; i--)
            {
                if (startDate.AddDays(i).DayOfWeek == DayOfWeek.Saturday ||
                    startDate.AddDays(i).DayOfWeek == DayOfWeek.Sunday)
                {
                    days--;
                    continue;
                }
                expectedList.Add(startDate.AddDays(i));
            }

            startDate.NextWeekDays(daysAhead).ShouldBeEqual(expectedList);
        }
        [Theory]
        [InlineData("2017-01-01T01:00:00", -2)]
        [InlineData("2017-01-15", 0)]
        [InlineData("2017-03-02T12:33:21", -3)]
        [InlineData("2018-02-02T12:33:21", -10)]
        public void MustReturnDaysBeforeOfWeekBackwards(string value, int daysAhead)
        {
            var startDate = DateTime.Parse(value);

            var days = daysAhead;
            var expectedList = new List<DateTime>();
            for (int i = 0; i >= days; i--)
            {
                if (startDate.AddDays(i).DayOfWeek == DayOfWeek.Saturday ||
                    startDate.AddDays(i).DayOfWeek == DayOfWeek.Sunday)
                {
                    days--;
                    continue;
                }
                expectedList.Add(startDate.AddDays(i));
            }

            startDate.NextWeekDays(daysAhead, null, true).ShouldBeEqual(expectedList);
        }

        [Theory]
        [InlineData("2017-01-01T01:05:12", 10, "2017-01-01T01:05:10", DateTimeFragment.Seconds)]
        [InlineData("2017-03-02T12:33:59", 10, "2017-03-02T12:33:50", DateTimeFragment.Seconds)]
        [InlineData("2018-02-04T01:13:01", 10, "2018-02-04T01:13:00", DateTimeFragment.Seconds)]
        [InlineData("2018-02-05T19:03:01", 30, "2018-02-05T19:03:00", DateTimeFragment.Seconds)]
        [InlineData("2018-02-21T15:53:41", 30, "2018-02-21T15:53:30", DateTimeFragment.Seconds)]
        [InlineData("2018-02-25T13:43:01", 65, "2018-02-25T13:43:00", DateTimeFragment.Seconds)]

        [InlineData("2017-01-01T01:05:12", 10, "2017-01-01T01:00:00", DateTimeFragment.Minutes)]
        [InlineData("2017-03-02T12:33:59", 10, "2017-03-02T12:30:00", DateTimeFragment.Minutes)]
        [InlineData("2018-02-04T01:13:01", 10, "2018-02-04T01:10:00", DateTimeFragment.Minutes)]
        [InlineData("2018-02-05T19:03:01", 30, "2018-02-05T19:00:00", DateTimeFragment.Minutes)]
        [InlineData("2018-02-21T15:53:41", 30, "2018-02-21T15:30:00", DateTimeFragment.Minutes)]
        [InlineData("2018-02-25T13:43:01", 65, "2018-02-25T13:00:00", DateTimeFragment.Minutes)]

        [InlineData("2017-01-01T01:05:12", 10, "2017-01-01T00:00:00", DateTimeFragment.Hours)]
        [InlineData("2017-03-02T12:33:59", 10, "2017-03-02T10:00:00", DateTimeFragment.Hours)]
        [InlineData("2018-02-04T01:13:01", 10, "2018-02-04T00:00:00", DateTimeFragment.Hours)]
        [InlineData("2018-02-05T19:03:01", 15, "2018-02-05T15:00:00", DateTimeFragment.Hours)]
        [InlineData("2018-02-21T15:53:41", 15, "2018-02-21T15:00:00", DateTimeFragment.Hours)]
        [InlineData("2018-02-25T13:43:01", 65, "2018-02-25T00:00:00", DateTimeFragment.Hours)]

        [InlineData("2017-01-01T01:05:12", 10, "2017-01-01T00:00:00", DateTimeFragment.Days)]
        [InlineData("2017-03-02T12:33:59", 10, "2017-03-01T00:00:00", DateTimeFragment.Days)]
        [InlineData("2018-02-04T01:13:01", 10, "2018-02-01T00:00:00", DateTimeFragment.Days)]
        [InlineData("2018-02-05T19:03:01", 15, "2018-02-01T00:00:00", DateTimeFragment.Days)]
        [InlineData("2018-02-21T15:53:41", 15, "2018-02-15T00:00:00", DateTimeFragment.Days)]
        [InlineData("2018-02-25T13:43:01", 65, "2018-02-01T00:00:00", DateTimeFragment.Days)]
        public void MustRoundBySeconds(string value, int roundDecimal, string expected, DateTimeFragment fragment)
        {
            var startDate = DateTime.Parse(value);
            var expectedDate = DateTime.Parse(expected);
            startDate.RoundTime(roundDecimal, fragment).ShouldBe(expectedDate);
        }

        [Theory]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T00:00:00")]
        [InlineData("2017-03-02T12:33:59", "2017-03-02T00:00:00")]
        [InlineData("2018-02-04T01:13:01", "2018-02-04T00:00:00")]
        [InlineData("2018-02-05T19:03:01", "2018-02-05T00:00:00")]
        [InlineData("2018-02-21T15:53:41", "2018-02-21T00:00:00")]
        [InlineData("2018-02-25T13:43:01", "2018-02-25T00:00:00")]
        public void MustBeThatDay(string date, string expected)
        {
            DateTime.Parse(date).ThatDay().ShouldBe(DateTime.Parse(expected));
        }

        [Theory]
        [InlineData("2017-01-01T01:05:12", "2017-01-02T00:00:00")]
        [InlineData("2017-03-02T12:33:59", "2017-03-01T00:00:00")]
        [InlineData("2018-02-04T01:13:01", "2018-01-04T00:00:00")]
        [InlineData("2018-02-05T19:03:01", "2017-02-05T00:00:00")]
        [InlineData("2018-02-21T15:53:41", "2018-02-01T00:00:00")]
        [InlineData("2018-02-25T13:43:01", "2018-02-23T00:00:00")]
        public void MustNotBeThatDay(string date, string expected)
        {
            DateTime.Parse(date).ThatDay().ShouldNotBe(DateTime.Parse(expected));
        }

        [Theory]
        [InlineData("2017-01-01T01:05:12", "2016-12-31T00:00:00")]
        [InlineData("2017-03-02T12:33:59", "2017-03-01T00:00:00")]
        [InlineData("2018-02-04T01:13:01", "2018-02-03T00:00:00")]
        [InlineData("2018-02-05T19:03:01", "2018-02-04T00:00:00")]
        [InlineData("2018-02-21T15:53:41", "2018-02-20T00:00:00")]
        [InlineData("2018-02-25T13:43:01", "2018-02-24T00:00:00")]
        public void MustBePreviousDay(string date, string expected)
        {
            DateTime.Parse(date).PreviousDay().ShouldBe(DateTime.Parse(expected));
        }

        [Theory]
        [InlineData("2017-01-01T01:05:12", "2016-12-30T00:00:00")]
        [InlineData("2017-03-02T12:33:59", "2017-02-28T00:00:00")]
        [InlineData("2018-02-04T01:13:01", "2018-01-04T00:00:00")]
        [InlineData("2018-02-05T19:03:01", "2017-02-06T00:00:00")]
        [InlineData("2018-02-21T15:53:41", "2018-02-01T00:00:00")]
        [InlineData("2018-02-25T13:43:01", "2018-02-23T00:00:00")]
        public void MustNotBePreviousDay(string date, string expected)
        {
            DateTime.Parse(date).PreviousDay().ShouldNotBe(DateTime.Parse(expected));
        }

        [Theory]
        [InlineData("2017-01-01T01:05:12", "2017-01-02T00:00:00")]
        [InlineData("2016-12-31T12:33:59", "2017-01-01T00:00:00")]
        [InlineData("2018-02-04T01:13:01", "2018-02-05T00:00:00")]
        [InlineData("2018-02-05T19:03:01", "2018-02-06T00:00:00")]
        [InlineData("2018-02-21T15:53:41", "2018-02-22T00:00:00")]
        [InlineData("2018-02-25T13:43:01", "2018-02-26T00:00:00")]
        public void MustBeNextDay(string date, string expected)
        {
            DateTime.Parse(date).NextDay().ShouldBe(DateTime.Parse(expected));
        }

        [Theory]
        [InlineData("2017-01-01T01:05:12", "2017-01-03T00:00:00")]
        [InlineData("2017-03-02T12:33:59", "2017-03-01T00:00:00")]
        [InlineData("2018-02-04T01:13:01", "2018-01-04T00:00:00")]
        [InlineData("2018-02-05T19:03:01", "2017-02-05T00:00:00")]
        [InlineData("2018-02-21T15:53:41", "2018-02-01T00:00:00")]
        [InlineData("2018-02-25T13:43:01", "2018-02-27T00:00:00")]
        public void MustNotBeNextDay(string date, string expected)
        {
            DateTime.Parse(date).NextDay().ShouldNotBe(DateTime.Parse(expected));
        }
        [Theory]
        [InlineData("2019-01-01 15:02", "2019-01-01 17:02")]   
        [InlineData("2019-06-01 15:02", "2019-06-01 18:02")]    
        public void ToUniversalTime(string date, string expected)
        {
            DateTime.Parse(date).ToUniversalTime().ShouldBe(DateTime.Parse(expected));
            TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(date), TimeZoneInfo.Local).ShouldBe(DateTime.Parse(expected));
            TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(date), TimeZoneInfo.Utc).ShouldNotBe(DateTime.Parse(expected));
        }
        [Theory]
        [InlineData("2019-01-01 15:02", "2019-01-01 17:02")]   
        [InlineData("2019-06-01 15:02", "2019-06-01 18:02")]    
        public void FromUniversalTime(string expected, string date)
        {
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(date), TimeZoneInfo.Local).ShouldBe(DateTime.Parse(expected));
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(date), TimeZoneInfo.Utc).ShouldNotBe(DateTime.Parse(expected));
        }
        [Theory]
        [InlineData("2019-01-01", "2019-01-01", "2018-12-31 23:55:00", "2019-01-01 01:55:00", "2018-12-31", "2018-12-31")]
        [InlineData("2019-01-01", "2019-01-01", "2019-01-01 10:55:00", "2019-01-01 12:55:00", "2019-01-01", "2019-01-01")]
        [InlineData("2019-01-01", "2019-01-01", "2019-01-01 00:00:00", "2019-01-01 02:00:00", "2019-01-01", "2019-01-01")]
        [InlineData("2019-01-01", "2019-01-01", "2019-01-01 23:59:59", "2019-01-02 01:59:59", "2019-01-01", "2019-01-01")]
        [InlineData("2019-01-01", "2019-01-01", "2019-02-01 23:59:59", "2019-02-02 01:59:59", "2019-01-01", "2019-01-01")]
        [InlineData("2019-01-01", "2019-01-01", "2016-02-01 23:59:59", "2016-02-02 01:59:59", "2019-01-01", "2019-01-01")]
        [InlineData("2019-06-01", "2019-06-02", "2019-06-01 23:55:00", "2019-06-01 23:55:00", "2019-06-01", "2019-06-02")]
        [InlineData("2019-06-01", "2019-06-02", "2019-06-01 10:55:00", "2019-06-01 10:55:00", "2019-06-01", "2019-06-02")]
        public void ShouldConvertTimezone(string param1, string param2, string param3, string param4, string expected1, string expected2)
        {
            var result = DateTimeExtensions.ConvertFromUTC(DateTime.Parse(param1), DateTime.Parse(param2), DateTime.Parse(param3),
                DateTime.Parse(param4));

            result.Item1.ShouldBe(DateTime.Parse(expected1));
            result.Item2.ShouldBe(DateTime.Parse(expected2));
        }

        [Theory]
        [InlineData("2019-01-01", "2019-01-01", "2019-01-01 04:55:00", "2018-12-31 23:55:00", "2019-01-01", "2019-01-01")]
        [InlineData("2019-01-01", "2019-01-01", "2019-01-01 10:55:00", "2019-01-01 05:55:00", "2019-01-01", "2019-01-01")]
        [InlineData("2019-01-01", "2019-01-01", "2019-01-02 00:00:00", "2019-01-01 19:00:00", "2019-01-02", "2019-01-02")]
        [InlineData("2019-01-01", "2019-01-01", "2019-01-01 23:59:59", "2019-01-01 18:59:59", "2019-01-01", "2019-01-01")]
        [InlineData("2019-01-01", "2019-01-01", "2019-02-01 23:59:59", "2019-02-01 18:59:59", "2019-01-01", "2019-01-01")]
        [InlineData("2019-01-01", "2019-01-01", "2016-02-01 23:59:59", "2016-02-01 18:59:59", "2019-01-01", "2019-01-01")]
        [InlineData("2019-06-01", "2019-06-02", "2019-06-01 23:55:00", "2019-06-01 23:55:00", "2019-06-01", "2019-06-02")]
        [InlineData("2019-06-01", "2019-06-02", "2019-06-01 10:55:00", "2019-06-01 10:55:00", "2019-06-01", "2019-06-02")]
        public void ShouldConvertNonMinusTimezone(string param1, string param2, string param3, string param4, string expected1, string expected2)
        {
            var result = DateTimeExtensions.ConvertFromUTC(DateTime.Parse(param1), DateTime.Parse(param2), DateTime.Parse(param3),
                DateTime.Parse(param4));

            result.Item1.ShouldBe(DateTime.Parse(expected1));
            result.Item2.ShouldBe(DateTime.Parse(expected2));
        }

        [Theory]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T01:05:12", "00:00:05")]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T01:05:13", "00:00:05")]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T01:05:10", "00:00:05")]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T01:02:12", "00:05:00")]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T02:05:12", "01:00:05")]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T01:05:00", "00:01:00")]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T01:10:12", "01:00:00")]
        public void ShouldBeInRange(string date, string toCompare, string range)
        {
            DateTime.Parse(date).IsInRange(DateTime.Parse(toCompare), TimeSpan.Parse(range));
        }

        [Theory]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T01:05:12", "00:00:00")]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T01:01:13", "00:00:05")]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T01:05:10", "00:00:01")]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T01:02:12", "00:00:00")]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T03:55:12", "01:00:05")]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T01:05:00", "00:01:00")]
        [InlineData("2017-01-01T01:05:12", "2017-01-01T01:00:12", "01:00:00")]
        public void ShouldNotBeInRange(string date, string toCompare, string range)
        {
            DateTime.Parse(date).IsInRange(DateTime.Parse(toCompare), TimeSpan.Parse(range));
        }
    }
}
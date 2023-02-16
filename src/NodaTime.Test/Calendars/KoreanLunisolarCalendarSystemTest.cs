using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NodaTime.Test.Calendars
{
    public class KoreanLunisolarCalendarSystemTest

    {
        [Test]
        public void LeapYears()
        {
            var calendar = CalendarSystem.KoreanLunisolar;
            Assert.IsFalse(calendar.IsLeapYear(918));
            Assert.IsTrue(calendar.IsLeapYear(936));
            Assert.IsFalse(calendar.IsLeapYear(1000));
            Assert.IsTrue(calendar.IsLeapYear(1116));
            Assert.IsTrue(calendar.IsLeapYear(1392));
            Assert.IsFalse(calendar.IsLeapYear(1443));
            Assert.IsFalse(calendar.IsLeapYear(1592));
            Assert.IsFalse(calendar.IsLeapYear(1776));
            Assert.IsFalse(calendar.IsLeapYear(1894));
            Assert.IsTrue(calendar.IsLeapYear(1906));
            Assert.IsTrue(calendar.IsLeapYear(1960));
            Assert.IsTrue(calendar.IsLeapYear(1987));
            Assert.IsFalse(calendar.IsLeapYear(2000));
            Assert.IsFalse(calendar.IsLeapYear(2022));
            Assert.IsTrue(calendar.IsLeapYear(2023));
            Assert.IsTrue(calendar.IsLeapYear(2050));
        }

        [Test]
        public void Boundaries()
        {
            var calendar = CalendarSystem.KoreanLunisolar;

            // year range is 918-2050

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // We don't know yet 917Y/12M in KLC has 29 or 30 days but...
                LocalDate beforeMinDate = new LocalDate(917, 12, 29, calendar);
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                LocalDate afterMaxDate = new LocalDate(2051, 1, 1, calendar);
            });

            Assert.DoesNotThrow(() =>
            {
                LocalDate minDate = new LocalDate(918, 1, 1, calendar);
                LocalDate maxDate = new LocalDate(2050, 13, 29, calendar);
            });

        }

        [Test]
        public void BclEquivalence()
        {
            var nodaCalendar = CalendarSystem.KoreanLunisolar;
            var bclCalendar = new KoreanLunisolarCalendar();
            LocalDate nodaDate, nodaDate2;
            DateTime bclDate;

            for (int year = 918; year < 2051; ++year)
            {
                var nodaMaxMonth = nodaCalendar.GetMonthsInYear(year);
                var bclMaxMonth = bclCalendar.GetMonthsInYear(year);
                Assert.AreEqual(nodaMaxMonth, bclMaxMonth);

                for (int month = 1; month <= nodaMaxMonth; ++month)
                {
                    nodaDate = new LocalDate(year, month, 1, nodaCalendar); // 0918-01-01 KLC
                    bclDate = bclCalendar.ToDateTime(year, month, 1, 0, 0, 0, 0, KoreanLunisolarCalendar.GregorianEra); // 0918-02-19 ISO
                    nodaDate2 = LocalDate.FromDateTime(bclDate).WithCalendar(nodaCalendar); // FIXME: 0918-02-19 ISO -> 0918-01-18 KLC ????

                    
                    Assert.AreEqual(nodaDate.ToDateTimeUnspecified(), bclDate); // SUCCESS
                    Assert.AreEqual(nodaDate, nodaDate2); // FAIL
                }
            }
        }

        [Test]
        public void LeapMonthNames()
        {
            var calendar = CalendarSystem.KoreanLunisolar;
            
            Assert.AreEqual(calendar.GetMonthName(new YearMonth(2023, 2)), "2");
            Assert.AreEqual(calendar.GetMonthNumberFromMonthName(2023, 2), 2);

            Assert.AreEqual(calendar.GetMonthName(2023, 3), "윤2");
            Assert.AreEqual(calendar.GetMonthNumberFromMonthName(2023, "윤2"), 3);
            Assert.Throws<ArgumentException>(() => calendar.GetMonthNumberFromMonthName(2023, "윤11"));

            Assert.AreEqual(calendar.GetMonthName(new LocalDate(2023, 4, 1, calendar)), "3");
            Assert.AreEqual(calendar.GetMonthNumberFromMonthName(2023, 3), 4);
            
            Assert.Throws<ArgumentException>(() => calendar.GetMonthNumberFromMonthName(2024, "윤5"));
        }
    }
}

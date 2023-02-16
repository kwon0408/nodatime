// Copyright 2023 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime.Annotations;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NodaTime.Calendars
{
    internal abstract partial class EastAsianLunisolarYearMonthDayCalculator: YearMonthDayCalculator
    {
        private const int BytesPerYear = 4;

        public abstract string LeapMonthPrefix { get; }

        // 4 bytes per year: 0b_LLLL_MMMM_DDDD_DDDD_PPPP_PPPP_PPPP_P000
        //
        // The first 4 bits (LLLL) indicate the leap month.
        // If the year has a leap month, the value is the number of the month
        // added as a leap month. Otherwise, the value is 0.
        // The leap month comes right after the month specified by this value.
        // For example, if a year has a leap month in month 6 (LLLL == 0b0110), 
        // the month 5 of that year is followed by month 6, month leap-6, and then month 7.
        // For leap years, month names and month numbers do not match.
        // See below for more information about this.
        // 
        // The next 4 bits (MMMM) and 8 bits (DDDD_DDDD) indicate the lunar new year day
        // in Gregorian Calendar.
        //
        // The final 16 bits (PPPP_PPPP_PPPP_P000) indicate the length of each month.
        // A year can have 13 months maximum, therefore only the first 13 bits in this field 
        // will be actually used.
        // The value of each bit is 1, if the month indicated by the bit has 30 days,
        // or 0, if it either has 29 days or is a non-existent 13th month.
        protected abstract byte[] YearInfo { get; }

        private static readonly GregorianYearMonthDayCalculator gregorian = new GregorianYearMonthDayCalculator();

        protected EastAsianLunisolarYearMonthDayCalculator
            (int minYear, int maxYear, int averageDaysPer10Years, int daysAtStartOfYear1)
            : base(minYear, maxYear, averageDaysPer10Years, daysAtStartOfYear1)
        {
        }

        private int GetStartByte([Trusted] int year)
            => (year - MinYear) * BytesPerYear;

        public int GetLeapMonth([Trusted] int year)
        {
            try {
                return YearInfo[GetStartByte(year)] >> 4; 
            }
            catch (IndexOutOfRangeException) 
            { 
                return 0;
            }
        }

        protected override int CalculateStartOfYearDays([Trusted] int year)
        {
            if (year == MaxYear + 1)
                return CalculateStartOfYearDays(MaxYear) + GetDaysInYear(MaxYear);

            int month = YearInfo[GetStartByte(year)] & 0x0F;
            int day = YearInfo[GetStartByte(year) + 1];
            return gregorian.GetDaysSinceEpoch(new YearMonthDay(year, month, day));

        }

        // Q: What are month numbers and names?
        // A: E.g. If Leap month is 6,
        // month names   1 - 2 - 3 - 4 - 5 - 6 - leap-6 - 7 - 8 -  9 - 10 - 11 - 12 correspond to
        // month numbers 1 - 2 - 3 - 4 - 5 - 6 - 7      - 8 - 9 - 10 - 11 - 12 - 13.  
        // Here we will assume all "month"s to be month number, as .NET BCL does.
        protected override int GetDaysFromStartOfYearToStartOfMonth([Trusted] int year, [Trusted] int month)
        {
            int d = 0;
            for (int m = 1; m < month; m++)
                d += GetDaysInMonth(year, m);

            return d;
        }

        internal override YearMonthDay AddMonths([Trusted] YearMonthDay yearMonthDay, int months)
        {
            // TODO: 1. get what year is containing the target month
            // starting from last month of current year
            int m = months - (GetMonthsInYear(yearMonthDay.Year) - yearMonthDay.Month);

            int y = yearMonthDay.Year + 1;
            for (; ; y++)
            {
                int monthsInYear = GetMonthsInYear(y);
                if (m < monthsInYear)
                    break;
                m -= monthsInYear;
            }

            // TODO: 2. if day 30 is out of target month, set day to 29            
            int daysInMonth = GetDaysInMonth(y, m);
            int d = Math.Min(yearMonthDay.Day, daysInMonth);

            return new YearMonthDay(y, m, d);
        }

        internal override int GetDaysInMonth([Trusted] int year, int month)
        {
            if (month < 1 || month > GetMonthsInYear(year))
                throw new ArgumentOutOfRangeException(nameof(month));

            ushort monthPattern;
            try
            {
                monthPattern = BitConverter.ToUInt16(YearInfo, GetStartByte(year));
            }
            catch (ArgumentOutOfRangeException) // workaround for year == 2051
            {
                monthPattern = 0;
            }

            int mask = 0x10000 >> month;
            return (mask & monthPattern) == 0 ? 29 : 30;
        }

        internal override int GetDaysInYear([Trusted] int year)
        {
            int d = 0;
            for (int m = 1; m <= GetMonthsInYear(year); m++)
                d += GetDaysInMonth(year, m);

            return d;
        }

        internal override int GetMonthsInYear([Trusted] int year)
        {
            return IsLeapYear(year) ? 13 : 12;
        }

        internal override YearMonthDay GetYearMonthDay([Trusted] int year, [Trusted] int dayOfYear)
        {
            int m = 1;
            int d = dayOfYear;
            for (; m <= GetMonthsInYear(year); m++)
            {
                int days = GetDaysInMonth(year, m);
                if (0 < d && d < days)
                    d -= days;
                else
                    break;
            }

            return new YearMonthDay(year, m, d);
        }

        internal override bool IsLeapYear([Trusted] int year)
        {
            return GetLeapMonth(year) != 0;
        }

        internal override int MonthsBetween([Trusted] YearMonthDay start, [Trusted] YearMonthDay end)
        {
            // Note: Below logics are from RegularYearMonthDayCalculator,
            // with modifications for "irregular" months-in-year.
            int startMonth = start.Month;
            int startYear = start.Year;
            int endMonth = end.Month;
            int endYear = end.Year;

            // support irregular months
            int diff = endMonth - startMonth;
            for (int y = startYear; y < endYear; ++y)
                diff += GetMonthsInYear(y);


            // If we just add the difference in months to start, what do we get?
            YearMonthDay simpleAddition = AddMonths(start, diff);

            // Note: this relies on naive comparison of year/month/date values.
            if (start <= end)
            {
                // Moving forward: if the result of the simple addition is before or equal to the end,
                // we're done. Otherwise, rewind a month because we've overshot.
                return simpleAddition <= end ? diff : diff - 1;
            }
            else
            {
                // Moving backward: if the result of the simple addition (of a non-positive number)
                // is after or equal to the end, we're done. Otherwise, increment by a month because
                // we've overshot backwards.
                return simpleAddition >= end ? diff : diff + 1;
            }
        }

        internal override YearMonthDay SetYear(YearMonthDay yearMonthDay, [Trusted] int year)
        {
            // 1. change year: nothing to do, just call the same variable in YearMonthDay()

            // 2. if leap month is invalid, convert leap month to normal month
            int leapMonthInOriginalYear = GetLeapMonth(yearMonthDay.Year);
            bool fallbackToNormalMonth =
                leapMonthInOriginalYear == yearMonthDay.Month - 1 // source month is leap month
                && leapMonthInOriginalYear != GetLeapMonth(year); // target year has different (or no) leap month 

            int m = yearMonthDay.Month - (fallbackToNormalMonth ? 1 : 0);

            // 3. if day 30 is out of target month, set day to 29
            int daysInMonth = GetDaysInMonth(year, m);
            int d = Math.Min(yearMonthDay.Day, daysInMonth);

            return new YearMonthDay(year, m, d);
        }
    }
}

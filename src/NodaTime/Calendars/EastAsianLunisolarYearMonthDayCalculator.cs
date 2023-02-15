// Copyright 2023 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace NodaTime.Calendars
{
    internal abstract partial class EastAsianLunisolarYearMonthDayCalculator: YearMonthDayCalculator
    {
        private const int BytesPerYear = 4;

        // 4 bytes per year.
        // 0b_LLLL_MMMM_000D_DDDD_PPPP_PPPP_PPPP_P000
        //
        // The first 4 bits (LLLL) indicate the leap month.
        // If the year has a leap month, the value is the number of the month
        // added as a leap month. Otherwise, the value is 0.
        // For example, if a year has a leap month in month 6 (LLLL == 0b0110), 
        // the month 5 of that year is followed by month 6, month 6-leap, and then month 7.
        // 
        // The next 4 bits (MMMM) and 8 bits (000D_DDDD) indicate the lunar new year day
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

        private int GetLeapMonth([Trusted] int year)
            => YearInfo[GetStartByte(year)] >> 4;

        private int GetMonthPatterns([Trusted] int year)
            => BitConverter.ToInt16(YearInfo, GetStartByte(year));



        protected override int CalculateStartOfYearDays([Trusted] int year)
        {
            int month = YearInfo[GetStartByte(year)] & 0x0F;
            int day = YearInfo[GetStartByte(year) + 1];
            return gregorian.GetDaysSinceEpoch(new YearMonthDay(year, month, day));
        }

        protected override int GetDaysFromStartOfYearToStartOfMonth([Trusted] int year, [Trusted] int month)
        {

        }

        internal override YearMonthDay AddMonths([Trusted] YearMonthDay yearMonthDay, int months)
        {

        }

        internal override int GetDaysInMonth([Trusted] int year, int month)
        {
            if (month < 1 || month > GetMonthsInYear(year))
                throw new ArgumentOutOfRangeException(nameof(month));

            int mask = 0x10000 >> month;
            return (mask & GetMonthPatterns(year)) == 0 ? 29 : 30;
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

        }

        internal override bool IsLeapYear([Trusted] int year)
        {
            return GetLeapMonth(year) != 0;
        }

        internal override int MonthsBetween([Trusted] YearMonthDay start, [Trusted] YearMonthDay end)
        {

        }

        internal override YearMonthDay SetYear(YearMonthDay yearMonthDay, [Trusted] int year)
        {

        }
    }
}

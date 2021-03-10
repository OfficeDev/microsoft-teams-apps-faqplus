// <copyright file="HolidayService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.Holiday
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class HolidayService
    {
        private List<DateTime> Holidays = new List<DateTime>();
        private List<DateTime> WeekendMakeUps = new List<DateTime>();

        /// <summary>
        /// Holiday service to get a date is holiday or not
        /// </summary>
        public HolidayService()
        {
            //清明
            AddHolidays(DateTime.Parse("2021-04-03"), DateTime.Parse("2021-04-05"));
            //劳动
            AddHolidays(DateTime.Parse("2021-05-01"), DateTime.Parse("2021-05-05"));
            //端午
            AddHolidays(DateTime.Parse("2021-06-12"), DateTime.Parse("2021-06-14"));
            //中秋
            AddHolidays(DateTime.Parse("2021-09-18"), DateTime.Parse("2021-09-21"));
            //国庆
            AddHolidays(DateTime.Parse("2021-10-01"), DateTime.Parse("2021-10-07"));

            WeekendMakeUps.Add(DateTime.Parse("2021-04-25"));
            WeekendMakeUps.Add(DateTime.Parse("2021-05-08"));
            WeekendMakeUps.Add(DateTime.Parse("2021-09-18"));
            WeekendMakeUps.Add(DateTime.Parse("2021-09-26"));
            WeekendMakeUps.Add(DateTime.Parse("2021-10-09"));
        }

        
        public bool IsHoliday(DateTime dt)
        {
            if (Holidays.Contains(dt))
            {
                return true;
            }
            if ((dt.DayOfWeek.Equals(DayOfWeek.Saturday) || dt.DayOfWeek.Equals(DayOfWeek.Sunday)) && !WeekendMakeUps.Contains(dt))
            {
                return true;
            }
            return false;
        }

        private void AddHolidays(DateTime startDay, DateTime endDay)
        {
            var currentDay = startDay;
            while (currentDay <= endDay)
            {
                Holidays.Add(currentDay);
                currentDay = currentDay.AddDays(1);
            }
        }
    }
}

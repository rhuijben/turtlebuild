using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtleUtils.Globalization;
using NUnit.Framework.SyntaxHelpers;

namespace TurtleTests
{
	[TestFixture]
	public class UtilTests
	{
		/// <summary>
		/// Tests the year for weeks.
		/// </summary>
		/// <param name="year">The year.</param>
		public void TestYearForWeeks(int year)
		{
			DateTime dt = new DateTime(year, 1, 1) - new TimeSpan(14, 0, 0, 0);

			for (int i = 0; i < 30; i++)
			{
				dt += new TimeSpan(1, 0, 0, 0);

				Assert.That(QQnDateUtils.GetWeekNumber(dt, true), Is.EqualTo(WeekNumber_Entire4DayWeekRule(dt)), "Weeknumber equal for {0}", dt);
			}
		}

		/// <summary>
		/// Tests the weeks.
		/// </summary>
		[Test]
		public void TestWeeks()
		{
			for (int i = 1980; i < 2020; i++)
			{
				TestYearForWeeks(i);
			}
		}

		[Test]
		public void Test1982()
		{
			TestYearForWeeks(1982);
		}

		#region Reference implementation
		// Testcode from: http://konsulent.sandelien.no/VB_help/Week/, only used for testing our optimized implementation
		// which does not use floats, etc.
		private int WeekNumber_Entire4DayWeekRule(DateTime date)
		{
			const int JAN = 1;
			const int DEC = 12;
			const int LASTDAYOFDEC = 31;
			const int FIRSTDAYOFJAN = 1;
			const int THURSDAY = 4;
			bool ThursdayFlag = false;

			// Get the day number since the beginning of the year
			int DayOfYear = date.DayOfYear;

			// Get the numeric weekday of the first day of the 
			// year (using sunday as FirstDay)
			int StartWeekDayOfYear =
				 (int)(new DateTime(date.Year, JAN, FIRSTDAYOFJAN)).DayOfWeek;
			int EndWeekDayOfYear =
				 (int)(new DateTime(date.Year, DEC, LASTDAYOFDEC)).DayOfWeek;

			// Compensate for the fact that we are using monday
			// as the first day of the week
			if (StartWeekDayOfYear == 0)
				StartWeekDayOfYear = 7;
			if (EndWeekDayOfYear == 0)
				EndWeekDayOfYear = 7;

			// Calculate the number of days in the first and last week
			int DaysInFirstWeek = 8 - (StartWeekDayOfYear);
			int DaysInLastWeek = 8 - (EndWeekDayOfYear);

			// If the year either starts or ends on a thursday it will have a 53rd week
			if (StartWeekDayOfYear == THURSDAY || EndWeekDayOfYear == THURSDAY)
				ThursdayFlag = true;

			// We begin by calculating the number of FULL weeks between the start of the year and
			// our date. The number is rounded up, so the smallest possible value is 0.
			int FullWeeks = (int)Math.Ceiling((DayOfYear - (DaysInFirstWeek)) / 7.0);

			int WeekNumber = FullWeeks;

			// If the first week of the year has at least four days, then the actual week number for our date
			// can be incremented by one.
			if (DaysInFirstWeek >= THURSDAY)
				WeekNumber = WeekNumber + 1;

			// If week number is larger than week 52 (and the year doesn't either start or end on a thursday)
			// then the correct week number is 1. 
			if (WeekNumber > 52 && !ThursdayFlag)
				WeekNumber = 1;

			// If week number is still 0, it means that we are trying to evaluate the week number for a
			// week that belongs in the previous year (since that week has 3 days or less in our date's year).
			// We therefore make a recursive call using the last day of the previous year.
			if (WeekNumber == 0)
				WeekNumber = WeekNumber_Entire4DayWeekRule(
					 new DateTime(date.Year - 1, DEC, LASTDAYOFDEC));
			return WeekNumber;
		}
		#endregion

	}
}

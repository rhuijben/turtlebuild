using System;
using System.Collections.Generic;

namespace QQn.TurtleUtils.Globalization
{
	/// <summary>
	/// 
	/// </summary>
	public static class QQnDateUtils
	{
		/// <summary>
		/// Calculates the ISO 8601 week number for gregorian calenders as used in a.o. the EU.
		/// </summary>
		/// <param name="date">The date.</param>
		/// <returns>The calender-weeknumber as defined in ISO 8601</returns>
		public static int GetWeekNumber(DateTime date)
		{
			return GetWeekNumber(date, false);
		}

		/// <summary>
		/// Calculates the week number for the gregorian calender. If 
		/// <paramref name="iso8601wrapping"/> is set to true weeks are wrapped as defined in ISO 8601
		/// </summary>
		/// <param name="date">The date.</param>
		/// <param name="iso8601wrapping">if set to <c>true</c> wraps weeks accross the year boundary (ISO 8601) otherwise use strictly ascending numbers</param>
		/// <returns>The weeknumber</returns>
		public static int GetWeekNumber(DateTime date, bool iso8601wrapping)
		{
			// Drop time information
			int dayOfYear = date.DayOfYear;

			int firstDayOfYear = FirstDayOfYear(date.Year);

			int dayInYear = date.DayOfYear - 1; // monday = 1

			int weekStart = 1 - firstDayOfYear - 7; // days start counting at 1

			if (firstDayOfYear > 4) // If there are 4 days of last year in the first week, we start counting at 0
				weekStart += 7;

			// Mental note: (dayInYear - week1Start) can not be < 0; as we truncate towards zero
			int week = (dayInYear - weekStart) / 7;

			if (!iso8601wrapping)
				return week;

			if (week > 52)
			{
				firstDayOfYear = FirstDayOfYear(date.Year + 1);

				// If there are 4 or more weekdays of the next year in the week
				// We are in week 1, otherwise we are in week 53
				if (firstDayOfYear <= 4)
					return 1;
			}
			else if (week == 0)
			{
				// We must be in the last week of the next year
				return GetWeekNumber(new DateTime(date.Year - 1, 12, 31), false);
			}

			return week;
		}

		private static int FirstDayOfYear(int year)
		{
			int firstDayOfYear = (int)new DateTime(year, 1, 1).DayOfWeek;

			if (firstDayOfYear == (int)DayOfWeek.Sunday) // ISO Weeks start at monday, which has value 1 in .Net
				firstDayOfYear = 7;

			return firstDayOfYear;
		}
	}
}

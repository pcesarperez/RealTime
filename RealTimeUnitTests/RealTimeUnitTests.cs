using System;

using NUnit.Framework;


namespace com.adastrafork.tools.realtime.tests {
	/// <summary>
	/// Tests the connection to NTP servers, trying to obtain the UTC time from it.
	/// </summary>
	[TestFixture]
	internal sealed class RealTimeUnitTests {
		/// <summary>
		/// Tests the connection to the default NTP server.
		/// </summary>
		[Test]
		public void GetRealTimeFromDefaultServer ( ) {
			var realTime = new RealTime ( );

			try {
				var now = realTime.Now;
				var nowInMyTimeZone = realTime.NowInMyTimeZone;

				Warn.If (realTime.TheAnswerIsReliable, Is.False);

				Console.WriteLine ($"Current UTC time from the default server {realTime.NtpServer} is: {now}.");
				Console.WriteLine ($"Current time from the default server {realTime.NtpServer} in the system time zone is: {nowInMyTimeZone}.");
			} catch (Exception e) {
				Assert.Fail ($"The operation has failed with the following message: {e.Message}");
			}
		}


		/// <summary>
		/// Tests the connection to a specific NTP server.
		/// </summary>
		[Test]
		public void GetRealTime ( ) {
			var realTime = new RealTime ("time.windows.com");

			try {
				var now = realTime.Now;
				var nowInMyTimeZone = realTime.NowInMyTimeZone;

				Warn.If (realTime.TheAnswerIsReliable, Is.False);

				Console.WriteLine ($"Current UTC time from {realTime.NtpServer} is: {now}.");
				Console.WriteLine ($"Current time from {realTime.NtpServer} in the system time zone is: {nowInMyTimeZone}.");
			} catch (Exception e) {
				Assert.Fail ($"The operation has failed with the following message: {e.Message}");
			}
		}
	}
}
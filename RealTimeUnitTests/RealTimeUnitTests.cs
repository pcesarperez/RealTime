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
				Console.WriteLine ($"Current UTC time from the default server {realTime.NtpServer} is: {realTime.GetRealTime ( ).Result}.");
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
				Console.WriteLine ($"Current UTC time from {realTime.NtpServer} is: {realTime.GetRealTime ( ).Result}.");
			} catch (Exception e) {
				Assert.Fail ($"The operation has failed with the following message: {e.Message}");
			}
		}
	}
}
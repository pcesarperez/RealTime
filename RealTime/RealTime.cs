using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using NodaTime;


namespace com.adastrafork.tools.realtime {
	/// <summary>
	/// Real time management using a NTP server.
	/// </summary>
	public sealed class RealTime {
		private const string DEFAULT_NTP_SERVER = "pool.ntp.org";
		private const int UDP_PORT = 123;
		private const int TIMEOUT_MS = 3000;
		private const string UTC_TIMEZONE_TZDB_ID = "Etc/UTC";


		#region Public members.

		/// <summary>
		/// Sets up the real time manager using the default NTP server (pool.ntp.org).
		/// </summary>
		public RealTime ( ) {
			NtpServer = DEFAULT_NTP_SERVER;
		}


		/// <summary>
		/// Sets up the real time manager using a specified NTP server.
		/// </summary>
		/// 
		/// <param name="ntpServer">Hostname of the NTP server used to get the real time.</param>
		public RealTime (string ntpServer) {
			NtpServer = ntpServer;
		}


		/// <summary>
		/// NTP server used to get the real time.
		/// </summary>
		public string NtpServer { get; }


		/// <summary>
		/// Gets the UTC date and time from the NTP server specified in the setup.
		/// </summary>
		public ZonedDateTime Now => GetRealTime ( ).Result;


		/// <summary>
		/// Gets the date and time from the NTP server specified in the setup, using the system time zone.
		/// </summary>
		public ZonedDateTime NowInMyTimeZone => Now.WithZone (DateTimeZoneProviders.Tzdb.GetSystemDefault ( ));


		/// <summary>
		/// Determines if the answer comes from the NTP server (in which case the date and time are considered reliable) or from the local machine.
		/// </summary>
		public bool TheAnswerIsReliable { get; private set; }

		#endregion


		#region Private members.

		/// <summary>
		/// Tries to get the UTC date and time from the NTP server specified in the setup.
		/// </summary>
		/// 
		/// <see cref="https://github.com/HansHinnekint/EncryptionLib/blob/master/EncryptionLibrary/DateTimeGenerator.cs"/>
		/// 
		/// <returns>Date and time in Coordinated Universal Time obtained from the NTP server specified in setup time.</returns>
		private async Task<ZonedDateTime> GetRealTime ( ) {
			var ntpData = GetNtpData ( );

			try {
				var addresses = Dns.GetHostEntry (NtpServer).AddressList;
				var ipEndPoint = new IPEndPoint (addresses [0], UDP_PORT);

				using (var socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
					await Task.Run (( ) => {
						socket.Connect (ipEndPoint);
						socket.ReceiveTimeout = TIMEOUT_MS;

						socket.Send (ntpData);
						socket.Receive (ntpData);
					});

					socket.Close ( );
				}

				TheAnswerIsReliable = true;

				return ParseRealTime (ntpData);
			} catch (Exception) {
				TheAnswerIsReliable = false;

				return new ZonedDateTime (Instant.FromDateTimeUtc (DateTime.UtcNow), DateTimeZoneProviders.Tzdb [UTC_TIMEZONE_TZDB_ID]);
			}
		}


		/// <summary>
		/// Sets up the NTP data array used to send data to/receive data from the server.
		/// </summary>
		/// 
		/// <remarks>
		/// See the following RFCs for further information:
		/// 
		/// <list type="bullet">
		/// <item>RFC 2030 (https://tools.ietf.org/html/rfc2030).</item>
		/// <item>RFC 4330 (https://tools.ietf.org/html/rfc4330).</item>
		/// <item>RFC 5905 (https://tools.ietf.org/html/rfc5905).</item>
		/// <item>RFC 7822 (https://tools.ietf.org/html/rfc7822).</item>
		/// </list>
		/// 
		/// The data array is built with a 48 bytes size, of which 16 are reserved for the digest part.
		/// 
		/// The first item contains the leap indicator, version number and operation mode, as follows:
		/// 
		/// <list type="bullet">
		/// <item>Leap indicator: 0 (no warning).</item>
		/// <item>Version number: 3 (IPv4 only).</item>
		/// <item>Operation mode: 3 (client mode).</item>
		/// </list>
		/// </remarks>
		/// 
		/// <returns>Data array used to dialog with the NTP server.</returns>
		private byte [ ] GetNtpData ( ) {
			var ntpData = new byte [48];
			ntpData [0] = 0x1B;

			return ntpData;
		}


		/// <summary>
		/// Parses the data received from the NTP server to get a suitable <code>DateTime</code> object.
		/// </summary>
		/// 
		/// <see cref="https://stackoverflow.com/a/20157068"/>
		/// 
		/// <param name="ntpData">Data obtained from the NTP server.</param>
		/// 
		/// <returns><code>DateTime</code> object with the UTC real time obtained from the NTP server.</returns>
		private ZonedDateTime ParseRealTime (byte [ ] ntpData) {
			ulong intPart = (ulong) ntpData [40] << 24 | (ulong) ntpData [41] << 16 | (ulong) ntpData [42] << 8 | ntpData [43];
			ulong fractPart = (ulong) ntpData [44] << 24 | (ulong) ntpData [45] << 16 | (ulong) ntpData [46] << 8 | ntpData [47];

			var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

			var ntpBaseDateTime = new LocalDateTime (1900, 1, 1, 0, 0, 0);
			var utcTimeZone = DateTimeZoneProviders.Tzdb [UTC_TIMEZONE_TZDB_ID];

			return utcTimeZone.AtStrictly (ntpBaseDateTime).PlusMilliseconds ((long) milliseconds);
		}

		#endregion
	}
}
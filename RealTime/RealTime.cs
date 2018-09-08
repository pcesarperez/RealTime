using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;


namespace com.adastrafork.tools.RealTime {
	/// <summary>
	/// Real time management using a NTP server.
	/// </summary>
	public sealed class RealTime {
		private const int UDP_PORT = 123;
		private const int TIMEOUT_MS = 3000;


		#region Public members.

		/// <summary>
		/// Sets up the real time manager.
		/// </summary>
		/// 
		/// <param name="ntpServer">URI of the NTP server used to get the real time.</param>
		public RealTime (string ntpServer) {
			NtpServer = ntpServer;
		}


		/// <summary>
		/// URI of the NTP server used to get the real time.
		/// </summary>
		public string NtpServer { get; }


		/// <summary>
		/// Gets the UTC real time from the NTP server specified in setup time.
		/// </summary>
		/// 
		/// <see cref="https://github.com/HansHinnekint/EncryptionLib/blob/master/EncryptionLibrary/DateTimeGenerator.cs"/>
		/// 
		/// <returns>Date and time in coordinated universal time obtained from the NTP server specified in setup time.</returns>
		public async Task<DateTime> GetRealTime ( ) {
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

				return ParseRealTime (ntpData);
			} catch (Exception) {
				return DateTime.UtcNow;
			}
		}

		#endregion


		#region Private members.

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
		private DateTime ParseRealTime (byte [ ] ntpData) {
			ulong intPart = (ulong) ntpData [40] << 24 | (ulong) ntpData [41] << 16 | (ulong) ntpData [42] << 8 | ntpData [43];
			ulong fractPart = (ulong) ntpData [44] << 24 | (ulong) ntpData [45] << 16 | (ulong) ntpData [46] << 8 | ntpData [47];

			var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

			return (new DateTime (1900, 1, 1)).AddMilliseconds ((long) milliseconds);
		}

		#endregion
	}
}
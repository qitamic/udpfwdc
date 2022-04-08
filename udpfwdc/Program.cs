using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace udpfwdc
{
	class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				IPAddress ipLocal = null, ipRemote = null, ipBind = null;
				int iLocalPort = 0, iRemotePort = 0, iTimeoutMs = 0;

				string sErrorArg = "";
				if (!IPAddress.TryParse(args[0], out ipLocal)) sErrorArg = args[0];
				else if (!int.TryParse(args[1], out iLocalPort)) sErrorArg = args[1];
				else if (!IPAddress.TryParse(args[2], out ipRemote)) sErrorArg = args[2];
				else if (!int.TryParse(args[3], out iRemotePort)) sErrorArg = args[3];
				else if (!IPAddress.TryParse(args[4], out ipBind)) sErrorArg = args[4];
				else if (!int.TryParse(args[5], out iTimeoutMs)) sErrorArg = args[5];

				if (sErrorArg.Length == 0)
				{
					IPEndPoint epLocal = new IPEndPoint(ipLocal, iLocalPort);
					IPEndPoint epRemote = new IPEndPoint(ipRemote, iRemotePort);
					IPEndPoint epBind = new IPEndPoint(ipBind, 0);
					bool bDaemon = args.Length > 6 && args[6].ToLower().StartsWith("d");
					bool bStartedInCmd = (Console.CursorLeft != 0) || (Console.CursorTop != 0);

					Exception exError;
					UdpFwd cuf = new UdpFwd(epLocal, epRemote, epBind, iTimeoutMs, out exError);

					if (bDaemon && !bStartedInCmd)
						SetWindowState(enWinState.SW_HIDE);
					
					if (!bDaemon && exError != null)
						throw exError;

					while (true) Thread.Sleep(3600000);
				}
				else
				{
					Console.WriteLine();
					Console.WriteLine("Error parsing argument: " + sErrorArg);
					Console.WriteLine();
					ShowHelp();
				}
			} 
			catch (Exception ex)
			{
				string sOut = "Execption: ";
				Exception exInner = ex;
				do
				{
					sOut = sOut + exInner.Message + "\n" + exInner.StackTrace + "\n";
					exInner = exInner.InnerException;
				} while (exInner != null);

				Console.WriteLine(sOut);
				ShowHelp();
			}
		}

		private static void ShowHelp()
		{
			Console.WriteLine("Usage: udpfwdc.exe LocalIP LocalPort RemoteIP RemotePort RemoteBindingIP TimeoutMs [d, daemon]");
			Console.WriteLine();
			Console.WriteLine("E.g. 1: udpfwdc.exe 127.0.0.1 53 8.8.8.8 53 0.0.0.0 5000");
			Console.WriteLine("E.g. 2: udpfwdc.exe 127.0.0.1 53 8.8.8.8 53 0.0.0.0 5000 d");
			Console.WriteLine("E.g. 3: udpfwdc.exe 127.0.0.1 53 9.9.9.9 9953 0.0.0.0 5000");
			Console.WriteLine();
			Console.WriteLine("Press any key to exit . . .");
			Console.ReadKey();
		}

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();
		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		private enum enWinState
		{
			SW_HIDE = 0,
			SW_NORMAL = 1,
			SW_SHOWMINIMIZED = 2,
			SW_MAXIMIZE = 3,
			SW_SHOWNOACTIVATE = 4,
			SW_SHOW = 5,
			SW_MINIMIZE = 6,
			SW_SHOWMINNOACTIVE = 7,
			SW_SHOWNA = 8,
			SW_RESTORE = 9,
			SW_SHOWDEFAULT = 10,
			SW_FORCEMINIMIZE = 11,
		}
		private static void SetWindowState(enWinState en)
		{
			ShowWindow(GetConsoleWindow(), (int)en);
		}
	}
}

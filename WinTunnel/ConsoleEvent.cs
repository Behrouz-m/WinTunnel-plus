using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace WinTunnel
{
	/// <summary>
	/// Summary description for ConsoleCtrl.
	/// </summary>
	public class ConsoleCtrl: IDisposable
	{
		/// <summary>
		/// The Console events. 
		/// </summary>
		public enum ConsoleEvent
		{
			CTRL_C = 0,		
			CTRL_BREAK = 1,
			CTRL_CLOSE = 2,
			CTRL_LOGOFF = 5,
			CTRL_SHUTDOWN = 6
		}

		/// <summary>
		/// Handler to be called when a console event occurs.
		/// </summary>
		public delegate void ControlEventHandler(ConsoleEvent consoleEvent);

		/// <summary>
		/// Event fired when a console event occurs
		/// </summary>
		public event ControlEventHandler ControlEvent;

		ControlEventHandler eventHandler;

		/// <summary>
		/// Create a new instance.
		/// </summary>
		public ConsoleCtrl()
		{
			eventHandler = new ControlEventHandler(Handler);
			SetConsoleCtrlHandler(eventHandler, true);
		}

		~ConsoleCtrl()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		void Dispose(bool disposing)
		{
			if (disposing)
			{
				GC.SuppressFinalize(this);
			}
			if (eventHandler != null)
			{
				SetConsoleCtrlHandler(eventHandler, false);
				eventHandler = null;
			}
		}

		private void Handler(ConsoleEvent consoleEvent)
		{
			if (ControlEvent != null)
				ControlEvent(consoleEvent);
		}

		[DllImport("kernel32.dll")]
		static extern bool SetConsoleCtrlHandler(ControlEventHandler e, bool add);		
	}
}

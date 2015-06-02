using System;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;


namespace WinTunnel
{
	public class WinTunnel : System.ServiceProcess.ServiceBase
	{
		public static String SERVICE_NAME = "WinTunnel";
		public static String SERVERICE_DISPLAY_NAME = "Windows TCP Tunnel";

		private static ManualResetEvent shutdownEvent = new ManualResetEvent(false);

		private ConnectionManager m_connMgr;
		private AppConfig m_config;

		private Logger logger;

		private bool m_debug = false;

		private ConsoleCtrl m_ctrl = null;

		public WinTunnel()
		{
			CanPauseAndContinue = true;
			ServiceName = SERVICE_NAME;
			AutoLog = false;
		}

		// The main entry point for the process
		static void Main(string[] args)
		{
			if (args.Length==1 && args[0].CompareTo("-debug") ==0 ) //run as a console application 
			{
				System.Console.WriteLine("Starting WinTunnel as a console application...");
				WinTunnel tunnel = new WinTunnel();
				tunnel.m_debug = true;
				tunnel.OnStart(args);
				return;
			}
			else if (args.Length == 1 && args[0].CompareTo("-remove") ==0 ) //remove service
			{
				System.Console.WriteLine("Remove WinTunnel as a service...");
				String argument = "-u " + Process.GetCurrentProcess().MainModule.ModuleName;
				String launchCmd = RuntimeEnvironment.GetRuntimeDirectory() + "InstallUtil.exe";
				launchProcess(launchCmd, argument);
				return;
			}
			else if (args.Length  > 0 && args[0].CompareTo("-install") ==0 ) //install as a service
			{
				System.Console.WriteLine("Installing WinTunnel as a service...");
		
				StringBuilder argument = new StringBuilder();
				int i=1;
				while(i < args.Length)
				{
					if (args[i].ToLower().CompareTo("-user") == 0)
					{
						argument.Append(" /user=");
						argument.Append(args[i+1]);
						i+=2;
					}
					else if ( args[i].ToLower().CompareTo("-password") == 0)
					{
						argument.Append(" /password=");
						argument.Append( args[i+1]);
						i+=2;
					}
					else
					{
						i++;
					}
				}

				argument.Append(" ");
				argument.Append( Process.GetCurrentProcess().MainModule.ModuleName );

				String launchCmd = RuntimeEnvironment.GetRuntimeDirectory() + "InstallUtil.exe";
				launchProcess(launchCmd, argument.ToString());
				return;
			}

			System.ServiceProcess.ServiceBase[] ServicesToRun;
			ServicesToRun = new System.ServiceProcess.ServiceBase[] { new WinTunnel() };
			System.ServiceProcess.ServiceBase.Run(ServicesToRun);
		}

		static void launchProcess(String binary, String argument)
		{
			System.Diagnostics.ProcessStartInfo psInfo =
				new System.Diagnostics.ProcessStartInfo(binary, argument);
				
			System.Console.WriteLine();
			System.Console.WriteLine(psInfo.FileName + " " + psInfo.Arguments);
			System.Console.WriteLine();

			psInfo.RedirectStandardOutput = true;
			psInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			psInfo.UseShellExecute = false;
			System.Diagnostics.Process ps;
			ps = System.Diagnostics.Process.Start(psInfo);
			System.IO.StreamReader msgOut = ps.StandardOutput;
			ps.WaitForExit(5000); //wait up to 5 seconds 
			if (ps.HasExited)
			{
				System.Console.WriteLine(msgOut.ReadToEnd()); //write the output
			}
			return;
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			Thread t = new Thread( new ThreadStart(startApplication) );
			t.Name = "main";
			t.Start();
		}
 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			//Signal the main thread to exit
			shutdownEvent.Set();
		}
		
		public static void consoleEventHandler(ConsoleCtrl.ConsoleEvent consoleEvent)
		{
			if (ConsoleCtrl.ConsoleEvent.CTRL_C == consoleEvent)
			{
				Logger.getInstance().info("Received CTRL-C from Console. Shutting down...");
				WinTunnel.shutdownEvent.Set();
			}
			else
			{
				Logger.getInstance().warn("Received unknown event {0}.  Ignoring...", consoleEvent);
			}
		}

		private void startApplication()
		{
			
			logger = Logger.getInstance();
			logger.initialize(m_debug);
			logger.info("");
			logger.info("===============================");
			logger.info("*** Starting up WinTunnel ****");
			logger.info("===============================");

			logger.info("Starting thread... ");

			//create a signal handler to detect Ctrl-C to stop the service
			if (m_debug)
			{
				m_ctrl = new ConsoleCtrl();
				m_ctrl.ControlEvent += new ConsoleCtrl.ControlEventHandler(consoleEventHandler);
			}

			//Load configuration and startup
			m_config = new AppConfig();
			if ( ! m_config.initialize() )
			{
				logger.error("Error loading configuration file.  Exiting...");
				return;
			}

			//Initialize the threadpool
			MyThreadPool pool = MyThreadPool.getInstance();
			pool.initialize();

			m_connMgr = ConnectionManager.getInstance();
		
			foreach (ProxyConfig cfg in m_config.m_proxyConfigs)
			{
				ProxyClientListenerTask task = new ProxyClientListenerTask(cfg);
				pool.addTask(task);
			}

			shutdownEvent.WaitOne(); //now just wait for signal to exit
			logger.info("Thread is initiating shutdown... ");

			if (m_ctrl != null)
			{
				logger.info("Releasing Console Event handler. ");
				m_ctrl = null;
			}
			
			//Shutdown the connection manager
			m_connMgr.shutdown();
			logger.info("Connection Manager has been terminated. ");

			//Shutdown the thread pool
			pool.Stop();
			logger.info("ThreadPool has been stopped. ");

			logger.info("Terminating thread... ");
			logger.info("*** WinTunnel exited. ****");
			logger.close();
		}
	}
}

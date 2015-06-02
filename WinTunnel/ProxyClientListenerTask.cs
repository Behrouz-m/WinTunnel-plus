using System;
using System.Net.Sockets;

namespace WinTunnel
{
	/// <summary>
	/// Summary description for SocketListenerTask.
	/// </summary>
	public class ProxyClientListenerTask: ITask
	{
		public ProxyConfig m_config;
	
		public Socket listenSocket = null;

		public static Logger logger;

		private static ConnectionManager m_mgr = ConnectionManager.getInstance();

		public ProxyClientListenerTask(ProxyConfig config)
		{
			Console.WriteLine("ProxyClientListenerTask {0} created.", this);
			m_config = config;
			logger = Logger.getInstance();
		}

		#region ITask Members

		public void run()
		{
			listenSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			listenSocket.Bind(m_config.localEP);
			listenSocket.Listen(10000); //allow up to 10 pending connections
			logger.info("[{0}] Waiting for client connection at {1}...", m_config.serviceName, m_config.localEP.ToString());

			listenSocket.BeginAccept( new AsyncCallback(ProxyClientListenerTask.acceptCallBack), this);
		}

		public String getName()
		{
			return "ProxyClientListenerTask[" + m_config.localEP.ToString() + "]"; 
		}

		#endregion

		//Call back when the socket has connected
		public static void acceptCallBack(IAsyncResult ar)
		{
			
			ProxyConnection conn = null;

			try
			{
				ProxyClientListenerTask listener = (ProxyClientListenerTask) ar.AsyncState;

				//create a new task for connecting to the server side.
				conn = m_mgr.getConnection();
				conn.serviceName = listener.m_config.serviceName;
				conn.clientSocket = listener.listenSocket.EndAccept(ar); //accept the client connection

				logger.info("[{0}] Conn#{1} Accepted new connection. Local: {2}, Remote: {3}.", 
					conn.serviceName,
					conn.connNumber,
					conn.clientSocket.LocalEndPoint.ToString(), 
					conn.clientSocket.RemoteEndPoint.ToString() );

				conn.serverEP = listener.m_config.serverEP;
				
				//Start listening for connection on this port again
				listener.listenSocket.BeginAccept( new AsyncCallback(ProxyClientListenerTask.acceptCallBack), listener);

				ProxyServerConnectTask serverTask = new ProxyServerConnectTask(conn); //now try to connect to the server
				MyThreadPool.getInstance().addTask(serverTask);
			} 
			catch (SocketException se)
			{
				logger.error("[{0}] Conn# {1} Socket Error occurred when accepting client socket. Error Code is: {2}",
					conn.serviceName, conn.connNumber, se.ErrorCode);
				if (conn != null)
				{
					conn.Release();
				}
			}
			catch (Exception e)
			{
				logger.error("[{0}] Conn# {1} Error occurred when accepting client socket. Error is: {2}",
					conn.serviceName, conn.connNumber, e);
				if (conn != null)
				{
					conn.Release();
				}
			}
			finally
			{
				conn = null; //free reference to the object
			}
		}
		
	}
}

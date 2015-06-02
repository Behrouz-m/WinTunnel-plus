using System;
using System.Net.Sockets;
using System.Threading;

namespace WinTunnel
{
	/// <summary>
	/// Summary description for ServerConnectTask.
	/// </summary>
	public class ProxyServerConnectTask: ITask 
	{
		ProxyConnection m_conn;
		static Logger logger = Logger.getInstance();
		
		public ProxyServerConnectTask(ProxyConnection conn)
		{
			m_conn = conn;
		}
		#region ITask Members

		public void run()
		{
			m_conn.serverSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			m_conn.serverSocket.BeginConnect(m_conn.serverEP, new AsyncCallback(connectCallBack), m_conn);	
		}

		public String getName()
		{
			return "ProxyServerConnectTask[" + m_conn.serverEP.ToString() + "]";
		}

		#endregion
	

		//Call back when the socket to the server has connected
		public static void connectCallBack(IAsyncResult ar)
		{
			ProxyConnection conn = (ProxyConnection) ar.AsyncState;

			try
			{
				conn.serverSocket.EndConnect(ar); 
							
				logger.info("[{0}] ProxyConnection#{1}--connected to Server.   Server: {2}, Local: {3}.",
					conn.serviceName,
					conn.connNumber,
					conn.serverSocket.RemoteEndPoint, 
					conn.serverSocket.LocalEndPoint);

				//create task for proxying data between the client and server socket
				ProxySwapDataTask dataTask  = new ProxySwapDataTask(conn);
				MyThreadPool.getInstance().addTask(dataTask);
			} 
			catch (SocketException se)
			{
				if (!conn.isShutdown)
				{
					if (se.ErrorCode == 10060)
					{
						logger.error("[{0}] Conn#{1} Socket Connect Timed out for server {2}",
							conn.serviceName, conn.connNumber, conn.serverEP);
					}
					else
					{
						logger.error("[{0}] Conn#{1} Socket Error occurred when connecting to server. Error Code is: {2}",
							conn.serviceName, conn.connNumber, se.ErrorCode);
					}
					
					conn.Release();
				}
			}
			catch (Exception e)
			{
				if (!conn.isShutdown)
				{	
					logger.error("[{0}] Conn# {1} Error occurred when connecting to server. Error is: {2}",
						conn.serviceName, conn.connNumber, e);
					conn.Release();
				}
			}
			finally
			{
				conn = null;  //free reference to the object
			}
		}
	}
}

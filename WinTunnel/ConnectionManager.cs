using System;
using System.Collections;

namespace WinTunnel
{
	/// <summary>
	/// Summary description for ConnectionManager.
	/// </summary>
	public class ConnectionManager
	{
		private ArrayList m_connections= null;

		private static ConnectionManager m_mgr = null;
		private static int m_connCount = 0;

		private static int m_releaseCount = 0;

		private Logger logger;

		private ConnectionManager()
		{
			logger = Logger.getInstance();

			logger.info("ConnectionManager object instantiated.");
			m_connections = ArrayList.Synchronized( new ArrayList());
		}

		public static ConnectionManager getInstance()
		{
			if (m_mgr == null)
			{
				m_mgr = new ConnectionManager();
			}
			return m_mgr;
		}

		public ProxyConnection getConnection()
		{	
			ProxyConnection conn = null;
			lock(this)
			{
				logger.info("Allocating ProxyConnection#{0} for new connection.", m_connCount);
				conn = new ProxyConnection(); //create a new one
				
				m_connections.Add(conn);
				conn.connNumber = m_connCount++;
			}
			return conn;
		}

		public bool Release(ProxyConnection conn)
		{
			m_releaseCount++;

			if (conn.clientSocket != null)
			{
				logger.info("[{0}] Releasing ProxyConnection#{1}: Client {2}, Server {3}.",
					conn.serviceName, conn.connNumber, 
					conn.clientSocket.RemoteEndPoint.ToString(),
					conn.serverEP.ToString());
			}
			else
			{
				logger.info("[{0}] Releasing ProxyConnection#{1}: Server {2}.", 
					conn.serviceName, conn.connNumber, 
					conn.serverEP);
			}

			conn.disconnect();
			m_connections.Remove(conn);

			if (m_releaseCount%100 == 0)
			{
				logger.info("Process is currently using {0} bytes of memory.", System.GC.GetTotalMemory(true));
			}

			return true;
		}

		public bool shutdown()
		{
			logger.info("There are {0} connections in the Connection List.", m_connections.Count);

			int try_count = 20;
			try_again:
			try
			{
				foreach (ProxyConnection conn in m_connections)
				{
					logger.info("[{0}] Disconnecting conn#{1}...", conn.serviceName, conn.connNumber);
					conn.disconnect();
				}
			}
			catch (System.Exception)
			{
				if (--try_count > 0)
					goto try_again;
			}
			
			m_connections.Clear(); //remove from the array list
			return true;
		}
	}
}

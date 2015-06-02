using System;
using System.Net.Sockets;
using System.Net;

namespace WinTunnel
{
	/// <summary>
	/// Summary description for ProxyConnection.
	/// </summary>
	public class ProxyConnection
	{
		public int connNumber;

		public string m_serviceName;
		public String serviceName
		{
            get { return m_serviceName; }
			set
			{
                m_serviceName = value;
				m_IsClient = serviceName.StartsWith("CLIENT");
				m_IsServer = serviceName.StartsWith("SERVER");
			}
		}

		bool m_IsClient = false;
		bool m_IsServer = false;

		public bool IsClient
		{
			get { return m_IsClient; }
		}

		public bool IsServer
		{
			get { return m_IsServer; }
		}

		public bool ClientConnectionIsFromOurClient = false;
		public bool ServerConnectionIsFromOurServer = false;

		public bool isShutdown = false;

		public Socket clientSocket; //socket for communication with the client
		
		public IPEndPoint serverEP;

		public Socket serverSocket; //Socket for communication with the server

		public const int BUFFER_SIZE = 6 * 1024;
		public const int BUFFER_SIZE_MINUS_ONE_K = BUFFER_SIZE - 2 * 1024;

		public byte[] clientReadBuffer = new byte[BUFFER_SIZE];
		public byte[] serverReadBuffer = new byte[BUFFER_SIZE];

		public int clientNumBytes;
		public byte[] clientSendBuffer = new byte[BUFFER_SIZE];
		public int serverNumBytes;
		public byte[] serverSendBuffer = new byte[BUFFER_SIZE];
		
		private static ConnectionManager m_mgr = ConnectionManager.getInstance();
		private static Logger logger = Logger.getInstance();

		public void Release()
		{
			m_mgr.Release(this);
		}

		public void disconnect()
		{
			isShutdown = true;
			try
			{
				if (serverSocket != null)
				{
					if (serverSocket.Connected)	serverSocket.Shutdown(SocketShutdown.Both);
					serverSocket.Close();
				}

				if (clientSocket != null)
				{
					if (clientSocket.Connected) clientSocket.Shutdown(SocketShutdown.Both);
					clientSocket.Close();
				}
				
			}
			catch (SocketException se)
			{
				logger.error("Socket Error occurred while shutting down sockets: {0}.", se);
			}
			catch (Exception e)
			{
				logger.error("Error occurred while shutting down sockets: {0}.", e);
			}
			finally
			{
				serverSocket = null;
				clientSocket = null;
				serverEP = null;

				clientReadBuffer = null;
				clientSendBuffer = null;

				serverReadBuffer = null;
				serverSendBuffer = null;
			}
		}
	}
}

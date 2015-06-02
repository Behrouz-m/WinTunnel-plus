using System;
using System.Net;
using System.Net.Sockets;

namespace WinTunnel
{
	/// <summary>
	/// Summary description for ProxyDataTask.
	/// </summary>
	public class ProxySwapDataTask: ITask
	{
		//static byte XOR = 0x07;
		/*static readonly byte[] HttpResponseHeader = System.Text.Encoding.ASCII.GetBytes(@"HTTP/0.1 403 Forbidden
Connection:close

");*/

		static readonly byte[] HttpResponseHeader = System.Text.Encoding.ASCII.GetBytes(@"HTTP/1.1 200 OK
Connection: close
Content-Type: application/octet-stream

");//text/html

//        static readonly byte[] HttpResponseHeader =  System.Text.Encoding.ASCII.GetBytes(@"HTTP/0.1 206 Partial Content
//Connection: Keep-Alive
//Content-Length: 85211302
//Date: Tue, 07 May 2013 07:22:35 GMT
//Content-Range: bytes 97244004-182455305/182455306
//Content-Type: application/octet-stream
//Server: Microsoft-IIS/6.0
//Last-Modified: Tue, 05 Jun 2012 17:38:03 GMT
//Accept-Ranges: bytes
//MicrosoftOfficeWebServer: 5.0_Pub
//X-Powered-By: ASP.NET
//
//");

		static readonly byte[] HttpRequestHeader = System.Text.Encoding.ASCII.GetBytes(@"POST /f-u__-c-k-you.tar.gz HTTP/1.1
Host: 87.236.211.189
Content-Type: application/octet-stream

");

		/*static readonly byte[] HttpRequestHeader = System.Text.Encoding.ASCII.GetBytes(@"GET /f-u__-c-k-you.tar.gz HTTP/0.1
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; rv:20.0) Gecko/20100101 Firefox/20.0
Host: www.su-ck--my--co-ck.com
Accept: 
Accept-Encoding: identity
Accept-Language: en-US
Accept-Charset: *
Range: bytes=7216269-
Proxy-Authorization: Basic YmVocm91ejp0ZXJpc21h

");*/

		ProxyConnection m_conn;
		static Logger logger;

		public ProxySwapDataTask(ProxyConnection conn)
		{
			m_conn = conn;
			logger = Logger.getInstance();
		}
		#region ITask Members

		static readonly byte[] http_suffix = { 0x0d, 0x0a, 0x0d, 0x0a };

		static byte[] RandomizeRequestHeader()
		{
			Random rnd = new Random(Environment.TickCount);

			byte[] h = (byte[])HttpRequestHeader.Clone();

			for (int i = 6; i < 13; i++ )
				h[i] = (byte)rnd.Next(97, 122);

			//for (int i = 132; i < 146; i++)
			//	h[i] = (byte)rnd.Next(97, 122);

			return h;
		}
		/*
		static byte[] RandomizeResponseHeader()
		{
			Random rnd = new Random(Environment.TickCount);
			byte[] h = (byte[])HttpResponseHeader.Clone();

			for (int i = 5; i < 13; i++)
				h[i] = (byte)rnd.Next(97, 122);

			for (int i = 132; i < 14; i++)
				h[i] = (byte)rnd.Next(97, 122);

			return h;
		}
		*/
		static bool CheckHttpSuffix(byte[] buff, int size)
		{
			if (buff[size - 4] == 0x0d &&
				buff[size - 3] == 0x0a &&
				buff[size - 2] == 0x0d &&
				buff[size - 1] == 0x0a
				)
				return true;

			return false;
		}

		static byte XOR = 0x27;

		static int CopyBuffer(byte[] input, int input_size, ref byte[] output, ProxyConnection conn, bool client_to_server)
		{
			Array.Copy(input, 0, output, 0, input_size);

			for (int i = 0; i < input_size; i++)
				output[i] ^= XOR;
			return input_size;
			
			//bool client = conn.IsClient;
			//bool server = conn.IsServer;
			////const int max_xor = int.MaxValue;
			////++XOR;
			//// remove response header buffer
			//if (input.Length > 10 &&
			//    input[0] == 'H' &&
			//    input[1] == 'T' &&
			//    input[2] == 'T' &&
			//    input[3] == 'P' &&
			//    input[4] == '/' &&
			//    input[5] == '1' &&
			//    input[6] == '.' &&
			//    input[7] == '1')
			//{
			//    conn.ServerConnectionIsFromOurServer = true;
			//    bool remove_suffix = CheckHttpSuffix(input, input_size);
			//    int total_lengh = input_size - HttpResponseHeader.Length - (remove_suffix ? http_suffix.Length : 0);
			//    Array.Copy(input, HttpResponseHeader.Length, output, 0, total_lengh);

			//    for (int i = 0; i < total_lengh; i++)
			//        output[i] ^= XOR;

			//    return total_lengh;
			//}

			////remove request header buffer
			//if (input.Length > 10 &&
			//    input[27] == 'H' &&
			//    input[28] == 'T' &&
			//    input[29] == 'T' &&
			//    input[30] == 'P' &&
			//    input[31] == '/' &&
			//    input[32] == '1' &&
			//    input[33] == '.' &&
			//    input[34] == '1')
			//{
			//    conn.ClientConnectionIsFromOurClient = true;
			//    bool remove_suffix = CheckHttpSuffix(input, input_size);
			//    int total_lengh = input_size - HttpRequestHeader.Length - (remove_suffix ? http_suffix.Length : 0);
			//    Array.Copy(input, HttpRequestHeader.Length, output, 0, total_lengh);

			//    for (int i = 0; i < total_lengh; i++)
			//        output[i] ^= XOR;

			//    return total_lengh;
			//}

			//bool add_request_header = client_to_server & client;
			//bool add_response_header = !client_to_server & server;
			//bool ConnectionIsSafe = add_request_header || conn.ServerConnectionIsFromOurServer || conn.ClientConnectionIsFromOurClient;

			//// add header buffer
			//if (ConnectionIsSafe &&
			//    input.Length > 6 && 
			//    (input[0] == 'H' &&
			//    input[1] == 'T' &&
			//    input[2] == 'T' &&
			//    input[3] == 'P') || (
			//    input[0] == 'G' &&
			//    input[1] == 'E' &&
			//    input[2] == 'T'
			//    ))
			//{
			//    if (add_request_header)
			//    {
			//        bool add_suffix = CheckHttpSuffix(input, input_size);
			//        int suffix_size = add_suffix ? http_suffix.Length : 0;
			//        var req_header = RandomizeRequestHeader();

			//        int total_lengh = HttpRequestHeader.Length + input_size + suffix_size;
			//        Array.Copy(req_header, 0, output, 0, HttpRequestHeader.Length);
			//        Array.Copy(input, 0, output, HttpRequestHeader.Length, input_size);

			//        if(add_suffix)
			//            Array.Copy(http_suffix, 0, output, total_lengh - http_suffix.Length, http_suffix.Length);

			//        for (int i = HttpRequestHeader.Length; i < total_lengh - suffix_size; i++)
			//            output[i] ^= XOR;

			//        return total_lengh;
			//    }

			//    if (add_response_header)
			//    {
			//        bool add_suffix = CheckHttpSuffix(input, input_size);
			//        int suffix_size = add_suffix ? http_suffix.Length : 0;

			//        int total_lengh = HttpResponseHeader.Length + input_size + suffix_size;
			//        Array.Copy(HttpResponseHeader, 0, output, 0, HttpResponseHeader.Length);
			//        Array.Copy(input, 0, output, HttpResponseHeader.Length, input_size);

			//        if (add_suffix)
			//            Array.Copy(http_suffix, 0, output, total_lengh - http_suffix.Length, http_suffix.Length);

			//        for (int i = HttpResponseHeader.Length; i < total_lengh - suffix_size; i++)
			//            output[i] ^= XOR;
			//        return total_lengh;
			//    }
			//}

			//// only copy buffers
			//Array.Copy(input, 0, output, 0, input_size);

			//if(ConnectionIsSafe)
			//    for (int i = 0; i < input_size; i++)
			//        output[i] ^= XOR;

			//return input_size;
		}

		public void run()
		{
			//validate that both the client side and server side sockets are ok.  If so, do read/write
			if (m_conn.clientSocket == null || m_conn.serverSocket == null)
			{
				logger.error("[{0}] ProxyConnection#{1}--Either client socket or server socket is null.",
							 m_conn.serviceName, m_conn.connNumber);
				m_conn.Release();
				return;
			}

			if (m_conn.clientSocket.Connected && m_conn.serverSocket.Connected)
			{
				//Read data from the client socket
				m_conn.clientSocket.BeginReceive( m_conn.clientReadBuffer, 0, ProxyConnection.BUFFER_SIZE_MINUS_ONE_K, 0,
					new AsyncCallback(clientReadCallBack), m_conn);
			
				//Read data from the server socket
				m_conn.serverSocket.BeginReceive(m_conn.serverReadBuffer, 0, ProxyConnection.BUFFER_SIZE_MINUS_ONE_K, 0,
					new AsyncCallback(serverReadCallBack), m_conn);
			}
			else
			{
				logger.error("[{0}] ProxyConnection#{1}: Either the client or server socket got disconnected.", 
					m_conn.serviceName, m_conn.connNumber );
				m_conn.Release();
			}
			m_conn = null;
		}

		public String getName()
		{
			return "ProxySwapDataTask[(#" + m_conn.connNumber + ") "+ m_conn.serverEP.ToString() + " <===> " + m_conn.clientSocket.RemoteEndPoint.ToString() + "]"; 
		}

		#endregion

		private static void clientReadCallBack(IAsyncResult ar)
		{
			ProxyConnection conn = (ProxyConnection) ar.AsyncState;

			int numBytes =0;
			try
			{
				SocketError se;
				numBytes = conn.clientSocket.EndReceive(ar, out se);
		
				//if (se == SocketError.Success)
				if(numBytes > 0) //write to the server side socket
				{						
					//copy the bytes to the server send buffer and call send
					numBytes = CopyBuffer(conn.clientReadBuffer, numBytes, ref conn.serverSendBuffer, conn, true);
					//Array.Copy(conn.clientReadBuffer, 0, conn.serverSendBuffer, 0, numBytes);

					//for (int i = 0; i < conn.serverSendBuffer.Length; i++ )
					//	conn.serverSendBuffer[i] ^= XOR;

					if (numBytes <= 0)
					{
						conn.Release();
					}
					else
					{
						conn.serverNumBytes += numBytes;
						conn.serverSocket.BeginSend(conn.serverSendBuffer, 0, numBytes, 0,
							new AsyncCallback(serverSendCallBack), conn);
					}
				}
				else
				{
					logger.info("[{0}] ProxyConnection#{1}: Detected Client Socket disconnect.", 
						conn.serviceName, conn.connNumber );
					conn.Release();
				}
			}	
			catch (SocketException se)
			{
				if (!conn.isShutdown)
				{
					if (se.ErrorCode != 10053 || se.ErrorCode != 10054)
					{
						logger.error("[{0}] ProxyConnection#{1}: Socket Error occurred when reading data from the client socket.  Error Code is: {2}.",
							conn.serviceName, conn.connNumber, se.ErrorCode);
					}
					conn.Release();
				}
			}
			catch (Exception e)
			{
				if (!conn.isShutdown)
				{
					logger.error("[{0}] ProxyConnection#{1}:  Error occurred when reading data from the client socket.  The error is: {2}.",
							conn.serviceName, conn.connNumber, e);
					conn.Release();
				}
			}
			finally
			{
				conn = null;
			}
		}

		private static void serverReadCallBack(IAsyncResult ar)
		{
			ProxyConnection conn = (ProxyConnection) ar.AsyncState;
			int numBytes = 0;

			try
			{
				SocketError se;
				numBytes = conn.serverSocket.EndReceive(ar, out se);
		
				//if (se == SocketError.Success)
				if(numBytes > 0) //write to the client side socket
				{						
					//copy the bytes to the client send buffer and call send
					//Array.Copy(conn.serverReadBuffer, 0, conn.clientSendBuffer, 0, numBytes);
					numBytes = CopyBuffer(conn.serverReadBuffer, numBytes, ref conn.clientSendBuffer, conn, false);

					//for (int i = 0; i < conn.clientSendBuffer.Length; i++)
					//    conn.clientSendBuffer[i] ^= XOR;
					if (numBytes <= 0)
					{
						conn.Release();
					}
					else
					{
						conn.clientNumBytes += numBytes;
						conn.clientSocket.BeginSend(conn.clientSendBuffer, 0, numBytes, 0,
							new AsyncCallback(clientSendCallBack), conn);
					}
				}
				else
				{
					//Server must have disconnected the socket
					logger.info("[{0}] ProxyConnection#{1}: Detected Server Socket disconnect.", 
						conn.serviceName, conn.connNumber );
					conn.Release();
				}
			}
			catch (SocketException se)
			{
				if (!conn.isShutdown)
				{
					if (se.ErrorCode != 10053 || se.ErrorCode != 10054)
					{
						logger.error("[{0}] ProxyConnection#{1}: Socket Error occurred when reading data from the server socket.  Error Code is: {2}.",
							conn.serviceName, conn.connNumber, se.ErrorCode);
					}
					conn.Release();
				}
			}
			catch (Exception e)
			{
				if (!conn.isShutdown)
				{
					logger.error("[{0}] ProxyConnection#{1}:  Error occurred when reading data from the server socket.  The error is: {2}.", 
						conn.serviceName, conn.connNumber, e );
					conn.Release();
				}
			}
			finally
			{
				conn = null;
			}
		}

		private static void clientSendCallBack(IAsyncResult ar)
		{
			ProxyConnection conn = (ProxyConnection) ar.AsyncState;
			try
			{
				SocketError se;
				int numBytes = conn.clientSocket.EndSend(ar, out se);
		
				if (se == SocketError.Success && numBytes == conn.clientNumBytes) //read from the server side socket
				{	
					conn.clientNumBytes=0;
					conn.serverSocket.BeginReceive(conn.serverReadBuffer, 0, ProxyConnection.BUFFER_SIZE_MINUS_ONE_K, 0,
						new AsyncCallback(serverReadCallBack), conn);
				}
				else
				{
					conn.clientNumBytes -= numBytes;
				}
			}
			catch (SocketException se)
			{
				if (!conn.isShutdown)
				{
					if (se.ErrorCode != 10053 || se.ErrorCode != 10054)
					{
						logger.error("[{0}] ProxyConnection#{1}: Socket Error occurred when writing data to the client socket.  Error Code is: {2}.",
							conn.serviceName, conn.connNumber, se.ErrorCode);
					}
					conn.Release();
				}
			}
			catch (Exception e)
			{
				if (!conn.isShutdown)
				{
					logger.error("[{0}] ProxyConnection#{1}:  Error occurred when writing data to the client socket.  The error is: {2}.", 
						conn.serviceName,conn.connNumber, e );
					conn.Release();
				}
			}
			finally
			{
				conn = null;
			}
		}

		private static void serverSendCallBack(IAsyncResult ar)
		{
			ProxyConnection conn = (ProxyConnection) ar.AsyncState;
			try
			{
				SocketError se;
				int numBytes = conn.serverSocket.EndSend(ar, out se);
		
				if (se == SocketError.Success 
					//&& numBytes == conn.serverNumBytes
					) //finished sending the data, now read from client socket again
				{	
					conn.serverNumBytes =0;
					conn.clientSocket.BeginReceive(conn.clientReadBuffer, 0, ProxyConnection.BUFFER_SIZE_MINUS_ONE_K, 0,
						new AsyncCallback(clientReadCallBack), conn);
				}
				else
				{
					conn.serverNumBytes -= numBytes;
				}
			}	
			catch (SocketException se)
			{
				if (!conn.isShutdown)
				{
					if (se.ErrorCode != 10053 || se.ErrorCode != 10054)
					{
						logger.error("[{0}] ProxyConnection#{1}: Socket Error occurred when writing data to the server socket.  Error Code is: {2}.",
							conn.serviceName, conn.connNumber, se.ErrorCode);
					}
					conn.Release();
				}
			}
			catch (Exception e)
			{
				if (!conn.isShutdown)
				{
					logger.error( "[{0}] ProxyConnection#{1}, conn#{2}:  Error occurred when writing data to the server socket.  The error is: {2}.",
						conn.serviceName, conn.connNumber, e );
					conn.Release();
				}
			}
			finally
			{
				conn = null;
			}
		}
	}
}


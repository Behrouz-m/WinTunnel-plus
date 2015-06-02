using System;
using System.Net.Sockets;
using System.Net;

namespace WinTunnel
{
	/// <summary>
	/// Summary description for ProxyConfig.
	/// </summary>
	public class ProxyConfig 
	{
		public String serviceName;
		public IPEndPoint localEP;
		public IPEndPoint serverEP;

		public String asString()
		{
			if (serviceName != null)
				return "[" + serviceName + "] " + localEP.ToString() + " ===> " + serverEP.ToString();
			else
				return "Not Initialized.";
		}
	}
}

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Collections;

namespace WinTunnel
{
	/// <summary>
	/// Summary description for AppConfig.
	/// </summary>
	public class AppConfig
	{
		String initFilePath=null;
		Logger logger= null;

		const String configFile = "WinTunnel.ini";

		public ArrayList m_proxyConfigs;

		private Hashtable m_params;
		
		public bool initialize()
		{
			logger = Logger.getInstance();

			m_proxyConfigs = new ArrayList();
			m_params = new Hashtable();
			
			if (File.Exists(configFile)) //look in the current directory first
			{
				initFilePath = configFile;
			}
			else //look at the directory where the binary is located
			{
				String binFullPath = Process.GetCurrentProcess().MainModule.FileName;
				int idx = binFullPath.LastIndexOf("\\");
				String binDir = binFullPath.Substring(0, idx+1);

				if (File.Exists(binDir + configFile))
				{
					initFilePath = binDir + configFile;
				}
			}

			if (initFilePath == null)
			{
				logger.error("Initialization file was not found in the current directory or install directory!");
				return false;
			}

			return loadConfiguration();
		}

		public bool loadConfiguration()
		{

			try
			{
				using (StreamReader sr = new StreamReader(initFilePath))
				{
					String line;

					String listenHost=null;
					String targetHost=null;
					String serviceName=null;
					int idx = 0 ;

					while ((line = sr.ReadLine()) != null)
					{
						line.Trim();
						//skip comments or blank lines.
						if (line.StartsWith("#") || line.Length==0)
						{
							continue;
						}
						else if (line.StartsWith("["))
						{
							idx = line.LastIndexOf("]");
							serviceName = line.Substring(1, idx-1).Trim();
						}
						else if (line.StartsWith("accept"))
						{	
							idx = line.IndexOf("=") + 1;
							listenHost = line.Substring(idx).Trim();
						}
						else if (line.StartsWith("connect"))
						{
							idx = line.IndexOf("=") + 1;
							targetHost = line.Substring(idx).Trim();
							if (serviceName.Length > 0 && targetHost.Length > 0 && listenHost.Length > 0)
							{
								String listenPort;
								String listenIP;
								String targetPort;
								String targetIP;

								ProxyConfig cfg = new ProxyConfig();
								cfg.serviceName = serviceName;

								idx = listenHost.IndexOf(":");
								if (idx > 0)
								{
									listenIP = listenHost.Substring(0, idx);
									listenPort = listenHost.Substring(idx+1);
									cfg.localEP = new IPEndPoint(IPAddress.Parse(listenIP), Int32.Parse(listenPort));
								}
								else
								{
									listenPort = listenHost;
									cfg.localEP = new IPEndPoint(IPAddress.Any, Int32.Parse(listenPort));
								}

								idx = targetHost.IndexOf(":");
								targetIP = targetHost.Substring(0, idx);
								targetPort = targetHost.Substring(idx+1);
								cfg.serverEP = new IPEndPoint(IPAddress.Parse(targetIP), Int32.Parse(targetPort));

								m_proxyConfigs.Add(cfg);
								logger.debug("Loaded Proxy config: " + cfg.asString());

								//clear out values
								serviceName = null;
								targetHost = null;
								listenHost = null;
							}
						}
						else if (line.IndexOf("=") > 0)
						{
							idx = line.IndexOf("=");
							String key = line.Substring(0, idx);
							String param = line.Substring(idx+1);
							m_params.Add(key, param);
						}
						else
						{
							logger.warn("Ignoring unknown configuration line: {0}.", line);
						}
					}
				}

				return true;
			}
			catch (Exception e)
			{
				logger.error("Unable to read configuration file: {0}", configFile);
				logger.error("The exception is {0}.", e);
			}

			return false;
		}

		public String getParameter(String key)
		{
			return (String) m_params[key];
		}

		public String getParameter(String key, String defaultValue)
		{
			String strValue = getParameter(key);
			if (strValue == null)
			{
				return defaultValue;
			}
			else
			{
				return strValue;
			}
		}

		public int getParameter(String key, int defaultValue)
		{
			String strValue = getParameter(key);
			if (strValue == null)
			{
				return defaultValue;
			}
			else
			{
				return Int32.Parse(strValue);
			}
		}
	}
}

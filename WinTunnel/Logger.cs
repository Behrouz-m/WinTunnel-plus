using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace WinTunnel
{
	/// <summary>
	/// Summary description for Logger.
	/// </summary>
	public class Logger
	{
		private static Logger m_logger;

		private StreamWriter m_logWriter;
		private int m_maxFileCount;
		private int m_maxFileSize;
		private String m_logName;

		private bool m_logToConsole = false;

		public const int DEBUG =0;
		public const int INFO =1;
		public const int WARN = 2;
		public const int ERROR = 3;

		private Logger() {} //private constructor--for singleton
		
		public static Logger getInstance()
		{
			if (m_logger == null)
			{
				m_logger = new Logger();
			}
			return m_logger;
		}

		public bool initialize(bool debug)
		{
			//Check the location of the assembly
			String binFullPath = Process.GetCurrentProcess().MainModule.FileName;
			int idx = binFullPath.LastIndexOf("\\");
			String binDir = binFullPath.Substring(0, idx+1);
			
			return initialize(debug, binDir + "WinTunnel.log", 1024000, 10);
		}

		public bool initialize(bool debug, String logName, int maxFileSize, int maxCount)
		{
			m_maxFileSize = maxFileSize;
			m_maxFileCount = maxCount;
			m_logName = logName;

			if (debug) //log message to console as well
			{
				m_logToConsole = true; 
			}

			try
			{
				//Create the stream for writing the log
				m_logWriter = new StreamWriter( m_logName, true);
			}
			catch (Exception e)
			{
				System.Console.WriteLine("Unable to create log {0}.  The exception is {1}.", logName, e);
				m_logWriter = null;
			}
			return true;

		}

		public void close()
		{
			if (m_logWriter != null)
			{
				m_logWriter.Close();
				m_logWriter = null;
			}
		}

		public void log(int level, String msg, params object[] vars)
		{
#if(DEBUG)
			String convertedMsg = convertToLogMsg(level, msg, vars);
			writeToLog(convertedMsg);
#endif

		}

		public void debug(String msg, params object[] vars)
		{
#if(DEBUG)
			String convertedMsg = convertToLogMsg(DEBUG, msg, vars);
			writeToLog(convertedMsg);
#endif
		}
		
		public void info(String msg, params object[] vars)
		{
#if(DEBUG)
			String convertedMsg = convertToLogMsg(INFO, msg, vars);
			writeToLog(convertedMsg);
#endif
		}
		
		public void warn(String msg, params object[] vars)
		{
#if(DEBUG)
			String convertedMsg = convertToLogMsg(WARN, msg, vars);
			writeToLog(convertedMsg);
#endif
		}

		public void error(String msg, params object[] vars)
		{
#if(DEBUG)
			String convertedMsg = convertToLogMsg(ERROR, msg, vars);
			writeToLog(convertedMsg);
#endif
		}

		private String convertToLogMsg( int level, String msg, object[] vars)
		{
			StringBuilder builder = new StringBuilder();
			//create the header:  TimeStamp [Level] 
			builder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff "));
			switch(level)
			{	
				case DEBUG: 
					builder.Append("[DEBUG] "); break;
				case INFO:
					builder.Append("[INFO]  "); break;
				case WARN:
					builder.Append("[WARN]  "); break;
				case ERROR:
					builder.Append("[ERROR] "); break;
			}

			builder.Append("[");
			builder.Append(Thread.CurrentThread.Name);
			builder.Append( "] ");

			builder.Append(msg);

			//do variable substitution
			if (vars != null)
			{
				for (int i=0; i< vars.Length; i++)
				{
					builder.Replace("{" + i + "}", vars[i]==null ? "" : vars[i].ToString());
				}
			}
			return builder.ToString();
		}

		public void writeToLog(String msg)
		{
			if (m_logToConsole) System.Console.WriteLine(msg);

			if (m_logWriter != null)
			{
				lock(m_logWriter)
				{
					try
					{
						m_logWriter.Write(msg);
						m_logWriter.Write(Environment.NewLine);
						m_logWriter.Flush();
			
						if (m_logWriter.BaseStream.Length >= m_maxFileSize)
						{
							m_logWriter.Close();
							String newName = m_logName + ".";

							if (File.Exists(newName + m_maxFileCount)) File.Delete(newName + m_maxFileCount); //remove the last one if it exists
							for (int i= m_maxFileCount -1; i> 0; i--)
							{
								if (File.Exists(newName+i)) File.Move(newName+i, newName+(i+1));
							}
							File.Move(m_logName, m_logName + ".1"); //move the current one to the have the .1 extension
							m_logWriter = new StreamWriter(m_logName);
						}
					}
					catch (Exception e)
					{
						m_logWriter = null;
						System.Console.WriteLine("Exception occurred when writing to log: {0}", e);
					}
				}
			}
		}
	}
}

using System;
using System.Collections;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WinTunnel
{
	[RunInstallerAttribute(true)]
	public class ProjectInstaller : Installer
	{
		private ServiceInstaller serviceInstaller;
		private ServiceProcessInstaller processInstaller;

		public ProjectInstaller()
		{
			processInstaller = new ServiceProcessInstaller();
			serviceInstaller = new ServiceInstaller();

			serviceInstaller.DisplayName = WinTunnel.SERVERICE_DISPLAY_NAME;
			serviceInstaller.ServiceName = WinTunnel.SERVICE_NAME;

			serviceInstaller.StartType = ServiceStartMode.Manual;
		
			Installers.Add(serviceInstaller);
			Installers.Add(processInstaller);
		}

		protected override void OnBeforeInstall(IDictionary savedState)
		{
			base.OnBeforeInstall (savedState);
		
			String userAcct = GetContextParameter("user");
			String password = GetContextParameter("password");

			if (userAcct.Length > 0) 
			{
				Console.WriteLine("The install user/pwd is {0}, {1}.", userAcct, password);
				processInstaller.Account = ServiceAccount.User;
				processInstaller.Username = userAcct;
				processInstaller.Password = password;
			}
			else
			{
				processInstaller.Account = ServiceAccount.LocalSystem;
			}
		}

		public string GetContextParameter(string key) 
		{
			string sValue = "";
			try 
			{
				sValue = this.Context.Parameters[key].ToString();
			}
			catch 
			{
				sValue = "";
			}

			return sValue;
		}
	}
}

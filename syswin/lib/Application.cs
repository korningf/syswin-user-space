using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security;
using System.Security.Principal;
using System.Management;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace syswin
{
	public class Application
	{
		protected Hashtable env = (Hashtable)Environment.GetEnvironmentVariables();
		protected string machine = Environment.MachineName;
		protected string domain = Environment.UserDomainName;
		protected string system = Environment.SystemDirectory;

		protected string syswin = "c:\\syswin";
		protected string cygwin = "c:\\cygwin";

		protected SecurityHandler security;
		protected InputArgs Args;

		public Application ()
		{
			security = new SecurityHandler ();
		}


		public static string GetEnvironmentVariable (string name, string fallback)
		{
			string value = Environment.GetEnvironmentVariable(name);
			if (value == null) 
			{
				value = fallback;
			}
			return value;
		}

	}

}


using System;
using System.Security;
using System.Security.Principal;
using System.Management;
using System.Diagnostics;
using System.IO;

namespace syswin
{
	public class SecurityHandler
	{
	
		protected WindowsIdentity Identity;
		protected WindowsPrincipal Principal;
		protected IntPtr LogonToken;


		public SecurityHandler()
		{
			Identity = WindowsIdentity.GetCurrent();
			Principal = new WindowsPrincipal(Identity);
			LogonToken = Identity.Token;
		}


		public WindowsIdentity GetIdentity()
		{
			return Identity;
		}

		public WindowsPrincipal GetPrincipal()
		{
			return Principal;
		}

		public IntPtr GetToken()
		{
			return LogonToken;
		}

		public bool IsAdministrator()
		{
			return Principal.IsInRole(WindowsBuiltInRole.Administrator);
		}

		public bool IsUser(String user)
		{
			// crude and kludgy for now: need to check domain/host later
			return Identity.Name.ToLower().EndsWith(user.ToLower());
		}

		public SecureString ToSecureString(string s)
		{
			SecureString pass = new SecureString();
			char[] c = s.ToCharArray ();
			for (int i=0; i< s.Length; i++) {
				pass.AppendChar(c[i]);
			}
			return pass;
		}

		public string FromSecureString(SecureString pass)
		{
			return pass.ToString();
		}

		// @HACK: kludgy workaround since I can't get readKey to work.
		// it thinks app is a WindowsApp in instead of a ConsoleApp
		// in theory .csproj outputType=Exe => implies => ConsoleApp
		// is there another param in assembly or manifest?
		public string PromptForStringPassword(String user)
		{
			String pass;
			Console.WriteLine ("Enter password for {0}: ", user);

			ConsoleColor h = Console.ForegroundColor;
			Console.ForegroundColor = Console.BackgroundColor;

			pass = Console.ReadLine ();

			Console.ForegroundColor = h;

			return pass;
		}

		// @HACK: disabled.  readKey() fails.
		public SecureString PromptForSecurePassword(String user)
		{
			SecureString pass = new SecureString ();
			Console.WriteLine ("Enter password for {0}: ", user);

		  	ConsoleKeyInfo k = Console.ReadKey(true);
	      	while (k.Key != ConsoleKey.Enter)
	      	{
	         	 if (k.Key == ConsoleKey.Backspace)
	          	{
	                // remove one character from the list of pass characters
					pass.RemoveAt (pass.Length - 1);
	                // get the location of the cursor
	                int pos = Console.CursorLeft;
	                // move the cursor to the left by one character
	                Console.SetCursorPosition(pos - 1, Console.CursorTop);
	                // replace it with space
	                Console.Write(" ");
	                // move the cursor to the left by one character again
	                Console.SetCursorPosition(pos - 1, Console.CursorTop);
	          	}
			  	else
			  	{
					Console.Write("*");
					pass.AppendChar(k.KeyChar);
			  	}
	          	k = Console.ReadKey(true);
	      	}
	      // add a new line because user pressed enter at the end of their pass
	      Console.WriteLine();

	      return pass;
	   }
		
    }

}
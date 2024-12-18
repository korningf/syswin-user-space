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

/**
 * @module: 	su.exe
 * @project: 	Manufacture.net - SysWin utilities (syswin.manufacture.net)
 * @author: 	Francis Korning (fkorning@yahoo.ca)
 * @rigths: 	(c) 2014 Francis Korning (manufacture.net)
 * @license: 	Lesser GNU Public License (LGPL)
 * 
 * The su utility (aka sudo, superuser, or supershell) allows to launch command shells
 * either under elevated privileges, or as the administrator, or even another user.
 * 
 * It assumes adminstrator rights, uses uac dialog, and skips password prompt if possible.
 * 
 * It differs from runas and psexec in that it allows an elevated user command without the
 * need to prompt for a password: it uses instead the uac administrator privilege dialogue.
 * However if the current user is already the desired execution administrator or user, no
 * password is needed.  Otherwise it wraps the command into a runas secure password prompt.
 * Passwords can also be specified in the command line, allowing (insecure) cmd scripting.
 
 * The cmd shell can either run a command and exit, or with /keep will keep the shell
 * active in the desired directory. crucially unlike runas, psexec, shellrunas, or netsh
 * an optional /cmd parameter allows to subsequently execute chained shell commands as
 * the desired user from within the desired directory, even for an interactive /keep shell.
 * 
 * This is done through some fancy shell magic. There is hard-wired support for two types
 * of cmd shells: windows cmd.exe and cygwin bash.exe, both of which will spawn a shell
 * and execute sub commands correctly.
 * 
 * credits:
 * 
 * 	 Gerson Kurz for pserv/gtools su.exe (BSDL) : (http://p-nand-q.com/download/gtools/)
 *   His su.exe only did an elevated user cmd via uac, no runas, no password or prompt.
 *   Support was added for cygwin bash.exe and critically for executing shell commands,
 *   and most importantly wrapping them with a secure password prompt via runas.exe.
 *   Also windows prompt support was added to reflect user and hostname in cmd shells.
 *   Finally options were added and reworked for a POSIX compliant su / sudo hybrid.
 * 
 * 
 * parameters:
 * 
 * usage: su.exe [options]
 * options:
 *              -h --help /help:  this usage and options help message (/?)
 *            -s --shell /shell:  cmd shell executable to invoke (bash*|cmd)
 *                -d --dir /dir:  directory from which to run cmd shell (pwd*)
 *              -u --user /user:  user with whom to call shell (administrator*)
 *              -p --pass /pass:  password to run cmd shell (no prompt)
 *              -k --keep /keep:  keep cmd shell interactive (default *)
 *            -l --login /login:  login shell sourcing local user profile (default *)
 *           -E --envcr /envcr:   use domain user env credentials (excludes login !)
 *           -N --netcr /netcr:   raise local network credentials (excludes login !)
 *           -S --sacvr /savcr:   save and share login credentials (sso login prompt !)
 *            -m --mimic /mimic:  mimic and preserve environment variables (non login +)
 * 			    -e --envs /envs:  export additional environment variables (key[=value][,..])
 * 			    -x --xtok /xtok:  separator token char for envs and args (default comma ',')
 *                -c --cmd /cmd:  execute this chained cmd shell command
 * 
 *            * default is /login /keep if no chained cmd is given
 *            ! only one of [--login|--raise] can be specified
 *            + only for non-login cmd|bash shell as elevated user 
 * 
 * logic:
 * 
 * parse cmd shell:
 *   
 *   first determine cmd shell executable:
 *   shell=null|cmd|cmd.exe  		(default) ->  %SYSTEM32%\cmd.exe
 *   shell=sh|sh.exe|bash|bash.exe 	(cygwin)  ->  %CYGWIN%\bin\bash.exe
 *   shell=another exe,com,cmd,bat 			  ->  locate from PATH
 * 
 * determine user:
 * 
 *   next determine user elevation mode. 4 modes:
 *   user==null (default) 	-> Elevated user, no password prompt, uac elevation dialog
 *   user==current user  	-> Elevated user, no password prompt, uac elevation dialog
 *   user==administrator 	-> Impersonating administrator, possible password prompt
 *   user==another user 	-> Impersonating another user, possible password prompt
 * 
 * examples: 
 * 
 * - (default) launch an interactive windows cmd shell as elevated user (no password, uac admin rights check)
 *
 *  	su 
 *
 *
 * - launch a interactive windows cmd shell in a directory as elevated user (no password, uac admin rights check)
 *
 *  	su /dir c:/work /shell cmd
 * 
 *
 * - launch an interactive cygwin bash shell in a directory as elevated user (no password, uac admin rights check)
 * 
 *  	su --dir c:/work --shell bash
 * 
 *
 * - launch an interactive cmd shell in a directory as administrator (wrap via runas password prompt)
 *
 *  	su /dir c:/work /shell cmd /user administrator
 *
 *
 * - launch an interactive cygwin shell in a directory as cyg_server (wrap via runas password prompt)
 *
 *  	su --dir c:/work --shell bash --user cyg_server
 *
 *
 * - execute a windows cmd exec in a directory as administrator and keep shell interactive (wrap via runas password prompt)
 * 
 *  	su /keep /login /dir c:/work /shell cmd /user administrator /cmd "whoami"
 *  
 *
 * - execute a cygwin bash exec in a directory as cyg_server and keep shell interactive (wrap via runas password prompt)
 *
 *  	su --keep --login --dir c:/work --shell bash --user cyg_server --cmd "whoami"
 *
 *
 * - execute a windows cmd exec in a directory as elevated user and keep shell interactive (no password, uac admin rights check)
 *
 *  	su /keep /login /dir c:/work /shell cmd /cmd "net stop sshd"
 *
 *
 * - execute a cygwin bash shell in a directory as elevated user and keep shell interactive
 *
 *  	su --keep --login --dir c:/work --shell bash --cmd "cygrunsrv --start sshd && cygrunsrv --query sshd"
 *
 *
 * - execute an explicit windows cmd exec in a directory and keep shell interactive
 *
 *  	su /dir c:/work /shell c:/windows/system32/cmd.exe /cmd "/k ^cd c:/work^"
 *
 *
 * - execute an explicit cygwin bash exec in a directory and keep shell interactive
 *
 *  	su --dir c:/work --shell c:/cygwin/bin/bash.exe --cmd "--login -i -c 'cd c:/work && exec bash"
 *
 *
 * todo: 
 * 
 * - /login,/group,env options
 * - csh,tcsh,ksh,zsh,etc..
 * - rsh,ssh, (+ ssh-agent fwd)
 * - shellrunas and LSA credentials
 * - kerberos/afs/dfs/ntlm tokens
 * 
**/
namespace syswin
{

	public class su : Application
	{
		protected string shell_windows_cmd = "cmd.exe";
		protected string shell_cygwin_bash = "bash.exe";

		protected string setenv_windows_cmd = "set";
		protected string setenv_cygwin_bash = "export";

		protected string prompt_cmd = "prompt %COMPUTERNAME%\\%USERNAME% $P$G$S";
		protected string prompt_bash = "export PS1=\"\\[\\e]0;\\w\\a\\]\\n\\[\\e[32m\\]\\u@\\h \\[\\e[33m\\]\\w\\[\\e[0m\\]\n\\$\"";


		public su() : base()
		{
			Console.OutputEncoding = Encoding.GetEncoding(Encoding.Default.CodePage);
		}

		/// <summary>
		/// This program finds an executable on the PATH. 
		/// It can also find other stuff on the path, but 
		/// mostly it finds the executable.s
		/// </summary>
		/// <param name="args"></param>
		protected void Run(string[] args)
		{
			Args = new InputArgs(this.GetType().Name,
				string.Format(resource.IDS_TITLE, AppVersion.Get()) + "\r\n"
				+ "author: Francis Korning (fkorning@yahoo.ca)"  + "\r\n"
				+ resource.IDS_COPYRIGHT + "\r\n" + resource.IDS_LICENSING + "\r\n"
			);

			Args.Add(InputArgType.Flag, 'h', "help", null, Presence.Optional, resource.IDS_OPT_help);
			Args.Add(InputArgType.Parameter, 's', "shell", null, Presence.Optional, resource.IDS_OPT_shell);
			Args.Add(InputArgType.Parameter, 'd', "dir", Environment.CurrentDirectory, Presence.Optional, resource.IDS_OPT_dir);
			Args.Add(InputArgType.Parameter, 'u', "user", null, Presence.Optional, resource.IDS_OPT_user);
			Args.Add(InputArgType.Parameter, 'p', "pass", null, Presence.Optional, resource.IDS_OPT_pass);
			Args.Add(InputArgType.Flag, 'k', "keep", false, Presence.Optional, resource.IDS_OPT_keep);
			Args.Add(InputArgType.Flag, 'l', "login", false, Presence.Optional, resource.IDS_OPT_login);
			//Args.Add(InputArgType.Flag, 'o', "local", false, Presence.Optional, resource.IDS_OPT_local);
			Args.Add(InputArgType.Flag, 'E', "envcr", false, Presence.Optional, resource.IDS_OPT_envcr);
			Args.Add(InputArgType.Flag, 'N', "netcr", false, Presence.Optional, resource.IDS_OPT_netcr);
			Args.Add(InputArgType.Flag, 'S', "savcr", false, Presence.Optional, resource.IDS_OPT_savcr);
			Args.Add(InputArgType.Flag, 'm', "mimic", false, Presence.Optional, resource.IDS_OPT_mimic);
			Args.Add(InputArgType.Parameter, 'e', "envs", null, Presence.Optional, resource.IDS_OPT_envs);
			//Args.Add(InputArgType.Parameter, 'a', "args", null, Presence.Optional, resource.IDS_OPT_args);
			Args.Add(InputArgType.Parameter, 'o', "xtok", ",", Presence.Optional, resource.IDS_OPT_xtok);
			Args.Add(InputArgType.Parameter, 'c', "cmd", null, Presence.Optional, resource.IDS_OPT_cmd);


			if (Args.Process(args))
			{
				string shell = Args.GetString("shell");
				string dir = Args.GetString ("dir");
				string user = Args.GetString ("user");
				string pass = Args.GetString ("pass");
				bool keep = Args.GetFlag ("keep");
				bool login = Args.GetFlag ("login");
				bool envcr = Args.GetFlag ("envcr");
				bool netcr = Args.GetFlag ("netcr");
				bool savcr = Args.GetFlag ("savcr");
				bool mimic = Args.GetFlag ("mimic");
				string envs = Args.GetString ("envs");
				//string args = Args.GetString ("args");
				char xtok = Args.GetCharacter ("xtok");
				string exec = Args.GetString ("cmd");

				string runas = Path.Combine (system, "runas.exe");
				string executable = shell;
				string command = null;
				string setenv = "";
				string prompt = "";

				//Console.WriteLine ("identity: " + security.GetIdentity ().Name + "\ttoken: " + security.GetToken () + "\r\n");

				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.Verb = "runas";
				startInfo.WorkingDirectory = dir;

				// first determine cmd shell executable :
				// shell=sh|sh.exe|bash|bash.exe 	(cygwin*)  -> 	%CYGWIN%\bin\bash.exe
				// shell=cmd|cmd.exe 	   	        (windows) -> 	%SYSTEM32%\cmd.exe
				// shell=another exe,com,shell,bat 			  -> 	locate from PATH

				if (shell == null)
				{
					Console.WriteLine("defaulting to cygwin bash: {0}", executable);
					shell = "bash";
				}
				if ( "sh".Equals (shell.ToLower()) || "sh.exe".Equals (shell.ToLower()) || "bash".Equals(shell.ToLower()) || "bash.exe".Equals(shell.ToLower()) )
				{
					command = shell_cygwin_bash;
					executable = Path.Combine (cygwin, "bin\\bash.exe");
					setenv = setenv_cygwin_bash;
					Console.WriteLine ("using cygwin bash: {0}", executable);
				}
				else if ("cmd".Equals(shell.ToLower()) || "cmd.exe".Equals(shell.ToLower()))
				{
					command = shell_windows_cmd;
					executable = Path.Combine(system, "cmd.exe");
					setenv = setenv_windows_cmd;
					Console.WriteLine("using windows cmd: {0}", executable);
				}
				else if ( ProcessInfoTools.FindExecutable(shell, out executable) )
				{
					command = Path.GetFileName (shell);
					Console.WriteLine ("using custom shell: {0}", executable);
				}
				else
				{
					Console.WriteLine(resource.IDS_ERR_CommandNotFound, shell);
					throw new Exception (String.Format(resource.IDS_ERR_CommandNotFound, shell));
				}


				// next map out environment variable exports.
				// apparently we can only override env variables
				// if UseShellExecute == false, which defeats the
				// purpose of su.  the hack is to set/export the
				//variables by hand in our executable cmd chain.

				//startInfo.UseShellExecute = false;
				String exports = null;

				// mimic and preserve current environment variables
				if ( mimic && (!login) && (user == null) )
				{
					IEnumerator i = env.Keys.GetEnumerator ();
					while (i.MoveNext())
					{
						string k = (string) i.Current;
						string v = Environment.GetEnvironmentVariable (k);
						//startInfo.EnvironmentVariables.Add(k, v);
						exports = (exports == null) ? "" : exports + " && ";
						exports = exports + String.Format ("{0} {1}={2} ", setenv, k, v);
					}
				}
				// export and override new environment variables
				if ( (envs != null) && (envs.Length > 0) )
				{
					String[] pairs = envs.Split(xtok);
					int n = pairs.Length;
					for (int j=0; j<n; j++)
					{
						// special security case: NEVER override COMSPEC or PATH
						String[] p = pairs[j].Trim().Split('=');
						// --envs key1 : mimic and export current variable
						if (p.Length == 1)
						{
							string k = p [0];
							string v = Environment.GetEnvironmentVariable (k);
							exports = (exports == null) ? "" : exports + " && ";
							exports = exports + String.Format ("{0} {1}={2} ", setenv, k, v);
						}
						// --envs key1=val1 : override and export new variable
						else if (p.Length == 2)
						{
							String k = p [0];
							String v = p [1];
							//startInfo.EnvironmentVariables.Add (k, v);
							exports = (exports == null) ? "" : exports + " && ";
							exports = exports + String.Format ("{0} {1}={2} ", setenv, k, v);
						}
						else
						{
							Console.WriteLine(resource.IDS_ERR_InvalidVariables);
							Environment.Exit (1);
						}
					}
				}
				//startInfo.UseShellExecute = true;
				Console.WriteLine ("exports: {0}", exports);


				// now build cmd shell
				// 

				// special case: if chained executable cmd is null
				// we assume an interactive shell: --login --keep 
				if (exec == null)
				{
					keep = true;
					login = true;
					// special case: assume a login profile shell
					// unless we are overring the user environment
				    // using runas flags envcr/env netcr/netonly
					if ( envcr || netcr) {
						login = false;
					}
				}


				// cygwin bash
				if (shell_cygwin_bash.Equals (command))
				{
					//bash shell must usingle single quote (') -> only pass in double quotes.
					//command = "c:/cygwin/bin/bash.exe --login -i -c 'cd {dir} && {exec} && exec bash'"
					prompt = prompt_bash;
					exec =
						string.Format("{0}{1} -c '{2}{3}{4}{5}'",
							login ? "--login" : "",
							keep ? " -i" : "", 
							(exports == null) ? "" : exports + " && ",
							" cd " + dir.Replace("\\", "/"),  // use posix file separator 
							(exec == null) ? "" : " && " + exec,
							keep ? " && exec bash" : "" 
						);
				}
				// windows cmd
				else if (shell_windows_cmd.Equals(command))
				{
					//runas shell doesn't accept single quote ('), so we use the carret (^)
					//command = "c:/windows/system32/cmd.exe /k ^cd /d {dir} && {exec}^"
					prompt = prompt_cmd;
					exec =
						string.Format("{0} ^{1}{2}{3}{4}^",
							keep ? "/k" : "/c",
							prompt + " && ",
							(exports == null) ? "" : exports + " && ",
							" cd /d " + dir,
							(exec == null) ? "" : " && " + exec
						);
				}
				// arbitrary shell
				else
				{

				}

				// next determine user elevation mode. 4 modes:
				// user==administrator* -> Impersonating administrator, possible password prompt
				// user==(another user) -> Impersonating another user, possible password prompt
				// user==(current user) -> Elevated user, no password prompt, gui elevation dialog

				if (user == null)
				{
					Console.WriteLine (resource.IDS_MSG_ElevatingUserToAdministrator);
					user = "administrator";
				}
				if ("administrator".Equals (user.ToLower()))
				{
					if (security.IsAdministrator ())
					{
						Console.WriteLine (resource.IDS_MSG_AlreadyRunningAsAdministrator);
					}
					else
					{
						Console.WriteLine (resource.IDS_MSG_ImpersonatingAdministratorRole);
						if (pass == null)
						{
							Console.WriteLine ("wrapping runas command: {0}", runas);
							// @HACK: go via runas.exe to skip password
							//pass = security.PromptForStringPassword(user);
							shell = executable.Replace("\"", "\\\"");
							executable = runas;
							exec =
								string.Format ("{0}{1}{2}{3}/user:%COMPUTERNAME%\\{4} \"{5} {6}\"", 
									login ? "/profile " : "",
									envcr ? "/env " : "",
									netcr ? "/netonly " : "",
									savcr ? "/savecreds " : "",
									user, 
									shell, 
									exec
								);
							Console.WriteLine ("arguments: {0}", exec);
						}
						if (pass != null)
						{
							startInfo.UseShellExecute = false;
							startInfo.UserName = user;
							startInfo.Password = security.ToSecureString (pass);
						}
					}
				}
				else
				{
					if (security.IsUser(user) )
					{
						Console.WriteLine (resource.IDS_MSG_AlreadyRunningAsUser);
					}
					else
					{
						Console.WriteLine (resource.IDS_MSG_ImpersonatingUserRole);
						if (pass == null)
						{
							Console.WriteLine ("wrapping runas command: {0}", runas);
							// @HACK: go via runas.exe to skip password
							//pass = security.PromptForStringPassword(user);
							shell = executable.Replace("\"", "\\\"");
							executable = runas;
							exec =
								string.Format ("{0}{1}{2}{3}/user:%COMPUTERNAME%\\{4} \"{5} {6}\"",
									login ? "/profile " : "",
									envcr ? "/env " : "",
									netcr ? "/netonly " : "",
									savcr ? "/savcred " : "",
									user,
									shell, 
									exec
								);
							Console.WriteLine ("arguments: {0}", exec);
						}
						if (pass != null)
						{
							startInfo.UseShellExecute = false;
							startInfo.UserName = user;
							startInfo.Password = security.ToSecureString (pass);
						}
					}
				}

				startInfo.FileName = executable;
				startInfo.Arguments = exec;

				Console.WriteLine ("executing shell: {0} {1}", executable, exec);
				System.Diagnostics.Process.Start(startInfo);
			}
			else
			{
				Console.WriteLine(resource.IDS_ERR_InvalidArguments);
				Environment.Exit (1);
			}

		}

		static void Main(string[] args)
		{
			new su().Run(args);
		}

	}

}

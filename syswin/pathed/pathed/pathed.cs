using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Security;

namespace syswin
{
    /// <summary>
    /// This program allows you to view and modify the PATH environment.
    /// </summary>
    class pathed : Application
    {

        /// <summary>
        /// Current environment variable type
        /// </summary>
        protected EnvironmentVariableTarget EnvironmentVariableTarget = EnvironmentVariableTarget.Process;

        /// <summary>
        /// Name of environment variable, defaults to PATH
        /// </summary>
		protected string EnvironmentVariableName = "PATH";

        /// <summary>
        /// This program allows you to view and modify the PATH environment.
        /// </summary>
        /// <param name="args"></param>
        protected void Run(string[] args)
        {
			Hashtable env = (Hashtable)Environment.GetEnvironmentVariables();

            Console.OutputEncoding = Encoding.GetEncoding(Encoding.Default.CodePage);
            Args = new InputArgs("pathed", string.Format(resource.IDS_TITLE, AppVersion.Get()) + "\r\n" + resource.IDS_COPYRIGHT);

            Args.Add(InputArgType.Flag, 'm', "machine", false, Presence.Optional, resource.IDS_CMD_machine_doc);
            Args.Add(InputArgType.Flag, 'u', "user", false, Presence.Optional, resource.IDS_CMD_user_doc);
            Args.Add(InputArgType.ExistingDirectory, 'a', "add", "", Presence.Optional, resource.IDS_CMD_add_doc);
            Args.Add(InputArgType.ExistingDirectory, 'p', "append", "", Presence.Optional, resource.IDS_CMD_append_doc);
            Args.Add(InputArgType.StringList, 'r', "remove", null, Presence.Optional, resource.IDS_CMD_remove_doc);
            Args.Add(InputArgType.Flag, 's', "slim", false, Presence.Optional, resource.IDS_CMD_slim_doc);
            Args.Add(InputArgType.Parameter, 'e', "env", "PATH", Presence.Optional, resource.IDS_CMD_env_doc);

            if (Args.Process(args))
            {
                EnvironmentVariableName = Args.GetString("env");

                if (Args.GetFlag("slim"))
                    SlimPath();

                if (Args.GetFlag("machine"))
                    EnvironmentVariableTarget = EnvironmentVariableTarget.Machine;

                else if (Args.GetFlag("user"))
                    EnvironmentVariableTarget = EnvironmentVariableTarget.User;

                try
                {
                    List<string> removeItems = Args.GetStringList("remove");
                    if (removeItems != null)
                        Remove(removeItems);

                    string add = Args.GetString("add");
                    if (!string.IsNullOrEmpty(add))
                        AddHead(SanitizePath(add));

                    string append = Args.GetString("append");
                    if (!string.IsNullOrEmpty(append))
                        AddTail(SanitizePath(append));
                }
                catch (SecurityException ex)
                {
                    if (EnvironmentVariableTarget == EnvironmentVariableTarget.Machine)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(resource.IDS_ERR_access_denied);
                        return;
                    }
                    else throw;
                }

                ListPath();
            }
        }

		public string SanitizePath(string path)
		{
			if (path.Equals("."))
				return Environment.CurrentDirectory;
			return path;
		}

		public void ListPath()
		{
			string PathVariable = Environment.GetEnvironmentVariable(EnvironmentVariableName, EnvironmentVariableTarget);
			if (PathVariable == null)
			{
				Console.WriteLine(resource.IDS_ERR_variable_does_not_exist, EnvironmentVariableName);
			}
			else
			{
				string[] tokens = PathVariable.Split(';');
				int index = 0;
				foreach (string token in tokens)
				{
					if (string.IsNullOrEmpty(token))
						continue;
					if (!Directory.Exists(token))
					{
						Console.WriteLine(resource.IDS_RESULT_invalid, index, token);
					}
					else
					{
						Console.WriteLine(resource.IDS_RESULT_valid, index, token);
					}

					++index;
				}
			}
		}

        public void SlimPath()
        {
            List<string> vars = new List<string>();

            EnvironmentVariableTarget[] evts = {
                EnvironmentVariableTarget.Machine,
                EnvironmentVariableTarget.User };

            foreach (EnvironmentVariableTarget evt in evts)
            {
                string PathVariable = Environment.GetEnvironmentVariable(EnvironmentVariableName, evt);
                if (PathVariable != null)
                {
                    string[] tokens = PathVariable.Split(';');
                    foreach (string token in tokens)
                    {
                        if (Directory.Exists(token))
                            vars.Add(token);
                    }
                    vars.Add("\u0000");
                }
            }

            StringList.MakeUnique(vars, StringComparison.OrdinalIgnoreCase);

            StringBuilder temp = new StringBuilder();
            bool first = true;
            int evt_index = 0;
            foreach (string s in vars)
            {
                if (s.Equals("\u0000"))
                {
                    Environment.SetEnvironmentVariable(EnvironmentVariableName, temp.ToString(), evts[evt_index++]);
                    temp = new StringBuilder();
                    first = true;
                }
                else
                {
                    if (!first)
                        temp.Append(";");
                    first = false;
                    temp.Append(s);
                }
            }
            Environment.SetEnvironmentVariable(EnvironmentVariableName, temp.ToString(), evts[evt_index++]);
        }

        public void Remove(List<string> items)
        {
            string PathVariable = Environment.GetEnvironmentVariable(EnvironmentVariableName, EnvironmentVariableTarget);
            if (PathVariable != null)
            {
                string[] tokens = PathVariable.Split(';');

                foreach (string remove_this_item in items)
                {
                    int remove_this_index;

                    if (remove_this_item.Equals("invalid", StringComparison.OrdinalIgnoreCase))
                    {
                    }
                    else if (int.TryParse(remove_this_item, out remove_this_index))
                    {
                        if (remove_this_index < tokens.Length)
                            tokens[remove_this_index] = null;
                    }
                    else
                    {
                        int index = 0;
                        foreach (string token in tokens)
                        {
                            if (token.Equals(remove_this_item, StringComparison.OrdinalIgnoreCase))
                            {
                                tokens[index] = null;
                            }
                            ++index;
                        }
                    }
                }

                StringBuilder result = new StringBuilder();
                bool first = true;
                foreach (string token in tokens)
                {
                    if (token != null)
                    {
                        if (!first)
                            result.Append(";");
                        result.Append(token);
                        first = false;
                    }
                }
                Environment.SetEnvironmentVariable(EnvironmentVariableName, result.ToString(), EnvironmentVariableTarget);
            }
        }

        public void AddHead(string var)
        {
            string PathVariable = Environment.GetEnvironmentVariable(EnvironmentVariableName, EnvironmentVariableTarget);
            if (PathVariable != null)
            {
                PathVariable = var + ";" + PathVariable;
                Environment.SetEnvironmentVariable(EnvironmentVariableName, PathVariable, EnvironmentVariableTarget);
            }
        }

        public void AddTail(string var)
        {
            string PathVariable = Environment.GetEnvironmentVariable(EnvironmentVariableName, EnvironmentVariableTarget);
            if (PathVariable != null)
            {
                PathVariable = PathVariable + ";" + var;
                Environment.SetEnvironmentVariable(EnvironmentVariableName, PathVariable, EnvironmentVariableTarget);
            }
        } 

        static void Main(string[] args)
        {
            new pathed().Run(args);           
        }

    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Net;

/**
 * @module: 	httpget.exe
 * @project: 	Manufacture.net - SysWin utilities (syswin.manufacture.net)
 * @author: 	Francis Korning (fkorning@yahoo.ca)
 * @rigths: 	(c) 2014 Francis Korning (manufacture.net)
 * @license: 	Lesser GNU Public License (LGPL)
 * 
 * The httpget utility (aka wget, HttpRequest, HttpGetRequest) allows to fetch
 * text or binary files from a url via http get requests using the current NTLM
 * credentials.
 * 
 * This is a very rudimentary utility and is not meant to replace cygwin's wget.
 * It does https, but it doesn't handle redirects, formwards, or multipart types.
 * It is thus mainly provided to be able to retrieve software before cygwin wget.
 * This way we can programatically and transparently download cygwin installers
 * (cyg-setup-x86.exe) in order to configure our manufacture syswin platform.
 * 
 * httpget works in one of 2 two MIME content-type modes depending on mime type:
 * 
 * 	mode	content-types							example
 *  ----	-------------							--------
 * 	text 	text|message 							text/plain
 * 	binary	application|image|audio|video|model		application/octet-stream
 * 
 * 
 * parameters:
 * 
 * usage: httpget.exe [options]
 * options:
 *              -h --help /help:  this usage and options help message (/?)
 *                -u --url /url:  url of file to be downloaded (http/https)
 *                -d --dir /dir:  directory from which to run cmd shell
 * 			 	-f --file /file:  filename to save file (default: index.htm)
 *              -t --type /type:  mime content-type (text|application|image|audio|video)
 *              -w --winx /winx:  use WINx CRLF line separator (\r\n) for text file types 
 *              -x --unix /unix:  use UNIX LF line separator (\n) for text file types 
 *              -i --time /time:  timeout for download (seconds)
 * 
 * 
 * logic:
 * 
 *  parse args
 *
 * create request:
 * 
 *  create http request
 * 	- set NTLM credentials
 * 	- set proxy parameters
 * 
 * connect to url:
 * 
 *   check headers:
 *   - status
 *   - content-type
 *   - content-length
 *
 * determine mode: 
 * 	
 *  text-mode:
 *   - check content-type
 * 	 - read text stream
 * 	 - save to text file
 * 
 *  mime-mode:
 *   - check content-type
 * 	 - read data stream
 * 	 - save to data file
 * 
 * 
 * examples:
 * 
 * - (default) download cygwin index html
 *
 *  	httpget -d tmp -u https://www.cygwin.com/ -t text -f index.htm
 * 
 * 
 * - download cygwin plain text license file
 *
 *  	httpget --dir tmp --url https://www.cygwin.com/COPYING --type text --file cyg-license.txt
 *
 *
 * - download cygwin setup executable binary file
 *
 *  	httpget --dir tmp --url https://www.cygwin.com/setup-x86.exe --type application --file cyg-setup-x86.exe
 *
 *
 * download sysinternals binary zip file
 *
 *  	httpget /dir tmp /url http://download.sysinternals.com/files/SysinternalsSuite.zip /type application /file sys-internals.zip
 *
 *
 * todo: 
 * 
 * 	-H/HttpProxy <Host:Port>
 * 	-U/ProxyUser -P/ProxyPass
 * 	-a/useragent, -c/charset, -e/encoding
**/

namespace syswin
{
	public class httpget : Application
    {

		public httpget() : base()
		{

		}


		/// <summary>
		/// This program finds an executable on the PATH. 
		/// It can also find other stuff on the path, but 
		/// mostly it finds the executable.s
		/// </summary>
		/// <param name="args"></param>
		protected void Run(string[] args)
		{
			string type_text = "text";
			string type_message = "message";
			string type_multipart= "multipart";
			string type_application = "application";
			string type_example = "example";
			string type_model = "model";
			string type_image = "image";
			string type_audio = "audio";
			string type_video = "video";



			Console.OutputEncoding = Encoding.GetEncoding(Encoding.Default.CodePage);

			Args = new InputArgs(this.GetType().Name,
				string.Format(resource.IDS_TITLE, AppVersion.Get()) + "\r\n"
				+ "author: Francis Korning (fkorning@yahoo.ca)"  + "\r\n"
				+ resource.IDS_COPYRIGHT + "\r\n" + resource.IDS_LICENSING + "\r\n"
			);

			Args.Add(InputArgType.Parameter, 'h', "help", null, Presence.Optional, resource.IDS_OPT_help);
			Args.Add(InputArgType.Parameter, 'u', "url", null, Presence.Optional, resource.IDS_OPT_url);
			Args.Add(InputArgType.Parameter, 'd', "dir", null, Presence.Optional, resource.IDS_OPT_dir);
			Args.Add(InputArgType.Parameter, 'f', "file", "index.htm", Presence.Optional, resource.IDS_OPT_dir);
			Args.Add(InputArgType.Parameter, 't', "type", "", Presence.Optional, resource.IDS_OPT_type);
			Args.Add(InputArgType.Flag, 'w', "winx", false, Presence.Optional, resource.IDS_OPT_winx);
			Args.Add(InputArgType.Flag, 'x', "unix", false, Presence.Optional, resource.IDS_OPT_unix);
			Args.Add(InputArgType.Parameter, 'i', "time", 0, Presence.Optional, resource.IDS_OPT_time);


			if (Args.Process(args))
			{
				string url = Args.GetString("url");
				string dir = Args.GetString("dir");
				string file = Args.GetString("file");
				string type = Args.GetString("type");
				bool winx = Args.GetFlag ("winx");
				bool unix = Args.GetFlag ("unix");
				int timeout = Args.GetInteger("time");


				HttpWebRequest request = null;
				HttpWebResponse response = null;

                if (url == null || url.Length == 0)
                {
					Console.WriteLine(resource.IDS_ERR_UrlLMandatory);
                    Environment.Exit(1);
                }

                try
                {
                    // Create a request for the URL.         
					request = (HttpWebRequest) WebRequest.Create(url);                    

					string host = request.RequestUri.Host;
					int port = request.RequestUri.Port;
					string path = request.RequestUri.LocalPath;
					string query = request.RequestUri.Query;

					// guess file from url path
					int i = -1;
					if (request.RequestUri.IsFile)
					{
						if (file == null || file.Length == 0)
						{
							i = path.LastIndexOf("/");
							if (i >= 0 && i < path.Length - 1)
							{
								file = path.Substring(i + 1);
							}
						}
					}
						
					Console.WriteLine(resource.IDS_MSG_CreatingHttpRequest, host, port, path, type, file);



					// If a timeout was provided, use it
					if (timeout > 0)
					{
						request.Timeout = timeout;
					}
						

                    // expose the credentials.
					Console.WriteLine(resource.IDS_MSG_ExposingNetworkCredentials);
                    request.Credentials = CredentialCache.DefaultCredentials;



                    // Get the response.
                    response = (HttpWebResponse)request.GetResponse();


					//string filter = response.GetResponseHeader(text);
					HttpStatusCode code = response.StatusCode;
					string status = response.StatusDescription;
					string charset = response.CharacterSet;
					string encoding = response.ContentEncoding;
					string mime = response.ContentType.ToLower();
					long length = response.ContentLength;

					// special case for mime content-type: strip ending semi-colon (;)
					mime = mime.Replace(";", "");

					Stream inStream = null;				// url input stream
					StreamReader reader = null;			// text input readers
					TextWriter writer = null;			// text output writer

					FileStream outStream = null;		// binary output stream
					byte[] buffer = new byte[1024];		// binary stream buffer

					String fileName = null;				// output file name



					// Display the status.
					Console.WriteLine(resource.IDS_MSG_FetchingHttpResponse, status, charset, encoding, mime, length);


					// check type
					if ( (type != null) && (type.Length != 0) )
					{
						if (! mime.StartsWith(type) )
						{
							Console.WriteLine(resource.IDS_WARN_MimeContentTypeMismatch, type, mime);
						}
					}

					// if no dir or file specified, we skip the content.
					if ( (dir == null) || (dir.Length == 0) )
					{
						Environment.Exit(0);
					}


                    // Get the stream containing content returned by the server.
					inStream = response.GetResponseStream();

                    try
                    {
						fileName = string.Format("{0}\\{1}", dir, file);
						//new FileInfo (dir).Create();
						//new FileInfo (fileName).Delete();


						// multipart stream:
						if (   ( mime.StartsWith(type_multipart) || mime.StartsWith(type_example) ) 
							|| ( (type != null) && type.StartsWith(type_multipart) )
							|| ( (type != null) && type.StartsWith(type_example) )
						   )
						{
							Console.WriteLine(resource.IDS_ERR_MimeContentTypeUnsupported, "" + type, "" + mime);
						}


						//
						// text stream
						//
						if (   ( mime.StartsWith(type_text) || mime.StartsWith(type_message) ) 
							|| ( (type != null) && type.StartsWith(type_text) )
							|| ( (type != null) && type.StartsWith(type_message) )
						   )
						{
							Console.WriteLine(resource.IDS_MSG_DownloadingPlainTextFile, "" + type, "" + file);

							// Open the stream using a StreamReader for easy access.
							reader = new StreamReader(inStream);
							writer = new StreamWriter(fileName);

							// Read entine content in order to preserve line terminators.
							string content = reader.ReadToEnd();
							string magic = content.Substring(0, 4);
							string body = content.Substring(5);
							if (winx)
							{
								content = content.Replace("\n", "\r\n");
								content = content.Replace("\r\r\n", "\r\n");
							}
							if (unix)
							{
								content = content.Replace("\r\n", "\n");
							}

							writer.Write(content);

							// Read using buffered lines - this strips line terminators
							//string content = null;
							//while ( (content = reader.ReadLine()) != null)
							//{
							//	if (crlf)
							//	{
							//		writer.WriteLine(content);
							//	}
							//	else
							//	{
							//		writer.Write(content + "\n");
							//	}
							//}

							writer.Close();
							reader.Close();

							Console.WriteLine(resource.IDS_MSG_DownloadedPlainTextFile, "" + mime, "" + fileName);
						}


						//
						// binary stream 
						//
						if (   ( (type == null) || (type.Length == 0) )
							|| ( type.StartsWith(type_application) || mime.StartsWith(type_application) )
							|| ( type.StartsWith(type_image) || mime.StartsWith(type_image) )
							|| ( type.StartsWith(type_audio) || mime.StartsWith(type_audio) )
							|| ( type.StartsWith(type_video) || mime.StartsWith(type_video) )
							|| ( type.StartsWith(type_model) || mime.StartsWith(type_model) )
						   )
						{
							Console.WriteLine(resource.IDS_MSG_DownloadingBinaryAppFile, "" + type, "" + file);

							outStream = new FileStream(fileName, FileMode.Create);

							int bytesRead;
							while((bytesRead = inStream.Read(buffer, 0, buffer.Length)) != 0)
								outStream.Write(buffer, 0, bytesRead);

							outStream.Close();

							Console.WriteLine(resource.IDS_MSG_DownloadedBinaryAppFile, "" + mime, "" + fileName);
						}

						// Cleanup the streams and the response.
						inStream.Close();
						response.Close();
					}
					catch (System.IO.IOException ex)
					{
						Console.WriteLine(ex.Message.ToString());
					}

				}
                catch (System.UriFormatException ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
                catch (System.Net.WebException ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
            }

            Environment.Exit(0);
        }


		static void Main(string[] args)
		{
			new httpget().Run(args);
		}

    }


}


using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Utilities
{
    public class WebServer
    {
        private int _port;
        private string _basePath;
        private HttpListener _listener;        
        private Dictionary<string, Func<HttpListenerRequest, string>> _methods;

        public WebServer(int port)
        {
            if (!HttpListener.IsSupported) throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");

            _port = port;
            _basePath = "http://+:" + _port.ToString() + "/";
            _methods = new Dictionary<string, Func<HttpListenerRequest, string>>();            
        }

        public void AddMethod(string path, Func<HttpListenerRequest, string> method)
        {
            _methods[path] = method;            
        }

        public void CreateListener()
        {
            _listener = new HttpListener();
            foreach (string path in _methods.Keys)
            {
                if (String.IsNullOrEmpty(path))
                {
                    _listener.Prefixes.Add(_basePath);
                }
                else
                {
                    _listener.Prefixes.Add(_basePath + path + "/");
                }
            }
        }
        

        private void StartServer()
        {           
            if (_methods.Count == 0) throw new ArgumentException("You must register some methods using AddMethod");
            try
            {
                CreateListener();
                _listener.Start();
            }
            catch (System.Net.HttpListenerException)
            {
                if (MessageBox.Show("The Scoreboard Server needs permisson to run. Please enter your administrator password to grant permission.", "Scoreboard Server", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    try
                    {
                        RegisterUrlAndFirewall();

                        CreateListener();
                        _listener.Start();
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("The Scoreboard Server could not start. Try a different port or try running as administrator.\n\nError:\n" + exception.Message);
                        Stop();
                        throw;                           
                    }
                }
                else
                {
                    Stop();
                    throw;                   
                }                
            }
        }        

        public void Run()
        {
            StartServer();
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                string path = ctx.Request.RawUrl.Substring(1);
                                string rstr;

                                if (_methods.ContainsKey(path))
                                {
                                    Func<HttpListenerRequest, string> method = _methods[path];
                                    rstr = method(ctx.Request);
                                }
                                else
                                {
                                    rstr = "Invalid Request";
                                }
                                                            
                                byte[] buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);                                
                            }
                            catch { } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
        }

        public void Stop()
        {
            try
            {
                if (_listener != null)
                {
                    _listener.Stop();
                    _listener.Close();
                }
            }
            catch
            {
                // Ignore
                // This happens when the server does not get created properly.
            }
            _listener = null;
        }

        public void RegisterUrlAndFirewall()
        {
            StringBuilder command = new StringBuilder();
            command.AppendLine("netsh http add urlacl url = " + _basePath + " user = everyone");
            command.AppendLine("netsh advfirewall firewall add rule name =\"SimpleWebServer\" dir=in action=allow protocol=TCP localport=" + _port.ToString());

            string commandFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".cmd");
            File.WriteAllText(commandFileName, command.ToString());

            RunElevatedCommand(commandFileName, String.Empty);           
        }

        public static void RunElevatedCommand(string command, string args)
        {            
            ProcessStartInfo psi = new ProcessStartInfo(command, args);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = true;

            Process.Start(psi).WaitForExit();
        }

        public bool IsActive
        {
            get
            {
                return _listener != null;
            }
        }
    }
}


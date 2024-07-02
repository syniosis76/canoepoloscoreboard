using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
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
        private List<WebSocket> _webSockets = new List<WebSocket>();      
        private Dictionary<string, Func<HttpListenerRequest, string>> _methods;

        private static Dictionary<string, string> _contentType = new Dictionary<string, string>
        {
            { ".html", "text/html" }
            , { ".css", "text/css" }
            , { ".js", "text/javascript" }
            , { ".svg", "image/svg+xml" }
            , { ".woff2", "font/woff2" }
        };

        private Func<HttpListenerRequest, string> _defaultMethod { get; set; }
        public Func<HttpListenerRequest, string> DefaultMethod
        {
            get { return _defaultMethod; }
            set { _defaultMethod = value; }
        }

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
                            var listenerContext = c as HttpListenerContext;

                            if (listenerContext.Request.IsWebSocketRequest)
                            {
                                ProcessWebSocketRequest(listenerContext);
                            }
                            else
                            {    
                                try
                                {                                                            
                                    string path = listenerContext.Request.RawUrl.Substring(1);
                                    string rstr;

                                    if (_methods.ContainsKey(path))
                                    {
                                        Func<HttpListenerRequest, string> method = _methods[path];
                                        rstr = method(listenerContext.Request);
                                    }
                                    else if (_defaultMethod != null)
                                    {
                                        rstr = _defaultMethod(listenerContext.Request);    
                                    }
                                    else
                                    {
                                        rstr = "Invalid Request";
                                    }                                

                                    listenerContext.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                                    listenerContext.Response.AppendHeader("Content-Type", GetContentType(listenerContext.Request.Url.ToString()));

                                    byte[] buf;                                
                                    
                                    if (IsBinaryFile(path))
                                    {
                                        buf = Convert.FromBase64String(rstr);
                                    }
                                    else
                                    {
                                        buf = Encoding.UTF8.GetBytes(rstr);
                                    }                                
                                    
                                    listenerContext.Response.ContentLength64 = buf.Length;
                                    listenerContext.Response.OutputStream.Write(buf, 0, buf.Length);                                
                                }
                                catch { } // suppress any exceptions
                                finally
                                {
                                    // always close the stream
                                    listenerContext.Response.OutputStream.Close();
                                }
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
                CloseWebSockets();
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
            ProcessStartInfo psi = new ProcessStartInfo(command, args)
            {
                Verb = "runas",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            };

            Process.Start(psi).WaitForExit();
        }

        public bool IsActive
        {
            get
            {
                return _listener != null;
            }
        }

        public static bool IsBinaryFile(string url)
        {
            if (url.EndsWith(".woff2"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetFileExtensionFromUrl(string url)
        {
            url = url.Split('?')[0];
            url = url.Split('/')[^1];
            return url.Contains('.') ? url.Substring(url.LastIndexOf('.')) : "";
        }

        public static string GetContentType(string url)
        {
            string extension = GetFileExtensionFromUrl(url);
            if (_contentType.ContainsKey(extension))
            {
                return _contentType[extension];
            }
            else
            {
                return null;
            }
        }

        private async void ProcessWebSocketRequest(HttpListenerContext listenerContext)
        {            
            WebSocketContext webSocketContext = null;

            try
            {                
                // When calling `AcceptWebSocketAsync` the negotiated subprotocol must be specified. This sample assumes that no subprotocol 
                // was requested. 
                webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
                Console.WriteLine("ProcessWebSocketRequest");
            }
            catch(Exception e)
            {
                // The upgrade process failed somehow. For simplicity lets assume it was a failure on the part of the server and indicate this using 500.
                listenerContext.Response.StatusCode = 500;
                listenerContext.Response.Close();
                Console.WriteLine("Exception: {0}", e);
                return;
            }
                                
            WebSocket webSocket = webSocketContext.WebSocket;

            _webSockets.Add(webSocket);            
        }

        public void SendWebSocketMessage(string message)
        {
            foreach (WebSocket webSocket in _webSockets)
            {
                webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);  
            }            
        }

        private void CloseWebSockets()
        {
            foreach (WebSocket webSocket in _webSockets)
            {
                webSocket.Dispose();
            }

            _webSockets.Clear();
        }
    }
}


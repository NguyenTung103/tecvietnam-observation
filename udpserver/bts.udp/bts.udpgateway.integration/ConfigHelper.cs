using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace bts.udpgateway.integration
{
    static public class ConfigHelper
    {
        static private readonly object _sync = new object();

        static private string _connection_string;
        /// <summary>
        /// connection string to database
        /// </summary>
        static public string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connection_string))
                {
                    lock (_sync)
                    {
                        if (string.IsNullOrEmpty(_connection_string))
                            _connection_string = ConfigurationManager.AppSettings["connection_string"];
                    }
                }
                if (string.IsNullOrEmpty(_connection_string))
                    throw new Exception("connection_string is not configured.");
                return _connection_string;
            }
        }

        static private string _package_path;
        /// <summary>
        /// path to folder where packages are stored
        /// </summary>
        static public string PackagePath
        {
            get
            {
                if (string.IsNullOrEmpty(_package_path))
                {
                    lock (_sync)
                    {
                        if (string.IsNullOrEmpty(_package_path))
                            _package_path = ConfigurationManager.AppSettings["package_path"];
                    }
                }
                if (string.IsNullOrEmpty(_package_path))
                    throw new Exception("package_path is not configured.");
                if (!_package_path.EndsWith("\\"))
                    _package_path += "\\";
                //if (!Path.IsPathRooted(_package_path))
                //{
                //    string loc = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                //    if (!loc.EndsWith("\\"))
                //        loc += "\\";
                //    _package_path = loc + _package_path;
                //}
                return _package_path;
            }
        }

        static private string _message_path;
        /// <summary>
        /// path to folder where messages are stored
        /// </summary>
        static public string MessagePath
        {
            get
            {
                if (string.IsNullOrEmpty(_message_path))
                {
                    lock (_sync)
                    {
                        if (string.IsNullOrEmpty(_message_path))
                            _message_path = ConfigurationManager.AppSettings["message_path"];
                    }
                }
                if (string.IsNullOrEmpty(_message_path))
                    throw new Exception("_message_path is not configured.");
                if (!_message_path.EndsWith("\\"))
                    _message_path += "\\";
                //if (!Path.IsPathRooted(_message_path))
                //{
                //    string loc = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                //    if (!loc.EndsWith("\\"))
                //        loc += "\\";
                //    _message_path = loc + _message_path;
                //}
                return _message_path;
            }
        }

        static private string _wapi_base_url;
        /// <summary>
        /// web-api base url
        /// </summary>
        static public string WapiBaseUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_wapi_base_url))
                {
                    lock (_sync)
                    {
                        if (string.IsNullOrEmpty(_wapi_base_url))
                            _wapi_base_url = ConfigurationManager.AppSettings["wapi_base_url"];
                    }
                }
                if (string.IsNullOrEmpty(_wapi_base_url))
                    throw new Exception("wapi_base_url is not configured.");
                return _wapi_base_url;
            }
        }

        static private string _wapi_get_pending;
        /// <summary>
        /// web-api url from base to get pending messages
        /// </summary>
        static public string WapiGetPending
        {
            get
            {
                if (string.IsNullOrEmpty(_wapi_get_pending))
                {
                    lock (_sync)
                    {
                        if (string.IsNullOrEmpty(_wapi_get_pending))
                            _wapi_get_pending = ConfigurationManager.AppSettings["wapi_get_pending"];
                    }
                }
                if (string.IsNullOrEmpty(_wapi_get_pending))
                    throw new Exception("wapi_get_pending is not configured.");
                return _wapi_get_pending;
            }
        }

        static private string _wapi_update_done;
        /// <summary>
        /// web-api url from base to update pending messages
        /// </summary>
        static public string WapiUpdateDone
        {
            get
            {
                if (string.IsNullOrEmpty(_wapi_update_done))
                {
                    lock (_sync)
                    {
                        if (string.IsNullOrEmpty(_wapi_update_done))
                            _wapi_update_done = ConfigurationManager.AppSettings["wapi_update_done"];
                    }
                }
                if (string.IsNullOrEmpty(_wapi_update_done))
                    throw new Exception("wapi_update_done is not configured.");
                return _wapi_update_done;
            }
        }

        static private int _udp_port;
        /// <summary>
        /// udp port for communicating data with device
        /// </summary>
        static public int UdpPort
        {
            get
            {
                if (_udp_port <= 0)
                {
                    lock (_sync)
                    {
                        if (_udp_port <= 0)
                        {
                            string tmp = ConfigurationManager.AppSettings["udp_port"];
                            if (!int.TryParse(tmp, out _udp_port))
                                throw new Exception(string.Format("udp_port configuration is invalid (#value={0}).", tmp));
                        }
                    }
                }
                if (_udp_port <= 0)
                    throw new Exception("udp_port is not configured.");
                return _udp_port;
            }
        }

        static private int _delay_get_pending;
        /// <summary>
        /// time between each loop for getting pending message from web-api (total miliseconds)
        /// </summary>
        static public int DelayGetPending
        {
            get
            {
                if (_delay_get_pending <= 0)
                {
                    lock (_sync)
                    {
                        if (_delay_get_pending <= 0)
                        {
                            string tmp = ConfigurationManager.AppSettings["delay_get_pending"];
                            TimeSpan ts = new TimeSpan();
                            if (!TimeSpan.TryParse(tmp, out ts))
                                throw new Exception(string.Format("delay_get_pending configuration is invalid (#value={0}).", tmp));
                            _delay_get_pending = Convert.ToInt32(ts.TotalMilliseconds);
                        }
                    }
                }
                if (_delay_get_pending <= 0)
                    throw new Exception("delay_get_pending is not configured.");
                return _delay_get_pending;
            }
        }

        static private int _delay_to_send;
        /// <summary>
        /// maximum time difference between pending message and current time (total miliseconds)
        /// </summary>
        static public int DelayToSend
        {
            get
            {
                if (_delay_to_send <= 0)
                {
                    lock (_sync)
                    {
                        if (_delay_to_send <= 0)
                        {
                            string tmp = ConfigurationManager.AppSettings["delay_to_send"];
                            TimeSpan ts = new TimeSpan();
                            if (!TimeSpan.TryParse(tmp, out ts))
                                throw new Exception(string.Format("delay_to_send configuration is invalid (#value={0}).", tmp));
                            _delay_to_send = Convert.ToInt32(ts.TotalMilliseconds);
                        }
                    }
                }
                if (_delay_to_send <= 0)
                    throw new Exception("delay_to_send is not configured.");
                return _delay_to_send;
            }
        }

        static private int _delay_to_sync;
        /// <summary>
        /// time between each loop for synchronizing offline files (total miliseconds)
        /// </summary>
        static public int DelayToSync
        {
            get
            {
                if (_delay_to_sync <= 0)
                {
                    lock (_sync)
                    {
                        if (_delay_to_sync <= 0)
                        {
                            string tmp = ConfigurationManager.AppSettings["delay_to_sync"];
                            TimeSpan ts = new TimeSpan();
                            if (!TimeSpan.TryParse(tmp, out ts))
                                throw new Exception(string.Format("delay_to_sync configuration is invalid (#value={0}).", tmp));
                            _delay_to_sync = Convert.ToInt32(ts.TotalMilliseconds);
                        }
                    }
                }
                if (_delay_to_sync <= 0)
                    throw new Exception("delay_to_sync is not configured.");
                return _delay_to_sync;
            }
        }

        static private string _to_database;
        /// <summary>
        /// option: push data into database or not?
        /// </summary>
        static public bool ToDatabase
        {
            get
            {
                if (string.IsNullOrEmpty(_to_database))
                {
                    lock (_sync)
                    {
                        if (string.IsNullOrEmpty(_to_database))
                            _to_database = ConfigurationManager.AppSettings["to_database"];
                    }
                }
                if (string.IsNullOrEmpty(_to_database))
                    throw new Exception("to_database is not configured.");
                if (_to_database.ToLower() == "true")
                    return true;
                if (_to_database.ToLower() == "false")
                    return false;

                return false;
            }
        }

        static private string _to_filesystem;
        /// <summary>
        /// option: push data into filesystem or not?
        /// </summary>
        static public bool ToFilesystem
        {
            get
            {
                if (string.IsNullOrEmpty(_to_filesystem))
                {
                    lock (_sync)
                    {
                        if (string.IsNullOrEmpty(_to_filesystem))
                            _to_filesystem = ConfigurationManager.AppSettings["to_filesystem"];
                    }
                }
                if (string.IsNullOrEmpty(_to_filesystem))
                    throw new Exception("to_filesystem is not configured.");
                if (_to_filesystem.ToLower() == "true")
                    return true;
                if (_to_filesystem.ToLower() == "false")
                    return false;

                return false;
            }
        }
    }
}

using bts.udpgateway.integration;
using knote.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace bts.udpgateway
{
    static public class UDPProcess
    {
        #region MEMBER DEFINITIONS
        static private ManualResetEvent _runningFlag = new ManualResetEvent(true);
        static private Mutex _controlFlag = new Mutex(true, "control_flag");

        static private readonly object _cmd_SF = new object();
        static private Dictionary<string, Message> _Commands = new Dictionary<string, Message>();

        static private UdpClient _udpClient = null;
        static private IPEndPoint _endpoint = null;
        #endregion

        #region THREAD DEFINITIONS
        static private void _Extract()
        {
            try
            {
                //LogHelper.Write("[EXTRACT:BEGIN] _Extract thread is started", "UDPProcess._Extract");
                while (IsRunning)
                {
                    Thread.Sleep(ConfigHelper.DelayGetPending);
                    try
                    {
                        //LogHelper.Write("[EXTRACT:LOOP] _Extracting", "UDPProcess._Extract");

                        ReturnInfo ri = Integrator.GetPendingMessages();
                        if (ri.Code != ReturnCode.Success)
                        {
                            //LogHelper.Write(ri, "UDPProcess._Transmit");
                            continue;
                        }

                        List<Message> list = null;
                        if (ri.Data != null)
                        {
                            if (ri.Data is List<Message>)
                            {
                                list = (List<Message>)ri.Data;
                            }
                            else if (ri.Data is Message[])
                            {
                                list = new List<Message>((Message[])ri.Data);
                            }
                            else
                            {
                                //LogHelper.Write(string.Format("[EXTRACT:DO] Data is not List<Message> ({0})", ri.Data.GetType()), "UDPProcess._Transmit", LogLevel.ERROR);
                                continue;
                            }
                        }

                        lock (_cmd_SF)
                        {
                            foreach (Message msg in list)
                            {
                                /*
                                 * check delay to send MESSAGE
                                 */
                                //if (msg.time.Add(TimeSpan.FromMilliseconds(ConfigHelper.DelayToSend)) < DateTime.Now)
                                //    continue;

                                if (!_Commands.ContainsKey(msg.device_eqid))
                                {
                                    //LogHelper.Write(string.Format("[EXTRACT:DO] Add new pending message (#EQID={0};#CMD={1};)", msg.device_eqid, msg.command), "UDPProcess._Transmit");
                                    _Commands.Add(msg.device_eqid, msg);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //LogHelper.Write(ex, "UDPProcess._Extract");
                    }
                }
            }
            catch (Exception ex)
            {
                //LogHelper.Write(ex, "UDPProcess._Extract");
            }
            finally
            {
                //LogHelper.Write("[EXTRACT:END] _Extract thread is stopped", "UDPProcess._Extract");
            }
        }
        static private void _Transmit()
        {
            try
            {
                ////LogHelper.Write("[CAPTURE:BEGIN] _Transmit thread is started", "UDPProcess._Transmit");

                while (IsRunning)
                {
                    try
                    {
                        if (_udpClient == null)
                            _udpClient = new UdpClient(ConfigHelper.UdpPort);
                        if (_endpoint == null)
                            _endpoint = new IPEndPoint(IPAddress.Any, ConfigHelper.UdpPort);

                        ////LogHelper.Write("[CAPTURE:LOOP] Waiting for broadcast", "UDPProcess._Transmit");
                        byte[] bytes = _udpClient.Receive(ref _endpoint);
                        ////LogHelper.Write(string.Format("[CAPTURE:LOOP] Received {0} bytes from {1}", bytes.Length, _endpoint.ToString()), "UDPProcess._Transmit");
                        string package = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                        ////LogHelper.Write(string.Format("[CAPTURE:LOOP] [DATA]\r\n{0}\r\n", package), "UDPProcess._Transmit");

                        if (ConfigHelper.ToDatabase)
                        {
                            var strs = package.Split(new string[] { "END;" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string item in strs)
                            {
                                if (String.IsNullOrWhiteSpace(item))
                                    continue;

                                string subMessage = item + "END;";
                                ReturnInfo rii = Integrator.Insert(subMessage);

                                //FAILED TO insert sub-message into database
                                if (rii.Code != ReturnCode.Success)
                                {
                                    //LogHelper.Write(rii, "UDPProcess._Transmit");

                                    string fpx = null;
                                    if (subMessage.StartsWith("EQID="))
                                        fpx = subMessage.Substring(5, 10);

                                    ////LogHelper.Write("[CAPTURE:LOOP] [BEGIN] Write message to file.", "UDPProcess._Transmit");
                                    IOHelper.WriteMSG(subMessage, fpx);
                                    ////LogHelper.Write("[CAPTURE:LOOP] [END] Write message to file.", "UDPProcess._Transmit");
                                }

                                //Check if this is a time request message

                                //EQID=0000000111;REQ;15:21:57-15/09/17;TIME;END;
                                //Server gửi lại:
                                //cmdC24=11:33:00-15/09/17-6;
                                if (subMessage.Contains(";REQ;") && subMessage.Contains(";TIME;END;"))
                                {
                                    ////LogHelper.Write("[CAPTURE:LOOP] [BEGIN] Reply TIME Request.", "UDPProcess._Transmit");

                                    var commandBack = string.Format("cmdC24={0:00}:{1:00}:{2:00}-{3:00}/{4:00}/{5:00}-{6:d};", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year % 100, DateTime.Now.DayOfWeek + 1);
                                    ////LogHelper.Write(string.Format("[CAPTURE:LOOP] [CONTENT]\r\n{0}\r\n", commandBack), "UDPProcess._Transmit");
                                    byte[] dgram = Encoding.ASCII.GetBytes(commandBack);
                                    _udpClient.Send(dgram, dgram.Length, _endpoint);
                                    
                                    ////LogHelper.Write("[CAPTURE:LOOP] [END] Reply TIME Request.", "UDPProcess._Transmit");
                                }

                                if (subMessage.StartsWith("EQID="))
                                {
                                    string eqid = subMessage.Substring(5, 10);
                                    Message message = null;
                                    lock (_cmd_SF)
                                    {
                                        if (_Commands.ContainsKey(eqid))
                                            message = _Commands[eqid];
                                    }
                                    if (message != null)
                                    {
                                        ////LogHelper.Write(string.Format("[CAPTURE:LOOP] [BEGIN] Reply REQ from device (EQID={0}).", eqid), "UDPProcess._Transmit");

                                        byte[] dgram = Encoding.ASCII.GetBytes(message.command);
                                        int len = _udpClient.Send(dgram, dgram.Length, _endpoint);
                                        ////LogHelper.Write(string.Format("[CAPTURE:LOOP] [CONTENT]\r\n{0}\r\n", message.command), "UDPProcess._Transmit");

                                        lock (_cmd_SF)
                                        {
                                            ReturnInfo riu = Integrator.UpdatePendingMessage(message.id);
                                            if (riu.Code != ReturnCode.Success)
                                            {
                                                ////LogHelper.Write(riu, "UDPProcess._Transmit");
                                            }
                                            else
                                            {
                                                //if (_Commands.ContainsKey(message.device_eqid))
                                                //    _Commands.Remove(message.device_eqid);

                                                if (_Commands.ContainsKey(eqid))
                                                    _Commands.Remove(eqid);

                                                ////LogHelper.Write(string.Format("[CAPTURE:LOOP] Updated pending message (#EQID={0};#CMD={1})", message.device_eqid, message.command), "UDPProcess._Transmit");
                                            }
                                        }

                                        ////LogHelper.Write(string.Format("[CAPTURE:LOOP] [END] Reply REQ from device (EQID={0}).", eqid), "UDPProcess._Transmit");
                                    }
                                }
                            }
                        }
                        if (ConfigHelper.ToFilesystem)
                        {
                            IOHelper.WritePKG(package);
                        }
                    }
                    catch (SocketException sex)
                    {
                        ////LogHelper.Write(string.Format("[CAPTURE:LOOP]\r\n#Type={0}\r\n#Message={1}\r\n", sex.GetType(), sex.Message), "UDPProcess._Transmit", LogLevel.EXCEPTION);
                        if (_udpClient != null)
                        {
                            if (_udpClient.Client != null)
                            {
                                _udpClient.Client.Shutdown(SocketShutdown.Both);
                                _udpClient.Client.Close();
                            }
                            _udpClient.Close();
                            _udpClient = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        //LogHelper.Write(string.Format("[CAPTURE:LOOP]\r\n#Type={0}\r\n#Message={1}\r\n", ex.GetType(), ex.Message), "UDPProcess._Transmit", LogLevel.EXCEPTION);
                    }
                }
            }
            catch (Exception ex)
            {
                //LogHelper.Write(ex, "UDPProcess._Transmit");
            }
            finally
            {
                //LogHelper.Write("[CAPTURE:END] _Transmit thread is stopped", "UDPProcess._Transmit");

                if (_udpClient != null)
                {
                    if (_udpClient.Client != null)
                    {
                        _udpClient.Client.Shutdown(SocketShutdown.Both);
                        _udpClient.Client.Close();
                    }
                    _udpClient.Close();
                    _udpClient = null;
                }
            }
        }
        static private void _SyncOFFLINE()
        {
            try
            {
                //LogHelper.Write("[SYNC:BEGIN] _SyncOFFLINE thread is started", "UDPProcess._SyncOFFLINE");
                while (IsRunning)
                {
                    int count = 0;
                    int idx = 0;
                    try
                    {
                        //LogHelper.Write("[SYNC:LOOP] Start synchronizing offline files", "UDPProcess._SyncOFFLINE");
                        if (!System.IO.Directory.Exists(ConfigHelper.MessagePath))
                        {
                            //LogHelper.Write(string.Format("[SYNC:LOOP] Path is not found (#path={0}).", ConfigHelper.MessagePath), "UDPProcess._SyncOFFLINE");
                            continue;
                        }
                        string[] files = System.IO.Directory.GetFiles(ConfigHelper.MessagePath);
                        if (files == null || files.Length == 0)
                            Thread.Sleep(ConfigHelper.DelayToSync);
                        count = files.Length;

                        foreach (string fp in files)
                        {
                            string content = IOHelper.Read(fp);
                            if (string.IsNullOrEmpty(content))
                                continue;
                            ReturnInfo ri = Integrator.Insert(content);
                            if (ri.Code != ReturnCode.Success)
                                LogHelper.Write(ri, "UDPProcess._SyncOFFLINE");
                            else
                                IOHelper.Move(fp);
                            idx++;
                        }
                    }
                    catch (Exception ex)
                    {
                        //LogHelper.Write(ex, "UDPProcess._SyncOFFLINE");
                    }
                    finally
                    {
                        //LogHelper.Write(string.Format("[SYNC:LOOP] Synchronized {0}/{1} offline files", idx, count), "UDPProcess._SyncOFFLINE");

                        //LogHelper.Write("[SYNC:LOOP] End synchronizing offline files", "UDPProcess._SyncOFFLINE");
                    }
                }
            }
            catch (Exception ex)
            {
                //LogHelper.Write(ex, "UDPProcess._SyncOFFLINE");
            }
            finally
            {
                //LogHelper.Write("[SYNC:END] _SyncOFFLINE thread is stopped", "UDPProcess._SyncOFFLINE");
            }
        }

        static private Thread _thrExtract = null;
        static private Thread _thrTransmit = null;
        static private Thread _thrSync = null;
        #endregion

        #region PUBLIC CONTROLERS
        static public bool IsRunning
        {
            get
            {
                bool ir = !_runningFlag.WaitOne(0, false);
                return ir;
            }
        }

        static public bool Start()
        {
            try
            {
                //LogHelper.Write("[START:BEGIN] Starting threads...", "UDPProcess.Start");
                _controlFlag.WaitOne();

                if (IsRunning)
                    return false;

                _runningFlag.Reset();

                if (_thrExtract == null)
                    _thrExtract = new Thread(new ThreadStart(_Extract));

                if (_thrTransmit == null)
                    _thrTransmit = new Thread(new ThreadStart(_Transmit));

                if (_thrSync == null)
                    _thrSync = new Thread(new ThreadStart(_SyncOFFLINE));

                _thrExtract.Start();
                _thrTransmit.Start();
                _thrSync.Start();
            }
            catch (Exception ex)
            {
                _runningFlag.Set();
                //LogHelper.Write(ex, "UDPProcess.Start");
            }
            finally
            {
                _controlFlag.ReleaseMutex();
                //LogHelper.Write("[START:END] Starting threads...", "UDPProcess.Start");
            }
            return IsRunning;
        }
        static public void Stop()
        {
            try
            {
                //LogHelper.Write("[STOP:BEGIN] Stopping threads...", "UDPProcess.Stop");

                _controlFlag.WaitOne();

                if (IsRunning)
                    _runningFlag.Set();

                if (_udpClient != null)
                {
                    if (_udpClient.Client != null)
                    {
                        _udpClient.Client.Shutdown(SocketShutdown.Both);
                        _udpClient.Client.Close();
                    }
                    _udpClient.Close();
                    _udpClient = null;
                }
            }
            catch (Exception ex)
            {
                _runningFlag.Set();
                //LogHelper.Write(ex, "UDPProcess.Start");
            }
            finally
            {
                _controlFlag.ReleaseMutex();
                //LogHelper.Write("[STOP:END] Stopping threads.", "UDPProcess.Stop");
            }
        }
        #endregion

        #region SynOff
        static TimeSpan startTimeSpan = TimeSpan.Zero;
        static TimeSpan periodTimeSpan = TimeSpan.FromMinutes(15);

        static public Timer timer = new System.Threading.Timer((e) =>
        {
            Integrator.CheckDevice();
        }, null, startTimeSpan, periodTimeSpan);       
        #endregion
    }
}

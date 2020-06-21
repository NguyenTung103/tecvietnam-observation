using knote.utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace bts.udpgateway.integration
{
    static public class LogHelper
    {
        static ILayout _layout = null;
        static public void Register(ILayout layout)
        {
            _layout = layout;
        }

        static public LogLevel GetLogLevel(ReturnCode code)
        {
            LogLevel llv =
                        code == ReturnCode.Success ? LogLevel.INFO :
                        code == ReturnCode.Error ? LogLevel.ERROR :
                        LogLevel.EXCEPTION;
            return llv;
        }
        static public LogLevel GetLogLevel(ReturnInfo rinfo)
        {
            LogLevel llv =
                        rinfo.Code == ReturnCode.Success ? LogLevel.INFO :
                        rinfo.Code == ReturnCode.Error ? LogLevel.ERROR :
                        LogLevel.EXCEPTION;
            return llv;
        }

        static public void Write(string msg, string fnc = null, LogLevel llv = LogLevel.INFO)
        {
            XtraLog.Write(msg, fnc, llv);
            if (_layout != null)
                _layout.dump(string.Format("{0}:> [{1}] :> [{2}] :> {3}", DateTime.Now.ToString("HH:mm:ss.ffffff"), llv, fnc, msg));
        }
        static public void Write(ReturnInfo rinfo, string fnc = null)
        {
            XtraLog.Write(rinfo.Message, GetLogLevel(rinfo), fnc);
            if (_layout != null)
                _layout.dump(string.Format("{0}:> [{1}] :> [{2}] :> {3}", DateTime.Now.ToString("HH:mm:ss.ffffff"), GetLogLevel(rinfo), fnc, rinfo.Message));
        }
        static public void Write(Exception ex, string fnc = null)
        {
            XtraLog.Write(ex.Message, LogLevel.EXCEPTION, fnc);
            if (_layout != null)
                _layout.dump(string.Format("{0}:> [{1}] :> [{2}] :> {3}", DateTime.Now.ToString("HH:mm:ss.ffffff"), LogLevel.EXCEPTION, fnc, ex.Message));
        }
    }
}

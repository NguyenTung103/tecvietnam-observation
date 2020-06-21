using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace bts.udpgateway.integration
{
    static public class IOHelper
    {
        static private readonly object _sync = new object();

        /// <summary>
        /// Write package content to file
        /// </summary>
        /// <param name="content">package content</param>
        /// <returns>true: success, otherwise</returns>
        static public bool WritePKG(string content)
        {
            try
            {
                //LogHelper.Write("[PKG:BEGIN] Writing package to file...", "IOHelper.WritePKG");
                DateTime now = DateTime.Now;
                string daypath = ConfigHelper.PackagePath;
                if (!daypath.EndsWith("\\"))
                    daypath += "\\";
                daypath += now.ToString("yyyyMMdd");
                string fname = now.ToString("yyyyMMdd_HHmmss_fff") + ".txt";
                lock (_sync)
                {
                    if (!Directory.Exists(daypath))
                        Directory.CreateDirectory(daypath);
                    File.WriteAllText(daypath + "\\" + fname, content);
                }
                return true;
            }
            catch (Exception ex)
            {
                //LogHelper.Write(string.Format("[PKG:EXCEPTION]\r\n#Type={0}\r\n#Message={1}\r\n", ex.GetType(), ex.Message), "IOHelper.WritePKG", knote.utils.LogLevel.EXCEPTION);
                return false;
            }
            finally
            {
                //LogHelper.Write("[PKG:END] Writing package to file.", "IOHelper.WritePKG");
            }
        }
        /// <summary>
        /// Write message content to file
        /// </summary>
        /// <param name="content">message content</param>
        /// <param name="file_prefix">file prefix (suggest:EQID), default is NULL</param>
        /// <returns>true: success, otherwise</returns>
        static public bool WriteMSG(string content, string file_prefix = null)
        {
            try
            {
                //LogHelper.Write("[MSG:BEGIN] Writing message to file...", "IOHelper.WriteMSG");
                string dpath = ConfigHelper.MessagePath;
                DateTime now = DateTime.Now;
                string fname = 
                    (string.IsNullOrEmpty(file_prefix) ? "" : (file_prefix + "__")) + 
                    now.ToString("yyyyMMdd_HHmmss_fff") + ".txt";
                lock (_sync)
                {
                    if (!Directory.Exists(dpath))
                        Directory.CreateDirectory(dpath);
                    File.WriteAllText(dpath + fname, content);
                }
                return true;
            }
            catch (Exception ex)
            {
                //LogHelper.Write(string.Format("[MSG:EXCEPTION]\r\n#Type={0}\r\n#Message={1}\r\n", ex.GetType(), ex.Message), "IOHelper.WriteMSG", knote.utils.LogLevel.EXCEPTION);
                return false;
            }
            finally
            {
                //LogHelper.Write("[MSG:END] Writing message to file.", "IOHelper.WriteMSG");
            }
        }
        /// <summary>
        /// Read file content as text
        /// </summary>
        /// <param name="fpath">file path</param>
        /// <returns>file content</returns>
        static public string Read(string fpath)
        {
            try
            {
                //LogHelper.Write("[READ:BEGIN] Reading text from file...", "IOHelper.Read");
                string content = null;
                lock (_sync)
                {
                    content = File.ReadAllText(fpath);
                }
                return content;
            }
            catch (Exception ex)
            {
                //LogHelper.Write(string.Format("[READ:EXCEPTION]\r\n#Type={0}\r\n#Message={1}\r\n", ex.GetType(), ex.Message), "IOHelper.Read", knote.utils.LogLevel.EXCEPTION);
                return null;
            }
            finally
            {
                //LogHelper.Write("[READ:END] Reading text from file.", "IOHelper.Read");
            }
        }
        /// <summary>
        /// Move file from [source] to [target]
        /// <para>Remove file when not specify target</para>
        /// </summary>
        /// <param name="source">source path</param>
        /// <param name="target">target path (default is NULL for removing file)</param>
        /// <returns>true: success, otherwise</returns>
        static public bool Move(string source, string target = null)
        {
            try
            {
                //LogHelper.Write("[MOVE:BEGIN] (RE)Moving file...", "IOHelper.Move");
                lock (_sync)
                {
                    if (string.IsNullOrEmpty(target))
                        File.Delete(source);
                    else
                        File.Move(source, target);
                }
                return true;
            }
            catch (Exception ex)
            {
                //LogHelper.Write(string.Format("[MOVE:EXCEPTION]\r\n#Type={0}\r\n#Message={1}\r\n", ex.GetType(), ex.Message), "IOHelper.Move", knote.utils.LogLevel.EXCEPTION);
                return false;
            }
            finally
            {
                //LogHelper.Write("[MOVE:END] (RE)Moving file.", "IOHelper.Move");
            }
        }
        /// <summary>
        /// Remove directory and sub-content inside (sub-directories, files)
        /// </summary>
        /// <param name="dpath">directory path</param>
        /// <param name="rem_subs">include inside option (default is TRUE)</param>
        /// <returns>true: success, otherwise</returns>
        static public bool Remove(string dpath, bool rem_subs = true)
        {
            try
            {
                //LogHelper.Write("[REM:BEGIN] Removing directory...", "IOHelper.Remove");
                lock (_sync)
                {
                    Directory.Delete(dpath, rem_subs);
                }
                return true;
            }
            catch (Exception ex)
            {
                //LogHelper.Write(string.Format("[REM:EXCEPTION]\r\n#Type={0}\r\n#Message={1}\r\n", ex.GetType(), ex.Message), "IOHelper.Remove", knote.utils.LogLevel.EXCEPTION);
                return false;
            }
            finally
            {
                //LogHelper.Write("[REM:END] Removing directory.", "IOHelper.Remove");
            }
        }
    }
}

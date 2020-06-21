using knote.utils;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using RestSharp;
using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace bts.udpgateway.integration
{
    static public class Integrator
    {
        static public ReturnInfo Insert(string msg_content)
        {
            try
            {
                DateTime date = DateTime.Now.AddDays(-1);
                string path = "D:/bts.udpgateway/logs/" + date.Year + date.Month.ToString("D2") + date.Day.ToString("D2") + ".log";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (msg_content.Contains("ALM"))
                {
                    DataAlarmMongo dataAlarmMongo = InitAlarm(msg_content);
                    InsertAlarmMongo(dataAlarmMongo);
                    RegisterSMSData registerSMSData = new RegisterSMSData();
                    ObservationData observationData = new ObservationData();
                    SiteData siteData = new SiteData();
                    SMSServerData sMSServerData = new SMSServerData();
                    RegisterSMS registerSMS = registerSMSData.GetRegisterSMS(dataAlarmMongo.Device_Id).FirstOrDefault();
                    if (registerSMS != null)
                    {
                        SMSServer sMSServer = sMSServerData.FindByKey(registerSMS.SMSServerId);
                        List<string> listCode = registerSMS.CodeObservation.Split(',').ToList();
                        string content = "", dsSDT = "";
                        Site site = siteData.GetSite(dataAlarmMongo.Device_Id).FirstOrDefault();
                        if (site != null)
                        {
                            content = content + RejectMarks(site.Name) + "," + dataAlarmMongo.DateCreate.ToString("HH:mm:ss");
                            foreach (var item in listCode)
                            {
                                Observation observation = observationData.FindAll().Where(i => i.Code.Trim() == item.Trim()).FirstOrDefault();
                                if (observation != null)
                                {
                                    if (dataAlarmMongo.AMATI.Contains("0-1") || dataAlarmMongo.AMATI.Contains("1-1"))
                                    {
                                        content = content + "#" + observation.Name;
                                    }
                                    else if (dataAlarmMongo.AMAFR.Contains("0-1") || dataAlarmMongo.AMAFR.Contains("1-1"))
                                    {
                                        content = content + "#" + observation.Name;
                                    }
                                }

                            }
                            dsSDT = registerSMS.DanhSachSDT;
                            if (dsSDT != "")
                            {
                                SendSMS(sMSServer.AddressIP.Trim(), sMSServer.Port, dsSDT, content);
                            }
                        }

                    }

                }
                else if (msg_content.Contains("SEQ"))
                {
                    InsertMongo(Init(msg_content));
                }
                else if (msg_content.Contains("RP"))
                {
                    ReportDaily(msg_content);
                }
                return new ReturnInfo(ReturnCode.Success, (string)null, (object)null);
            }
            catch (Exception ex)
            {
                XtraLog.Write($"[GET:RESULT] Insert mongo :{ex.Message}, {ex.StackTrace}", LogLevel.INFO, "Integrator.Insert");
                return new ReturnInfo(ReturnCode.Success, (string)null, (object)null);
            }
        }
        static public ReturnInfo GetPendingMessages()
        {
            try
            {
                //XtraLog.Write("[GET:BEGIN] Get pending message from web-api", LogLevel.INFO, "Integrator.GetPendingMessages");
                RestClient client = new RestClient(ConfigHelper.WapiBaseUrl);
                RestRequest request = new RestRequest(ConfigHelper.WapiGetPending, Method.GET);
                IRestResponse response = client.Execute(request);
                if (response == null)
                    return new ReturnInfo(ReturnCode.Error, string.Format("[GET:RESULT] Cannot get response from web-api ({0})", ConfigHelper.WapiGetPending));

                List<Message> list = new JsonDeserializer().Deserialize<List<Message>>(response);
                if (list == null)
                    list = new List<Message>();
                return new ReturnInfo(data: list);
            }
            catch (Exception ex)
            {
                //XtraLog.Write(string.Format("[GET:EXCEPTION]\r\n{0} : {1}\r\n[TRACE]:\r\n{2}", ex.GetType(), ex.Message, ex.StackTrace), LogLevel.EXCEPTION, "Integrator.GetPendingMessages");
                return new ReturnInfo(data: ex);
            }
            finally
            {
                //XtraLog.Write("[GET:END] Get pending message from web-api", LogLevel.INFO, "Integrator.GetPendingMessages");
            }
        }
        static public ReturnInfo UpdatePendingMessage(int message_id)
        {
            try
            {
                //XtraLog.Write("[UPDATE:BEGIN] Update pending message via web-api", LogLevel.INFO, "Integrator.UpdatePendingMessage");
                RestClient client = new RestClient(ConfigHelper.WapiBaseUrl);
                string request_url = string.Format("{0}/{1}", ConfigHelper.WapiUpdateDone, message_id);
                RestRequest request = new RestRequest(request_url, Method.GET);
                IRestResponse response = client.Execute(request);
                if (response == null)
                    return new ReturnInfo(ReturnCode.Error, string.Format("[UPDATE:RESULT] Cannot get response from web-api ({0})", ConfigHelper.WapiGetPending));

                /*
                 * not know response format of update api
                 */
                //List<Message> list = new JsonDeserializer().Deserialize<List<Message>>(response);
                //if (list == null)
                //    list = new List<Message>();

                return new ReturnInfo(ReturnCode.Success);
            }
            catch (Exception ex)
            {
                //XtraLog.Write(string.Format("[UPDATE:EXCEPTION]\r\n{0} : {1}\r\n[TRACE]:\r\n{2}", ex.GetType(), ex.Message, ex.StackTrace), LogLevel.EXCEPTION, "Integrator.UpdatePendingMessage");
                return new ReturnInfo(data: ex);
            }
            finally
            {
                //XtraLog.Write("[UPDATE:END] Update pending message from web-api", LogLevel.INFO, "Integrator.UpdatePendingMessage");
            }
        }
        #region SQL and Mongo
        private static async void InsertMongo(DataMongo obj)
        {
            var connectionString = "mongodb://tecvietnam2020%40:tecvietnam%4012344321aA@14.177.239.150:27017/?authSource=DataObservation&readPreference=primary";
            var client = new MongoClient(connectionString);
            IMongoDatabase db = client.GetDatabase("DataObservation");
            var collection = db.GetCollection<DataMongo>("Data");
            await collection.InsertOneAsync(obj);
        }

        private static async void InsertAlarmMongo(DataAlarmMongo obj)
        {
            var connectionString = "mongodb://tecvietnam2020%40:tecvietnam%4012344321aA@14.177.239.150:27017/?authSource=DataObservation&readPreference=primary";
            var client = new MongoClient(connectionString);
            IMongoDatabase db = client.GetDatabase("DataObservation");
            var collection = db.GetCollection<DataAlarmMongo>("DataAlarm");
            await collection.InsertOneAsync(obj);
        }

        private static DataMongo Init(string message)
        {
            DataMongo dataMongo = new DataMongo();
            IDictionary<string, string> dictionary1 = (IDictionary<string, string>)new Dictionary<string, string>();
            Dictionary<string, string> dictionary2 = new Dictionary<string, string>()
              {
                {
                  "EQID",
                  "ID thiet bi"
                },
                {
                  "SEQ",
                  "Lưu vào DB"
                },
                {
                  "DATE",
                  "Thời gian"
                },
                {
                  "BTI",
                  "Nhiet do trong phong"
                },
                {
                  "BHU",
                  "Do am moi truong"
                },
                {
                  "BTO",
                  "Nhiet do ben ngoai"
                },
                {
                  "BDR",
                  "Dot nhap"
                },
                {
                  "BFL",
                  "Ngap nuoc "
                },
                {
                  "BFR",
                  "Chay/Khoi"
                },
                {
                  "BPS",
                  "Nguon cap cho sensor"
                },
                {
                  "BAV",
                  "Dien ap AC"
                },
                {
                  "BAP",
                  "Tan so"
                },
                {
                  "BAC",
                  "Dong dien AC"
                },
                {
                  "BAF",
                  "Cosa"
                },
                {
                  "BWS",
                  "Tốc độ gió"
                },
                {
                  "BV1",
                  "Dien ap acquy 1"
                },
                {
                  "BC1",
                  "Dong dien acquy 1"
                },
                {
                  "BT1",
                  "Nhiet do acquy 1"
                },
                {
                  "BV2",
                  "Dien ap acquy 2"
                },
                {
                  "BC2",
                  "Dong dien acquy 2"
                },
                {
                  "BT2",
                  "Nhiet do acquy 2"
                },
                {
                  "BSE",
                  "Dien dang su dung"
                },
                {
                  "BA1",
                  "Dieu hoa 1"
                },
                {
                  "BB1",
                  "Dong dieu hoa 1"
                },
                {
                  "BA2",
                  "Dieu hoa 2"
                },
                {
                  "BB2",
                  "Dong dieu hoa 2"
                },
                {
                  "BA3",
                  "Dieu hoa 3"
                },
                {
                  "BB3",
                  "Dong dieu hoa 3"
                },
                {
                  "BA4",
                  "Dieu hoa 4"
                },
                {
                  "BB4",
                  "Dong dieu hoa 4"
                },
                {
                  "BFA",
                  "Quat AC"
                },
                {
                  "BFD",
                  "Quat DC"
                },
                {
                  "BPW",
                  "Cong suat dien AC"
                }
              };
            string[] strArray = message.Split(';');
            string s1 = strArray[0].Split('=')[1];
            dataMongo.Device_Id = int.Parse(s1);
            string str1 = strArray[1];
            string str2 = strArray[2];
            dataMongo.DateCreate = DateTime.Now;
            dictionary1.Add(new KeyValuePair<string, string>("EQID", s1));
            dictionary1.Add(new KeyValuePair<string, string>("SEQ", "1"));
            dictionary1.Add(new KeyValuePair<string, string>("DATE", str2));
            for (int index = 3; index < strArray.Length - 2; ++index)
            {
                strArray[index] = strArray[index].Trim();
                string str3 = strArray[index].Substring(0, 3);
                string s2 = double.Parse(strArray[index].Substring(3)).ToString();
                switch (str3)
                {
                    case "BA1":
                        dataMongo.BA1 = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BA2":
                        dataMongo.BA2 = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BA3":
                        dataMongo.BA3 = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BA4":
                        dataMongo.BA4 = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BAC":
                        dataMongo.BAC = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BAF":
                        dataMongo.BAF = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BAP":
                        dataMongo.BAP = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BAV":
                        dataMongo.BAV = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BB1":
                        dataMongo.BB1 = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BB2":
                        dataMongo.BB2 = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BB3":
                        dataMongo.BB3 = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BC1":
                        dataMongo.BC1 = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BC2":
                        dataMongo.BC2 = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BDR":
                        dataMongo.BDR = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BFA":
                        dataMongo.BFA = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BFD":
                        dataMongo.BFD = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BFL":
                        dataMongo.BFL = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BFR":
                        dataMongo.BFR = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BHU":
                        dataMongo.BHU = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BPS":
                        dataMongo.BPS = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BPW":
                        dataMongo.BPW = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BSE":
                        dataMongo.BSE = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BT1":
                        dataMongo.BT1 = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BT2":
                        dataMongo.BT2 = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BTI":
                        dataMongo.BTI = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BTO":
                        dataMongo.BTO = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BV1":
                        dataMongo.BV1 = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BV2":
                        dataMongo.BV2 = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                    case "BWS":
                        dataMongo.BWS = s2 == null ? 2.0 : double.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        break;
                }
            }
            return dataMongo;
        }

        private static DataAlarmMongo InitAlarm(string message)
        {
            DataAlarmMongo dataAlarmMongo = new DataAlarmMongo();
            Dictionary<string, string> dictionary1 = new Dictionary<string, string>();
            Dictionary<string, string> dictionary2 = new Dictionary<string, string>()
      {
        {
          "AMATI",
          "Nhiet do phong cao"
        },
        {
          "AMIHU",
          "Do am cao"
        },
        {
          "AMADR",
          "Co dot nhap"
        },
        {
          "AMAFL",
          "Co ngap nuoc"
        },
        {
          "AMAFR",
          "Co chay khoi"
        },
        {
          "AMIPS",
          "Chap nguon sensor"
        },
        {
          "AMIAL",
          "Dien AC thap"
        },
        {
          "AMIAH",
          "Dien AC cao"
        },
        {
          "AMIAP",
          "Tan so khong on dinh"
        },
        {
          "AMIAC",
          "Mat dien AC"
        },
        {
          "AMIGN",
          "Chay may phat"
        },
        {
          "AMIAR",
          "Dieu hoa gap su co"
        },
        {
          "AMIL1",
          "Dien ap acquy 1 thap"
        },
        {
          "AMIH1",
          "Dien ap acquy 1 cao"
        },
        {
          "AMIT1",
          "Nhiet do acquy 1 cao"
        },
        {
          "AMIL2",
          "Dien ap acquy 2 thap"
        },
        {
          "AMIH2",
          "Dien ap acquy 2 cao"
        },
        {
          "AMIT2",
          "Nhiet do acquy 2 cao"
        }
      };
            string[] strArray1 = message.Split(';');
            string s = strArray1[0].Split('=')[1];
            dataAlarmMongo.Device_Id = int.Parse(s);
            dataAlarmMongo.DateCreate = DateTime.Now;
            for (int index1 = 3; index1 < strArray1.Length - 2; ++index1)
            {
                strArray1[index1] = strArray1[index1].Trim();
                int[] numArray = new int[2] { 5, 3 };
                if (strArray1[index1].Length > 6)
                {
                    string[] strArray2 = strArray1[index1].Split('|');
                    for (int index2 = 0; index2 < strArray2.Length; ++index2)
                    {
                        string str1 = strArray2[index2].Substring(0, numArray[index2]);
                        string str2 = strArray2[index2].Substring(numArray[index2]);
                        switch (str1)
                        {
                            case "AMADR":
                                dataAlarmMongo.AMADR = str2;
                                break;
                            case "AMAFL":
                                dataAlarmMongo.AMAFL = str2;
                                break;
                            case "AMAFR":
                                dataAlarmMongo.AMAFR = str2;
                                break;
                            case "AMATI":
                                dataAlarmMongo.AMATI = str2;
                                break;
                            case "AMIAC":
                                dataAlarmMongo.AMIAC = str2;
                                break;
                            case "AMIAH":
                                dataAlarmMongo.AMIAH = str2;
                                break;
                            case "AMIAL":
                                dataAlarmMongo.AMIAL = str2;
                                break;
                            case "AMIAP":
                                dataAlarmMongo.AMIAP = str2;
                                break;
                            case "AMIAR":
                                dataAlarmMongo.AMIAR = str2;
                                break;
                            case "AMIGN":
                                dataAlarmMongo.AMIGN = str2;
                                break;
                            case "AMIH1":
                                dataAlarmMongo.AMIH1 = str2;
                                break;
                            case "AMIH2":
                                dataAlarmMongo.AMIH2 = str2;
                                break;
                            case "AMIHU":
                                dataAlarmMongo.AMIHU = str2;
                                break;
                            case "AMIL1":
                                dataAlarmMongo.AMIL1 = str2;
                                break;
                            case "AMIL2":
                                dataAlarmMongo.AMIL2 = str2;
                                break;
                            case "AMIPS":
                                dataAlarmMongo.AMIPS = str2;
                                break;
                            case "AMIT1":
                                dataAlarmMongo.AMIT1 = str2;
                                break;
                            case "AMIT2":
                                dataAlarmMongo.AMIT2 = str2;
                                break;
                        }
                    }
                }
                else
                {
                    string str1 = strArray1[index1].Substring(0, numArray[0]);
                    string str2 = strArray1[index1].Substring(numArray[0]);
                    switch (str1)
                    {
                        case "AMADR":
                            dataAlarmMongo.AMADR = str2;
                            continue;
                        case "AMAFL":
                            dataAlarmMongo.AMAFL = str2;
                            continue;
                        case "AMAFR":
                            dataAlarmMongo.AMAFR = str2;
                            continue;
                        case "AMATI":
                            dataAlarmMongo.AMATI = str2;
                            continue;
                        case "AMIAC":
                            dataAlarmMongo.AMIAC = str2;
                            continue;
                        case "AMIAH":
                            dataAlarmMongo.AMIAH = str2;
                            continue;
                        case "AMIAL":
                            dataAlarmMongo.AMIAL = str2;
                            continue;
                        case "AMIAP":
                            dataAlarmMongo.AMIAP = str2;
                            continue;
                        case "AMIAR":
                            dataAlarmMongo.AMIAR = str2;
                            continue;
                        case "AMIGN":
                            dataAlarmMongo.AMIGN = str2;
                            continue;
                        case "AMIH1":
                            dataAlarmMongo.AMIH1 = str2;
                            continue;
                        case "AMIH2":
                            dataAlarmMongo.AMIH2 = str2;
                            continue;
                        case "AMIHU":
                            dataAlarmMongo.AMIHU = str2;
                            continue;
                        case "AMIL1":
                            dataAlarmMongo.AMIL1 = str2;
                            continue;
                        case "AMIL2":
                            dataAlarmMongo.AMIL2 = str2;
                            continue;
                        case "AMIPS":
                            dataAlarmMongo.AMIPS = str2;
                            continue;
                        case "AMIT1":
                            dataAlarmMongo.AMIT1 = str2;
                            continue;
                        case "AMIT2":
                            dataAlarmMongo.AMIT2 = str2;
                            continue;
                        default:
                            continue;
                    }
                }
            }
            return dataAlarmMongo;
        }
        private static void SendSMS(string SERVER_IP, int PORT_NO, string sdt, string message)
        {
            //---data to send to the server---
            string textToSend = $"***\r\n{sdt}\r\n{message}\r\n";
            //---create a TCPClient object at the IP and port no.---
            TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
            NetworkStream nwStream = client.GetStream();
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);

            //---send the text---           
            nwStream.Write(bytesToSend, 0, bytesToSend.Length);
            client.Close();
        }
        #endregion
        #region Chuyển đổi chữ có dấu thành không dấu
        static string RejectMarks(string text)
        {
            string[] pattern = new string[7];
            char[] replaceChar = new char[14];

            // Khởi tạo giá trị thay thế

            replaceChar[0] = 'a';
            replaceChar[1] = 'd';
            replaceChar[2] = 'e';
            replaceChar[3] = 'i';
            replaceChar[4] = 'o';
            replaceChar[5] = 'u';
            replaceChar[6] = 'y';
            replaceChar[7] = 'A';
            replaceChar[8] = 'D';
            replaceChar[9] = 'E';
            replaceChar[10] = 'I';
            replaceChar[11] = 'O';
            replaceChar[12] = 'U';
            replaceChar[13] = 'Y';

            //Mẫu cần thay thế tương ứng

            pattern[0] = "(á|à|ả|ã|ạ|ă|ắ|ằ|ẳ|ẵ|ặ|â|ấ|ầ|ẩ|ẫ|ậ)"; //letter a
            pattern[1] = "đ"; //letter d
            pattern[2] = "(é|è|ẻ|ẽ|ẹ|ê|ế|ề|ể|ễ|ệ)"; //letter e
            pattern[3] = "(í|ì|ỉ|ĩ|ị)"; //letter i
            pattern[4] = "(ó|ò|ỏ|õ|ọ|ô|ố|ồ|ổ|ỗ|ộ|ơ|ớ|ờ|ở|ỡ|ợ)"; //letter o
            pattern[5] = "(ú|ù|ủ|ũ|ụ|ư|ứ|ừ|ử|ữ|ự)"; //letter u
            pattern[6] = "(ý|ỳ|ỷ|ỹ|ỵ)"; //letter y

            for (int i = 0; i < pattern.Length; i++)
            {
                MatchCollection matchs = Regex.Matches(text, pattern[i], RegexOptions.IgnoreCase);
                foreach (Match m in matchs)
                {
                    if (i == 0)
                    {
                        text = text.Replace(m.Value[0], 'a');
                    }
                    else if (i == 1)
                    {
                        text = text.Replace(m.Value[0], 'd');
                    }
                    else if (i == 2)
                    {
                        text = text.Replace(m.Value[0], 'e');
                    }
                    else if (i == 3)
                    {
                        text = text.Replace(m.Value[0], 'i');
                    }
                    else if (i == 4)
                    {
                        text = text.Replace(m.Value[0], 'o');
                    }
                    else if (i == 5)
                    {
                        text = text.Replace(m.Value[0], 'u');
                    }
                    else if (i == 6)
                    {
                        text = text.Replace(m.Value[0], 'y');
                    }
                }
            }
            return text;
        }
        #endregion
        static public bool CheckDevice()
        {
            List<Site> lst = new List<Site>();
            SiteData siteData = new SiteData();
            lst = siteData.FindAll().ToList();
            foreach (var item in lst)
            {
                try
                {
                    var start = DateTime.Now.AddMinutes(-5); //2017-04-05 15:21:23.234
                    var end = DateTime.Now;//2017-04-04 15:21:23.234
                    var connectionString = "mongodb://tecvietnam2020%40:tecvietnam%4012344321aA@14.177.239.150:27017/?authSource=DataObservation&readPreference=primary";
                    var client = new MongoClient(connectionString);
                    IMongoDatabase db = client.GetDatabase("DataObservation");
                    var collection = db.GetCollection<DataMongo>("Data");
                    var filterBuilder = Builders<DataMongo>.Filter;
                    var filter = filterBuilder.Where(w => w.Device_Id == item.DeviceId) & filterBuilder.Gte(x => x.DateCreate, start) &
                            filterBuilder.Lte(x => x.DateCreate, end);
                    List<DataMongo> searchResult2 = collection.Find(filter).ToList();
                    if (searchResult2.Count() == 0)
                    {
                        item.IsActive = false;
                        siteData.Update(item);
                    }
                    else
                    {
                        item.IsActive = true;
                        siteData.Update(item);
                    }
                }
                catch (Exception ex)
                {
                    XtraLog.Write($"[GET:RESULT] Update Active Device :{ex.Message}, {ex.StackTrace}", LogLevel.INFO, "Integrator.Insert");
                }

            }

            bool result = false;

            return result;
        }
        #region report
        private static void ReportDaily(string message)
        {
            var obj = message.Split(';');
            var data = obj[3].Split(',');            
            // Lấy deviceId
            int deviceId = Convert.ToInt32(obj[0].Split('=')[1]);
            // Lấy kiểu report
            string typeReport = obj[1];
            //Chuyen doi ngay request
            string date = obj[2];
            string[] gioRq = obj[2].Split('-')[0].Split(':');
            string[] ngayRq = obj[2].Split('-')[1].Split('/');
            //Ngay du lieu
            DateTime dateReport = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]));
            DateTime dateRequestReport = new DateTime(Convert.ToInt32("20" + ngayRq[2]), Convert.ToInt32(ngayRq[1]), Convert.ToInt32(ngayRq[0]), Convert.ToInt32(gioRq[0]), Convert.ToInt32(gioRq[1]), Convert.ToInt32(gioRq[2]));

            //Data
            if (obj[1].Contains("RP0"))
            {
                InsertDailyNhietDo(data, obj, deviceId, typeReport, dateReport, dateRequestReport, message);
            }
            else if (obj[1].Contains("RP1"))
            {
                InsertDailyDoAm(data, obj, deviceId, typeReport, dateReport, dateRequestReport, message);
            }
            else if (obj[1].Contains("RP2"))
            {
                InsertDailyApSuat(data, obj, deviceId, typeReport, dateReport, dateRequestReport, message);
            }
            else if (obj[1].Contains("RP3"))
            {
                InsertDailyLuongMua(data, obj, deviceId, typeReport, dateReport, dateRequestReport, message);
            }
            else if (obj[1].Contains("RP4"))
            {
                InsertDailyHuongGio(data, obj, deviceId, typeReport, dateReport, dateRequestReport, message);
            }
            else if (obj[1].Contains("RP5"))
            {
                InsertDailyMucNuoc(data, obj, deviceId, typeReport, dateReport, dateRequestReport, message);
            }
            else if (obj[1].Contains("RP6"))               
            {
                InsertDailyTocGio(data, obj, deviceId, typeReport, dateReport, dateRequestReport, message);                
            }                  
        }
        /// <summary>
        /// Trả về giá trị double cho dữ liệu đầu vào là chuỗi
        /// </summary>
        /// <param name="input">giá trị string của chuỗi muốn chuyển đổi sang double</param>
        /// <param name="code">mã biểu đồ</param>
        /// <returns></returns>
        private static double CheckValueReport(string input, string code)
        {
            double result = 0;
            if (code.Trim() == "RP0" || code.Trim() == "RP1" || code.Trim() == "RP2" || code.Trim() == "RP6")
            {
                if (input == "*")
                {
                    return result;

                }
                else
                {
                    string value = input.Substring(0,3);
                    result = double.Parse(value+"."+input.Substring(3,4), System.Globalization.CultureInfo.InvariantCulture);                  
                    return result;
                }
            }
            else if (code.Trim() == "RP3" || code.Trim() == "RP4")
            {
                if (input == "*")
                {
                    return result;
                }
                else
                {                    
                    result = double.Parse(input, System.Globalization.CultureInfo.InvariantCulture);                    
                }
                return result;
            }
            else if (code.Trim() == "RP5")
            {
                if (input == "*")
                {
                    return result;
                }
                else
                {
                    string value = input.Substring(0, 1);
                    result = double.Parse(value + "." + input.Substring(1, 4), System.Globalization.CultureInfo.InvariantCulture);
                }

                return result;
            }
            return result;

        }
        /// <summary>
        /// Chèn dữ liệu vào bảng nhiệt độ
        /// </summary>
        /// <param name="data"></param>
        /// <param name="obj"></param>
        /// <param name="deviceId"></param>
        /// <param name="typeCode"></param>
        /// <param name="dateReport"></param>
        /// <param name="dateRequest"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool InsertDailyNhietDo(string[] data, string[] obj, int deviceId, string typeCode, DateTime dateReport, DateTime dateRequest, string message)
        {
            ReportDailyNhietDo report = new ReportDailyNhietDo();
            ReportDailyNhietDoData reportDailyNhietDoData = new ReportDailyNhietDoData();
            report.DeviceId = deviceId;
            report.ReportTypeCode = typeCode;
            report.DateReport = dateReport;
            report.DateRequestReport = dateRequest;
            report.ContentReport = message;
            try
            {
                if (data[27] != "*")
                {
                    report.TimeMaxValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[27]), Convert.ToInt32(data[28]), 00);
                }
                if (data[30] != "*")
                {
                    report.TimeMinValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[30]), Convert.ToInt32(data[31]), 00);
                }
            }
            catch (Exception ex)
            {

            }
            report.MaxValue = CheckValueReport(data[26], obj[1]);
            report.MinValue = CheckValueReport(data[29], obj[1]);
            report.Distance1 = CheckValueReport(data[2], obj[1]);
            report.Distance2 = CheckValueReport(data[3], obj[1]);
            report.Distance3 = CheckValueReport(data[4], obj[1]);
            report.Distance4 = CheckValueReport(data[5], obj[1]);
            report.Distance5 = CheckValueReport(data[6], obj[1]);
            report.Distance6 = CheckValueReport(data[7], obj[1]);
            report.Distance7 = CheckValueReport(data[8], obj[1]);
            report.Distance8 = CheckValueReport(data[9], obj[1]);
            report.Distance9 = CheckValueReport(data[10], obj[1]);
            report.Distance10 = CheckValueReport(data[11], obj[1]);
            report.Distance11 = CheckValueReport(data[12], obj[1]);
            report.Distance12 = CheckValueReport(data[13], obj[1]);
            report.Distance13 = CheckValueReport(data[14], obj[1]);
            report.Distance14 = CheckValueReport(data[15], obj[1]);
            report.Distance15 = CheckValueReport(data[16], obj[1]);
            report.Distance16 = CheckValueReport(data[17], obj[1]);
            report.Distance17 = CheckValueReport(data[18], obj[1]);
            report.Distance18 = CheckValueReport(data[19], obj[1]);
            report.Distance19 = CheckValueReport(data[20], obj[1]);
            report.Distance20 = CheckValueReport(data[21], obj[1]);
            report.Distance21 = CheckValueReport(data[22], obj[1]);
            report.Distance22 = CheckValueReport(data[23], obj[1]);
            report.Distance23 = CheckValueReport(data[24], obj[1]);
            report.Distance24 = CheckValueReport(data[25], obj[1]);
            return reportDailyNhietDoData.Insert(report);
        }
        /// <summary>
        /// Chèn dữ liệu vào bảng Độ ẩm
        /// </summary>
        /// <param name="data"></param>
        /// <param name="obj"></param>
        /// <param name="deviceId"></param>
        /// <param name="typeCode"></param>
        /// <param name="dateReport"></param>
        /// <param name="dateRequest"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool InsertDailyDoAm(string[] data, string[] obj, int deviceId, string typeCode, DateTime dateReport, DateTime dateRequest, string message)
        {
            ReportDailyDoAm report = new ReportDailyDoAm();
            ReportDailyDoAmData reportDailyData = new ReportDailyDoAmData();
            report.DeviceId = deviceId;
            report.ReportTypeCode = typeCode;
            report.DateReport = dateReport;
            report.DateRequestReport = dateRequest;
            report.ContentReport = message;
            try
            {
                if (data[27] != "*")
                {
                    report.TimeMaxValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[27]), Convert.ToInt32(data[28]), 00);
                }
                if (data[30] != "*")
                {
                    report.TimeMinValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[30]), Convert.ToInt32(data[31]), 00);
                }
            }
            catch (Exception ex)
            {

            }
            report.MaxValue = CheckValueReport(data[26], obj[1]);
            report.MinValue = CheckValueReport(data[29], obj[1]);
            report.Distance1 = CheckValueReport(data[2], obj[1]);
            report.Distance2 = CheckValueReport(data[3], obj[1]);
            report.Distance3 = CheckValueReport(data[4], obj[1]);
            report.Distance4 = CheckValueReport(data[5], obj[1]);
            report.Distance5 = CheckValueReport(data[6], obj[1]);
            report.Distance6 = CheckValueReport(data[7], obj[1]);
            report.Distance7 = CheckValueReport(data[8], obj[1]);
            report.Distance8 = CheckValueReport(data[9], obj[1]);
            report.Distance9 = CheckValueReport(data[10], obj[1]);
            report.Distance10 = CheckValueReport(data[11], obj[1]);
            report.Distance11 = CheckValueReport(data[12], obj[1]);
            report.Distance12 = CheckValueReport(data[13], obj[1]);
            report.Distance13 = CheckValueReport(data[14], obj[1]);
            report.Distance14 = CheckValueReport(data[15], obj[1]);
            report.Distance15 = CheckValueReport(data[16], obj[1]);
            report.Distance16 = CheckValueReport(data[17], obj[1]);
            report.Distance17 = CheckValueReport(data[18], obj[1]);
            report.Distance18 = CheckValueReport(data[19], obj[1]);
            report.Distance19 = CheckValueReport(data[20], obj[1]);
            report.Distance20 = CheckValueReport(data[21], obj[1]);
            report.Distance21 = CheckValueReport(data[22], obj[1]);
            report.Distance22 = CheckValueReport(data[23], obj[1]);
            report.Distance23 = CheckValueReport(data[24], obj[1]);
            report.Distance24 = CheckValueReport(data[25], obj[1]);
            return reportDailyData.Insert(report);
        }
        /// <summary>
        /// Chèn dữ liệu vào bảng Áp suất
        /// </summary>
        /// <param name="data"></param>
        /// <param name="obj"></param>
        /// <param name="deviceId"></param>
        /// <param name="typeCode"></param>
        /// <param name="dateReport"></param>
        /// <param name="dateRequest"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool InsertDailyApSuat(string[] data, string[] obj, int deviceId, string typeCode, DateTime dateReport, DateTime dateRequest, string message)
        {
            ReportDailyApSuat report = new ReportDailyApSuat();
            ReportDailyApSuatData reportDailyData = new ReportDailyApSuatData();
            report.DeviceId = deviceId;
            report.ReportTypeCode = typeCode;
            report.DateReport = dateReport;
            report.DateRequestReport = dateRequest;
            report.ContentReport = message;
            try
            {
                if (data[27] != "*")
                {
                    report.TimeMaxValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[27]), Convert.ToInt32(data[28]), 00);
                }
                if (data[30] != "*")
                {
                    report.TimeMinValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[30]), Convert.ToInt32(data[31]), 00);
                }
            }
            catch (Exception ex)
            {

            }
            report.MaxValue = CheckValueReport(data[26], obj[1]);
            report.MinValue = CheckValueReport(data[29], obj[1]);
            report.Distance1 = CheckValueReport(data[2], obj[1]);
            report.Distance2 = CheckValueReport(data[3], obj[1]);
            report.Distance3 = CheckValueReport(data[4], obj[1]);
            report.Distance4 = CheckValueReport(data[5], obj[1]);
            report.Distance5 = CheckValueReport(data[6], obj[1]);
            report.Distance6 = CheckValueReport(data[7], obj[1]);
            report.Distance7 = CheckValueReport(data[8], obj[1]);
            report.Distance8 = CheckValueReport(data[9], obj[1]);
            report.Distance9 = CheckValueReport(data[10], obj[1]);
            report.Distance10 = CheckValueReport(data[11], obj[1]);
            report.Distance11 = CheckValueReport(data[12], obj[1]);
            report.Distance12 = CheckValueReport(data[13], obj[1]);
            report.Distance13 = CheckValueReport(data[14], obj[1]);
            report.Distance14 = CheckValueReport(data[15], obj[1]);
            report.Distance15 = CheckValueReport(data[16], obj[1]);
            report.Distance16 = CheckValueReport(data[17], obj[1]);
            report.Distance17 = CheckValueReport(data[18], obj[1]);
            report.Distance18 = CheckValueReport(data[19], obj[1]);
            report.Distance19 = CheckValueReport(data[20], obj[1]);
            report.Distance20 = CheckValueReport(data[21], obj[1]);
            report.Distance21 = CheckValueReport(data[22], obj[1]);
            report.Distance22 = CheckValueReport(data[23], obj[1]);
            report.Distance23 = CheckValueReport(data[24], obj[1]);
            report.Distance24 = CheckValueReport(data[25], obj[1]);
            return reportDailyData.Insert(report);
        }
        /// <summary>
        /// Chèn dữ liệu vào bảng Lượng Mưa
        /// </summary>
        /// <param name="data"></param>
        /// <param name="obj"></param>
        /// <param name="deviceId"></param>
        /// <param name="typeCode"></param>
        /// <param name="dateReport"></param>
        /// <param name="dateRequest"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool InsertDailyLuongMua(string[] data, string[] obj, int deviceId, string typeCode, DateTime dateReport, DateTime dateRequest, string message)
        {
            ReportDailyLuongMua report = new ReportDailyLuongMua();
            ReportDailyLuongMuaData reportDailyData = new ReportDailyLuongMuaData();
            report.DeviceId = deviceId;
            report.ReportTypeCode = typeCode;
            report.DateReport = dateReport;
            report.DateRequestReport = dateRequest;
            report.ContentReport = message;
            try
            {
                if (data[27] != "*")
                {
                    report.TimeMaxValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[27]), Convert.ToInt32(data[28]), 00);
                }
                if (data[30] != "*")
                {
                    report.TimeMinValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[30]), Convert.ToInt32(data[31]), 00);
                }
            }
            catch (Exception ex)
            {

            }
            report.TongLuongMua = CheckValueReport(data[26], obj[1]);
            report.Distance1 = CheckValueReport(data[2], obj[1]);
            report.Distance2 = CheckValueReport(data[3], obj[1]);
            report.Distance3 = CheckValueReport(data[4], obj[1]);
            report.Distance4 = CheckValueReport(data[5], obj[1]);
            report.Distance5 = CheckValueReport(data[6], obj[1]);
            report.Distance6 = CheckValueReport(data[7], obj[1]);
            report.Distance7 = CheckValueReport(data[8], obj[1]);
            report.Distance8 = CheckValueReport(data[9], obj[1]);
            report.Distance9 = CheckValueReport(data[10], obj[1]);
            report.Distance10 = CheckValueReport(data[11], obj[1]);
            report.Distance11 = CheckValueReport(data[12], obj[1]);
            report.Distance12 = CheckValueReport(data[13], obj[1]);
            report.Distance13 = CheckValueReport(data[14], obj[1]);
            report.Distance14 = CheckValueReport(data[15], obj[1]);
            report.Distance15 = CheckValueReport(data[16], obj[1]);
            report.Distance16 = CheckValueReport(data[17], obj[1]);
            report.Distance17 = CheckValueReport(data[18], obj[1]);
            report.Distance18 = CheckValueReport(data[19], obj[1]);
            report.Distance19 = CheckValueReport(data[20], obj[1]);
            report.Distance20 = CheckValueReport(data[21], obj[1]);
            report.Distance21 = CheckValueReport(data[22], obj[1]);
            report.Distance22 = CheckValueReport(data[23], obj[1]);
            report.Distance23 = CheckValueReport(data[24], obj[1]);
            report.Distance24 = CheckValueReport(data[25], obj[1]);
            return reportDailyData.Insert(report);
        }
        /// <summary>
        /// Chèn dữ liệu vào bảng hướng gió
        /// </summary>
        /// <param name="data"></param>
        /// <param name="obj"></param>
        /// <param name="deviceId"></param>
        /// <param name="typeCode"></param>
        /// <param name="dateReport"></param>
        /// <param name="dateRequest"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool InsertDailyHuongGio(string[] data, string[] obj, int deviceId, string typeCode, DateTime dateReport, DateTime dateRequest, string message)
        {
            ReportDailyHuongGio report = new ReportDailyHuongGio();
            ReportDailyHuongGioData reportDailyData = new ReportDailyHuongGioData();
            report.DeviceId = deviceId;
            report.ReportTypeCode = typeCode;
            report.DateReport = dateReport;
            report.DateRequestReport = dateRequest;
            report.ContentReport = message;
            try
            {
                if (data[27] != "*")
                {
                    report.TimeMaxValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[27]), Convert.ToInt32(data[28]), 00);
                }
                if (data[30] != "*")
                {
                    report.TimeMinValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[30]), Convert.ToInt32(data[31]), 00);
                }
            }
            catch (Exception ex)
            {

            }
            report.HuongGio1 = int.Parse(data[2].Substring(0, 2));
            report.HuongGio2 = int.Parse(data[3].Substring(0, 2));
            report.HuongGio3 = int.Parse(data[4].Substring(0, 2));
            report.HuongGio4 = int.Parse(data[5].Substring(0, 2));
            report.HuongGio5 = int.Parse(data[6].Substring(0, 2));
            report.HuongGio6 = int.Parse(data[7].Substring(0, 2));
            report.HuongGio7 = int.Parse(data[8].Substring(0, 2));
            report.HuongGio8 = int.Parse(data[9].Substring(0, 2));
            report.Distance1 = CheckValueReport(data[2], obj[1]);
            report.Distance2 = CheckValueReport(data[3], obj[1]);
            report.Distance3 = CheckValueReport(data[4], obj[1]);
            report.Distance4 = CheckValueReport(data[5], obj[1]);
            report.Distance5 = CheckValueReport(data[6], obj[1]);
            report.Distance6 = CheckValueReport(data[7], obj[1]);
            report.Distance7 = CheckValueReport(data[8], obj[1]);
            report.Distance8 = CheckValueReport(data[9], obj[1]);
            report.Distance9 = CheckValueReport(data[10], obj[1]);
            report.Distance10 = CheckValueReport(data[11], obj[1]);
            report.Distance11 = CheckValueReport(data[12], obj[1]);
            report.Distance12 = CheckValueReport(data[13], obj[1]);
            report.Distance13 = CheckValueReport(data[14], obj[1]);
            report.Distance14 = CheckValueReport(data[15], obj[1]);
            report.Distance15 = CheckValueReport(data[16], obj[1]);
            report.Distance16 = CheckValueReport(data[17], obj[1]);
            report.Distance17 = CheckValueReport(data[18], obj[1]);
            report.Distance18 = CheckValueReport(data[19], obj[1]);
            report.Distance19 = CheckValueReport(data[20], obj[1]);
            report.Distance20 = CheckValueReport(data[21], obj[1]);
            report.Distance21 = CheckValueReport(data[22], obj[1]);
            report.Distance22 = CheckValueReport(data[23], obj[1]);
            report.Distance23 = CheckValueReport(data[24], obj[1]);
            report.Distance24 = CheckValueReport(data[25], obj[1]);
            report.HuongGioDacTrungNhieuNhat = CheckValueReport(data[26], obj[1]);
            report.HuongGioDacTrungNhieuThuHai = CheckValueReport(data[29], obj[1]);
            return reportDailyData.Insert(report);
        }
        /// <summary>
        /// Chèn dữ liệu vào bảng mực nước
        /// </summary>
        /// <param name="data"></param>
        /// <param name="obj"></param>
        /// <param name="deviceId"></param>
        /// <param name="typeCode"></param>
        /// <param name="dateReport"></param>
        /// <param name="dateRequest"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool InsertDailyMucNuoc(string[] data, string[] obj, int deviceId, string typeCode, DateTime dateReport, DateTime dateRequest, string message)
        {
            ReportDailyMucNuoc report = new ReportDailyMucNuoc();
            ReportDailyMucNuocData reportDailyData = new ReportDailyMucNuocData();
            report.DeviceId = deviceId;
            report.ReportTypeCode = typeCode;
            report.DateReport = dateReport;
            report.DateRequestReport = dateRequest;
            report.ContentReport = message;
            try
            {
                if (data[27] != "*")
                {
                    report.TimeMaxValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[27]), Convert.ToInt32(data[28]), 00);
                }
                if (data[30] != "*")
                {
                    report.TimeMinValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[30]), Convert.ToInt32(data[31]), 00);
                }
            }
            catch (Exception ex)
            {

            }                   
            report.Distance1 = CheckValueReport(data[2], obj[1]);
            report.Distance2 = CheckValueReport(data[3], obj[1]);
            report.Distance3 = CheckValueReport(data[4], obj[1]);
            report.Distance4 = CheckValueReport(data[5], obj[1]);
            report.Distance5 = CheckValueReport(data[6], obj[1]);
            report.Distance6 = CheckValueReport(data[7], obj[1]);
            report.Distance7 = CheckValueReport(data[8], obj[1]);
            report.Distance8 = CheckValueReport(data[9], obj[1]);
            report.Distance9 = CheckValueReport(data[10], obj[1]);
            report.Distance10 = CheckValueReport(data[11], obj[1]);
            report.Distance11 = CheckValueReport(data[12], obj[1]);
            report.Distance12 = CheckValueReport(data[13], obj[1]);
            report.Distance13 = CheckValueReport(data[14], obj[1]);
            report.Distance14 = CheckValueReport(data[15], obj[1]);
            report.Distance15 = CheckValueReport(data[16], obj[1]);
            report.Distance16 = CheckValueReport(data[17], obj[1]);
            report.Distance17 = CheckValueReport(data[18], obj[1]);
            report.Distance18 = CheckValueReport(data[19], obj[1]);
            report.Distance19 = CheckValueReport(data[20], obj[1]);
            report.Distance20 = CheckValueReport(data[21], obj[1]);
            report.Distance21 = CheckValueReport(data[22], obj[1]);
            report.Distance22 = CheckValueReport(data[23], obj[1]);
            report.Distance23 = CheckValueReport(data[24], obj[1]);
            report.Distance24 = CheckValueReport(data[25], obj[1]);
            return reportDailyData.Insert(report);
        }
        /// <summary>
        /// Chèn dữ liệu vào bảng tốc độ gió
        /// </summary>
        /// <param name="data"></param>
        /// <param name="obj"></param>
        /// <param name="deviceId"></param>
        /// <param name="typeCode"></param>
        /// <param name="dateReport"></param>
        /// <param name="dateRequest"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool InsertDailyTocGio(string[] data, string[] obj, int deviceId, string typeCode, DateTime dateReport, DateTime dateRequest, string message)
        {
            ReportDailyTocDoGio report = new ReportDailyTocDoGio();
            ReportDailyTocDoGioData reportDailyData = new ReportDailyTocDoGioData();
            report.DeviceId = deviceId;
            report.ReportTypeCode = typeCode;
            report.DateReport = dateReport;
            report.DateRequestReport = dateRequest;
            report.ContentReport = message;
            try
            {
                if (data[27] != "*")
                {
                    report.TimeMaxValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[28]), Convert.ToInt32(data[29]), 00);
                }
                if (data[30] != "*")
                {
                    report.TimeMinValue = new DateTime(DateTime.Now.Year, Convert.ToInt32(data[1]), Convert.ToInt32(data[0]), Convert.ToInt32(data[32]), Convert.ToInt32(data[33]), 00);
                }
            }
            catch (Exception ex)
            {

            }
            report.TocDoGioLonNhat = CheckValueReport(data[26], obj[1]);
            report.HuongGioCuaTocDoLonNhat = CheckValueReport(data[27], obj[1]);
            report.TocDoGioNhoNhat = CheckValueReport(data[30], obj[1]);
            report.HuongGioCuarTocDoNhoNhat = CheckValueReport(data[31], obj[1]);
            report.Distance1 = CheckValueReport(data[2], obj[1]);
            report.Distance2 = CheckValueReport(data[3], obj[1]);
            report.Distance3 = CheckValueReport(data[4], obj[1]);
            report.Distance4 = CheckValueReport(data[5], obj[1]);
            report.Distance5 = CheckValueReport(data[6], obj[1]);
            report.Distance6 = CheckValueReport(data[7], obj[1]);
            report.Distance7 = CheckValueReport(data[8], obj[1]);
            report.Distance8 = CheckValueReport(data[9], obj[1]);
            report.Distance9 = CheckValueReport(data[10], obj[1]);
            report.Distance10 = CheckValueReport(data[11], obj[1]);
            report.Distance11 = CheckValueReport(data[12], obj[1]);
            report.Distance12 = CheckValueReport(data[13], obj[1]);
            report.Distance13 = CheckValueReport(data[14], obj[1]);
            report.Distance14 = CheckValueReport(data[15], obj[1]);
            report.Distance15 = CheckValueReport(data[16], obj[1]);
            report.Distance16 = CheckValueReport(data[17], obj[1]);
            report.Distance17 = CheckValueReport(data[18], obj[1]);
            report.Distance18 = CheckValueReport(data[19], obj[1]);
            report.Distance19 = CheckValueReport(data[20], obj[1]);
            report.Distance20 = CheckValueReport(data[21], obj[1]);
            report.Distance21 = CheckValueReport(data[22], obj[1]);
            report.Distance22 = CheckValueReport(data[23], obj[1]);
            report.Distance23 = CheckValueReport(data[24], obj[1]);
            report.Distance24 = CheckValueReport(data[25], obj[1]);
            return reportDailyData.Insert(report);
        }
        #endregion

    }


}

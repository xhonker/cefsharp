using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using newCRM.Tools;
using RestSharp;
namespace 上海CRM管理系统.Tools
{
    public class httpHellper
    {
        /// <summary>
        /// GET请求
        /// </summary>
        /// <param name="url"></param>
        public static string GET(string url)
        {
            string retString = "";
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    System.GC.Collect();

                    System.Net.ServicePointManager.DefaultConnectionLimit = 50;

                    string urlcontext = "";

                    if (url.Contains("?"))
                    {
                        urlcontext = url + "&guid=" + Guid.NewGuid();
                    }
                    else
                    {
                        urlcontext = url + "?guid=" + Guid.NewGuid();
                    }

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(urlcontext, UriKind.Absolute));
                    request.AllowAutoRedirect = false;
                    request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                    request.KeepAlive = false;
                    request.ProtocolVersion = HttpVersion.Version10;
                    request.Accept = "text/html, application/xhtml+xml, */*";
                    request.Method = "GET";

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream myResponseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                    string tempstr = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    myResponseStream.Close();
                    request.Abort();
                    //retString = DesHelper.DecryptDES(tempstr);//解密
                    //retString = tempstr;
                }
                catch (Exception ex)
                {

                }
            }

            return retString;
        }

        public static string Get(string url)
        {
            string retString = "";
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    System.GC.Collect();

                    string urlcontext = "";

                    if (url.Contains("?"))
                    {
                        urlcontext = url + "&guid=" + Guid.NewGuid();
                    }
                    else
                    {
                        urlcontext = url + "?guid=" + Guid.NewGuid();
                    }

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(urlcontext, UriKind.Absolute));
                    request.AllowAutoRedirect = false;
                    request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                    request.KeepAlive = false;
                    request.ProtocolVersion = HttpVersion.Version10;
                    request.Accept = "text/html, application/xhtml+xml, */*";
                    request.Method = "GET";

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream myResponseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                    string tempstr = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    myResponseStream.Close();
                    retString = tempstr;
                }
                catch (Exception ex)
                {

                }
            }

            return retString;
        }


        public static string get(string url, string token)
        {
            string retString = "";
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    System.GC.Collect();

                    string urlcontext = "";

                    if (url.Contains("?"))
                    {
                        urlcontext = url + "&guid=" + Guid.NewGuid();
                    }
                    else
                    {
                        urlcontext = url + "?guid=" + Guid.NewGuid();
                    }

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(urlcontext, UriKind.Absolute));
                    request.AllowAutoRedirect = false;
                    request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                    request.KeepAlive = false;
                    request.ProtocolVersion = HttpVersion.Version10;
                    request.Accept = "text/html, application/xhtml+xml, */*";
                    request.Method = "GET";


                    request.Headers["TOKEN"] = token;

                    request.Headers["PL"] = "KF";

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream myResponseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                    string tempstr = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    myResponseStream.Close();
                    retString = tempstr;
                }
                catch (Exception ex)
                {

                }
            }

            return retString;
        }
        /// <summary>
        /// 上传文件 含参数
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string PostRequest(string filePath)
        {
            var errCount = 0;
            //分割大小
            var byteCount = 4 * 1024 * 1024;
            //提交数据
            byte[] data;
            int merge = 0;
            long chunkSize = byteCount;
            //当前块
            int cruuent = 1;
            long totalByte = 0;
            var callId = Path.GetFileNameWithoutExtension(filePath);
            var fileName = Path.GetFileName(filePath);
            // 总分片数
            double totalChunk;

            ConstDefault.chunkFile upFile;
            using (FileStream fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader bReader = new BinaryReader(fStream))
                {
                    // 文件大小
                    long totalSize = fStream.Length;
                    totalChunk = Math.Round((double)totalSize / (double)byteCount) + 1;
                    if (totalChunk == 1)
                    {
                        data = new byte[totalSize];
                        bReader.Read(data, 0, (int)totalSize);
                        upFile = new ConstDefault.chunkFile();
                        upFile.call_id = callId;
                        upFile.fileName = fileName;
                        upFile.totalSize = totalSize;
                        upFile.totalChunk = 1;
                        upFile.chunkSize = chunkSize;
                        upFile.index = 1;
                        upFile.data = data;
                        var upDataFileResult = upDataFile(upFile);
                        var result = JsonHelper.JsonDeserialize<ConstDefault.result>(upDataFileResult);

                        if (result.code == 200)
                        {
                            upFile = new ConstDefault.chunkFile();
                            upFile.call_id = callId;
                            upFile.fileName = fileName;
                            upFile.totalSize = totalSize;
                            upFile.totalChunk = 1;
                            upFile.chunkSize = 0;
                            upFile.index = 2;
                            upFile.merge = 1;
                            var upDataFileResult1 = upDataFile(upFile);
                            var result1 = JsonHelper.JsonDeserialize<ConstDefault.result>(upDataFileResult1);
                            if (result1.code == 1)
                            {
                                return upDataFileResult1;
                            }
                        }
                        else if (result.code == 400)
                        {
                            upDataFile(upFile);
                            errCount++;
                            if (errCount > 3)
                            {
                                return "";
                            }
                        }
                        else if (result.code == 500)
                        {
                            return "";
                        }
                    }
                    else
                    {
                        for (; cruuent <= totalChunk; cruuent++)
                        {
                            upFile = new ConstDefault.chunkFile();
                            if (cruuent == totalChunk)
                            {
                                data = null;
                                merge = 1;
                            }
                            else
                            {
                                if (totalByte + byteCount > totalSize)
                                {
                                    var size = Convert.ToInt64((totalSize - totalByte));
                                    data = new byte[size];
                                    bReader.Read(data, 0, Convert.ToInt32((totalSize - totalByte)));
                                }
                                else
                                {
                                    totalByte += byteCount;
                                    data = new byte[byteCount];
                                    bReader.Read(data, 0, byteCount);
                                }
                            }

                            upFile.call_id = callId;
                            upFile.fileName = fileName;
                            upFile.totalSize = totalSize;
                            upFile.totalChunk = (int)totalChunk - 1;
                            upFile.chunkSize = chunkSize;
                            upFile.index = cruuent;
                            upFile.data = data;
                            upFile.merge = merge;

                            var upDataFileResult = upDataFile(upFile);
                            var result = JsonHelper.JsonDeserialize<ConstDefault.result>(upDataFileResult);

                            if (result.code == 1)
                            {
                                return upDataFileResult;
                            }
                            else if (result.code == 400)
                            {
                                upDataFile(upFile);
                                errCount++;
                                if (errCount > 3)
                                {
                                    return "";
                                }
                            }
                            else if (result.code == 500)
                            {
                                return "";
                            }
                        }
                    }
                }
            }

            return "";
            #region 加入Restsharp库
            //var client = new RestClient("http://crm-test.lanyife.com.cn");
            //var requst = new RestRequest("call/post/save-file", Method.POST);
            //requst.AddParameter("call_id", callId);
            //requst.AddFile("LyCall[file]", filePath);

            //IRestResponse response = client.Execute(requst);
            //var t = response.StatusCode;
            //return response.Content;
            #endregion

            #region 原生拼接
            //byte[] fileContentByte = new byte[1024];
            //FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            //fileContentByte = new byte[fs.Length];
            //fs.Read(fileContentByte, 0, Convert.ToInt32(fs.Length));
            //fs.Close();


            //string boundary = "----WebKitFormBoundary7MA4YWxkTrZu0gW";
            //string Enter = "\r\n";
            //string call_id = "--" + boundary + Enter
            //    + "Content-Disposition: form-data; name=\"call_id\"" + Enter + Enter
            //    + callId + Enter;

            //string fileContent = "--" + boundary + Enter
            //                         + "Content-Type:application/octet-stream" + Enter
            //                         + "Content-Disposition: form-data; name=\"LyCall[file]\";filename=\"" + filename + "\""
            //                         + Enter + Enter + Enter;


            //string end = Enter + "--" + boundary + "--";

            //var call_id_StrByte = Encoding.UTF8.GetBytes(call_id);

            //var fileContentStrByte = Encoding.UTF8.GetBytes(fileContent);

            //var endStrByte = Encoding.UTF8.GetBytes(end);


            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.Method = "POST";
            //request.ContentType = "multipart/form-data;boundary=" + boundary;
            //request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.108 Safari/537.36";
            //Stream requestStream = request.GetRequestStream();
            //requestStream.Write(call_id_StrByte, 0, call_id_StrByte.Length);
            //requestStream.Write(fileContentStrByte, 0, fileContentStrByte.Length);
            //requestStream.Write(fileContentByte, 0, fileContentByte.Length);
            //requestStream.Write(endStrByte, 0, end.Length);


            //using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //{
            //    Stream responseStream = response.GetResponseStream();
            //    StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
            //    result = streamReader.ReadToEnd();
            //}
            //return result;
            #endregion

        }

        public static string upDataFile(ConstDefault.chunkFile chunk)
        {
            var client = new RestClient(ConstDefault.upFileUrl);
            var requst = new RestRequest("call/post/chunk-upload", Method.POST);

            requst.AddParameter("fileName", chunk.fileName);
            requst.AddParameter("totalSize", chunk.totalSize);
            requst.AddParameter("totalChunk", chunk.totalChunk);
            requst.AddParameter("chunkSize", chunk.chunkSize);
            requst.AddParameter("index", chunk.index);
            requst.AddParameter("call_id", chunk.call_id);

            if (chunk.data != null)
            {
                requst.AddFile("data", chunk.data, chunk.fileName);
            }

            if (chunk.merge != 0)
            {
                requst.AddParameter("merge", chunk.merge);
            }
            IRestResponse response = client.Execute(requst);
            System.Diagnostics.Debug.WriteLine(response.Content);

            return response.Content;
        }

        public static string isSuccess(string data, ConstDefault.chunkFile upfile)
        {
            var errCount = 0; // 错误重试
            var result = JsonHelper.JsonDeserialize<ConstDefault.result>(data);
            if (result.code == 1)
            {
                return "上传成功";
            }
            else if (result.code == 400)
            {
                upDataFile(upfile);
                errCount++;
                if (errCount > 3)
                {
                    return "重试失败";
                }
            }
            else if (result.code == 500)
            {
                return "上传失败";
            }
            return "";
        }
        public static string POST(string url, string strContent, ref string token)
        {
            string strValue = "";

            try
            {
                System.GC.Collect();

                //创建一个HTTP请求  
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                //Post请求方式  
                request.Method = "POST";

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers["TOKEN"] = token;
                }
                request.Headers["PL"] = "KF";

                //内容类型
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] payload;
                //将Json字符串转化为字节  
                payload = System.Text.Encoding.UTF8.GetBytes(strContent);
                //设置请求的ContentLength   
                request.ContentLength = payload.Length;
                //发送请求，获得请求流  

                Stream writer = request.GetRequestStream();//获取用于写入请求数据的Stream对象

                //将请求参数写入流
                writer.Write(payload, 0, payload.Length);
                writer.Close();//关闭请求流

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream s = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(s, Encoding.GetEncoding("utf-8"));
                strValue = myStreamReader.ReadToEnd();


                if (string.IsNullOrEmpty(token))
                {
                    WebHeaderCollection head = response.Headers;

                    string[] tokenarray = null;
                    for (int i = 0; head.GetEnumerator().MoveNext(); i++)
                    {
                        string key = head.GetKey(i);
                        if (key == "Token")
                        {
                            tokenarray = head.GetValues(i);
                            token = tokenarray[0];
                            break;
                        }
                    }
                }

                myStreamReader.Close();
                s.Close();
                request.Abort();
            }
            catch (Exception exl)
            {

            }
            return strValue;
        }



        /*public string POST(string url, string data)
        {
            string retString = "";

            try
            {
                System.GC.Collect();

                string urlcontext = "";

                if (url.Contains("?"))
                {
                    urlcontext = url + "&guid=" + Guid.NewGuid();
                }
                else
                {
                    urlcontext = url + "?guid=" + Guid.NewGuid();
                }


                byte[] httpData = Encoding.UTF8.GetBytes(data);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlcontext);

                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";

                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Timeout = 20000;
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                request.Method = "POST";
                request.AllowAutoRedirect = false;

                Stream myRequestStream = request.GetRequestStream();
                myRequestStream.Write(httpData, 0, httpData.Length);
                myRequestStream.Close();


                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string tempstr = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                retString = tempstr;

            }
            catch (Exception exl)
            {

            }
            return retString;
        }*/




        public static Boolean DownFile(string downDirectoryPath, string url, ref string message)
        {
            bool ret = false;
            if (!System.IO.Directory.Exists(downDirectoryPath))
            {
                System.IO.Directory.CreateDirectory(downDirectoryPath);
            }
            string fileName = url.Substring(url.LastIndexOf("/") + 1);
            string filePath = downDirectoryPath + @"/" + fileName;
            try
            {
                WebRequest req = WebRequest.Create(url);
                WebResponse res = req.GetResponse();
                if (res.ContentLength > 0)
                {
                    WebClient wClient = new WebClient();
                    wClient.DownloadFile(url, filePath);
                }
                ret = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return ret;
        }


        public static Boolean DownBigFile(string downDirectoryPath, string url, ref string message)
        {
            if (!System.IO.Directory.Exists(downDirectoryPath))
            {
                System.IO.Directory.CreateDirectory(downDirectoryPath);
            }

            string fileName = url.Substring(url.LastIndexOf("\\") + 1);
            string filePath = downDirectoryPath + @"/" + fileName;

            int buffSize = 1024 * 2; //2K的下载

            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);

                WebRequest webReq = WebRequest.Create(url);
                WebResponse webRes = webReq.GetResponse();

                Stream srm = webRes.GetResponseStream();
                while (true)
                {
                    byte[] buffer = new byte[buffSize];
                    int downByte = srm.Read(buffer, 0, buffSize);
                    if (downByte <= 0)
                    {
                        break;
                    };
                    fs.Write(buffer, 0, downByte);
                }
                srm.Close();
                fs.Close();
            }
            catch (WebException ex)
            {
                message = ex.Message;
                return false;
            }
            message = "";
            return true;
        }

    }

}

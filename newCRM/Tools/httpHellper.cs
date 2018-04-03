using System;
using System.Configuration;
using System.IO;
using System.Net;
using RestSharp;
using Newtonsoft.Json;
using newCRM.Tools;

namespace 上海CRM管理系统.Tools
{
    /// <summary>
    /// HTTP请求
    /// </summary>
    public class httpHellper
    {
        /// <summary>
        /// 分块上传文件
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
            FileInfo fi = new FileInfo(filePath);
            var call_start = fi.CreationTime.ToString();
            var call_end = fi.LastWriteTime.ToString();
            ConstDefault.chunkFile upFile;
            using (FileStream fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader bReader = new BinaryReader(fStream))
                {
                    // 文件大小
                    long totalSize = fStream.Length;

                    totalChunk = Math.Ceiling((double)totalSize / (double)byteCount) + 1;
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
                        upFile.call_start = call_start;
                        upFile.call_end = call_end;
                        var upDataFileResult = upDataFile(upFile);
                        if (upDataFileResult == "")
                        {
                            VoipHelper.WriteLog(string.Format("上传失败==>> {0}", upDataFileResult));
                            return "";
                        }
                        var result = JsonConvert.DeserializeObject<ConstDefault.result>(upDataFileResult);
                        if (result.code == 200)
                        {
                            upFile = new ConstDefault.chunkFile();
                            upFile.call_id = callId;
                            upFile.fileName = fileName;
                            upFile.totalSize = totalSize;
                            upFile.call_start = call_start;
                            upFile.call_end = call_end;
                            upFile.totalChunk = 1;
                            upFile.chunkSize = 0;
                            upFile.index = 2;
                            upFile.merge = 1;
                            var upDataFileResult1 = upDataFile(upFile);
                            var resultMerge = JsonConvert.DeserializeObject<ConstDefault.result>(upDataFileResult1);
                            if (resultMerge.code == 1)
                            {
                                return upDataFileResult1;
                            }
                        }
                        else if (result.code == 400)
                        {
                            VoipHelper.WriteLog(string.Format("上传失败==>> {0}", upDataFileResult));
                            upDataFile(upFile);
                            errCount++;
                            if (errCount > 3)
                            {
                                return "";
                            }
                        }
                        else if (result.code == 500)
                        {
                            VoipHelper.WriteLog(string.Format("上传失败==>> {0}", upDataFileResult));
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
                            upFile.call_start = call_start;
                            upFile.call_end = call_end;

                            var upDataFileResult = upDataFile(upFile);
                            if (!string.IsNullOrEmpty(upDataFileResult))
                            {
                                var result = JsonConvert.DeserializeObject<ConstDefault.result>(upDataFileResult);

                                if (result.code == 1)
                                {
                                    return upDataFileResult;
                                }
                                else if (result.code == 400)
                                {
                                    VoipHelper.WriteLog(string.Format("上传失败==>> {0}", upDataFileResult));
                                    upDataFile(upFile);
                                    errCount++;
                                    if (errCount > 3)
                                    {
                                        return "";
                                    }
                                }
                                else if (result.code == 500)
                                {
                                    VoipHelper.WriteLog(string.Format("上传失败==>> {0}", upDataFileResult));
                                    return "";
                                }
                            }
                            else
                            {
                                VoipHelper.WriteLog(string.Format("上传失败==>> {0}", upDataFileResult));
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
        /// <summary>
        /// 上传单个块
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public static string upDataFile(ConstDefault.chunkFile chunk)
        {
            try
            {
                string serverAddress = ConfigurationManager.AppSettings["server"];
                var client = new RestClient(serverAddress);
                var requst = new RestRequest("call/post/chunk-upload", Method.POST);

                requst.AddParameter("fileName", chunk.fileName);
                requst.AddParameter("totalSize", chunk.totalSize);
                requst.AddParameter("totalChunk", chunk.totalChunk);
                requst.AddParameter("chunkSize", chunk.chunkSize);
                requst.AddParameter("index", chunk.index);
                requst.AddParameter("call_id", chunk.call_id);
                requst.AddParameter("call_start", chunk.call_start);
                requst.AddParameter("call_end", chunk.call_end);

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
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    VoipHelper.WriteLog(string.Format("上传出错 ==>> {0}", response.Content.ToString()));
                    return "";
                }

                return response.Content;
            }
            catch (Exception err)
            {
                VoipHelper.WriteLog(string.Format("上传出错 ==>> {0}", err));
                return "";
            }

        }
        /// <summary>
        /// 上传日志
        /// </summary>
        /// <param name="username">员工工号</param>
        /// <param name="file">文件路径</param>
        /// <returns></returns>
        public static string uploadLog(string username, string file)
        {
            try
            {
                string serverAddress = ConfigurationManager.AppSettings["server"];
                var client = new RestClient(serverAddress);
                var requst = new RestRequest("/call/post/save-file", Method.POST);
                requst.AddParameter("username", username);
                requst.AddFile("LyCall[file]", file);

                IRestResponse response = client.Execute(requst);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    VoipHelper.WriteLog(string.Format("日志上传成功"));
                    return response.Content;
                }
                else
                {
                    VoipHelper.WriteLog(string.Format("日志上传失败 ==>> {0} username ==>> {1} filePath ==>> {2}", response.Content, username, file));
                    return "";
                }
            }
            catch (Exception err)
            {
                VoipHelper.WriteLog(string.Format("日志上传失败==>> {0}", err));
                return err.ToString();
            }
        }
    }

}

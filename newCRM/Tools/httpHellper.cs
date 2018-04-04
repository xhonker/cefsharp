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
                            if (result.code == 200)
                            {

                            }
                            else if (result.code == 1)
                            {
                                return upDataFileResult;
                            }
                            else
                            {
                                return upDataFileResult;
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
            return "";
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

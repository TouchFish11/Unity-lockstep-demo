using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Utility;

namespace Editor.AssetBundle.Core
{
    /// <summary>
    /// 负责上传 AB 包到服务器
    /// </summary>
    public class AssetBundleUploader
    {
        private readonly Action<string> logAction;
        private int totalUploadCount;
        private int finishedUploadCount;

        public AssetBundleUploader(Action<string> logAction = null)
        {
            this.logAction = logAction;
        }

        private void Log(string msg) => logAction?.Invoke(msg);

        public void Upload(string serverDataPath, string serverUrl, bool useAuth, string user, string pwd,
                           bool autoChunkSize, uint customChunkSize)
        {
            var dir = new DirectoryInfo(serverDataPath);
            var files = new List<FileInfo>();
            foreach (var file in dir.GetFiles())
            {
                if (file.Extension == FileUtility.AbSuffix || file.Extension == ".json")
                    files.Add(file);
            }

            totalUploadCount = files.Count;
            finishedUploadCount = 0;

            Log($"--- Starting Upload Data ({totalUploadCount} files) ---");
            foreach (var file in files)
            {
                UploadFileAsync(file.FullName, file.Name, serverUrl, useAuth, user, pwd, autoChunkSize, customChunkSize);
            }
        }

        private async void UploadFileAsync(string filePath, string fileName, string serverUrl,
                                           bool useAuth, string user, string pwd,
                                           bool autoChunkSize, uint customChunkSize)
        {
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var req = WebRequest.Create(new Uri(serverUrl)) as HttpWebRequest;
                        if (req == null) return;
                        req.Method = WebRequestMethods.Http.Post;
                        req.ContentType = "multipart/form-data;boundary=MrQiu";
                        req.Timeout = 500000;

                        if (useAuth)
                            req.Credentials = new NetworkCredential(user, pwd);
                        req.PreAuthenticate = true;

                        var head = "--MrQiu\r\n" +
                                   $"Content-Disposition:form-data;name=\"file\";filename=\"{fileName}\"\r\n" +
                                   "Content-Type:application/octet-stream\r\n\r\n";
                        var headBytes = Encoding.UTF8.GetBytes(head);
                        var endBytes = Encoding.UTF8.GetBytes("\r\n--MrQiu--\r\n");

                        using (var fileStream = File.OpenRead(filePath))
                        {
                            req.ContentLength = headBytes.Length + fileStream.Length + endBytes.Length;

                            long chunkSize;
                            if (autoChunkSize)
                            {
                                chunkSize = req.ContentLength switch
                                {
                                    >= 1024 * 1024 * 100 => 1024 * 1024,
                                    >= 1024 * 1024 * 50 => 65536,
                                    > 1024 * 1024 => 4096,
                                    _ => req.ContentLength
                                };
                            }
                            else
                            {
                                chunkSize = customChunkSize;
                            }

                            using var upStream = req.GetRequestStream();
                            upStream.Write(headBytes, 0, headBytes.Length);
                            var buffer = new byte[chunkSize];
                            int read;
                            while ((read = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                                upStream.Write(buffer, 0, read);
                            upStream.Write(endBytes, 0, endBytes.Length);
                        }

                        var res = req.GetResponse() as HttpWebResponse;
                        if (res != null && res.StatusCode == HttpStatusCode.OK)
                        {
                            Log($"{fileName}：Upload Success，Progress：{Interlocked.Increment(ref finishedUploadCount)}/{totalUploadCount}");
                        }
                        else
                        {
                            Log($"{fileName}：Upload Fail，StatusCode：{res?.StatusCode}");
                        }
                        res?.Close();
                    }
                    catch (Exception e)
                    {
                        Interlocked.Increment(ref finishedUploadCount);
                        Log($"Upload error file：{fileName}，Exception：{e.Message}");
                    }
                });
            }
            catch (Exception e)
            {
                Log($"Upload Exception：{e.Message}");
            }

            if (finishedUploadCount == totalUploadCount && totalUploadCount > 0)
            {
                Log("--- End Upload Data Over ---");
                totalUploadCount = 0;
                finishedUploadCount = 0;
            }
        }
    }
}
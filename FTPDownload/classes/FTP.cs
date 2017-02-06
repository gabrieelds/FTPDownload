using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTPDownload.classes {
    class FTP {
        private string host = null;
        private string user = null;
        private string pass = null;
        private FtpWebRequest ftpRequest = null;
        private FtpWebResponse ftpResponse = null;

        /* Construct Object */
        public FTP(string hostIP, string userName, string password) {
            host = "ftp://"+hostIP;
            user = userName;
            pass = password;
        }

        public void downloadDir(string remotePath, string localPath) {
            Console.WriteLine(host + remotePath);
            FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(host + remotePath);
            listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            listRequest.Credentials = new NetworkCredential(user, pass);

            List<string> lines = new List<string>();

            using (FtpWebResponse listResponse = (FtpWebResponse)listRequest.GetResponse())
            using (Stream listStream = listResponse.GetResponseStream())
            using (StreamReader listReader = new StreamReader(listStream)) {
                while (!listReader.EndOfStream) {
                    lines.Add(listReader.ReadLine());
                }
            }

            foreach (string line in lines) {
                string[] tokens =
                    line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                string name = tokens[8];
                string permissions = tokens[0];

                string localFilePath = Path.Combine(localPath, name);
                string fileUrl = host + remotePath + name;

                if (permissions[0] == 'd') {
                    if (!Directory.Exists(localFilePath)) {
                        Directory.CreateDirectory(localFilePath);
                    }
                    downloadDir(host, localPath);
                } else {
                    Console.WriteLine("Baixando: " + fileUrl);
                    FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(fileUrl);
                    downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                    downloadRequest.Credentials = listRequest.Credentials;
                    using (FtpWebResponse downloadResponse =
                              (FtpWebResponse)downloadRequest.GetResponse())
                    using (Stream sourceStream = downloadResponse.GetResponseStream())
                        if (getFileSize(fileUrl)  > 0){
                        using (Stream targetStream = File.Create(localFilePath)) {
                            byte[] buffer = new byte[10240];
                            int read;
                            while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0) {
                                targetStream.Write(buffer, 0, read);
                            }
                            delete(fileUrl);
                        }
                    }
                }
            }
        }

        /* Delete File */
        public void delete(string deleteFile){
            try{
                ftpRequest = (FtpWebRequest)WebRequest.Create(deleteFile);
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpResponse.Close();
                ftpRequest = null;
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            return;
        }

        /* Get the Size of a File */
        public long getFileSize(string fileName) {
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(new Uri(fileName));
            request.Proxy = null;
            request.Credentials = new NetworkCredential(user, pass);
            request.Method = WebRequestMethods.Ftp.GetFileSize;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            long size = response.ContentLength;
            response.Close();
            Console.WriteLine("Tamanho:" +size);
            return size;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTPDownload.classes;

namespace FTPDownload {
    class Program {
        static void Main(string[] args) {
            /* Create Object Instance */
            FTP ftpClient = new FTP(args[0], args[1], args[2]);

            /* Download a File */
            ftpClient.downloadDir(args[3], args[4]);

            ftpClient = null;
        }
    }
}

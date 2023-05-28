using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWorkerProject.Services
{
    public interface IMyService
    {
        Task Przetworz(string path, string fileName);
        string PrzetworzNazwePliku(string input);
        public List<string> FullScanFolder(string path);
        public byte[] StworzBody(object input);
    }
}

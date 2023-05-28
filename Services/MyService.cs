using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWorkerProject.Services
{
    public class MyService : IMyService
    {
        private ILogger<MyService> _logger;
        private OracleDatabaseService _db;
        private int id;

        public MyService(ILogger<MyService> logger)
        {
            _logger = logger;
            id = 0;
        }


        /// <summary>
        /// path musi sie konczyc \ !!!
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task Przetworz(string path, string fileName)
        {
            if(path.EndsWith("\\") == false)
                path = path + "\\";

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            
            if (string.IsNullOrEmpty(fileNameWithoutExt))
                return;
            
            await Task.Delay(1000);

            var fullPathName = path + fileName;
            try
            {
                File.Move(fullPathName, Status.Ok + fullPathName);

                var query = "INSERT INTO mojatabela VALUES (:path,:file)";
                _db.ExecuteQuery(query, new { path = Status.Ok + fullPathName, file = fileName });
            }
            catch (Exception)
            {
                _logger.LogCritical($"Z: {fullPathName}, do: {Status.Ok + fullPathName}");
            }
            
            _logger.LogWarning($"{(id++)}.Jestem w service, odebrano: {fileNameWithoutExt}, przerobiono na {Status.Ok + fullPathName}");
        }

        public string PrzetworzNazwePliku(string input)
        {
            return input.Replace(".jpeg", "").Replace(".jpg", "");
        }

        public List<string> FullScanFolder(string path)
        {
            var filesPaths = Directory.GetFiles(path);

            List<string> result = new List<string>();
            for(int i = 0; i < filesPaths.Length; i++)
            {
                var file = filesPaths[i];
                var fileName = Path.GetFileName(file);

                // Regex sprawdzajacy plik
                if (file.Contains("_REGEX") || file.Contains("_OK"))
                    continue;
                
                if (file.Contains("kopia"))
                {
                    File.Move(file, file.Remove(file.Length - fileName.Length) + "REGEX_" + fileName);
                }
                // Sprawdzajacy koncowki pliku ??? 

                result.Add(file);
            }
            return result;
        }

        public byte[] StworzBody(object input)
        {
            var message = JsonConvert.SerializeObject(input);
            var body = Encoding.UTF8.GetBytes(message);

            return body;
        }

    }
}

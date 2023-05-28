using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQWorkerProject.Services;

namespace RabbitMQWorkerProject
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IMyService _myService;
        private string _kolejka;
        private bool _firstUsage;
        private string _path;

        public Worker(ILogger<Worker> logger, IMyService myService)
        {
            _logger = logger;
            _myService = myService;
            _firstUsage = true;
            _kolejka = "MarcinowaRabbitowaKolejka3";
            _path = @"C:\Users\knapi\Desktop\watcher_folder";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var watcher = new FileSystemWatcher(_path);

            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            using (var conn = factory.CreateConnection())
            {
                /* Tworzenie kanalu i konfiguracja kolejki */
                var channel = conn.CreateModel();
                channel.QueueDeclare
                (
                    _kolejka,
                    true,
                    false,
                    false,
                    null
                );
                
                /* Konfiguracja consumera (odbieranie wiadomosci) */
                var consumer = new EventingBasicConsumer(channel);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 200, global: false);
                channel.BasicConsume
                (
                    _kolejka,
                    false,
                    consumer
                );

                /* Nieulotna pamiec */
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                /* Reagowanie na zmiany w folderze */
                watcher.Created += delegate (object sender, FileSystemEventArgs args)
                {
                    var fileName = args.Name;
                    var fileNameToSend = _myService.PrzetworzNazwePliku(fileName);

                    var body = _myService.StworzBody(fileNameToSend);
                    
                    channel.BasicPublish("", _kolejka, properties, body);
                };

                /* Odbieranie wiadomosci */
                consumer.Received += async (sender, args) =>
                {
                    var fileName = Encoding.UTF8.GetString(args.Body.ToArray());

                    await _myService.Przetworz(_path, fileName);
                    channel.BasicAck(args.DeliveryTag, false);
                };

                /* Skanowanie calego folderu na start, wyslanie plikow */
                var filesToSend = _myService.FullScanFolder(_path);

                foreach (var file in filesToSend)
                {
                    var body = _myService.StworzBody(file);
                    channel.BasicPublish("", _kolejka, properties, body);
                }
                
                /* Aktywacja watchera i nieskonczona petla */
                watcher.EnableRaisingEvents = true;
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
                
            }
        }
    }
}
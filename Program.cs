using RabbitMQWorkerProject.Services;

namespace RabbitMQWorkerProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IMyService, MyService>();
                    services.AddSingleton<OracleDatabaseService>();
                    services.AddHostedService<Worker>();
                })
                .Build();

            host.Run();
        }
    }
}
using Azure.Data.Tables;
using Azure.Storage.Queues;

namespace _10433939_CLDV6212_POE_P2.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;
        public QueueService(string connectionString, string queueName)
        {
            _queueClient = new QueueClient(connectionString, queueName);
        }
        public async Task SendMessage(string message)
        {
            await _queueClient.SendMessageAsync(message);
        }
    }
}

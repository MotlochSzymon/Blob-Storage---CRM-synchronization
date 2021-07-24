using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Predica.Xrm.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predica.QueueService
{
    public class LeadQueueSender
    {
        static private ServiceBusClient client;
        static private ServiceBusSender sender;
        static private ILogger log;
        public LeadQueueSender(ILogger logger)
        {
            client = CreateQueueClient();
            sender = CreateQueueSender();
            log = logger;
        }

        private ServiceBusClient CreateQueueClient()
        {
            var connectionString = Environment.GetEnvironmentVariable("QUEUECONNECTIONSTRING");
            return new ServiceBusClient(connectionString);
        }

        private ServiceBusSender CreateQueueSender()
        {
            var queueName = Environment.GetEnvironmentVariable("LEADQUEUENAME");
            return client.CreateSender(queueName);
        }

        public async void SendUsaLeadsToQueue(List<Contact> contacts)
        {
            using (ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync())
            {
                foreach (var contact in contacts)
                {
                    if (contact.Address1_Country != null && (contact.Address1_Country == "USA" || contact.Address1_Country.ToLower() == "united states of america"))
                    {
                        var objectToSend = JsonConvert.SerializeObject(contact);
                        if (!messageBatch.TryAddMessage(new ServiceBusMessage(objectToSend)))
                        {
                            log.LogError($"Contact {contact?.FirstName} {contact?.LastName} is too large to fit in the batch.");
                            continue;
                        }

                        log.LogInformation($"Contact {contact?.FirstName} {contact?.LastName} will be sent to Azure Service Bus Queue");
                    }
                    else
                    {
                        log.LogInformation($"Contact {contact?.FirstName} {contact?.LastName} is not from USA and this data won't be send to Azure Service Bus Queue");
                    }
                }

                try
                {
                    await sender.SendMessagesAsync(messageBatch);
                }
                finally
                {
                    await sender.DisposeAsync();
                    await client.DisposeAsync();
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Server;

namespace UpNet.Sandbox
{
    public class LocalStorage : IMqttServerStorage
    {
        public IList<MqttApplicationMessage> Messages { get; set; }

        public Task SaveRetainedMessagesAsync(IList<MqttApplicationMessage> messages)
        {
            Messages = messages;
            return Task.CompletedTask;
        }

        public Task<IList<MqttApplicationMessage>> LoadRetainedMessagesAsync() =>
            Task.FromResult(Messages);
    }
}
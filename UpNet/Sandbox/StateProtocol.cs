using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;

namespace UpNet.Sandbox
{
    public static class StateProtocol
    {
        // TODO: Header?
        // TODO: Message reuse? State encoder reusing the same application message only replacing the payload?
        // TODO: Then also, fetch state returning arrays only?
        // TODO: IAsyncEnumerable??

        private static readonly byte[] Terminator = Encoding.UTF8.GetBytes($"{nameof(StateProtocol)}-terminator");

        public static async IAsyncEnumerable<MqttApplicationMessage> FetchStateAsync(
            IMqttClient client)
        {
            // TODO: Handler might be overwritten somewhere else?

            // Target channel. Receives the messages from state fetching and produces the resulting enumerable.
            var channel = Channel.CreateUnbounded<MqttApplicationMessage>();

            // Correlation topic used for this method call.
            var correlationTopic = $"state-{Guid.NewGuid()}";

            // Memorize handler to reinstate later. Replace with correlation handling.
            var handler = client.ApplicationMessageReceivedHandler;
            client.UseApplicationMessageReceivedHandler(async receivedEvent =>
            {
                // Check if working on correlation topic.
                if (correlationTopic == receivedEvent.ApplicationMessage.Topic)
                {
                    // Correlation topic, see if payload is not empty. Empty will mark end segment.
                    if (receivedEvent.ApplicationMessage.Payload?.SequenceEqual(Terminator) != true)
                    {
                        // Write received message, non-management operation.
                        await channel.Writer.WriteAsync(receivedEvent.ApplicationMessage);
                    }
                    else
                    {
                        // Complete the channel.
                        channel.Writer.Complete();
                    }
                }
                else
                {
                    // Not a correlation topic, pass to handler.
                    if (handler != null)
                        await handler.HandleApplicationMessageReceivedAsync(receivedEvent);
                }
            });

            // Subscribe to correlation.
            await client.SubscribeAsync(correlationTopic);

            // Request and read all messages and return them.
            await client.PublishAsync("state-requests", correlationTopic);
            await foreach (var message in channel.Reader.ReadAllAsync())
                yield return message;

            // Unsubscribe from correlation topic.
            await client.UnsubscribeAsync(correlationTopic);

            // Reinstate original handler.
            client.ApplicationMessageReceivedHandler = handler;
        }

        public static async Task<bool> TryProvideStateAsync(
            IMqttServer server,
            Func<string, IAsyncEnumerable<MqttApplicationMessage>> stateEncoder,
            MqttApplicationMessage message)
        {
            // If not a state request, nothing to do.
            if ("state-requests" != message.Topic)
                return false;

            // Get topic to publish to.
            var correlationTopic = message.ConvertPayloadToString();

            // Responsible but incorrect.
            if (null == correlationTopic)
                throw new ArgumentException("The message is a state request, but is empty", nameof(message));

            // Encode state with the given function. Send all.
            await foreach (var statePart in stateEncoder(correlationTopic))
                await server.PublishAsync(statePart);

            // Publish terminator.
            await server.PublishAsync(new MqttApplicationMessage
            {
                Topic = correlationTopic,
                Payload = Terminator
            });

            // Handled.
            return true;
        }
    }
}
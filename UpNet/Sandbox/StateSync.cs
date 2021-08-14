using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Server;

namespace UpNet.Sandbox
{
    public static class StateSync
    {
        public static async Task StatSyncSandbox()
        {
            var mqttFactory = new MqttFactory();
            if (Console.ReadLine()?.ToLower().StartsWith("s") == true)
            {
                var state = "";

                async IAsyncEnumerable<MqttApplicationMessage> EncodeState(string correlationTopic)
                {
                    yield return new MqttApplicationMessage
                    {
                        Topic = correlationTopic,
                        Payload = Encoding.UTF8.GetBytes(state)
                    };
                }

                var server = mqttFactory.CreateMqttServer();
                await server.StartAsync(new MqttServerOptionsBuilder()
                    .WithConnectionBacklog(100)
                    .WithDefaultEndpointPort(1884)
                    .WithApplicationMessageInterceptor(async mqttEvent =>
                    {
                        if (await StateProtocol.TryProvideStateAsync(server, EncodeState, mqttEvent.ApplicationMessage))
                            return;

                        if ("data" == mqttEvent.ApplicationMessage.Topic)
                            state += (string?) mqttEvent.ApplicationMessage.ConvertPayloadToString();
                    })
                    .Build());

                Console.WriteLine("Started server");
                Console.ReadLine();
                await server.StopAsync();
            }
            else
            {
                // TODO: Initial state not set if no client sent anything yet.
                var state = "";
                var client = mqttFactory.CreateMqttClient();
                client.UseApplicationMessageReceivedHandler(mqttEvent =>
                {
                    if (mqttEvent.ApplicationMessage.Topic == "data")
                    {
                        state += mqttEvent.ApplicationMessage.ConvertPayloadToString();
                        Console.WriteLine($"State changed to {state}");
                    }
                });

                await client.ConnectAsync(new MqttClientOptionsBuilder()
                    .WithTcpServer("localhost", 1884)
                    .Build());

                await client.SubscribeAsync("data");

                await foreach (var message in StateProtocol.FetchStateAsync(client))
                    state = message.ConvertPayloadToString();

                Console.WriteLine($"Connected client on initial state {state}");

                string? line;
                while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
                    await client.PublishAsync("data", line);

                await client.UnsubscribeAsync("data");
                await client.DisconnectAsync();
            }
        }
    }
}
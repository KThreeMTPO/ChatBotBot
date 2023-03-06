using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System;
using System.Threading.Tasks;

var factory = new MqttFactory();

MqttServer server = InitializeServer(factory);

await server.StartAsync();

var ClientThread = Task.Run(() => InitializeClient(factory));

Console.ReadLine();

await server.StopAsync();


static void InitializeClient(MqttFactory factory)
{
    var client = factory.CreateMqttClient();
    {
        var clientOptions = new MqttClientOptionsBuilder().WithTcpServer("localhost").Build();


        client.DisconnectedAsync += async e =>
        {
            Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {               
                await client.ConnectAsync(clientOptions);
            }
            catch
            {
                Console.WriteLine("### RECONNECTING FAILED ###");
            }
        };
        client.ConnectedAsync += async e =>
        {
            var mqttSubscribeOptions = new MqttClientSubscribeOptionsBuilder()

                .WithTopicFilter("/sensor/microphone_data")
                .WithTopicFilter("/sensor/video_data")

                .Build();
            var result = await client.SubscribeAsync(mqttSubscribeOptions);
            Console.WriteLine(result.ToString());
        };
        client.ApplicationMessageReceivedAsync += msg =>
        {
            var message = msg.ApplicationMessage;
            Console.WriteLine($"Received TOPIC: {message.Topic}, message: {message.ConvertPayloadToString()}");
            return Task.CompletedTask;
        };
        client.ConnectAsync(clientOptions);
    }
}

static MqttServer InitializeServer(MqttFactory factory)
{
    var options = new MqttServerOptionsBuilder()
        .WithDefaultEndpoint()
        .WithDefaultEndpointPort(1883)
        .Build();

    var server = factory.CreateMqttServer(options);
    server.ApplicationMessageNotConsumedAsync += Server_ApplicationMessageNotConsumedAsync;

    Task Server_ApplicationMessageNotConsumedAsync(ApplicationMessageNotConsumedEventArgs arg)
    {
        //Console.WriteLine("Unhandled: " + arg.ApplicationMessage.ConvertPayloadToString());
        return Task.CompletedTask;
    }

    server.StartedAsync += async (args) =>
    {
        Console.WriteLine("MQTT server started.");
    };
    return server;
}
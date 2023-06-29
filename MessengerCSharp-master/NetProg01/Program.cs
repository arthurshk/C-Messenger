using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CommonLibrary;
using CommonLibrary.Requests;

public class Client
{
    private string serverIP;
    private int serverPort;
    private TcpClient client;
    private NetworkStream stream;

    public Client(string serverIP, int serverPort)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
    }

    public void Connect()
    {
        client = new TcpClient();
        client.Connect(serverIP, serverPort);
        stream = client.GetStream();
    }

    public void Disconnect()
    {
        stream.Close();
        client.Close();
    }

    public int GetMessageCount(string login)
    {
        Connect();
        var requestData = Data.Create(new GetMessagesRequest { Me = new Client { Login = login } });
        var requestJson = JsonSerializer.Serialize(requestData);
        SendMessage(requestJson);

        var responseJson = ReadMessage();
        var responseData = JsonSerializer.Deserialize<Data>(responseJson);
        var response = responseData?.Content;
        Disconnect();

        if (response != null && response.Type == DataType.GetMessagesResponse)
        {
            var getMessagesResponse = JsonSerializer.Deserialize<GetMessagesResponse>(response.Content);
            return getMessagesResponse.Messages.Count;
        }

        return 0;
    }

    public List<Message> GetMessages(string login)
    {
        Connect();
        var requestData = Data.Create(new GetMessagesRequest { Me = new Client { Login = login } });
        var requestJson = JsonSerializer.Serialize(requestData);
        SendMessage(requestJson);

        var responseJson = ReadMessage();
        var responseData = JsonSerializer.Deserialize<Data>(responseJson);
        var response = responseData?.Content;
        Disconnect();

        if (response != null && response.Type == DataType.GetMessagesResponse)
        {
            var getMessagesResponse = JsonSerializer.Deserialize<GetMessagesResponse>(response.Content);
            return getMessagesResponse.Messages;
        }

        return new List<Message>();
    }

    public bool SendMessage(Message message)
    {
        Connect();
        var requestData = Data.Create(new SendMessageRequest { Message = message });
        var requestJson = JsonSerializer.Serialize(requestData);
        SendMessage(requestJson);

        var responseJson = ReadMessage();
        var responseData = JsonSerializer.Deserialize<Data>(responseJson);
        var response = responseData?.Content;
        Disconnect();

        if (response != null && response.Type == DataType.SendMessageResponse)
        {
            var sendMessageResponse = JsonSerializer.Deserialize<SendMessageResponse>(response.Content);
            return sendMessageResponse.Success;
        }

        return false;
    }

    private void SendMessage(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }

    private string ReadMessage()
    {
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        return Encoding.ASCII.GetString(buffer, 0, bytesRead);
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Client");

        var client = new Client("127.0.0.1", 5000);

        Console.Write("Login: ");
        var login = Console.ReadLine();

        var messageCount = client.GetMessageCount(login);

        Console.WriteLine("Message count: {0}", messageCount);

        if (messageCount > 0)
        {
            var messages = client.GetMessages(login);
            foreach (var message in messages)
            {
                Console.WriteLine("From: {0}", message.From.Login);
                Console.WriteLine("To: {0}", message.To.Login);
                Console.WriteLine(message.CreatedAt.ToString("G"));
                Console.WriteLine(message.Text);
                Console.WriteLine();
            }
        }

        Console.Write("Message to: ");
        var toLogin = Console.ReadLine();
        Console.Write("Text: ");
        var text = Console.ReadLine();

        var message = new Message
        {
            From = new CommonLibrary.Client { Login = login },
            To = new CommonLibrary.Client { Login = toLogin },
            Text = text,
            CreatedAt = DateTime.Now,
        };

        if (client.SendMessage(message))
        {
            Console.WriteLine("Send success");
        }
    }
}
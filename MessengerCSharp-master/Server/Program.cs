using CommonLibrary;
using Server;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        IPEndPoint endPoint = new IPEndPoint(ipAddress, 5000);

        serverSocket.Bind(endPoint);
        serverSocket.Listen(1000);

        Console.WriteLine("Server");

        Core core = new Core();

        Action<Socket> worker = (clientSocket) =>
        {
            var buffer = new byte[4096];
            int bytesRead = clientSocket.Receive(buffer);

            string incoming = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine(incoming);

            var request = JsonSerializer.Deserialize<Data>(incoming);
            var response = JsonSerializer.Serialize(core.Handle(request));
            Console.WriteLine(response);

            clientSocket.Send(Encoding.UTF8.GetBytes(response));

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        };

        while (true)
        {
            Socket clientSocket = serverSocket.Accept();
            Task.Run(() => worker(clientSocket));
        }
    }
}
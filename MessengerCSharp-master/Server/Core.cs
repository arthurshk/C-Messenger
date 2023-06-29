using CommonLibrary;
using CommonLibrary.Requests;
using CommonLibrary.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Server;
{
    public class Core
    {
        private List<Client> _clients = new List<Client>();
        private List<Message> _messages = new List<Message>();

        public Core()
        {
        }

        private T Deserialize<T>(Data data) where T : class
        {
            return JsonSerializer.Deserialize<T>(data.Content);
        }

        private bool IsClientAuthenticated(Client client)
        {
            var existingClient = _clients.FirstOrDefault(x => x.Login == client.Login);
            return existingClient != null && client.Password == existingClient.Password;
        }

        private Data HandleLoginRequest(Data data)
        {
            var request = Deserialize<LoginRequest>(data);

            var client = _clients.FirstOrDefault(x => x.Login == request.Me.Login);
            if (client == null)
            {
                _clients.Add(request.Me);
                return Data.Create(new LoginResponse
                {
                    Success = true,
                    MessagesCount = 0,
                });
            }

            if (!IsClientAuthenticated(request.Me))
            {
                return Data.Create(new ErrorResponse
                {
                    Error = "Invalid password"
                });
            }

            var messagesCount = _messages.Count(x => x.To.Login == request.Me.Login);
            return Data.Create(new LoginResponse
            {
                Success = true,
                MessagesCount = messagesCount,
            });
        }

        private Data HandleSendMessageRequest(Data data)
        {
            var request = Deserialize<SendMessageRequest>(data);

            if (!IsClientAuthenticated(request.Message.From))
            {
                return Data.Create(new ErrorResponse
                {
                    Error = "Not authorized"
                });
            }

            _messages.Add(request.Message);
            return Data.Create(new SendMessageResponse { Success = true });
        }

        private Data HandleGetMessagesRequest(Data data)
        {
            var request = Deserialize<GetMessagesRequest>(data);

            if (!IsClientAuthenticated(request.Me))
            {
                return Data.Create(new ErrorResponse
                {
                    Error = "Not authorized"
                });
            }

            var clientMessages = _messages.Where(x => x.To.Login == request.Me.Login).ToList();
            _messages.RemoveAll(x => x.To.Login == request.Me.Login);

            return Data.Create(new GetMessagesResponse
            {
                Messages = clientMessages
            });
        }

        public Data Handle(Data request)
        {
            switch (request.Type)
            {
                case DataType.LoginRequest:
                    return HandleLoginRequest(request);
                case DataType.SendMessageRequest:
                    return HandleSendMessageRequest(request);
                case DataType.GetMessageRequest:
                    return HandleGetMessagesRequest(request);
                default:
                    return Data.Create(new ErrorResponse
                    {
                        Error = "Unknown request"
                    });
            }
        }
    }
}

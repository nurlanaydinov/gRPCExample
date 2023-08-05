using Grpc.Core;
using grpcMessageServer;

namespace grpcServer.Services
{
   
    public class MessageService : Message.MessageBase
    {
        private readonly ILogger<MessageService> _logger;
        public MessageService(ILogger<MessageService> logger)
        {
            _logger = logger;
        }
        //server streaming
        //public override async Task Send(MessageRequest request, IServerStreamWriter<MessageResponse> responseStream, ServerCallContext context)
        //{
        //    Console.WriteLine($"message:{request.Message} | name: {request.Name}");
        //    for (int i = 0;i<10; i++)
        //    {
        //        await Task.Delay(10);
        //        await responseStream.WriteAsync(new MessageResponse
        //        {
        //            Message = "dddd" + i
        //        });
        //    }
        // }
        //Client Streaming
        //public override async Task<MessageResponse> Send(IAsyncStreamReader<MessageRequest> requestStream, ServerCallContext context)
        //{
        //    while (await requestStream.MoveNext(context.CancellationToken))
        //    {
        //        Console.WriteLine($"message:{requestStream.Current.Message} | name: {requestStream.Current.Name}");
        //    }
        //    return new MessageResponse
        //    {
        //        Message = "Data Collected!.."
        //    };
        //}
        //bi- directional streaming
     
        public override async Task Send(IAsyncStreamReader<MessageRequest> requestStream, IServerStreamWriter<MessageResponse> responseStream, ServerCallContext context)
        {
            var task1 = Task.Run(async () =>
            {
                while (await requestStream.MoveNext(context.CancellationToken))
                {
                    Console.WriteLine($"message:{requestStream.Current.Message} | name: {requestStream.Current.Name}");
                }
            });

            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                await responseStream.WriteAsync(new MessageResponse
                {
                    Message = "Success" + i
                });
            }
            await task1;
        }
    }
}

using Google.Protobuf;
using Grpc.Net.Client;
using grpcFileTransportClient;
using grpcMessageClient;
using grpcServer;

namespace grpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5229");
            //var messageClient = new Message.MessageClient(channel);
            var fileTransportClient = new FileService.FileServiceClient(channel);
            //unary
            //MessageResponse response = await messageClient.SendAsync(new MessageRequest
            //{
            //    Message = "Wild One",
            //    Name = "Hannibal"
            //});
            //await Console.Out.WriteLineAsync(response.Message);
            //server streaming
            //var response = messageClient.Send(new MessageRequest
            // {
            //     Message = "Test",
            //     Name = "Test2",
            // });
             CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            // while (await response.ResponseStream.MoveNext(cancellationTokenSource.Token))
            // {
            //     await Console.Out.WriteLineAsync(response.ResponseStream.Current.Message);
            // }

            //CLient streaming
            //var request = messageClient.Send();
            //for (int i = 0; i < 10; i++)
            //{
            //    await Task.Delay(500);
            //    await request.RequestStream.WriteAsync(new MessageRequest
            //    {
            //        Message = "Request1",
            //        Name = "Request2"
            //    });
            //}
            //await request.RequestStream.CompleteAsync();
            //await Console.Out.WriteLineAsync((await request.ResponseAsync).Message);

            //bi-directional streaming
            //var request = messageClient.Send();
            //var task1 = Task.Run(async () =>
            //{
            //    for (int i = 0; i < 10; i++)
            //    {
            //        await Task.Delay(1000);
            //        await request.RequestStream.WriteAsync(new MessageRequest
            //        {
            //            Message = "request 1",
            //            Name = "request 2"
            //        });
            //    }
            //});
            //while (await request.ResponseStream.MoveNext(cancellationTokenSource.Token))
            //{
            //    await Console.Out.WriteLineAsync(request.ResponseStream.Current.Message);
            //}

            //await task1;
            //await request.RequestStream.CompleteAsync();

            //var greetClient = new Greeter.GreeterClient(channel);

            //HelloReply result = await greetClient.SayHelloAsync(new HelloRequest
            //{
            //    Name = "salam"
            //});

            //Console.WriteLine(result.Message);

            string file = @"C:\Users\Asus\Downloads\Veri yapıları ve Algoritmalar - Data Structures and Algorithms (1).mp4";
            using FileStream fileStream = new FileStream(file, FileMode.Open);

            var content = new BytesContent
            {
                FileSize = fileStream.Length,
                ReadedByte = 0,
                Info = new grpcFileTransportClient.FileInfo
                {
                    FileName = Path.GetFileNameWithoutExtension(fileStream.Name),
                    FileExtension = Path.GetExtension(fileStream.Name)
                }
            };
            var upload = fileTransportClient.FileUpload();
            byte[] buffer = new byte[2048];
            while ((content.ReadedByte = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0) 
            {
                content.Buffer = ByteString.CopyFrom(buffer);
                await upload.RequestStream.WriteAsync(content);

            }
            await upload.RequestStream.CompleteAsync();
            fileStream.Close();
            
        }
    }
}
using Grpc.Net.Client;
using grpcFileClient;

namespace grpcFileStream
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5229");

            var client = new FileService.FileServiceClient(channel);

            string downloadPath = @"C:\Users\Asus\Desktop\gRPCExample\grpcFileStream\DownloadFiles";
            var fileInfo = new grpcFileClient.FileInfo
            {
                FileName = "Veri yapıları ve Algoritmalar - Data Structures and Algorithms (1)",
                FileExtension = ".mp4"
            };

            FileStream fileStream = null;
            var download = client.FileDownload(fileInfo);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            int count = 0;
            decimal chunkSize = 0;
            while ( await download.ResponseStream.MoveNext((cancellationTokenSource.Token)))
            {
                if (count++ == 0)
                {
                    fileStream = new FileStream($@"{downloadPath}\{download.ResponseStream.Current.Info.FileName}{download.ResponseStream.Current.Info.FileExtension}", FileMode.CreateNew);
                    fileStream.SetLength(download.ResponseStream.Current.FileSize);

                }
                var buffer = download.ResponseStream.Current.Buffer.ToByteArray();
                await fileStream.WriteAsync(buffer, 0, download.ResponseStream.Current.ReadedByte);

                await Console.Out.WriteLineAsync($"{Math.Round((chunkSize += download.ResponseStream.Current.ReadedByte)*100)/download.ResponseStream.Current.FileSize}%");
            }
            await Console.Out.WriteLineAsync("Downloaded...");
            await fileStream.DisposeAsync();
            fileStream.Close();
        }
    }
}


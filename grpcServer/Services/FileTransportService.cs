﻿using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using grpcFileTransportServer;

namespace grpcServer.Services
{
    public class FileTransportService : FileService.FileServiceBase
    {
        private readonly ILogger<FileTransportService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileTransportService(ILogger<FileTransportService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public override async Task<Empty> FileUpload(IAsyncStreamReader<grpcFileTransportServer.BytesContent> requestStream, ServerCallContext context)
        {
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "files");
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            FileStream fileStream = null;
            try
            {
                int count = 0;
                decimal chunkSize = 0;
                while (await requestStream.MoveNext())
                {
                    if (count++ == 0)
                    {
                        fileStream = new FileStream($"{path}/{requestStream.Current.Info.FileName}{requestStream.Current.Info.FileExtension}", FileMode.CreateNew);
                        fileStream.SetLength(requestStream.Current.FileSize);
                    }
                    var buffer = requestStream.Current.Buffer.ToByteArray();
                    await fileStream.WriteAsync(buffer, 0, buffer.Length);

                    await Console.Out.WriteLineAsync($"{Math.Round(((chunkSize += requestStream.Current.ReadedByte)*100)/ requestStream.Current.FileSize)}%");
                }
            }
            catch 
            {

                
            }
            await fileStream.DisposeAsync();
            fileStream.Close();          
            return new Empty();
        }

        public override async Task FileDownload(grpcFileTransportServer.FileInfo request, IServerStreamWriter<BytesContent> responseStream, ServerCallContext context)
        {
            string path = Path.Combine(_webHostEnvironment.WebRootPath,"files");
            using FileStream fileStream = new FileStream($"{path}/{request.FileName}{request.FileExtension}",FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[2048];

            BytesContent content = new BytesContent
            {
                FileSize = fileStream.Length,
                Info = new grpcFileTransportServer.FileInfo
                {
                    FileName = Path.GetFileNameWithoutExtension(path),
                    FileExtension = Path.GetExtension(fileStream.Name)
                },
                ReadedByte = 0
            };
            while ((content.ReadedByte = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                content.Buffer = ByteString.CopyFrom(buffer);
                await responseStream.WriteAsync(content);
            }
            fileStream.Close();
        }
    }
}

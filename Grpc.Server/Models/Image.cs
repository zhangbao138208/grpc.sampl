using System;

namespace Grpc.Server.Models
{
    public class Image
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public byte[] Data { get; set; }
        public Image()
        {
            Id = Guid.NewGuid();
        }
    }
}

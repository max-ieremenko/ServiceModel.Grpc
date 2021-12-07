using System.IO.Compression;

namespace Contract
{
    public static class CompressionSettings
    {
        public const CompressionLevel Level = CompressionLevel.Fastest;

        public const string Algorithm = "gzip";
    }
}

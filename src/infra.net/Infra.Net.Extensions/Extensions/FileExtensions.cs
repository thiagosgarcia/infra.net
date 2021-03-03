using System;
using System.Data.HashFunction;
using System.Data.HashFunction.CityHash;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Infra.Net.Extensions.Extensions
{
    public enum HashType
    {
        Md5,
        Sha256,
        Sha1
    }
    public enum DeterministicGuidAlgorithm
    {
        Md5,
        CityHash
    }
    public static class FileExtensions
    {
        public static string GetExtension(this IFormFile file)
        {
            return GetFilenameExtension(file.FileName);
        }

        private static string GetFilenameExtension(this string filename)
        {
            string extension = null;
            var indexOfExtension = filename.LastIndexOf('.');
            if (indexOfExtension >= 0)
                extension = filename.Substring(indexOfExtension);
            return extension;
        }

        public static Task<string> GetHash(this IFormFile file, HashType hashType, CancellationToken cancellationToken)
        {
            return GetHash(file.OpenReadStream(), hashType, cancellationToken);
        }
        public static async Task<string> GetHash(this Stream file, HashType hashType, CancellationToken cancellationToken)
        {
            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms, cancellationToken);
            return Encode(ms, hashType);
        }
        public static Task<IHashValue> GetCityHash(this IFormFile source, int desiredHashSize, CancellationToken cancellationToken)
        {
            return GetCityHash(source.OpenReadStream(), desiredHashSize, cancellationToken);
        }
        public static async Task<IHashValue> GetCityHash(this Stream source, int desiredHashSize, CancellationToken cancellationToken)
        {
            var cityHash = CityHashFactory.Instance.Create();
            await using var ms = new MemoryStream();
            await source.CopyToAsync(ms, cancellationToken);
            return cityHash.ComputeHash(ms.ToArray(), desiredHashSize);
        }
        public static Task<Guid> GenerateDeterministicGuid(this IFormFile source, DeterministicGuidAlgorithm algorithm = DeterministicGuidAlgorithm.CityHash, CancellationToken cancellationToken = default)
        {
            return GenerateDeterministicGuid(source.OpenReadStream(), algorithm, cancellationToken);
        }
        public static async Task<Guid> GenerateDeterministicGuid(this Stream source, DeterministicGuidAlgorithm algorithm = DeterministicGuidAlgorithm.CityHash, CancellationToken cancellationToken = default)
        {
            switch (algorithm)
            {
                case DeterministicGuidAlgorithm.Md5:
                    var md5 = await GetHash(source, HashType.Md5, cancellationToken);
                    return new Guid(md5);
                case DeterministicGuidAlgorithm.CityHash:
                default:
                    var cityHash = await GetCityHash(source, 128, cancellationToken);
                    return new Guid(cityHash.Hash);

            }
        }

        private static string Encode(MemoryStream ms, HashType hashType)
        {
            var fileBytes = ms.ToArray();
            return Encode(fileBytes, hashType);
        }

        private static string Encode(byte[] fileBytes, HashType hashType)
        {
            switch (hashType)
            {
                case HashType.Md5:
                    return Convert.ToHexString(MD5.HashData(fileBytes));
                case HashType.Sha256:
                    return Convert.ToHexString(SHA256.HashData(fileBytes));
                case HashType.Sha1:
                    return Convert.ToHexString(SHA1.HashData(fileBytes));
                default:
                    throw new ArgumentOutOfRangeException(nameof(hashType), hashType, "HashType should be one of the provided list");
            }
        }
    }
}

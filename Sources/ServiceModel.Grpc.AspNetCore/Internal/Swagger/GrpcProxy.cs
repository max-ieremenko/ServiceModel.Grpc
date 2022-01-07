// <copyright>
// Copyright 2021 Max Ieremenko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace ServiceModel.Grpc.AspNetCore.Internal.Swagger
{
    internal sealed class GrpcProxy : IRequestBodyPipeFeature, IHttpResponseBodyFeature, IHttpResponseTrailersFeature, IHttpResetFeature, IDisposable
    {
        private readonly HttpContext _context;
        private readonly string _originalProtocol;
        private readonly IRequestBodyPipeFeature _originalRequestBodyPipeFeature;
        private readonly IHttpResponseBodyFeature _originalResponseBodyFeature;
        private readonly IHttpResponseTrailersFeature? _originalResponseTrailersFeature;
        private readonly MemoryStream _responseStream;

        private PipeReader _requestReader = null!;
        private PipeWriter _responseWriter = null!;

        public GrpcProxy(HttpContext context)
        {
            _context = context;

            _originalProtocol = context.Request.Protocol;
            _originalRequestBodyPipeFeature = context.Features.Get<IRequestBodyPipeFeature>()!;
            _originalResponseBodyFeature = context.Features.Get<IHttpResponseBodyFeature>()!;
            _originalResponseTrailersFeature = context.Features.Get<IHttpResponseTrailersFeature>();

            _responseStream = new MemoryStream();
            Trailers = new HeaderDictionary();
        }

        PipeReader IRequestBodyPipeFeature.Reader => _requestReader;

        Stream IHttpResponseBodyFeature.Stream => throw new NotSupportedException();

        PipeWriter IHttpResponseBodyFeature.Writer => _responseWriter;

        public IHeaderDictionary Trailers { get; set; }

        public void Attach(byte[] requestBody)
        {
            _requestReader = PipeReader.Create(new MemoryStream(requestBody));
            _responseWriter = PipeWriter.Create(_responseStream);

            _context.Request.Protocol = ProtocolConstants.Http2;
            _context.Request.ContentType = ProtocolConstants.MediaTypeNameGrpc;
            _context.Request.ContentLength = requestBody.Length;

            _context.Features.Set<IRequestBodyPipeFeature>(this);
            _context.Features.Set<IHttpResponseBodyFeature>(this);
            _context.Features.Set<IHttpResponseTrailersFeature>(this);
        }

        public Status GetResponseStatus()
        {
            string status = _context.Response.Headers[ProtocolConstants.HeaderGrpcStatus];
            if (string.IsNullOrEmpty(status))
            {
                return new Status(StatusCode.OK, string.Empty);
            }

            var statusCode = (StatusCode)int.Parse(status, CultureInfo.InvariantCulture);
            var message = _context.Response.Headers[ProtocolConstants.HeaderGrpcMessage];
            return new Status(statusCode, message);
        }

        public async Task<MemoryStream> GetResponseBody()
        {
            await _responseWriter.FlushAsync().ConfigureAwait(false);
            _responseStream.Seek(0, SeekOrigin.Begin);
            return _responseStream;
        }

        public void Dispose()
        {
            _context.Request.Protocol = _originalProtocol;
            _context.Features.Set(_originalRequestBodyPipeFeature);
            _context.Features.Set(_originalResponseBodyFeature);
            _context.Features.Set(_originalResponseTrailersFeature);
        }

        void IHttpResponseBodyFeature.DisableBuffering() => _originalResponseBodyFeature.DisableBuffering();

        Task IHttpResponseBodyFeature.StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        Task IHttpResponseBodyFeature.SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken) => throw new NotSupportedException();

        void IHttpResetFeature.Reset(int errorCode)
        {
            // gRPC DeadlineExceeded, https://github.com/grpc/grpc-dotnet/blob/master/src/Grpc.AspNetCore.Server/Internal/HttpContextServerCallContext.cs
            // will be handled by GetResponseStatus()
        }

        Task IHttpResponseBodyFeature.CompleteAsync()
        {
            // gRPC DeadlineExceeded, https://github.com/grpc/grpc-dotnet/blob/master/src/Grpc.AspNetCore.Server/Internal/HttpContextServerCallContext.cs
            // will be handled by GetResponseStatus()
            return Task.CompletedTask;
        }
    }
}

using System;
using Grpc.Core;

namespace ServiceModel.Grpc.Client
{
    internal readonly struct ServerResponse
    {
        private readonly Status _responseStatus;
        private readonly Metadata _responseTrailers;
        private readonly Func<Status> _getResponseStatus;
        private readonly Func<Metadata> _getResponseTrailers;

        public ServerResponse(Metadata responseHeaders, Status responseStatus, Metadata responseTrailers)
        {
            responseHeaders.AssertNotNull(nameof(responseHeaders));

            ResponseHeaders = responseHeaders;
            _responseStatus = responseStatus;
            _responseTrailers = responseTrailers;

            _getResponseStatus = default;
            _getResponseTrailers = default;
        }

        public ServerResponse(Metadata responseHeaders, Func<Status> getResponseStatus, Func<Metadata> getResponseTrailers)
        {
            responseHeaders.AssertNotNull(nameof(responseHeaders));
            getResponseStatus.AssertNotNull(nameof(getResponseStatus));
            getResponseTrailers.AssertNotNull(nameof(getResponseTrailers));

            ResponseHeaders = responseHeaders;
            _getResponseStatus = getResponseStatus;
            _getResponseTrailers = getResponseTrailers;

            _responseStatus = default;
            _responseTrailers = default;
        }

        public Metadata ResponseHeaders { get; }

        public Status ResponseStatus
        {
            get
            {
                if (_getResponseStatus == null)
                {
                    return _responseStatus;
                }

                return _getResponseStatus();
            }
        }

        public Metadata ResponseTrailers
        {
            get
            {
                if (_getResponseTrailers == null)
                {
                    return _responseTrailers;
                }

                return _getResponseTrailers();
            }
        }
    }
}

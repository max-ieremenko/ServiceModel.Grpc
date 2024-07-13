// <copyright>
// Copyright Max Ieremenko
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

using System.Reflection;

namespace ServiceModel.Grpc.Internal.Descriptors;

internal sealed class OperationDescriptor : IOperationDescriptor
{
    private readonly Func<MethodInfo> _getContractMethod;
    private readonly bool _isAsync;
    private readonly int[] _requestHeaderParameters;
    private readonly int[] _requestParameters;
    private readonly IMessageAccessor _requestAccessor;
    private readonly IStreamAccessor? _requestStreamAccessor;
    private readonly IMessageAccessor _responseAccessor;
    private readonly IStreamAccessor? _responseStreamAccessor;
    private MethodInfo? _contractMethod;

    public OperationDescriptor(
        Func<MethodInfo> getContractMethod,
        bool isAsync,
        int[] requestHeaderParameters,
        int[] requestParameters,
        IMessageAccessor requestAccessor,
        IStreamAccessor? requestStreamAccessor,
        IMessageAccessor responseAccessor,
        IStreamAccessor? responseStreamAccessor)
    {
        _getContractMethod = getContractMethod;
        _isAsync = isAsync;
        _requestHeaderParameters = requestHeaderParameters;
        _requestParameters = requestParameters;
        _requestAccessor = requestAccessor;
        _requestStreamAccessor = requestStreamAccessor;
        _responseAccessor = responseAccessor;
        _responseStreamAccessor = responseStreamAccessor;
    }

    public MethodInfo GetContractMethod()
    {
        if (_contractMethod == null)
        {
            _contractMethod = _getContractMethod();
        }

        return _contractMethod;
    }

    public bool IsAsync() => _isAsync;

    public int[] GetRequestHeaderParameters() => _requestHeaderParameters;

    public int[] GetRequestParameters() => _requestParameters;

    public IMessageAccessor GetRequestAccessor() => _requestAccessor;

    public IStreamAccessor? GetRequestStreamAccessor() => _requestStreamAccessor;

    public IMessageAccessor GetResponseAccessor() => _responseAccessor;

    public IStreamAccessor? GetResponseStreamAccessor() => _responseStreamAccessor;
}
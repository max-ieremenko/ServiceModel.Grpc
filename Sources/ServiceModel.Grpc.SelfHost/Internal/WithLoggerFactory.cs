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
using IGrpcLogger = global::Grpc.Core.Logging.ILogger;

namespace ServiceModel.Grpc.SelfHost.Internal;

internal sealed class WithLoggerFactory<T>
{
    private readonly Func<T> _factory;
    private readonly IGrpcLogger _logger;

    private WithLoggerFactory(Func<T> factory, IGrpcLogger logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public static Func<T> Wrap(Func<T> factory, IGrpcLogger? logger)
    {
        if (logger == null)
        {
            return factory;
        }

        return new WithLoggerFactory<T>(factory, logger).Create;
    }

    public T Create()
    {
        T result;

        try
        {
            result = _factory();
            if (result == null)
            {
                throw new InvalidOperationException("{0} factory return null.".FormatWith(typeof(T).Name));
            }
        }
        catch (Exception ex)
        {
            _logger.Error("{0} factory failed: {1}", typeof(T).FullName, ex);
            throw;
        }

        return result;
    }
}
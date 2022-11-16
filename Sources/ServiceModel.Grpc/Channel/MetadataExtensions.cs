// <copyright>
// Copyright 2020 Max Ieremenko
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
using System.Runtime.CompilerServices;
using Grpc.Core;

namespace ServiceModel.Grpc.Channel;

internal static class MetadataExtensions
{
    public static bool ContainsHeader(this Metadata metadata, Metadata.Entry entry)
    {
        for (var i = 0; i < metadata.Count; i++)
        {
            if (HeadersAreEqual(metadata[i], entry))
            {
                return true;
            }
        }

        return false;
    }

    public static Metadata.Entry? FindHeader(this Metadata? metadata, string key, bool isBinary)
    {
        if (metadata == null)
        {
            return null;
        }

        for (var i = 0; i < metadata.Count; i++)
        {
            var entry = metadata[i];
            if (entry.Equals(key, isBinary))
            {
                return entry;
            }
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(this Metadata.Entry entry, string key, bool isBinary)
    {
        return entry.IsBinary == isBinary && string.Equals(key, entry.Key, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HeadersAreEqual(Metadata.Entry x, Metadata.Entry y)
    {
        if (!string.Equals(x.Key, y.Key, StringComparison.OrdinalIgnoreCase)
            || x.IsBinary != y.IsBinary)
        {
            return false;
        }

        if (x.IsBinary)
        {
            return SequenceEqual(x.ValueBytes, y.ValueBytes);
        }

        return string.Equals(x.Value, y.Value, StringComparison.Ordinal);
    }

    private static bool SequenceEqual(byte[] x, byte[] y)
    {
        if (x.Length != y.Length)
        {
            return false;
        }

        for (var i = 0; i < x.Length; i++)
        {
            if (x[i] != y[i])
            {
                return false;
            }
        }

        return true;
    }
}
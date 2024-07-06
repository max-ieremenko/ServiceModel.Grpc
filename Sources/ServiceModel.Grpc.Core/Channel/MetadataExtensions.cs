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

using Grpc.Core;

namespace ServiceModel.Grpc.Channel;

/// <summary>
/// Provides set of helpers for <see cref="Metadata"/>.
/// </summary>
public static class MetadataExtensions
{
    /// <summary>
    /// Determines whether <see cref="Metadata"/> instance contains the <see cref="Metadata.Entry"/> with the same key and value.
    /// </summary>
    /// <param name="metadata">The <see cref="Metadata"/> instance.</param>
    /// <param name="entry">The <see cref="Metadata.Entry"/> to look for.</param>
    /// <returns>Returns true if the same <see cref="Metadata.Entry"/> found.</returns>
    public static bool ContainsHeader(Metadata metadata, Metadata.Entry entry)
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

    /// <summary>
    /// Try to find the <see cref="Metadata.Entry"/> in the <see cref="Metadata"/> instance with a specific key and type.
    /// </summary>
    /// <param name="metadata">The <see cref="Metadata"/> instance.</param>
    /// <param name="key">The key value.</param>
    /// <param name="isBinary">The type.</param>
    /// <param name="header">Found <see cref="Metadata.Entry"/> instance or null.</param>
    /// <returns>Returns true if the <see cref="Metadata.Entry"/> found.</returns>
    public static bool TryFindHeader(Metadata? metadata, string key, bool isBinary, [NotNullWhen(true)] out Metadata.Entry? header)
    {
        header = null;
        if (metadata == null)
        {
            return false;
        }

        for (var i = 0; i < metadata.Count; i++)
        {
            var entry = metadata[i];
            if (Is(entry, key, isBinary))
            {
                header = entry;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether <see cref="Metadata.Entry"/> instance has the same key and type.
    /// </summary>
    /// <param name="entry">The <see cref="Metadata.Entry"/> instance.</param>
    /// <param name="key">The key value.</param>
    /// <param name="isBinary">The type.</param>
    /// <returns>Returns true if the <see cref="Metadata.Entry"/> has the same key and type.</returns>
    public static bool Is(Metadata.Entry entry, string key, bool isBinary) =>
        entry.IsBinary == isBinary && string.Equals(key, entry.Key, StringComparison.OrdinalIgnoreCase);

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
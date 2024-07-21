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

using System.Runtime.Serialization;

namespace ServiceModel.Grpc.TestApi.Domain;

[DataContract]
public sealed class ExceptionDetail
{
    public ExceptionDetail(Exception error)
    {
        Messages = Expand(error).Select(i => i.Message).ToArray();
        ErrorType = error.GetType().FullName!;
        Stack = error.StackTrace!;
    }

    [DataMember]
    public string ErrorType { get; set; }

    [DataMember]
    public string[] Messages { get; set; }

    [DataMember]
    public string Stack { get; set; }

    private IEnumerable<Exception> Expand(Exception error)
    {
        var ex = error;
        while (ex != null)
        {
            yield return ex;
            ex = ex.InnerException;
        }
    }
}
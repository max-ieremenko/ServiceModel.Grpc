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

namespace ServiceModel.Grpc.TestApi;

public sealed class LoggerMock
{
    public LoggerMock()
    {
        Errors = new List<string>();
        Warnings = new List<string>();
        Debug = new List<string>();

        var logger = new Mock<ILogger>(MockBehavior.Strict);
        logger
            .Setup(l => l.LogError(It.IsNotNull<string>(), It.IsNotNull<object[]>()))
            .Callback<string, object[]>((message, args) => Errors.Add(string.Format(message, args)));
        logger
            .Setup(l => l.LogWarning(It.IsNotNull<string>(), It.IsNotNull<object[]>()))
            .Callback<string, object[]>((message, args) => Warnings.Add(string.Format(message, args)));
        logger
            .Setup(l => l.LogDebug(It.IsNotNull<string>(), It.IsNotNull<object[]>()))
            .Callback<string, object[]>((message, args) => Debug.Add(string.Format(message, args)));

        Logger = logger.Object;
    }

    public ILogger Logger { get; }

    public List<string> Errors { get; }

    public List<string> Warnings { get; }

    public List<string> Debug { get; }

    public void ClearAll()
    {
        Errors.Clear();
        Warnings.Clear();
        Debug.Clear();
    }
}
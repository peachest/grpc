#region Copyright notice and license

// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Utils;
using Grpc.Testing;
using NUnit.Framework;

namespace Grpc.IntegrationTesting
{
    /// <summary>
    /// Runs interop tests in-process.
    /// </summary>
    public class InteropClientServerTest
    {
        const string Host = "localhost";
        Server server;
        Channel channel;
        TestService.TestServiceClient client;

        [OneTimeSetUp]
        public void Init()
        {
            // Disable SO_REUSEPORT to prevent https://github.com/grpc/grpc/issues/10755
            server = new Server(new[] { new ChannelOption(ChannelOptions.SoReuseport, 0) })
            {
                Services = { TestService.BindService(new TestServiceImpl()) },
                Ports = { { Host, ServerPort.PickUnused, TestCredentials.CreateSslServerCredentials() } }
            };
            server.Start();

            var options = new List<ChannelOption>
            {
                new ChannelOption(ChannelOptions.SslTargetNameOverride, TestCredentials.DefaultHostOverride)
            };
            int port = server.Ports.Single().BoundPort;
            channel = new Channel(Host, port, TestCredentials.CreateSslCredentials(), options);
            client = new TestService.TestServiceClient(channel);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            channel.ShutdownAsync().Wait();
            server.ShutdownAsync().Wait();
        }

        [Test]
        public void EmptyUnary()
        {
            InteropClient.RunEmptyUnary(client);
        }

        [Test]
        public void LargeUnary()
        {
            InteropClient.RunLargeUnary(client);
        }

        [Test]
        public async Task ClientStreaming()
        {
            await InteropClient.RunClientStreamingAsync(client);
        }

        [Test]
        public async Task ServerStreaming()
        {
            await InteropClient.RunServerStreamingAsync(client);
        }

        [Test]
        public async Task PingPong()
        {
            await InteropClient.RunPingPongAsync(client);
        }

        [Test]
        public async Task EmptyStream()
        {
            await InteropClient.RunEmptyStreamAsync(client);
        }

        [Test]
        public async Task CancelAfterBegin()
        {
            await InteropClient.RunCancelAfterBeginAsync(client);
        }

        [Test]
        public async Task CancelAfterFirstResponse()
        {
            await InteropClient.RunCancelAfterFirstResponseAsync(client);
        }

        [Test]
        public async Task TimeoutOnSleepingServer()
        {
            await InteropClient.RunTimeoutOnSleepingServerAsync(client);
        }

        [Test]
        public async Task CustomMetadata()
        {
            await InteropClient.RunCustomMetadataAsync(client);
        }

        [Test]
        public async Task StatusCodeAndMessage()
        {
            await InteropClient.RunStatusCodeAndMessageAsync(client);
        }

        [Test]
        public void UnimplementedService()
        {
            InteropClient.RunUnimplementedService(new UnimplementedService.UnimplementedServiceClient(channel));
        }

        [Test]
        public void UnimplementedMethod()
        {
            InteropClient.RunUnimplementedMethod(client);
        }
    }
}

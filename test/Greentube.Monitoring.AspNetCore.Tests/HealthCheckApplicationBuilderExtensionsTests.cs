﻿using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using Xunit;

namespace Greentube.Monitoring.AspNetCore.Tests
{
    public class HealthCheckApplicationBuilderExtensionsTests
    {
        private class Fixture
        {
            private IApplicationBuilder ApplicationBuilder { get; } = Substitute.For<IApplicationBuilder>();
            public IResourceStateCollector ResourceStateCollector { get; } = Substitute.For<IResourceStateCollector>();
            private IServiceProvider ServiceProvider { get; } = Substitute.For<IServiceProvider>();

            public Fixture()
            {
                ServiceProvider.GetService(typeof(IResourceStateCollector)).Returns(ResourceStateCollector);
                ApplicationBuilder.ApplicationServices.Returns(ServiceProvider);
            }

            public IApplicationBuilder GetSut()
            {
                return ApplicationBuilder;
            }
        }

        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void UseHealthCheckEndpoint_HappyPath_StartsCollector()
        {
            var sut = _fixture.GetSut();

            sut.UseHealthCheckEndpoint();

            _fixture.ResourceStateCollector.Received(1).Start();
        }

        [Fact]
        public void UseHealthCheckEndpoint_StoppingApplicationLifetime_StopsCollector()
        {
            var source = new CancellationTokenSource();
            var appLifetime = Substitute.For<IApplicationLifetime>();
            appLifetime.ApplicationStopping.Returns(source.Token);

            var sut = _fixture.GetSut();
            sut.ApplicationServices.GetService(typeof(IApplicationLifetime)).Returns(appLifetime);

            sut.UseHealthCheckEndpoint();
            source.Cancel();

            _fixture.ResourceStateCollector.Received(1).Stop();
        }

        [Fact]
        public void UseHealthCheckEndpoint_NullApplicationBuilder_ThrowsArgumentNull()
        {
            IApplicationBuilder app = null;
            Assert.Throws<ArgumentNullException>(() => app.UseHealthCheckEndpoint());
        }
    }
}
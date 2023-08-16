/*
 * Copyright 2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Monai.Deploy.Common.Miscellaneous;

namespace Monai.Deploy.Common.Miscellaneous.Tests
{
    public class HttpLoggingExtensionsTests
    {
        private readonly Mock<IServiceCollection> _services;

        public HttpLoggingExtensionsTests()
        {
            _services = new Mock<IServiceCollection>();
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        public void GivenConfigurationOptions_WhenAddHttpLoggingForMonaiIsCalled_ExpectCorrectLoggingFieldsToBeSet(bool logRequestBody, bool logResponseBody, bool logRequestQuery)
        {
            var appSettingsStub = new Dictionary<string, string> {
                {"Kestrel:LogHttpRequestBody", logRequestBody.ToString()},
                {"Kestrel:LogHttpResponseBody", logResponseBody.ToString()},
                {"Kestrel:LogHttpRequestQuery", logRequestQuery.ToString()}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(appSettingsStub)
                .Build();

            _services.Object.AddHttpLoggingForMonai(configuration);
            var invocation = _services.Invocations.LastOrDefault();

            Assert.NotNull(invocation);
            Assert.NotEmpty(invocation!.Arguments);
            var serviceDescriptor = invocation.Arguments[0] as ServiceDescriptor;
            Assert.NotNull(serviceDescriptor);
            var options = serviceDescriptor!.ImplementationInstance as ConfigureNamedOptions<HttpLoggingOptions>;
            Assert.NotNull(options);
            var httpOptions = new HttpLoggingOptions();
            options!.Action(httpOptions);

            Assert.True(httpOptions.LoggingFields.HasFlag(HttpLoggingFields.RequestPropertiesAndHeaders));
            Assert.True(httpOptions.LoggingFields.HasFlag(HttpLoggingFields.ResponsePropertiesAndHeaders));

            if (logRequestBody)
            {
                Assert.True(httpOptions.LoggingFields.HasFlag(HttpLoggingFields.RequestBody));
            }
            else
            {
                Assert.False(httpOptions.LoggingFields.HasFlag(HttpLoggingFields.RequestBody));
            }
            if (logResponseBody)
            {
                Assert.True(httpOptions.LoggingFields.HasFlag(HttpLoggingFields.ResponseBody));
            }
            else
            {
                Assert.False(httpOptions.LoggingFields.HasFlag(HttpLoggingFields.ResponseBody));
            }
            if (logRequestQuery)
            {
                Assert.True(httpOptions.LoggingFields.HasFlag(HttpLoggingFields.RequestQuery));
            }
            else
            {
                Assert.False(httpOptions.LoggingFields.HasFlag(HttpLoggingFields.RequestQuery));
            }
        }
    }
}

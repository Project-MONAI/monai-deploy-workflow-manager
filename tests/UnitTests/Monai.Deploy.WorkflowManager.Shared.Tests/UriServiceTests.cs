/*
 * Copyright 2023 MONAI Consortium
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


using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Filter;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Wrappers;

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous.Tests
{
    public class UriServiceTests
    {
        public UriServiceTests()
        {
        }

        [Fact]
        public void Should_Throw_For_Empty_Base()
        {
            Assert.Throws<UriFormatException>(() => new UriService(new Uri("")));
        }

        [Fact]
        public void Should_Create_Usable_Url()
        {
            var route = "/route";
            var pageNum = 3;
            var pageSize = 2;
            var service = new UriService(new Uri("http://localhost"));
            var filter = new PaginationFilter { PageNumber = pageNum, PageSize = pageSize };
            var uri = service.GetPageUriString(filter, route);

            Assert.Contains(route, uri);
            Assert.Contains($"pageNumber={pageNum}", uri);
            Assert.Contains($"pageSize={pageSize}", uri);
        }

        [Fact]
        public void Resonce_Should_Initialise()
        {
            var data = "some data here";
            var testObject = new Response<string>(data);
            Assert.Equal(data, testObject.Data);
            Assert.True(testObject.Succeeded);
            Assert.Equal(Array.Empty<string>(), testObject.Errors);
            Assert.Equal(string.Empty, testObject.Message);
        }
    }
}

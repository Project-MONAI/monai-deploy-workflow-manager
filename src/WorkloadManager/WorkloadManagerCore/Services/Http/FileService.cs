// Copyright 2021 MONAI Consortium
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkloadManager.Contracts.Grpc;

namespace Monai.Deploy.WorkloadManager.Core.Services.Http
{
    public class FileService : File.FileBase, IFileService
    {
        private readonly ILogger<FileService> _logger;

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public override Task<FileUploadResponse> Upload(IAsyncStreamReader<FileUploadRequest> requestStream, ServerCallContext context = default)
        {
            return base.Upload(requestStream, context);
        }

        public override Task Download(FileDownloadRequest request, IServerStreamWriter<FileDownloadResponse> responseStream, ServerCallContext context = default)
        {
            return base.Download(request, responseStream, context);
        }
    }
}

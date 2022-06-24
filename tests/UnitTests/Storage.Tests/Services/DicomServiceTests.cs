// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Monai.Deploy.Storage.API;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.Common;
using Monai.Deploy.WorkflowManager.Storage.Services;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Storage.Tests.Services
{
    public class DicomServiceTests
    {
        private IDicomService DicomService { get; set; }

        private readonly Mock<IStorageService> _storageService;

        private readonly Mock<ILogger<DicomService>> _logger;

        public DicomServiceTests()
        {
            _storageService = new Mock<IStorageService>();
            _logger = new Mock<ILogger<DicomService>>();

            DicomService = new DicomService(_storageService.Object, _logger.Object);
        }

        [Fact]
        public void GetDicomPathsForTask_NullInput_ThrowsException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await DicomService.GetDicomPathsForTask(null, null));
        }

        [Fact]
        public async Task GetDicomPathsForTask_MultipleValidFiles_ReturnsPaths()
        {
            var bucketName = "bucket";
            var outputDir = "output/dir";

            var expected = new List<string>
            {
                "/test/folder/dicom.dcm",
                "/dicom2.dcm"
            };

            var returnedFiles = new List<VirtualFileInfo>
            {
                new VirtualFileInfo("filename", "/test/folder/dicom.dcm", "tag", 500),
                new VirtualFileInfo("filename", "/dicom2.dcm", "tag2", 25),
            };

            _storageService.Setup(s => s.ListObjectsAsync(bucketName, outputDir, true, It.IsAny<CancellationToken>())).ReturnsAsync(returnedFiles);

            var files = await DicomService.GetDicomPathsForTask(outputDir, bucketName);

            files.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetDicomPathsForTask_MultipleNonDicom_ReturnsEmptyList()
        {
            var bucketName = "bucket";
            var outputDir = "output/dir";

            var expected = new List<string>();

            var returnedFiles = new List<VirtualFileInfo>
            {
                new VirtualFileInfo("filename", "/test/folder/dicom.txt", "tag", 500),
                new VirtualFileInfo("filename", "/dicom2.json", "tag2", 25),
            };

            _storageService.Setup(s => s.ListObjectsAsync(bucketName, outputDir, true, It.IsAny<CancellationToken>())).ReturnsAsync(returnedFiles);

            var files = await DicomService.GetDicomPathsForTask(outputDir, bucketName);

            files.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetPayloadPatientDetails_ValidPayloadIdAndBucket_ReturnsValues()
        {
            var bucketName = "bucket";
            var payloadId = Guid.NewGuid().ToString();

            var returnedFiles = new List<VirtualFileInfo>
            {
                new VirtualFileInfo("filename", $"{payloadId}/dcm/folder/dicom.dcm.json", "tag", 500),
                new VirtualFileInfo("filename", $"{payloadId}/dcm/dicom2.dcm", "tag2", 25),
            };

            _storageService.Setup(s => s.ListObjects(bucketName, $"{payloadId}/dcm/", true, It.IsAny<CancellationToken>())).Returns(returnedFiles);

            var files = DicomService.GetPayloadPatientDetails(payloadId, bucketName);

            //files.Should().BeEquivalentTo(expected);
        }
    }
}

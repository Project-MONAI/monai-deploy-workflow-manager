using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
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

        public DicomServiceTests()
        {
            _storageService = new Mock<IStorageService>();

            DicomService = new DicomService(_storageService.Object);
        }

        [Fact]
        public void GetDicomPathsForTask_NullInput_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => DicomService.GetDicomPathsForTask(null, null));
        }

        [Fact]
        public void GetDicomPathsForTask_MultipleValidFiles_ReturnsPaths()
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

            _storageService.Setup(s => s.ListObjects(bucketName, outputDir, true, It.IsAny<CancellationToken>())).Returns(returnedFiles);

            var files = DicomService.GetDicomPathsForTask(outputDir, bucketName);

            files.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetDicomPathsForTask_MultipleNonDicom_ReturnsEmptyList()
        {
            var bucketName = "bucket";
            var outputDir = "output/dir";

            var expected = new List<string>();

            var returnedFiles = new List<VirtualFileInfo>
            {
                new VirtualFileInfo("filename", "/test/folder/dicom.txt", "tag", 500),
                new VirtualFileInfo("filename", "/dicom2.json", "tag2", 25),
            };

            _storageService.Setup(s => s.ListObjects(bucketName, outputDir, true, It.IsAny<CancellationToken>())).Returns(returnedFiles);

            var files = DicomService.GetDicomPathsForTask(outputDir, bucketName);

            files.Should().BeEquivalentTo(expected);
        }
    }
}

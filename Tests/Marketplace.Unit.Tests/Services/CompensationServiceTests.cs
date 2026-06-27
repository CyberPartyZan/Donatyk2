using AutoFixture;
using AutoFixture.AutoMoq;
using Marketplace.BlobStorage;
using Marketplace.Repository;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using System.Security.Claims;

namespace Marketplace.Unit.Tests.Services
{
    public sealed class CompensationServiceTests
    {
        [Fact]
        public async Task GetAll_UsesRepositoryPagination()
        {
            var fixture = CreateFixture();
            var repo = fixture.Freeze<Mock<ICompensationRepository>>();

            var items = new List<CompensationReadModel>
            {
                CreateReadModel("Seller A"),
                CreateReadModel("Seller B")
            };

            repo.Setup(r => r.GetAll(2, 5, CompensationStatus.Pending))
                .ReturnsAsync((items, 12));

            var service = fixture.Create<CompensationService>();

            var result = await service.GetAll(2, 5, CompensationStatus.Pending);

            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(12, result.TotalGroups);
            Assert.Equal(2, result.Items.Count);
            repo.Verify(r => r.GetAll(2, 5, CompensationStatus.Pending), Times.Once);
        }

        [Fact]
        public async Task Update_UpdatesBatch()
        {
            var fixture = CreateFixture();
            var repo = fixture.Freeze<Mock<ICompensationRepository>>();

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            repo.Setup(r => r.Get(id1)).ReturnsAsync(CreateCompensation(id1, CompensationStatus.Pending));
            repo.Setup(r => r.Get(id2)).ReturnsAsync(CreateCompensation(id2, CompensationStatus.Requested));

            var service = fixture.Create<CompensationService>();

            await service.Update(new[] { id1, id2 }, CompensationStatus.Paid);

            repo.Verify(r => r.Update(It.Is<IReadOnlyCollection<Compensation>>(c =>
                c.Count == 2 &&
                c.All(x => x.Status == CompensationStatus.Paid))), Times.Once);
        }

        [Fact]
        public async Task RequestCompensation_MarksPendingAsRequested_AndUpdatesBatch()
        {
            var fixture = CreateFixture();
            var repo = fixture.Freeze<Mock<ICompensationRepository>>();
            var sellerId = Guid.NewGuid();

            var pending = new List<CompensationReadModel>
            {
                CreateReadModel("Seller A", sellerId, CompensationStatus.Pending),
                CreateReadModel("Seller A", sellerId, CompensationStatus.Pending)
            };

            repo.Setup(r => r.GetBySellerId(sellerId, CompensationStatus.Pending))
                .ReturnsAsync(pending);

            var service = fixture.Create<CompensationService>();

            var updated = await service.RequestCompensation(sellerId);

            Assert.Equal(2, updated);
            repo.Verify(r => r.Update(It.Is<IReadOnlyCollection<Compensation>>(c =>
                c.Count == 2 &&
                c.All(x => x.Status == CompensationStatus.Requested))), Times.Once);
        }

        [Fact]
        public async Task GetApprovementDocumentUrl_ReturnsUrl_ForOwner()
        {
            var compensationId = Guid.NewGuid();
            var ownerUserId = Guid.NewGuid();
            var repo = new Mock<ICompensationRepository>();
            var blobStorage = new Mock<IBlobStorageService>();

            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(JwtRegisteredClaimNames.Sub, ownerUserId.ToString())],
                "test"));

            repo.Setup(r => r.GetReadModel(compensationId)).ReturnsAsync(new CompensationReadModel
            {
                Id = compensationId,
                LotId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Amount = new Money(10m, Currency.USD),
                Status = CompensationStatus.Paid,
                SellerId = Guid.NewGuid(),
                SellerUserId = ownerUserId,
                SellerName = "Seller",
                ApprovementDocument = new BlobDto
                {
                    Id = Guid.NewGuid(),
                    FilePath = "compensations/approvals",
                    Key = "abc123"
                }
            });

            blobStorage.Setup(x => x.GetPresignedGetUrlAsync("abc123", "compensations/approvals", 600))
                .ReturnsAsync("https://minio/presigned");

            var service = new CompensationService(repo.Object, principal, blobStorage.Object);

            var url = await service.GetApprovementDocumentUrl(compensationId);

            Assert.Equal("https://minio/presigned", url);
        }

        [Fact]
        public async Task GetApprovementDocumentUrl_Throws_ForNonOwnerNonAdmin()
        {
            var compensationId = Guid.NewGuid();
            var repo = new Mock<ICompensationRepository>();
            var blobStorage = new Mock<IBlobStorageService>();

            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())],
                "test"));

            repo.Setup(r => r.GetReadModel(compensationId)).ReturnsAsync(new CompensationReadModel
            {
                Id = compensationId,
                LotId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Amount = new Money(10m, Currency.USD),
                Status = CompensationStatus.Paid,
                SellerId = Guid.NewGuid(),
                SellerUserId = Guid.NewGuid(),
                SellerName = "Seller",
                ApprovementDocument = new BlobDto
                {
                    Id = Guid.NewGuid(),
                    FilePath = "compensations/approvals",
                    Key = "abc123"
                }
            });

            var service = new CompensationService(repo.Object, principal, blobStorage.Object);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.GetApprovementDocumentUrl(compensationId));

            blobStorage.Verify(x => x.GetPresignedGetUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Process_UploadsApprovementDocument_AndProcessesBatch()
        {
            var fixture = CreateFixture();
            var repo = fixture.Freeze<Mock<ICompensationRepository>>();
            var blobStorage = fixture.Freeze<Mock<IBlobStorageService>>();

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            blobStorage.Setup(x => x.UploadAsync(It.IsAny<Stream>(), "compensations/approvals"))
                .ReturnsAsync("blob-key");

            repo.Setup(x => x.Process(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<Blob>()))
                .ReturnsAsync(2);

            var service = fixture.Create<CompensationService>();
            using var stream = new MemoryStream([1, 2, 3]);

            var updated = await service.Process([id1, id2], stream, "approval.pdf");

            Assert.Equal(2, updated);

            repo.Verify(x => x.Process(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2 && ids.Contains(id1) && ids.Contains(id2)),
                It.Is<Blob>(b =>
                    b.FilePath == "compensations/approvals" &&
                    b.Key == "blob-key" &&
                    b.FileName == "approval.pdf")),
                Times.Once);
        }

        [Fact]
        public async Task Process_DeletesUploadedBlob_WhenRepositoryFails()
        {
            var fixture = CreateFixture();
            var repo = fixture.Freeze<Mock<ICompensationRepository>>();
            var blobStorage = fixture.Freeze<Mock<IBlobStorageService>>();

            blobStorage.Setup(x => x.UploadAsync(It.IsAny<Stream>(), "compensations/approvals"))
                .ReturnsAsync("blob-key");

            repo.Setup(x => x.Process(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<Blob>()))
                .ThrowsAsync(new InvalidOperationException("No compensations are eligible for processing."));

            var service = fixture.Create<CompensationService>();
            using var stream = new MemoryStream([1, 2, 3]);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.Process([Guid.NewGuid()], stream, "approval.pdf"));

            blobStorage.Verify(x => x.DeleteAsync("blob-key", "compensations/approvals"), Times.Once);
        }

        private static IFixture CreateFixture() =>
            new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        private static Compensation CreateCompensation(Guid id, CompensationStatus status) =>
            new(id, Guid.NewGuid(), Guid.NewGuid(), new Money(10m, Currency.USD), status);

        private static CompensationReadModel CreateReadModel(
            string sellerName,
            Guid? sellerId = null,
            CompensationStatus status = CompensationStatus.Pending) =>
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                LotId = Guid.NewGuid(),
                Amount = new Money(10m, Currency.USD),
                Status = status,
                SellerId = sellerId ?? Guid.NewGuid(),
                SellerUserId = Guid.NewGuid(),
                SellerName = sellerName,
                SoldPrice = 100m,
                SoldDate = DateTime.UtcNow,
                BuyerName = "buyer@test.local",
                LotName = "Lot #1",
                LotImage = "lots/img/key"
            };
    }
}
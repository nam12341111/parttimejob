using FluentAssertions;
using Moq;
using PTJ.Application.DTOs.Application;
using PTJ.Application.Services;
using PTJ.Domain.Entities;
using PTJ.Domain.Interfaces;
using PTJ.Infrastructure.Services;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PTJ.Tests.Services;

public class ApplicationServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ApplicationService _applicationService;

    public ApplicationServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _applicationService = new ApplicationService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnSuccess_WhenJobExistsAndUserHasProfile()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateApplicationDto { JobPostId = 1, CoverLetter = "Hello" };

        // Setup: Profile exists
        _mockUnitOfWork.Setup(u => u.Profiles.FirstOrDefaultAsync(It.IsAny<Expression<Func<Profile, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Profile { Id = 10, UserId = 1 });

        // Setup: Job exists and is active
        _mockUnitOfWork.Setup(u => u.JobPosts.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JobPost { Id = 1, Status = PTJ.Domain.Enums.JobPostStatus.Active });

        // Setup: No existing application
        _mockUnitOfWork.Setup(u => u.Applications.FirstOrDefaultAsync(It.IsAny<Expression<Func<PTJ.Domain.Entities.Application, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PTJ.Domain.Entities.Application?)null);

        // Setup: Pending status exists
        _mockUnitOfWork.Setup(u => u.ApplicationStatuses.FirstOrDefaultAsync(It.IsAny<Expression<Func<ApplicationStatusLookup, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationStatusLookup { Id = 1, Name = "Pending" });
            
        // Setup: Add Application
        _mockUnitOfWork.Setup(u => u.Applications.AddAsync(It.IsAny<PTJ.Domain.Entities.Application>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PTJ.Domain.Entities.Application());
            
        // Setup: Add ApplicationHistory
        _mockUnitOfWork.Setup(u => u.ApplicationHistories.AddAsync(It.IsAny<ApplicationHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationHistory());

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _applicationService.CreateAsync(userId, dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Application submitted successfully");
    }

    [Fact]
    public async Task Create_ShouldFail_WhenJobNotFound()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateApplicationDto { JobPostId = 99 };

        _mockUnitOfWork.Setup(u => u.Profiles.FirstOrDefaultAsync(It.IsAny<Expression<Func<Profile, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Profile { Id = 10 });

        _mockUnitOfWork.Setup(u => u.JobPosts.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JobPost?)null);

        // Act
        var result = await _applicationService.CreateAsync(userId, dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Job post not found");
    }
}

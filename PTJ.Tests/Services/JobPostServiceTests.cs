using FluentAssertions;
using Moq;
using PTJ.Application.DTOs.JobPost;
using PTJ.Application.Services;
using PTJ.Domain.Entities;
using PTJ.Domain.Interfaces;
using PTJ.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PTJ.Tests.Services;

public class JobPostServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly JobPostService _jobPostService;

    public JobPostServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _jobPostService = new JobPostService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateJobPostDto
        {
            Title = "Backend Developer",
            Description = "Description",
            SalaryMin = 1000,
            Location = "HCM",
            RequiredSkills = new List<string> { "C#", "SQL" },
            Shifts = new List<CreateJobShiftDto>()
        };

        // Setup: Company owner check
        var company = new Company { Id = 1, OwnerId = 1 };
        _mockUnitOfWork.Setup(u => u.Companies.FirstOrDefaultAsync(It.IsAny<Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        // Setup: Repositories
        _mockUnitOfWork.Setup(u => u.JobPosts.AddAsync(It.IsAny<JobPost>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JobPost { Id = 100 }); // Return entity with ID
            
        _mockUnitOfWork.Setup(u => u.JobPostSkills.AddAsync(It.IsAny<JobPostSkill>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JobPostSkill());

        _mockUnitOfWork.Setup(u => u.JobShifts.AddAsync(It.IsAny<JobShift>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JobShift());

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Setup: FindAsync calls at the end of CreateAsync
        _mockUnitOfWork.Setup(u => u.JobShifts.FindAsync(It.IsAny<Expression<Func<JobShift, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<JobShift>());
            
        _mockUnitOfWork.Setup(u => u.JobPostSkills.FindAsync(It.IsAny<Expression<Func<JobPostSkill, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<JobPostSkill>());

        // Act
        var result = await _jobPostService.CreateAsync(userId, dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Job post created successfully");
        _mockUnitOfWork.Verify(u => u.JobPosts.AddAsync(It.IsAny<JobPost>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldFail_WhenUserHasNoCompany()
    {
        // Arrange
        var userId = 99;
        var dto = new CreateJobPostDto();

        _mockUnitOfWork.Setup(u => u.Companies.FirstOrDefaultAsync(It.IsAny<Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        // Act
        var result = await _jobPostService.CreateAsync(userId, dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("You must have a company");
    }
}

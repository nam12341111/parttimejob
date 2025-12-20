using FluentAssertions;
using Moq;
using PTJ.Application.DTOs.Auth;
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

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockJwtService = new Mock<IJwtService>();
        _authService = new AuthService(_mockUnitOfWork.Object, _mockJwtService.Object);
    }

    [Fact]
    public async Task Register_ShouldReturnSuccess_WhenEmailIsUnique()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FullName = "Test User"
        };

        // Setup: Email does not exist
        _mockUnitOfWork.Setup(u => u.Users.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Setup: Role exists
        _mockUnitOfWork.Setup(u => u.Roles.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = 1, Name = "STUDENT" });

        // Setup: Repositories
        _mockUnitOfWork.Setup(u => u.Users.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User());
        _mockUnitOfWork.Setup(u => u.UserRoles.AddAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserRole());
        _mockUnitOfWork.Setup(u => u.Profiles.AddAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Profile());
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Registration successful");
    }

    [Fact]
    public async Task Register_ShouldReturnFailure_WhenEmailExists()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Setup: Email exists
        _mockUnitOfWork.Setup(u => u.Users.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "existing@example.com" });

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Email already exists");
    }

    [Fact]
    public async Task Login_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var dto = new LoginDto { Email = "nonexistent@example.com", Password = "password" };

        _mockUnitOfWork.Setup(u => u.Users.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(dto, "127.0.0.1");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid email or password");
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FintechPSP.UserService.Controllers;
using FintechPSP.UserService.Models;
using FintechPSP.UserService.Repositories;
using FintechPSP.UserService.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintechPSP.UserService.Tests
{
    public class AdminUserControllerTests
    {
        private readonly Mock<ISystemUserRepository> _mockRepository;
        private readonly Mock<ILogger<AdminUserController>> _mockLogger;
        private readonly AdminUserController _controller;

        public AdminUserControllerTests()
        {
            _mockRepository = new Mock<ISystemUserRepository>();
            _mockLogger = new Mock<ILogger<AdminUserController>>();
            _controller = new AdminUserController(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetUsers_ReturnsOkResult_WithUsersList()
        {
            // Arrange
            var users = new List<SystemUser>
            {
                new SystemUser
                {
                    Id = Guid.NewGuid(),
                    Email = "cliente1@test.com",
                    Name = "Cliente 1",
                    Role = "cliente",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow.AddDays(-1)
                },
                new SystemUser
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@test.com",
                    Name = "Admin User",
                    Role = "admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow
                }
            };

            _mockRepository.Setup(r => r.GetUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                          .ReturnsAsync((users, users.Count));

            // Act
            var result = await _controller.GetUsers(1, 10, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task CreateUser_ValidData_ReturnsCreatedResult()
        {
            // Arrange
            var createRequest = new CreateUserRequest
            {
                Email = "novo@test.com",
                Name = "Novo Cliente",
                Role = "cliente",
                Document = "12345678901",
                Phone = "11999887766"
            };

            var createdUser = new SystemUser
            {
                Id = Guid.NewGuid(),
                Email = createRequest.Email,
                Name = createRequest.Name,
                Role = createRequest.Role,
                Document = createRequest.Document,
                Phone = createRequest.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.CreateUserAsync(It.IsAny<SystemUser>()))
                          .ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(createRequest);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var user = Assert.IsType<SystemUser>(createdResult.Value);
            Assert.Equal(createRequest.Email, user.Email);
            Assert.Equal(createRequest.Name, user.Name);
            Assert.Equal(createRequest.Role, user.Role);
        }

        [Fact]
        public async Task CreateUser_InvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var createRequest = new CreateUserRequest
            {
                Email = "invalid-email",
                Name = "Test User",
                Role = "cliente"
            };

            // Act
            var result = await _controller.CreateUser(createRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task CreateUser_DuplicateEmail_ReturnsConflict()
        {
            // Arrange
            var createRequest = new CreateUserRequest
            {
                Email = "existing@test.com",
                Name = "Test User",
                Role = "cliente"
            };

            _mockRepository.Setup(r => r.CreateUserAsync(It.IsAny<SystemUser>()))
                          .ThrowsAsync(new InvalidOperationException("Email já existe"));

            // Act
            var result = await _controller.CreateUser(createRequest);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.NotNull(conflictResult.Value);
        }

        [Fact]
        public async Task UpdateUser_ValidData_ReturnsOkResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateRequest = new UpdateUserRequest
            {
                Email = "updated@test.com",
                Name = "Updated Name",
                Role = "admin",
                IsActive = true
            };

            var existingUser = new SystemUser
            {
                Id = userId,
                Email = "old@test.com",
                Name = "Old Name",
                Role = "cliente",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var updatedUser = new SystemUser
            {
                Id = userId,
                Email = updateRequest.Email,
                Name = updateRequest.Name,
                Role = updateRequest.Role,
                IsActive = updateRequest.IsActive,
                CreatedAt = existingUser.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetUserByIdAsync(userId))
                          .ReturnsAsync(existingUser);
            _mockRepository.Setup(r => r.UpdateUserAsync(It.IsAny<SystemUser>()))
                          .ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.UpdateUser(userId, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var user = Assert.IsType<SystemUser>(okResult.Value);
            Assert.Equal(updateRequest.Email, user.Email);
            Assert.Equal(updateRequest.Name, user.Name);
            Assert.Equal(updateRequest.Role, user.Role);
        }

        [Fact]
        public async Task UpdateUser_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateRequest = new UpdateUserRequest
            {
                Email = "updated@test.com",
                Name = "Updated Name"
            };

            _mockRepository.Setup(r => r.GetUserByIdAsync(userId))
                          .ReturnsAsync((SystemUser)null);

            // Act
            var result = await _controller.UpdateUser(userId, updateRequest);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ExistingUser_ReturnsNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new SystemUser
            {
                Id = userId,
                Email = "test@test.com",
                Name = "Test User",
                Role = "cliente",
                IsActive = true
            };

            _mockRepository.Setup(r => r.GetUserByIdAsync(userId))
                          .ReturnsAsync(existingUser);
            _mockRepository.Setup(r => r.DeleteUserAsync(userId))
                          .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockRepository.Setup(r => r.GetUserByIdAsync(userId))
                          .ReturnsAsync((SystemUser)null);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetUsers_WithFilters_ReturnsFilteredResults()
        {
            // Arrange
            var filteredUsers = new List<SystemUser>
            {
                new SystemUser
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@test.com",
                    Name = "Admin User",
                    Role = "admin",
                    IsActive = true
                }
            };

            _mockRepository.Setup(r => r.GetUsersAsync(1, 10, "admin", "admin"))
                          .ReturnsAsync((filteredUsers, 1));

            // Act
            var result = await _controller.GetUsers(1, 10, "admin", "admin");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            _mockRepository.Verify(r => r.GetUsersAsync(1, 10, "admin", "admin"), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task CreateUser_EmptyEmail_ReturnsBadRequest(string email)
        {
            // Arrange
            var createRequest = new CreateUserRequest
            {
                Email = email,
                Name = "Test User",
                Role = "cliente"
            };

            // Act
            var result = await _controller.CreateUser(createRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("cliente")]
        [InlineData("admin")]
        [InlineData("sub-admin")]
        public async Task CreateUser_ValidRoles_ReturnsCreated(string role)
        {
            // Arrange
            var createRequest = new CreateUserRequest
            {
                Email = "test@test.com",
                Name = "Test User",
                Role = role
            };

            var createdUser = new SystemUser
            {
                Id = Guid.NewGuid(),
                Email = createRequest.Email,
                Name = createRequest.Name,
                Role = createRequest.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.CreateUserAsync(It.IsAny<SystemUser>()))
                          .ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(createRequest);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }
    }
}

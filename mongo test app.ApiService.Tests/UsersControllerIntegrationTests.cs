using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MongoTest.ApiService.Controllers;
using MongoTest.ApiService.Models;
using Xunit;

namespace MongoTest.ApiService.Tests;

public class UsersControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UsersControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllUsers_WhenEmpty_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        users.Should().NotBeNull();
        users.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateUser_ReturnsCreatedUser()
    {
        // Arrange
        var request = new CreateUserRequest("jdoe", "John", "Doe");

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.Content.ReadFromJsonAsync<User>();
        user.Should().NotBeNull();
        user!.Id.Should().NotBeNullOrEmpty();
        user.Username.Should().Be("jdoe");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task CreateUser_ThenGetAll_ReturnsTheCreatedUser()
    {
        // Arrange
        var request = new CreateUserRequest("asmith", "Alice", "Smith");
        var createResponse = await _client.PostAsJsonAsync("/api/users", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<User>();

        // Act
        var getResponse = await _client.GetAsync("/api/users");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await getResponse.Content.ReadFromJsonAsync<List<User>>();
        users.Should().ContainSingle(u => u.Id == created!.Id);
    }

    [Fact]
    public async Task GetUserById_WhenExists_ReturnsUser()
    {
        // Arrange
        var request = new CreateUserRequest("bwayne", "Bruce", "Wayne");
        var createResponse = await _client.PostAsJsonAsync("/api/users", request);
        var created = await createResponse.Content.ReadFromJsonAsync<User>();

        // Act
        var response = await _client.GetAsync($"/api/users/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<User>();
        user.Should().NotBeNull();
        user!.Username.Should().Be("bwayne");
    }

    [Fact]
    public async Task GetUserById_WhenDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/users/000000000000000000000000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUser_ExistingUser_UpdatesFields()
    {
        // Arrange
        var createRequest = new CreateUserRequest("kkent", "Clark", "Kent");
        var createResponse = await _client.PostAsJsonAsync("/api/users", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<User>();

        var updateRequest = new UpdateUserRequest("superman", "Clark", "Kent");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<User>();
        updated.Should().NotBeNull();
        updated!.Username.Should().Be("superman");
    }

    [Fact]
    public async Task UpdateUser_WhenDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new UpdateUserRequest("nobody", "No", "Body");

        // Act
        var response = await _client.PutAsJsonAsync("/api/users/000000000000000000000000", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_RemovesIt()
    {
        // Arrange
        var request = new CreateUserRequest("dprin", "Diana", "Prince");
        var createResponse = await _client.PostAsJsonAsync("/api/users", request);
        var created = await createResponse.Content.ReadFromJsonAsync<User>();

        // Act - Delete
        var deleteResponse = await _client.DeleteAsync($"/api/users/{created!.Id}");

        // Assert - Delete succeeded
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - Get after delete
        var getResponse = await _client.GetAsync($"/api/users/{created.Id}");

        // Assert - User gone
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_WhenDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/users/000000000000000000000000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
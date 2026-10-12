using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoTest.ApiService.Models;

namespace MongoTest.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMongoCollection<User> _users;

    public UsersController(IMongoDatabase database)
    {
        _users = database.GetCollection<User>("Users");
    }

    /// <summary>
    /// Get all users.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<User>>> GetAll()
    {
        var users = await _users.Find(_ => true).ToListAsync();
        return Ok(users);
    }

    /// <summary>
    /// Get a user by ID.
    /// </summary>
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<User>> GetById(string id)
    {
        var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user is null)
            return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<User>> Create([FromBody] CreateUserRequest request)
    {
        var user = new User
        {
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        await _users.InsertOneAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>
    /// Update an existing user.
    /// </summary>
    [HttpPut("{id:length(24)}")]
    public async Task<ActionResult<User>> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var update = Builders<User>.Update
            .Set(u => u.Username, request.Username)
            .Set(u => u.FirstName, request.FirstName)
            .Set(u => u.LastName, request.LastName);

        var result = await _users.UpdateOneAsync(u => u.Id == id, update);

        if (result.MatchedCount == 0)
            return NotFound();

        var updatedUser = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        return Ok(updatedUser);
    }

    /// <summary>
    /// Delete a user.
    /// </summary>
    [HttpDelete("{id:length(24)}")]
    public async Task<ActionResult> Delete(string id)
    {
        var result = await _users.DeleteOneAsync(u => u.Id == id);
        if (result.DeletedCount == 0)
            return NotFound();
        return NoContent();
    }
}

// Request DTOs

public record CreateUserRequest(string Username, string FirstName, string LastName);
public record UpdateUserRequest(string Username, string FirstName, string LastName);
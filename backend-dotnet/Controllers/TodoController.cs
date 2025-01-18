using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly TodoService _todoService;
    private readonly ILogger<TodosController> _logger;

    public TodosController(TodoService todoService, ILogger<TodosController> logger)
    {
        _todoService = todoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Todo>>> Get()
    {
        try
        {
            _logger.LogInformation("Getting all todos");
            var todos = await _todoService.GetAsync();
            _logger.LogInformation("Successfully retrieved {Count} todos", todos.Count);
            return Ok(todos);
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "MongoDB error while getting todos");
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting todos");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Todo>> Create(CreateTodoDto todoDto)
    {
        try
        {
            _logger.LogInformation("Creating new todo with title: {Title}", todoDto.Title);
            var todo = await _todoService.CreateAsync(todoDto);
            _logger.LogInformation("Successfully created todo with ID: {Id}", todo.Id);
            return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating todo");
            return StatusCode(500, new { error = "Failed to create todo", message = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Todo>> Update(string id, UpdateTodoDto todoDto)
    {
        try
        {
            _logger.LogInformation("Updating todo {Id}", id);
            var todo = await _todoService.UpdateAsync(id, todoDto);

            if (todo == null)
            {
                _logger.LogWarning("Todo {Id} not found", id);
                return NotFound(new { error = "Todo not found" });
            }

            _logger.LogInformation("Successfully updated todo {Id}", id);
            return Ok(todo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating todo {Id}", id);
            return StatusCode(500, new { error = "Failed to update todo", message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            _logger.LogInformation("Deleting todo {Id}", id);
            var result = await _todoService.DeleteAsync(id);

            if (!result)
            {
                _logger.LogWarning("Todo {Id} not found for deletion", id);
                return NotFound(new { error = "Todo not found" });
            }

            _logger.LogInformation("Successfully deleted todo {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting todo {Id}", id);
            return StatusCode(500, new { error = "Failed to delete todo", message = ex.Message });
        }
    }

    [HttpGet("health")]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            var todos = await _todoService.GetAsync();
            return Ok(new { 
                status = "healthy", 
                message = "Successfully connected to MongoDB",
                todoCount = todos.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(500, new { 
                status = "unhealthy", 
                message = ex.Message,
                details = ex.ToString()
            });
        }
    }
} 
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TodoApi.Models;
using Microsoft.Extensions.Logging;

namespace TodoApi.Services;

public class TodoService
{
    private readonly IMongoCollection<Todo> _todosCollection;
    private readonly ILogger<TodoService> _logger;

    public TodoService(IConfiguration configuration, ILogger<TodoService> logger)
    {
        _logger = logger;
        try
        {
            var mongoConnectionString = configuration["MONGODB_URI"];
            _logger.LogInformation("Attempting to connect with connection string: {ConnectionString}", 
                mongoConnectionString ?? "null");

            if (string.IsNullOrEmpty(mongoConnectionString))
            {
                mongoConnectionString = "mongodb://mongodb:27017/todo_db";
                _logger.LogWarning("No connection string found in configuration, using default: {DefaultConnection}", 
                    mongoConnectionString);
            }

            var mongoUrl = new MongoUrl(mongoConnectionString);
            var databaseName = mongoUrl.DatabaseName ?? "todo_db";
            
            _logger.LogInformation("Connecting to MongoDB at {Server} with database {Database}", 
                mongoUrl.Server, databaseName);
            
            var settings = MongoClientSettings.FromUrl(mongoUrl);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
            settings.ConnectTimeout = TimeSpan.FromSeconds(30);
            settings.RetryWrites = true;
            settings.RetryReads = true;

            var mongoClient = new MongoClient(settings);
            
            // Wait for MongoDB to be ready
            WaitForMongoDB(mongoClient).GetAwaiter().GetResult();
            
            var mongoDatabase = mongoClient.GetDatabase(databaseName);
            _todosCollection = mongoDatabase.GetCollection<Todo>("todos");
            
            _logger.LogInformation("Successfully connected to MongoDB");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MongoDB connection");
            throw;
        }
    }

    private async Task WaitForMongoDB(IMongoClient client)
    {
        var retryCount = 0;
        var maxRetries = 5;
        var delay = TimeSpan.FromSeconds(5);

        while (retryCount < maxRetries)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to MongoDB (Attempt {Attempt}/{MaxAttempts})", 
                    retryCount + 1, maxRetries);
                
                // Test the connection
                await client.ListDatabaseNamesAsync();
                _logger.LogInformation("Successfully connected to MongoDB");
                return;
            }
            catch (Exception ex)
            {
                retryCount++;
                if (retryCount == maxRetries)
                {
                    _logger.LogError(ex, "Failed to connect to MongoDB after {MaxAttempts} attempts", maxRetries);
                    throw;
                }

                _logger.LogWarning(ex, "Failed to connect to MongoDB. Retrying in {Delay} seconds...", delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }
    }

    public async Task<List<Todo>> GetAsync()
    {
        try
        {
            _logger.LogInformation("Attempting to fetch todos");
            var todos = await _todosCollection.Find(_ => true)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
            _logger.LogInformation("Successfully fetched {Count} todos", todos.Count);
            return todos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching todos");
            throw;
        }
    }

    public async Task<Todo> CreateAsync(CreateTodoDto todoDto)
    {
        var todo = new Todo
        {
            Title = todoDto.Title,
            Completed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _todosCollection.InsertOneAsync(todo);
        return todo;
    }

    public async Task<Todo?> UpdateAsync(string id, UpdateTodoDto todoDto)
    {
        var update = Builders<Todo>.Update;
        var updates = new List<UpdateDefinition<Todo>>();

        if (todoDto.Title != null)
            updates.Add(update.Set(x => x.Title, todoDto.Title));
        
        if (todoDto.Completed.HasValue)
            updates.Add(update.Set(x => x.Completed, todoDto.Completed.Value));

        if (!updates.Any())
            return null;

        var combinedUpdate = update.Combine(updates);
        var options = new FindOneAndUpdateOptions<Todo>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await _todosCollection.FindOneAndUpdateAsync<Todo>(
            x => x.Id == id,
            combinedUpdate,
            options
        );
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _todosCollection.DeleteOneAsync(x => x.Id == id);
        return result.DeletedCount > 0;
    }
} 
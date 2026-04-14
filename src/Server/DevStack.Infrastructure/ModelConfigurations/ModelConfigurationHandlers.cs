using DevStack.Domain.Entities;
using DevStack.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace DevStack.Infrastructure.ModelConfigurations;

public record CreateModelConfigurationCommand(
    Guid ProjectId,
    string Url,
    string Model,
    string? ModelAlias,
    string ApiKey,
    int MaxComplexity);

public record UpdateModelConfigurationCommand(
    Guid Id,
    string? Url,
    string? Model,
    string? ModelAlias,
    string? ApiKey,
    int? MaxComplexity);

public record DeleteModelConfigurationCommand(Guid Id);

public interface ICreateModelConfigurationHandler : DevStack.Application.ICommandHandler<Guid, CreateModelConfigurationCommand>
{
}

public interface IUpdateModelConfigurationHandler : DevStack.Application.ICommandHandler<UpdateModelConfigurationCommand>
{
}

public interface IDeleteModelConfigurationHandler : DevStack.Application.ICommandHandler<DeleteModelConfigurationCommand>
{
}

public class CreateModelConfigurationHandler : ICreateModelConfigurationHandler
{
    private readonly DevStackDbContext _dbContext;


    public CreateModelConfigurationHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task<Guid> Handle(CreateModelConfigurationCommand request, CancellationToken cancellationToken)
    {
        var config = new ModelConfiguration
        {
            ProjectId = request.ProjectId,
            Url = request.Url,
            Model = request.Model,
            ModelAlias = request.ModelAlias,
            ApiKey_Encrypted = request.ApiKey,
            MaxComplexity = request.MaxComplexity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.ModelConfigurations.Add(config);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return config.Id;
    }
}

public class UpdateModelConfigurationHandler : IUpdateModelConfigurationHandler
{
    private readonly DevStackDbContext _dbContext;

    public UpdateModelConfigurationHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(UpdateModelConfigurationCommand request, CancellationToken cancellationToken)
    {
        var config = await _dbContext.ModelConfigurations.FindAsync([request.Id], cancellationToken);
        if (config == null)
            throw new InvalidOperationException($"ModelConfiguration with ID {request.Id} not found.");

        if (!string.IsNullOrEmpty(request.Url)) config.Url = request.Url;
        if (!string.IsNullOrEmpty(request.Model)) config.Model = request.Model;
        if (request.ModelAlias is not null) config.ModelAlias = request.ModelAlias;
        if (request.ApiKey is not null) config.ApiKey_Encrypted = request.ApiKey;
        if (request.MaxComplexity.HasValue) config.MaxComplexity = request.MaxComplexity.Value;

        config.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteModelConfigurationHandler : IDeleteModelConfigurationHandler
{
    private readonly DevStackDbContext _dbContext;

    public DeleteModelConfigurationHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(DeleteModelConfigurationCommand request, CancellationToken cancellationToken)
    {
        var config = await _dbContext.ModelConfigurations.FindAsync([request.Id], cancellationToken);
        if (config == null)
            throw new InvalidOperationException($"ModelConfiguration with ID {request.Id} not found.");

        _dbContext.ModelConfigurations.Remove(config);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
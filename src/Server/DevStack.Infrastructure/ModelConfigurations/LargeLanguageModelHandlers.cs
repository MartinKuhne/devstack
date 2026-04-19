using DevStack.Domain.Entities;
using DevStack.Persistence;
using System.Threading.Tasks;

namespace DevStack.Infrastructure.ModelConfigurations;

public record CreateLargeLanguageModelCommand(
    string Url,
    string Model,
    string? ModelAlias,
    string ApiKey,
    int MaxComplexity,
    int MaxConcurrency);

public record UpdateLargeLanguageModelCommand(
    Guid Id,
    string? Url,
    string? Model,
    string? ModelAlias,
    string? ApiKey,
    int? MaxComplexity,
    int? MaxConcurrency);

public record DeleteLargeLanguageModelCommand(Guid Id);

public interface ICreateLargeLanguageModelHandler : DevStack.Application.ICommandHandler<Guid, CreateLargeLanguageModelCommand>
{
}

public interface IUpdateLargeLanguageModelHandler : DevStack.Application.ICommandHandler<UpdateLargeLanguageModelCommand>
{
}

public interface IDeleteLargeLanguageModelHandler : DevStack.Application.ICommandHandler<DeleteLargeLanguageModelCommand>
{
}

public class CreateLargeLanguageModelHandler : ICreateLargeLanguageModelHandler
{
    private readonly DevStackDbContext _dbContext;


    public CreateLargeLanguageModelHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task<Guid> Handle(CreateLargeLanguageModelCommand request, CancellationToken cancellationToken)
    {
        var model = new LargeLanguageModel
        {
            Url = request.Url,
            Model = request.Model,
            ModelAlias = request.ModelAlias,
            ApiKey = request.ApiKey,
            MaxComplexity = request.MaxComplexity,
            MaxConcurrency = request.MaxConcurrency
        };

        _dbContext.LargeLanguageModels.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return model.Id;
    }
}

public class UpdateLargeLanguageModelHandler : IUpdateLargeLanguageModelHandler
{
    private readonly DevStackDbContext _dbContext;

    public UpdateLargeLanguageModelHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(UpdateLargeLanguageModelCommand request, CancellationToken cancellationToken)
    {
        var model = await _dbContext.LargeLanguageModels.FindAsync([request.Id], cancellationToken);
        if (model == null)
            throw new InvalidOperationException($"LargeLanguageModel with ID {request.Id} not found.");

        if (!string.IsNullOrEmpty(request.Url)) model.Url = request.Url;
        if (!string.IsNullOrEmpty(request.Model)) model.Model = request.Model;
        if (request.ModelAlias is not null) model.ModelAlias = request.ModelAlias;
        if (request.ApiKey is not null) model.ApiKey = request.ApiKey;
        if (request.MaxComplexity.HasValue) model.MaxComplexity = request.MaxComplexity.Value;
        if (request.MaxConcurrency.HasValue) model.MaxConcurrency = request.MaxConcurrency.Value;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteLargeLanguageModelHandler : IDeleteLargeLanguageModelHandler
{
    private readonly DevStackDbContext _dbContext;

    public DeleteLargeLanguageModelHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(DeleteLargeLanguageModelCommand request, CancellationToken cancellationToken)
    {
        var model = await _dbContext.LargeLanguageModels.FindAsync([request.Id], cancellationToken);
        if (model == null)
            throw new InvalidOperationException($"LargeLanguageModel with ID {request.Id} not found.");

        _dbContext.LargeLanguageModels.Remove(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

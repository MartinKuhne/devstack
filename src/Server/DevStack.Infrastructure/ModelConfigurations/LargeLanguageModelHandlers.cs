using DevStack.Application;
using DevStack.Application.LargeLanguageModels.Commands;
using DevStack.Domain.Entities;
using DevStack.Persistence;

namespace DevStack.Infrastructure.ModelConfigurations;

public class CreateLargeLanguageModelHandler : ICommandHandler<Guid, CreateLargeLanguageModelCommand>
{
    private readonly DevStackDbContext _dbContext;

    public CreateLargeLanguageModelHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateLargeLanguageModelCommand request, CancellationToken cancellationToken)
    {
        var model = new LargeLanguageModel
        {
            Url = request.Url,
            Model = request.Model,
            ModelAlias = request.ModelAlias ?? string.Empty,
            ApiKey = request.ApiKey,
            Cost = request.Cost,
            MaxComplexity = request.MaxComplexity,
            MaxConcurrency = request.MaxConcurrency
        };

        _dbContext.LargeLanguageModels.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return model.Id;
    }
}

public class UpdateLargeLanguageModelHandler : ICommandHandler<UpdateLargeLanguageModelCommand>
{
    private readonly DevStackDbContext _dbContext;

    public UpdateLargeLanguageModelHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateLargeLanguageModelCommand request, CancellationToken cancellationToken)
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
        if (request.Cost.HasValue) model.Cost = request.Cost.Value;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteLargeLanguageModelHandler : ICommandHandler<DeleteLargeLanguageModelCommand>
{
    private readonly DevStackDbContext _dbContext;

    public DeleteLargeLanguageModelHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteLargeLanguageModelCommand request, CancellationToken cancellationToken)
    {
        var model = await _dbContext.LargeLanguageModels.FindAsync([request.Id], cancellationToken);
        if (model == null)
            throw new InvalidOperationException($"LargeLanguageModel with ID {request.Id} not found.");

        _dbContext.LargeLanguageModels.Remove(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

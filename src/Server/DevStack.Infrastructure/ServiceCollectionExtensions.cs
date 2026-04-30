using DevStack.Application.AgentTasks.Commands;
using DevStack.Application.AgentTasks.Queries;
using DevStack.Application.Deliverables.Commands;
using DevStack.Application.Deliverables.Queries;
using DevStack.Application.LargeLanguageModels.Commands;
using DevStack.Application.Projects.Commands;
using DevStack.Infrastructure.AgentTasks;
using DevStack.Infrastructure.Deliverables;
using DevStack.Infrastructure.ModelConfigurations;
using DevStack.Infrastructure.Projects;

namespace DevStack.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterCommandHandlers(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<Guid, CreateProjectCommand>, CreateProjectHandler>();
        services.AddScoped<ICommandHandler<UpdateProjectCommand>, UpdateProjectHandler>();
        services.AddScoped<ICommandHandler<DeleteProjectCommand>, DeleteProjectHandler>();
        services.AddScoped<GetProjectByIdHandler>();

        services.AddScoped<ICommandHandler<Guid, CreateDeliverableCommand>, CreateDeliverableHandler>();
        services.AddScoped<ICommandHandler<UpdateDeliverableCommand>, UpdateDeliverableHandler>();
        services.AddScoped<ICommandHandler<UpdateDeliverableStatusCommand>, UpdateDeliverableStatusHandler>();
        services.AddScoped<ICommandHandler<DeleteDeliverableCommand>, DeleteDeliverableHandler>();
        services.AddScoped<ICommandHandler<Deliverable?, GetDeliverableByIdQuery>, GetDeliverableByIdHandler>();

        services.AddScoped<ICommandHandler<Guid, CreateAgentTaskCommand>, CreateAgentTaskHandler>();
        services.AddScoped<ICommandHandler<UpdateAgentTaskCommand>, UpdateAgentTaskHandler>();
        services.AddScoped<ICommandHandler<UpdateAgentTaskStatusCommand>, UpdateAgentTaskStatusHandler>();
        services.AddScoped<ICommandHandler<DeleteAgentTaskCommand>, DeleteAgentTaskHandler>();
        services.AddScoped<ICommandHandler<AgentTask, GetAgentTaskByIdQuery>, GetAgentTaskByIdHandler>();

        services.AddScoped<ICommandHandler<Guid, CreateLargeLanguageModelCommand>, CreateLargeLanguageModelHandler>();
        services.AddScoped<ICommandHandler<UpdateLargeLanguageModelCommand>, UpdateLargeLanguageModelHandler>();
        services.AddScoped<ICommandHandler<DeleteLargeLanguageModelCommand>, DeleteLargeLanguageModelHandler>();

        return services;
    }
}

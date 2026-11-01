using DevStack.Persistence;

using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Api.GraphQL.Types;

// BUG-01 — relation OBJECT fields resolve to null on every query in this class.
// Category:        wrong-data (client-visible)
// Trigger path:    client selects agentTask(id:){ deliverable{title} project{name} dependsOnAgentTask{title} }
//                  -> GetAgentTask (below) loads only the AgentTask row; no Include for the navigations
//                  -> HotChocolate resolves each object field from the loaded entity's navigation property,
//                     which is unset; lazy-loading proxies are not enabled (no UseLazyLoadingProxies in
//                     Program.cs), so each field resolves to null
//                  -> the connection resolvers (GetProjects, GetLargeLanguageModels, GetDeliverables,
//                     GetAgentTasks) carry [UsePaging][UseFiltering]
//                     [UseSorting] but no [UseProjection], so the same holds for rows read through
//                     agentTasks / deliverables
//                  -> the response is HTTP 200 with data present and NO errors: the fields are simply null.
// Why this is a real bug: the fields exist in src/schema.graphql and are selected by shipped clients, so the
//                  server accepts documents it cannot satisfy and answers with nulls instead of an error.
// Why this is NOT a false positive: the nullability is observed, not inferred - the integration suite asserts
//                  null for each of these fields against a running server. The collection navigations that ARE
//                  included (Project.deliverables via GetProject's Include, Deliverable.agentTasks via
//                  GetDeliverable's Include) resolve correctly in
//                  the same responses, which isolates the cause to the missing Include / [UseProjection].
// Concrete consequence: src/AdminUi/src/graphql/queries/agentTask.graphql:18-26 and :48-55 select exactly
//                  these fields, so the task detail page and task list render empty deliverable and project
//                  columns, and a task's dependency is never shown.
// Confidence:      Confirmed.
// Fix direction:   add [UseProjection] to the four connection resolvers and Include the needed navigations
//                  (or add explicit field resolvers) on the four by-id queries.
// Pinned by:       DevStack.Tests.Integration.GraphQL Features/GraphQLQueryCoverage.feature (relation-object
//                  scenarios) and StepDefinitions/QueryCoverageSteps.cs. Those scenarios assert null today and
//                  must be inverted when this is fixed.

public class Query
{
    public Project? GetProject([Service] DevStackDbContext dbContext, Guid id)
    {
        var predicate = QuerySpecifications.ProjectById(id);
        return dbContext.Projects.Include(p => p.Deliverables).FirstOrDefault(predicate);
    }

    [UsePaging(MaxPageSize = 100)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Project> GetProjects(
        [Service] DevStackDbContext dbContext)
    {
        return dbContext.Projects.AsQueryable();
    }

    // SEC-01 — both LargeLanguageModel queries below return apiKey verbatim to any caller.
    // Category:        security / credential exposure
    // Trigger path:    any client posts { largeLanguageModels { nodes { apiKey } } } (or largeLanguageModel(id:))
    //                  -> GetLargeLanguageModel (:26-30) / GetLargeLanguageModels (:32-39) return the entity
    //                  -> LargeLanguageModel.ApiKey is a plain string property with no [GraphQLIgnore] and no
    //                     masking, and src/schema.graphql publishes it as apiKey: String!
    //                  -> the value is serialised into the response unchanged.
    //                  Additionally [UseFiltering] and [UseSorting] expose apiKey as a filterable and sortable
    //                  field, so a client can test apiKey values with eq / startsWith / in and recover a key
    //                  prefix-by-prefix even if the field itself were later masked.
    // Why this is a real bug: model credentials are secret material; the endpoint is anonymous (no
    //                  authentication or authorization is registered in Program.cs) and CORS allows any origin.
    // Why this is NOT a false positive: the integration suite creates a model with a known apiKey through the
    //                  API and reads the identical value back over GraphQL, so the echo is observed rather than
    //                  assumed. The key is also stored in plaintext (no ISecretService/AesSecretService is wired
    //                  into the write path), so the returned string is the usable credential.
    // Concrete consequence: any network-reachable client can harvest every configured model credential.
    // Confidence:      Confirmed.
    // Fix direction:   remove apiKey from the published type (or return a masked hint), drop it from the
    //                  filter/sort input types, and encrypt at rest via the existing ISecretService.
    // Pinned by:       DevStack.Tests.Integration.GraphQL Features/GraphQLFieldCoverage.feature (LLM field
    //                  round-trip scenario asserts the echo today; invert it when this is fixed).
    public LargeLanguageModel? GetLargeLanguageModel([Service] DevStackDbContext dbContext, Guid id)
    {
        var predicate = QuerySpecifications.LargeLanguageModelById(id);
        return dbContext.LargeLanguageModels.FirstOrDefault(predicate);
    }

    [UsePaging(MaxPageSize = 100)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<LargeLanguageModel> GetLargeLanguageModels(
        [Service] DevStackDbContext dbContext)
    {
        return dbContext.LargeLanguageModels.AsQueryable();
    }

    public Deliverable? GetDeliverable([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.Deliverables
            .Include(d => d.AgentTasks)
            .FirstOrDefault(d => d.Id == id);
    }

    [UsePaging(MaxPageSize = 100)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Deliverable> GetDeliverables(
        [Service] DevStackDbContext dbContext)
    {
        return dbContext.Deliverables.AsQueryable();
    }

    public AgentTask? GetAgentTask([Service] DevStackDbContext dbContext, Guid id)
    {
        var predicate = QuerySpecifications.AgentTaskById(id);
        return dbContext.AgentTasks.FirstOrDefault(predicate);
    }

    [UsePaging(MaxPageSize = 100)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AgentTask> GetAgentTasks(
        [Service] DevStackDbContext dbContext)
    {
        return dbContext.AgentTasks.AsQueryable();
    }
}

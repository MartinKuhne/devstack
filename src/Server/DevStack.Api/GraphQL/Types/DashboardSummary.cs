using DevStack.Domain.Entities;

namespace DevStack.Api.GraphQL.Types;

public class DashboardSummary
{
    public int ProjectsInFlight { get; set; }
    public int FeaturesInReview { get; set; }
    public int FeaturesFailed { get; set; }
    public int TasksInProgress { get; set; }
    public int TasksFailed { get; set; }
}

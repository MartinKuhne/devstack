using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// The GraphQL integration suite boots a single Postgres container for the whole test run and every
// scenario ends with `mutation { deleteTestData { ... } }`, which removes every row carrying the
// [DeleteAfterTest] marker - not just the rows created by that scenario.
//
// With the default xUnit behaviour each feature is its own test collection and collections run in
// parallel, so a scenario from one feature can delete the marker-prefixed fixture rows that a
// scenario in another feature is still using (observed as spurious "Unexpected Execution Error"
// not-found failures). Scenarios are therefore executed one at a time against the shared database.
[assembly: Parallelization(Mode = ParallelMode.None)]

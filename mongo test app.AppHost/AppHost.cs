var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.mongo_test_app_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.mongo_test_app_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

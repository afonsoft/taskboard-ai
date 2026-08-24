using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;
using Taskboard.Server;

namespace Taskboard.Tests.Integration;

public class ServerEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ServerEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Given_NoAuth_When_GetHealth_Then_Returns200()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Given_NoAuth_When_GetRoot_Then_Returns200OrRedirect()
    {
        var response = await _client.GetAsync("/");

        // Root may serve Blazor app or redirect to login
        response.StatusCode.ShouldBeOneOf(System.Net.HttpStatusCode.OK, System.Net.HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task Given_ProjectExists_When_ListProjects_Then_ReturnsProjectsObject()
    {
        var response = await _client.GetAsync("/api/projects");

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonObject>();
        result.ShouldNotBeNull();
        result["projects"].ShouldNotBeNull();
    }

    [Fact]
    public async Task Given_ValidProject_When_CreateProject_Then_ProjectCreated()
    {
        var project = new
        {
            name = "Test Project Integration"
        };

        var response = await _client.PostAsJsonAsync("/api/projects", project);

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<JsonObject>();
        created.ShouldNotBeNull();
        
        // Response format: { "project": { "id": "...", "name": "..." } }
        var projectObj = created["project"] as JsonObject;
        projectObj.ShouldNotBeNull();
        var name = projectObj!["name"]?.GetValue<string>();
        name.ShouldBe("Test Project Integration");
    }

    [Fact]
    public async Task Given_ProjectExists_When_CreateTask_Then_TaskCreated()
    {
        // First create a project
        var createProjectResponse = await _client.PostAsJsonAsync("/api/projects", new { name = "Project for Tasks" });
        createProjectResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        var projectObj = (await createProjectResponse.Content.ReadFromJsonAsync<JsonObject>())!["project"] as JsonObject;
        var projectId = projectObj!["id"]?.GetValue<string>();

        // Create a task
        var task = new
        {
            projectId = projectId,
            title = "Integration Test Task",
            status = "todo",
            priority = "high"
        };

        var response = await _client.PostAsJsonAsync("/api/tasks", task);

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<JsonObject>();
        created.ShouldNotBeNull();
        var taskObj = created["task"] as JsonObject;
        taskObj.ShouldNotBeNull();
        var title = taskObj!["title"]?.GetValue<string>();
        title.ShouldBe("Integration Test Task");
    }

    [Fact]
    public async Task Given_TaskExists_When_ListTasks_Then_ReturnsTasks()
    {
        var response = await _client.GetAsync("/api/tasks");

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonObject>();
        result.ShouldNotBeNull();
        result["tasks"].ShouldNotBeNull();
    }

    [Fact]
    public async Task Given_TaskExists_When_AddComment_Then_CommentAdded()
    {
        // Create project and task
        var createProjectResponse = await _client.PostAsJsonAsync("/api/projects", new { name = "Comment Test" });
        createProjectResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        var projectObj = (await createProjectResponse.Content.ReadFromJsonAsync<JsonObject>())!["project"] as JsonObject;
        var projectId = projectObj!["id"]?.GetValue<string>();

        var taskResponse = await _client.PostAsJsonAsync("/api/tasks", new { projectId, title = "Task for Comment", status = "todo", priority = "medium" });
        taskResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        var taskObj = (await taskResponse.Content.ReadFromJsonAsync<JsonObject>())!["task"] as JsonObject;
        var taskId = taskObj!["id"]?.GetValue<string>();

        // Add comment
        var comment = new { body = "Test comment from integration test" };
        var response = await _client.PostAsJsonAsync($"/api/tasks/{taskId}/comments", comment);

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<JsonObject>();
        created.ShouldNotBeNull();
        var commentObj = created["comment"] as JsonObject;
        commentObj.ShouldNotBeNull();
        var body = commentObj!["body"]?.GetValue<string>();
        body.ShouldBe("Test comment from integration test");
    }
}
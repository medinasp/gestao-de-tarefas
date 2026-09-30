using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GestaoDeTarefas.Tests.Functional;

public class TasksApiTests
{
    [Fact]
    public async Task Post_valid_task_returns_201_with_location_and_links()
    {
        using var factory = new TasksApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tasks", new { title = "Buy coffee", status = "Pending" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var code = body.GetProperty("code").GetString();
        Assert.StartsWith("TSK-", code);
        Assert.Equal("Pending", body.GetProperty("status").GetString());
        Assert.True(body.TryGetProperty("_links", out var links));
        Assert.Equal($"/api/tasks/{code}", links.GetProperty("self").GetProperty("href").GetString());
    }

    [Fact]
    public async Task Post_without_title_returns_400()
    {
        using var factory = new TasksApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tasks", new { title = "", status = "Pending" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_missing_task_returns_404()
    {
        using var factory = new TasksApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tasks/TSK-NOPE");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_after_create_returns_the_task()
    {
        using var factory = new TasksApiFactory();
        var client = factory.CreateClient();

        var created = await client.PostAsJsonAsync("/api/tasks", new { title = "Task", status = "Pending" });
        var code = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();

        var response = await client.GetAsync($"/api/tasks/{code}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Task", body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Put_updates_task_returns_200()
    {
        using var factory = new TasksApiFactory();
        var client = factory.CreateClient();

        var created = await client.PostAsJsonAsync("/api/tasks", new { title = "Old", status = "Pending" });
        var code = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();

        var response = await client.PutAsJsonAsync($"/api/tasks/{code}",
            new { title = "New", description = "updated", status = "Done" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("New", body.GetProperty("title").GetString());
        Assert.Equal("Done", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Put_without_status_returns_400()
    {
        using var factory = new TasksApiFactory();
        var client = factory.CreateClient();

        var created = await client.PostAsJsonAsync("/api/tasks", new { title = "Old", status = "Pending" });
        var code = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();

        var response = await client.PutAsJsonAsync($"/api/tasks/{code}", new { title = "New" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_missing_task_returns_404()
    {
        using var factory = new TasksApiFactory();
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/tasks/TSK-NOPE",
            new { title = "New", status = "Done" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_returns_204_then_get_returns_404()
    {
        using var factory = new TasksApiFactory();
        var client = factory.CreateClient();

        var created = await client.PostAsJsonAsync("/api/tasks", new { title = "Task", status = "Pending" });
        var code = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();

        var deleteResponse = await client.DeleteAsync($"/api/tasks/{code}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/tasks/{code}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_missing_task_returns_404()
    {
        using var factory = new TasksApiFactory();
        var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/tasks/TSK-NOPE");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_with_invalid_status_returns_400()
    {
        using var factory = new TasksApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tasks?status=Banana");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_filters_by_status()
    {
        using var factory = new TasksApiFactory();
        var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/tasks", new { title = "Pending one", status = "Pending" });
        await client.PostAsJsonAsync("/api/tasks", new { title = "Done one", status = "Done" });

        var response = await client.GetAsync("/api/tasks?status=Done");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, body.GetArrayLength());
        Assert.Equal("Done one", body[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task List_searches_by_title_and_description()
    {
        using var factory = new TasksApiFactory();
        var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/tasks", new { title = "Comprar café", status = "Pending" });
        await client.PostAsJsonAsync("/api/tasks", new { title = "Reunião", description = "levar o café", status = "Pending" });
        await client.PostAsJsonAsync("/api/tasks", new { title = "Enviar relatório", status = "Pending" });

        var response = await client.GetAsync("/api/tasks?search=CAFÉ");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, body.GetArrayLength());
    }

    [Fact]
    public async Task List_filters_by_due_date()
    {
        using var factory = new TasksApiFactory();
        var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/tasks", new { title = "Due Oct 1", dueDate = "2026-10-01", status = "Pending" });
        await client.PostAsJsonAsync("/api/tasks", new { title = "Due Nov 1", dueDate = "2026-11-01", status = "Pending" });

        var response = await client.GetAsync("/api/tasks?dueDate=2026-10-01");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, body.GetArrayLength());
        Assert.Equal("Due Oct 1", body[0].GetProperty("title").GetString());
    }
}

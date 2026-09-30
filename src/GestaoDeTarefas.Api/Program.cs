using System.Text.Json.Serialization;
using FluentValidation;
using GestaoDeTarefas.Api.Infrastructure;
using GestaoDeTarefas.Api.Repositories;
using GestaoDeTarefas.Api.Services;
using GestaoDeTarefas.Api.Validation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new() { Title = "Gestão de Tarefas", Version = "v1" }));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("GestaoDeTarefas"));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskRequestValidator>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

public partial class Program;

using GestaoDeTarefas.Api.Domain;

namespace GestaoDeTarefas.Api.Infrastructure;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Tasks.Any())
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        db.Tasks.AddRange(
            TodoTask.Create("Revisar pull request", "Feature de relatórios", today.AddDays(1), TodoTaskStatus.InProgress),
            TodoTask.Create("Escrever documentação", null, null, TodoTaskStatus.Pending),
            TodoTask.Create("Publicar release", "Versão 1.0", today.AddDays(-2), TodoTaskStatus.Done));

        db.SaveChanges();
    }
}

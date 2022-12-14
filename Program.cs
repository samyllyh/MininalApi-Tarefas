using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("TarefasDb"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Ola mundo");

//retornar frases aleatorias
app.MapGet("frases", async () =>
    await new HttpClient().GetStringAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes")
);

/*******mapeamento********/

//retornando uma lista de taferas
app.MapGet("/tarefas", async (AppDbContext db) =>await db.Tarefas.ToListAsync());

//retornar tafera pelo Id
app.MapGet("/taferas/{id}", async (int id, AppDbContext db) =>
    await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound() //se encontrar a tafera retora ela, se n retorna error
);

//retornar tarefas concluidas
app.MapGet("/tarefas/concluidas", async (AppDbContext db) => 
                                  await db.Tarefas.Where(t => t.IsConcluida).ToListAsync());

//criação de tarefas
app.MapPost("/tarefas", async (Tarefa tarefas, AppDbContext db) =>
{
    db.Tarefas.Add(tarefas);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefas.Id}", tarefas); //interpolação de strings que faz retornar a tarefa que foi criada
});

//atualizar tarefa
app.MapPut("/tarefas/{id}", async (int id, Tarefa inputtarefa, AppDbContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id); //achar a tarefa

    if (tarefa is null)
        return Results.NotFound();

    tarefa.Name = inputtarefa.Name;
    tarefa.IsConcluida = inputtarefa.IsConcluida;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

//deletar tarefas
app.MapDelete("/tarefas/{id}", async (int id, AppDbContext db) =>
{
    if(await db.Tarefas.FindAsync(id) is Tarefa tarefa)
    {
        db.Tarefas.Remove(tarefa);
        await db.SaveChangesAsync();
        return Results.Ok(tarefa);
    }
    return Results.NotFound();
});
app.Run();

class Tarefa
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsConcluida { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}
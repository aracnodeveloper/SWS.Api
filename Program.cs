using App.Shared.Bootstrap;

var builder = WebApplication.CreateBuilder(args);
var bootstrap = new Application();
bootstrap.ConfigureServices(builder);
var app = builder.Build();
bootstrap.Configure(app);
app.Run();

public partial class Program;

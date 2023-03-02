var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
string EnvPath = @".\";

string PythonPath = @"C:\Python310";
Environment.SetEnvironmentVariable("PYTHONHOME", PythonPath, EnvironmentVariableTarget.Process);
Environment.SetEnvironmentVariable("PYTHONPATH", $"{PythonPath}\\Lib\\site-packages;{PythonPath}\\Lib", EnvironmentVariableTarget.Process);
Python.Runtime.Runtime.PythonDLL = @$"{PythonPath}\python310.dll";

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

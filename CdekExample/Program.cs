using CdekExample;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddSingleton(new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build());
builder.Services.AddScoped<ICdekService, CdekService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseWhen(
    context => context.Request.Path.ToString().ToLower().Contains("/service.php"),
    appBranch => {
        appBranch.UseCdekMiddleware();
    });

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

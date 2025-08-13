using MAFFENG.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure dependency injection
builder.Services.AddScoped<IZipProcessor, ZipProcessor>();
builder.Services.AddScoped<IWordProcessor, WordProcessor>();
builder.Services.AddScoped<IConfigManager, ConfigManager>();

// Configure static files
builder.Services.AddHttpContextAccessor();

// Ensure directories exist
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "output");
var modelsPath = Path.Combine(Directory.GetCurrentDirectory(), "models");

Directory.CreateDirectory(uploadsPath);
Directory.CreateDirectory(outputPath);
Directory.CreateDirectory(modelsPath);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Get port from environment variable or use default 5000
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
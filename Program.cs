using MAFFENG.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register custom services
builder.Services.AddScoped<IWordProcessor, WordProcessor>();
builder.Services.AddScoped<IZipProcessor, ZipProcessor>();
builder.Services.AddScoped<IConfigManager, ConfigManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Ensure directories exist
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "output");
var modelsPath = Path.Combine(Directory.GetCurrentDirectory(), "models");

Directory.CreateDirectory(uploadsPath);
Directory.CreateDirectory(outputPath);
Directory.CreateDirectory(modelsPath);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run("http://0.0.0.0:5000");
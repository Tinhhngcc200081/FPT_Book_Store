using FPTBook.Data;
using FPTBook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using FPTBook.Controllers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultUI()
            .AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".BookTicket.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    
    options.Cookie.IsEssential = true;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();


//seed roles
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var loggerFactory = services.GetRequiredService<ILoggerFactory>();
	
		var context = services.GetRequiredService<ApplicationDbContext>();
		var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
		var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
		await ContextSeed.SeedRolesAsync(userManager, roleManager);
	    await ContextSeed.SeedSuperAdminAsync(userManager, roleManager);
    SeedData.Initialize(context);
}

builder.Services.AddDistributedMemoryCache();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
};

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapBookEndpoints();

app.Run();

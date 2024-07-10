using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.FileProviders;
using shopapp.data.Abstract;
using shopapp.data.Concrete.EfCore;
using shopapp.business.Abstract;
using shopapp.business.Concrete;
using shopapp.webui.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using shopapp.webui.EmailServices;
using shopapp.webui.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var serviceProvider = builder.Services.BuildServiceProvider();
var configuration = serviceProvider.GetRequiredService<IConfiguration>();

// builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlite(configuration.GetConnectionString("SqliteConnection")));
// builder.Services.AddDbContext<ShopContext>(options => options.UseSqlite(configuration.GetConnectionString("SqliteConnection")));

builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(configuration.GetConnectionString("MsSqlConnection")));
builder.Services.AddDbContext<ShopContext>(options => options.UseSqlServer(configuration.GetConnectionString("MsSqlConnection")));


builder.Services.AddIdentity<User,IdentityRole>().AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options => {
    // password
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;

    // lockout
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.AllowedForNewUsers = true;

    // user
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;

});

builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/accessdenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.Cookie = new CookieBuilder
    {
        HttpOnly = true,
        Name = ".ShopApp.Security.Cookie",
        SameSite = SameSiteMode.Strict
    };
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IProductService, ProductManager>();
builder.Services.AddScoped<ICategoryService, CategoryManager>();
builder.Services.AddScoped<ICartService, CartManager>();
builder.Services.AddScoped<IOrderService, OrderManager>();

builder.Services.AddScoped<IEmailSender,SmtpEmailSender>(i => 
    new SmtpEmailSender(
        builder.Configuration["EmailSender:Host"],
        builder.Configuration.GetValue<int>("EmailSender:Port"),
        builder.Configuration.GetValue<bool>("EmailSender:EnableSSL"),
        builder.Configuration["EmailSender:UserName"],
        builder.Configuration["EmailSender:Password"]
    )
);

var app = builder.Build();
app.MigrateDatabase();

var env = builder.Environment;

if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "node_modules")),
    RequestPath = "/modules"
});

app.UseAuthentication();
app.UseRouting();

app.UseAuthorization();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

using (var scope = scopeFactory.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();
    SeedIdentity.Seed(cartService ,userManager, roleManager, configuration).Wait();
}


app.MapControllerRoute(
    name: "orders",
    pattern: "orders",
    defaults: new { controller = "Order", action = "GetOrders" }
);


// Cart
app.MapControllerRoute(
    name: "checkout",
    pattern: "checkout",
    defaults: new { controller = "Cart", action = "Checkout" }
);

app.MapControllerRoute(
    name: "cart",
    pattern: "cart",
    defaults: new { controller = "Cart", action = "Index" }
);


// Products
app.MapControllerRoute(
    name: "search",
    pattern: "search",
    defaults: new { controller = "Shop", action = "search" }
);

app.MapControllerRoute(
    name: "products",
    pattern: "products/{category?}",
    defaults: new { controller = "Shop", action = "list" }
);

app.MapControllerRoute(
    name: "productdetails",
    pattern: "{url}",
    defaults: new { controller = "Shop", action = "details" }
);


// Admin User
app.MapControllerRoute(
    name: "adminuserlist",
    pattern: "admin/user/list",
    defaults: new { controller = "Admin", action = "UserList" }
);

app.MapControllerRoute(
    name: "adminuseredit",
    pattern: "admin/user/{id?}",
    defaults: new { controller = "Admin", action = "UserEdit" }
);


// Admin Role
app.MapControllerRoute(
    name: "adminrolecreate",
    pattern: "admin/role/create",
    defaults: new { controller = "Admin", action = "RoleCreate" }
);

app.MapControllerRoute(
    name: "adminrolelist",
    pattern: "admin/role/list",
    defaults: new { controller = "Admin", action = "RoleList" }
);

app.MapControllerRoute(
    name: "adminroleedit",
    pattern: "admin/role/{id?}",
    defaults: new { controller = "Admin", action = "RoleEdit" }
);

// Admin Graphics
app.MapControllerRoute(
    name: "admingraphics",
    pattern: "admin/graphics",
    defaults: new { controller = "Admin", action = "Graphics" }
);


// Admin Products
app.MapControllerRoute(
    name: "adminproducts",
    pattern: "admin/products",
    defaults: new { controller = "Admin", action = "ProductList" }
);

app.MapControllerRoute(
    name: "adminproductcreate",
    pattern: "admin/products/create",
    defaults: new { controller = "Admin", action = "ProductCreate" }
);

app.MapControllerRoute(
    name: "adminproductedit",
    pattern: "admin/products/{id?}",
    defaults: new { controller = "Admin", action = "ProductEdit" }
);


// Admin Categories
app.MapControllerRoute(
    name: "admincategories",
    pattern: "admin/categories",
    defaults: new { controller = "Admin", action = "CategoryList" }
);

app.MapControllerRoute(
    name: "admincategorycreate",
    pattern: "admin/categories/create",
    defaults: new { controller = "Admin", action = "CategoryCreate" }
);

app.MapControllerRoute(
    name: "admincategoryedit",
    pattern: "admin/categories/{id?}",
    defaults: new { controller = "Admin", action = "CategoryEdit" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();

using Blazored.LocalStorage;
using FSI.BusinessProcessManagement.Services;
using FSI.BusinessProcessManagement.Services.Auth;
using FSI.BusinessProcessManagement.Services.Http;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddCircuitOptions(o => o.DetailedErrors = true);
builder.Services.AddAuthorizationCore();

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<TokenAccessor>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProcessService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<DepartmentService>();

builder.Services.AddHttpClient("ApiAnon", http =>
{
    http.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]!); 
});

builder.Services.AddHttpClient<ApiClient>(http =>
{
    http.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]!); 
}); 

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

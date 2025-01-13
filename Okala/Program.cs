using Okala.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<HttpResponseMessageHandler>();
builder.Services.AddSingleton<CoinMarketCapService>();
builder.Services.AddScoped<CoinMarketCapHttpClient>()
    .AddHttpClient<CoinMarketCapHttpClient>(httpClient =>
    {
        httpClient.BaseAddress = new Uri("https://pro-api.coinmarketcap.com");
        httpClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", "0de2dfc8-c1b0-440a-8afd-e8d2795827b3");
        httpClient.DefaultRequestHeaders.Add("Accept", "*/*");        

    }).AddHttpMessageHandler<HttpResponseMessageHandler>();


var app = builder.Build();
//app.UseMiddleware<>
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
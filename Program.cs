var builder = WebApplication.CreateBuilder(args);

// ? ДОБАВЬ ЭТИ СТРОКИ
builder.Services.AddDistributedMemoryCache(); // для хранения данных сессии в памяти
builder.Services.AddSession();                // подключение сервиса сессии

// Остальное как есть
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ? Важно: вызов UseSession ДОЛЖЕН быть после UseRouting, но до UseEndpoints / UseAuthorization
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // <== Перенеси сюда, сразу после UseRouting

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authorization}/{action=Index}/{id?}");

app.Run();

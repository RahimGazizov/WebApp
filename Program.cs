var builder = WebApplication.CreateBuilder(args);

// ? ������ ��� ������
builder.Services.AddDistributedMemoryCache(); // ��� �������� ������ ������ � ������
builder.Services.AddSession();                // ����������� ������� ������

// ��������� ��� ����
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ? �����: ����� UseSession ������ ���� ����� UseRouting, �� �� UseEndpoints / UseAuthorization
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // <== �������� ����, ����� ����� UseRouting

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authorization}/{action=Index}/{id?}");

app.Run();

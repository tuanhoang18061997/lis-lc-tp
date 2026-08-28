using Management.BL;
using Management.BL.Services;
using Management.Interfaces;
using Management.Models;
using Management.Services;
using Management.Services.LabParsing.Parsers;
using Management.Services.LabParsing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using System.Net;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
QuestPDF.Settings.EnableDebugging = true;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IBarcodeService, BarcodeService>();
builder.Services.AddSingleton<IExternalLabParser, TamAnLabParser>();
builder.Services.AddSingleton<ExternalLabParserFactory>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(480);
});
// Add services to the container
builder.Services.AddControllers();

// ⭐ Thêm CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:8056", "http://lis.aibolit.vn")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Nếu cần gửi cookies/credentials
    });
});

#region Cau hinh CKS
// HttpClient cho MySign
builder.Services.AddHttpClient("mysign", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60); // dung o file SignServerService.cs khi goi SignPdfAsync (var client = _httpClientFactory.CreateClient("mysign");)
});

builder.Services.AddScoped<ISignServerService, SignServerService>();
#endregion Cau hinh CKS
builder.Services.AddSingleton<IPdfPostProcessor, PdfPostProcessor>();

builder.Services.AddDbContext<LABContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("LABConnectionString")), ServiceLifetime.Transient);
builder.Services.AddTransient<HospitalBL>();
builder.Services.AddTransient<UserBL>();
builder.Services.AddTransient<TypeBL>();
builder.Services.AddTransient<PatientXNBL>();
builder.Services.AddTransient<GroupBL>();
builder.Services.AddTransient<DoctorBL>();
builder.Services.AddTransient<LocationBL>();
builder.Services.AddTransient<ObjectBL>();
builder.Services.AddTransient<CategoryBL>();
builder.Services.AddTransient<ServiceBL>();
builder.Services.AddTransient<ResultXNBL>();
builder.Services.AddTransient<ServiceTestBL>();
builder.Services.AddTransient<SettingBL>();
builder.Services.AddTransient<ToolBL>();
builder.Services.AddTransient<ResultEditUnlockBL>();
builder.Services.AddTransient<ResultInvalidBL>();
builder.Services.AddTransient<ResultCDHABL>();
builder.Services.AddTransient<PatientCDHABL>();
builder.Services.AddTransient<SampleBL>();
builder.Services.AddTransient<FunctionBL>();
builder.Services.AddTransient<TestTypeBL>();
builder.Services.AddTransient<PrintSampleBL>();
builder.Services.AddTransient<DeviceBL>();
builder.Services.AddTransient<TestCodeBL>();
builder.Services.AddTransient<MapBL>();
builder.Services.AddTransient<ResultStandardBL>();
builder.Services.AddTransient<ReportBL>();
builder.Services.AddTransient<APIBL>();
builder.Services.AddTransient<APIBL_MHIS>();
builder.Services.AddTransient<PushResultHISBL>();
builder.Services.AddTransient<DigitalSignBL>();
builder.Services.AddHostedService<MyHostedService>();
builder.Services.AddTransient<ZaloOAConfigBL>();
builder.Services.AddTransient<ZaloOATemplateBL>();
builder.Services.AddTransient<ZnsSendRequestBL>();
builder.Services.AddTransient<ZnsWebhookBL>();
builder.Services.AddTransient<ZaloOATemplateBL>();
builder.Services.AddTransient<ZaloOAConfigBL>();
builder.Services.AddTransient<ExternalFileBL>();
builder.Services.AddTransient<PortalLinkBL>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1024L * 1024L * 100L; // 100 MB
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

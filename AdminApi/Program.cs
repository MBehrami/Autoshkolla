using System.Reflection;
using System.Text;
using AdminApi.Models;
using AdminApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


var AllowSpecificOrigins = "_allowSpecificOrigins";

//Sql Server Connection String
//builder.Services.AddDbContextPool<AppDbContext>(opt=>opt.UseSqlServer(builder.Configuration["ConnectionStrings:ApiConnStringMssql"]));

//Mysql Server Connection String
/* builder.Services.AddDbContextPool<AppDbContext>(opt=>opt.UseMySql
(builder.Configuration["ConnectionStrings:ApiConnStringMysql"],
ServerVersion.AutoDetect(builder.Configuration["ConnectionStrings:ApiConnStringMysql"]))); */

//Sqlite Connection String
builder.Services.AddDbContextPool<AppDbContext>(opt=>opt.UseSqlite(builder.Configuration["ConnectionStrings:ApiConnStringSqlite"]));

//PostgreSql Connection String
//builder.Services.AddDbContextPool<AppDbContext>(opt=>opt.UseNpgsql(builder.Configuration["ConnectionStrings:ApiConnStringPostgreSql"]));

//Oracle Connection String
//builder.Services.AddDbContextPool<AppDbContext>(opt=>opt.UseOracle(builder.Configuration["ConnectionStrings:ApiConnStringOracle"]));

builder.Services.AddScoped(typeof(ISqlRepository<>), typeof(SqlRepository<>));

builder.Services.AddCors(options=>
{
    options.AddPolicy(name:AllowSpecificOrigins,builder=>
        {
            builder.WithOrigins("http://localhost:3000","https://vueadmin.sangibproject.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

builder.Services.AddControllers();

builder.Services.AddTransient<IMailService,MailService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options=>{
    options.RequireHttpsMetadata=false;
    options.SaveToken=true;
#pragma warning disable CS8604 // Possible null reference argument.
    options.TokenValidationParameters=new TokenValidationParameters
        {
            ValidateIssuer=true,
            ValidateAudience=true,
            ValidateLifetime=true,
            ValidateIssuerSigningKey=true,
            ValidIssuer=builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
            ClockSkew = TimeSpan.Zero
        };
#pragma warning restore CS8604 // Possible null reference argument.
});

IdentityModelEventSource.ShowPII = true;

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AdminVUE API v2",
        Version = "v2",
        Description = "API to communicate with Client Project"
    });               
    options.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme()
    {
            Name = "Authorization",  
            Type = SecuritySchemeType.ApiKey,  
            Scheme = "Bearer",  
            BearerFormat = "JWT",  
            In = ParameterLocation.Header,  
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement  
    {  
        {  
                new OpenApiSecurityScheme  
                {  
                    Reference = new OpenApiReference  
                    {  
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"  
                    }  
                },  
                new string[] {}  

        }  
    });
    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.Configure<FormOptions>(o => {
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = int.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
});

builder.WebHost.UseUrls("http://localhost:5002");



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();

app.UseCors(AllowSpecificOrigins);

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
    RequestPath = new PathString("/Resources")
});
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "API v2");
    options.RoutePrefix=string.Empty;
});

app.Run();

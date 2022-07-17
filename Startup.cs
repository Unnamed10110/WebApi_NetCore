using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApiAutores.Controllers;
using WebApiAutores.Filtros;
using WebApiAutores.Middlewares;
using WebApiAutores.Servicios;
using WebApiAutores.Utilidades;
using WebApiAutores.Utilidades.HATEOAS;
using WebApiAutores.Utilidades.HEADERS;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WebApiAutores
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            // por inyeccion de dependencias se puede pasar tanto servicioA como servicioB pues el
            // controlador(AutoresController.cs) recibe como dependencia en el constructor la interface IServicio (servicioA y
            // servicioB implementan/heredan de IServicio)
            // solid -> principio de inyeccion de dependencias: las clases deben depender de abstracciones y no de tipos concretos
            //var autoresController = new AutoresController(new ApplicationDbContext(null), new ServicioA(new Logger()));
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
            
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {



            services.AddControllers(opciones =>
            {
                opciones.Filters.Add(typeof(FiltroDeExcepcion));
                opciones.Conventions.Add(new SwaggerAgrupaPorVersion());
            }).AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;// ignorar ciclos de referencias/json
            }).AddNewtonsoftJson();
            // sql server
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));


            //postgresql
            services.AddDbContext<ApplicationDbContext>(o => 
                o.UseNpgsql(Configuration.GetConnectionString("ConexionSQL")));

            // resolver automaticamente las dependencias (sistema de inyeccion de dependencias)
            // servicio transitorio con la interface Iservicio y la clase ServicioA -> Interface y clase
            //services.AddTransient<IServicio, ServicioA>();
            // servicio transitorio con un tipo concreto -> Directamente la clase
            //services.AddTransient<ServicioA>();

            // AddTransient -> nueva instancia de la clase siempre (Transient -> utilizar cuando los servicios son transitorios no
            // ocupan estado)

            // AddScoped -> el tiempo de vida aumenta, dentro del mismo contexto http la misma instancia, pero entre distintas
            // peticiones http distintas intancias// (ejemplo el servicio applicationdbcontext)
            //services.AddScoped<IServicio, ServicioA>();

            // AddSingleton -> Siempre la misma instancia incluso para distintos usuarios con distintas peticiones http (cuando el
            // servicio utiliza cache)
            //services.AddSingleton<IServicio, ServicioA>();


            //services.AddTransient<IServicio, ServicioA>();
            //// ejemplos de los tiempos de vida transient, scoped y singleton
            //services.AddTransient<ServicioTransient>();
            //services.AddScoped<ServicioScoped>();
            //services.AddSingleton<ServicioSingleton>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                // para los comentarios en el swagger
                var archivoXML = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var rutaXML = Path.Combine(AppContext.BaseDirectory, archivoXML);
                c.IncludeXmlComments(rutaXML);


                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WebApiAutores",
                    Version = "v1",
                    Description = "Api de Autores, Libros y Comentarios V1",
                    Contact = new OpenApiContact
                    {
                        Email = "trojan.v6@gmail.com",
                        Name = "Sergio Britos",
                        Url = new Uri("https://google.com")

                    }
                });
                c.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = "WebApiAutores",
                    Version = "v2",
                    Description = "Api de Autores, Libros y Comentarios V2",
                    Contact = new OpenApiContact
                    {
                        Email = "trojan.v6@gmail.com",
                        Name = "Sergio Britos",
                        Url = new Uri("https://google.com")

                    }
                });

                c.OperationFilter<AgregarParametroHATEOAS>(); // agregar headers a todos los endpoints
                //c.OperationFilter<AgregarParametroXVersion>();

                // esta es la configuracion para "loguearse en el swagger, pero no es necesaria para probar desde el postman"
                c.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme
                {
                    Name="Authorization",
                    Type= SecuritySchemeType.ApiKey,
                    Scheme="Bearer",
                    BearerFormat= "JWT" ,
                    In =ParameterLocation.Header,
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference=new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });

            //services.AddTransient<
            //>(); // registrar el servicio filtro en el sistema de inyeccion de dependencias

            //services.AddHostedService<EscribirEnArchivo>(); // servicio que escribe en archivo

            //services.AddResponseCaching(); // servicio para cache (filtro)

            
            // para los dtos
            services.AddAutoMapper(typeof(Startup));

            //services.AddIdentity<IdentityUser, IdentityRole>()
            //    .AddEntityFrameworkStores<DbContext>()
            //    .AddDefaultTokenProviders();
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones =>
                {
                    opciones.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                        ClockSkew = TimeSpan.Zero,
                    };
                    opciones.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            // Call this to skip the default logic and avoid using the default response
                            context.HandleResponse();

                            // Write to the response in any way you wish
                            context.Response.StatusCode = 401;
                            context.Response.Headers.Append("my-custom-header", "custom-value");

                            var firbida = new
                            {
                                error=context.Response.StatusCode,
                                message = "Credenciales Invalidas"
                            };
                            var json = JsonSerializer.Serialize(firbida);

                            Console.Write(json+"----------");
                            await context.Response.WriteAsync(json);


                        }
                    };


                }); // usar autenticacion (filtro)

            services.AddAuthorization(opciones =>
            {
                opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("EsAdmin"));
            });

            services.AddCors(opciones =>
            {
                opciones.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("https://www.apirequest.io").AllowAnyMethod().AllowAnyHeader().WithExposedHeaders( new string[] {"CantidadTotalRegistros"}); // cabecera personalizada
                });
            });

            services.AddDataProtection();

            services.AddTransient<HashService>();



            services.AddTransient<GeneradorEnlaces>();
            services.AddTransient<GeneradorEnlacesLibros>();
            services.AddTransient<HATEOASAutorFilterAttribute>();
            services.AddTransient<HATEOASLibroFilterAttribute>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();


            services.AddTransient<AlmacenadorArchivosLocal>();
            services.AddHttpContextAccessor();


        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            //app.UseMiddleware<LoguearRespuestaHTTPMiddleware>();
            app.UseLoguearRespuestaHTTP();


            //app.Map("/ruta1", app =>
            //{
            //    app.Run(async contexto =>
            //    {
            //        await contexto.Response.WriteAsync("Interceptando pipeline con un middelware");
            //    });
            //});
            

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApiAutores v1");
                    c.SwaggerEndpoint("/swagger/v2/swagger.json", "WebApiAutores v2");
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            //app.UseResponseCaching(); //utilizar cache

            app.UseAuthorization(); // utilizar autorizacion (middleware)

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }


    }
}

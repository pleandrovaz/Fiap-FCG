using Application.Interfaces;
using Application.Services;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Settings;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Reflection;

namespace IoC
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Settings
            // Substitui o uso de `Bind` por um mapeamento manual para evitar o erro
            // CS1061 quando a extensão `Bind` não está disponível.
            var jwtSection = configuration.GetSection("JwtSettings");
            var jwtSettings = new JwtSettings();
            BindSectionToInstance(jwtSection, jwtSettings);

            services.Configure<JwtSettings>(options =>
            {
                // atribui valores copiados para a instância de options
                options.Secret = jwtSettings.Secret;
                options.ExpirationHours = jwtSettings.ExpirationHours;
                options.Issuer = jwtSettings.Issuer;
                options.Audience = jwtSettings.Audience;
            });

            // Context
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            services.AddSingleton<DapperContext>();

            // Repositories
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IJogoRepository, JogoRepository>();
            services.AddScoped<IPromocaoRepository, PromocaoRepository>();
            services.AddScoped<IBibliotecaJogoRepository, BibliotecaJogoRepository>();

            // Services
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IJogoService, JogoService>();
            services.AddScoped<IPromocaoService, PromocaoService>();
            services.AddScoped<IBibliotecaJogoService, BibliotecaJogoService>();

            return services;
        }

        // Método auxiliar que faz o binding manual da seção para a instância fornecida.
        // Suporta propriedades públicas simples: string, int, bool, long, double.
        private static void BindSectionToInstance(IConfiguration section, object instance)
        {
            if (section == null || instance == null) return;

            var type = instance.GetType();
            foreach (var child in section.GetChildren())
            {
                var key = child.Key;
                var value = child.Value;
                if (string.IsNullOrEmpty(key) || value == null) continue;

                var prop = type.GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null || !prop.CanWrite) continue;

                try
                {
                    object? converted = ConvertToType(value, prop.PropertyType);
                    if (converted != null)
                    {
                        prop.SetValue(instance, converted);
                    }
                }
                catch
                {
                    // ignorar propriedades que não puderem ser convertidas
                }
            }
        }

        private static object? ConvertToType(string value, Type targetType)
        {
            if (targetType == typeof(string)) return value;
            if (targetType == typeof(int) || targetType == typeof(int?))
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) return i;
                return null;
            }
            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                if (bool.TryParse(value, out var b)) return b;
                return null;
            }
            if (targetType == typeof(long) || targetType == typeof(long?))
            {
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l)) return l;
                return null;
            }
            if (targetType == typeof(double) || targetType == typeof(double?))
            {
                if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d)) return d;
                return null;
            }
            // adicionar suportes adicionais conforme necessário
            return null;
        }
    }
}


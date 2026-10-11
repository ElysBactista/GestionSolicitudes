using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace GestionSolicitudes.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager)
    {
        try
        {
            // Solo asegura que existan los dos roles oficiales si no están creados
            string[] roles = ["Administrador", "Residente"];
            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ADVERTENCIA ROLES]: {ex.Message}");
        }
    }
}
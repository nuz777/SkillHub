#pragma warning disable CS0618
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Roles
        string[] roles = [UserRoles.Employee, UserRoles.Participant];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Instructores / Empleados iniciales
        var instructorsData = new List<(string Name, string Email, string Specialty, string Password)>
        {
            ("Carlos Mendoza", "carlos.mendoza@skillhub.com", "Fotografía", "Instructor123!"),
            ("Laura Gómez", "laura.gomez@skillhub.com", "Cocina", "Instructor123!"),
            ("Andrés Torres", "andres.torres@skillhub.com", "Oratoria", "Instructor123!"),
            ("Sofía Morales", "sofia.morales@skillhub.com", "Diseño Gráfico", "Instructor123!"),
            ("Admin SkillHub", "admin@skillhub.com", "Coordinación", "Admin123!")
        };

        foreach (var (name, email, specialty, password) in instructorsData)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = name,
                    Specialty = specialty,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, UserRoles.Employee);
            }

            // Asegurar que exista entidad Instructor en BD
            var instructorEntity = await context.Instructors.FirstOrDefaultAsync(i => i.Email == email);
            if (instructorEntity == null)
            {
                context.Instructors.Add(new Instructor
                {
                    FullName = name,
                    Email = email,
                    Specialty = specialty
                });
            }
        }
        await context.SaveChangesAsync();

        // 3. Participantes iniciales
        var participantsData = new List<(string Name, string Email, string Password)>
        {
            ("Juan Pérez", "juan.perez@example.com", "Participant123!"),
            ("María Rodríguez", "maria.rodriguez@example.com", "Participant123!"),
            ("Carlos López", "carlos.lopez@example.com", "Participant123!"),
            ("Ana Martínez", "ana.martinez@example.com", "Participant123!")
        };

        foreach (var (name, email, password) in participantsData)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = name,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, UserRoles.Participant);
            }
        }

        // 4. Talleres iniciales (con fechas futuras para catálogo)
        if (!await context.Workshops.AnyAsync())
        {
            var carlos = await context.Instructors.FirstAsync(i => i.Email == "carlos.mendoza@skillhub.com");
            var laura = await context.Instructors.FirstAsync(i => i.Email == "laura.gomez@skillhub.com");
            var andres = await context.Instructors.FirstAsync(i => i.Email == "andres.torres@skillhub.com");
            var sofia = await context.Instructors.FirstAsync(i => i.Email == "sofia.morales@skillhub.com");

            var baseDate = DateTime.UtcNow.Date.AddDays(7);

            var initialWorkshops = new List<Workshop>
            {
                new()
                {
                    Code = "FOT-001",
                    Title = "Fotografía Urbana y Manejo de Luz Natural",
                    Description = "Taller práctico sobre composición visual, captura callejera y aprovechamiento de luz ambiental.",
                    InstructorId = carlos.Id,
                    Instructor = carlos,
                    InstructorEmail = carlos.Email,
                    StartDate = baseDate.AddHours(9),
                    EndDate = baseDate.AddHours(13),
                    MaxCapacity = 15
                },
                new()
                {
                    Code = "COC-001",
                    Title = "Cocina Mediterránea Básica",
                    Description = "Aprende técnicas fundamentales de preparación, pastas frescas y salsas clásicas.",
                    InstructorId = laura.Id,
                    Instructor = laura,
                    InstructorEmail = laura.Email,
                    StartDate = baseDate.AddDays(1).AddHours(14),
                    EndDate = baseDate.AddDays(1).AddHours(18),
                    MaxCapacity = 12
                },
                new()
                {
                    Code = "ORA-001",
                    Title = "Oratoria y Comunicación Asertiva",
                    Description = "Técnicas de modulación de voz, lenguaje no verbal y persuasión para hablar en público con confianza.",
                    InstructorId = andres.Id,
                    Instructor = andres,
                    InstructorEmail = andres.Email,
                    StartDate = baseDate.AddDays(2).AddHours(10),
                    EndDate = baseDate.AddDays(2).AddHours(14),
                    MaxCapacity = 20
                },
                new()
                {
                    Code = "DIS-001",
                    Title = "Fundamentos de Diseño Visual y Branding",
                    Description = "Creación de identidades de marca, teoría del color, tipografía y diseño con propósito.",
                    InstructorId = sofia.Id,
                    Instructor = sofia,
                    InstructorEmail = sofia.Email,
                    StartDate = baseDate.AddDays(3).AddHours(15),
                    EndDate = baseDate.AddDays(3).AddHours(19),
                    MaxCapacity = 18
                }
            };

            context.Workshops.AddRange(initialWorkshops);
            await context.SaveChangesAsync();
        }

        // Si existe un archivo Excel local (por ejemplo en la raíz o Descargas), cargar datos adicionales
        await TrySeedFromExcelFileAsync(context, userManager);
    }

    private static async Task TrySeedFromExcelFileAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        string[] candidatePaths = [
            Path.Combine(AppContext.BaseDirectory, "SkillHub_Data.xlsx"),
            Path.Combine(Directory.GetCurrentDirectory(), "SkillHub_Data.xlsx"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "SkillHub_Data.xlsx"),
            "/home/cohorte5/Descargas/SkillHub_Data.xlsx"
        ];

        var excelPath = candidatePaths.FirstOrDefault(File.Exists);
        if (excelPath == null) return;

        try
        {
            using var package = new ExcelPackage(new FileInfo(excelPath));

            // Hoja Instructors
            var instWs = package.Workbook.Worksheets["Instructors"];
            if (instWs?.Dimension != null)
            {
                for (int row = 2; row <= instWs.Dimension.Rows; row++)
                {
                    var name = instWs.Cells[row, 1].Text.Trim();
                    var email = instWs.Cells[row, 2].Text.Trim().ToLowerInvariant();
                    var specialty = instWs.Cells[row, 3].Text.Trim();

                    if (!string.IsNullOrEmpty(email))
                    {
                        var user = await userManager.FindByEmailAsync(email);
                        if (user == null)
                        {
                            user = new ApplicationUser
                            {
                                UserName = email,
                                Email = email,
                                FullName = name,
                                Specialty = specialty,
                                EmailConfirmed = true
                            };
                            await userManager.CreateAsync(user, "Instructor123!");
                            await userManager.AddToRoleAsync(user, UserRoles.Employee);
                        }

                        if (!await context.Instructors.AnyAsync(i => i.Email == email))
                        {
                            context.Instructors.Add(new Instructor
                            {
                                FullName = name,
                                Email = email,
                                Specialty = specialty
                            });
                        }
                    }
                }
                await context.SaveChangesAsync();
            }

            // Hoja Participants
            var partWs = package.Workbook.Worksheets["Participants"];
            if (partWs?.Dimension != null)
            {
                for (int row = 2; row <= partWs.Dimension.Rows; row++)
                {
                    var name = partWs.Cells[row, 1].Text.Trim();
                    var email = partWs.Cells[row, 2].Text.Trim().ToLowerInvariant();

                    if (!string.IsNullOrEmpty(email))
                    {
                        var user = await userManager.FindByEmailAsync(email);
                        if (user == null)
                        {
                            user = new ApplicationUser
                            {
                                UserName = email,
                                Email = email,
                                FullName = name,
                                EmailConfirmed = true
                            };
                            await userManager.CreateAsync(user, "Participant123!");
                            await userManager.AddToRoleAsync(user, UserRoles.Participant);
                        }
                    }
                }
            }

            // Hoja Workshops
            var workWs = package.Workbook.Worksheets["Workshops"];
            if (workWs?.Dimension != null)
            {
                for (int row = 2; row <= workWs.Dimension.Rows; row++)
                {
                    var code = workWs.Cells[row, 1].Text.Trim().ToUpperInvariant();
                    var title = workWs.Cells[row, 2].Text.Trim();
                    var desc = workWs.Cells[row, 3].Text.Trim();
                    var instEmail = workWs.Cells[row, 4].Text.Trim().ToLowerInvariant();
                    
                    if (DateTime.TryParse(workWs.Cells[row, 5].Text, out var start) &&
                        DateTime.TryParse(workWs.Cells[row, 6].Text, out var end) &&
                        int.TryParse(workWs.Cells[row, 7].Text, out var cap))
                    {
                        if (!await context.Workshops.AnyAsync(w => w.Code == code))
                        {
                            var instructor = await context.Instructors.FirstOrDefaultAsync(i => i.Email == instEmail);
                            if (instructor != null)
                            {
                                context.Workshops.Add(new Workshop
                                {
                                    Code = code,
                                    Title = title,
                                    Description = desc,
                                    InstructorId = instructor.Id,
                                    InstructorEmail = instructor.Email,
                                    StartDate = start.ToUniversalTime(),
                                    EndDate = end.ToUniversalTime(),
                                    MaxCapacity = cap
                                });
                            }
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
        }
        catch
        {
            // Silencioso si el excel tiene formato alternativo
        }
    }
}

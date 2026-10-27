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
            ("Andrés Felipe Mejía Arango", "andres.mejia@correo.com", "Fotografía", "Instructor2026*"),
            ("Rosa Elena Barrios Polo", "rosa.barrios@correo.com", "Cocina", "Instructor2026*"),
            ("Mauricio Jiménez Cárdenas", "mauricio.jimenez@correo.com", "Oratoria", "Instructor2026*"),
            ("Natalia Herrera Castaño", "natalia.herrera@correo.com", "Diseño gráfico", "Instructor2026*"),
            ("Hernán Darío Quintero Gil", "hernan.quintero@correo.com", "Música", "Instructor2026*"),
            ("Paola Andrea Vargas Mora", "paola.vargas@correo.com", "Marketing digital", "Instructor2026*"),
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
            else
            {
                user.FullName = name;
                user.Specialty = specialty;
                await userManager.UpdateAsync(user);
                if (!await userManager.IsInRoleAsync(user, UserRoles.Employee))
                {
                    await userManager.AddToRoleAsync(user, UserRoles.Employee);
                }
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
            else
            {
                instructorEntity.FullName = name;
                instructorEntity.Specialty = specialty;
            }
        }
        await context.SaveChangesAsync();

        // Eliminar instructores anteriores
        string[] oldEmails = ["carlos.mendoza@skillhub.com", "laura.gomez@skillhub.com", "andres.torres@skillhub.com", "sofia.morales@skillhub.com"];
        foreach (var oldEmail in oldEmails)
        {
            var oldUser = await userManager.FindByEmailAsync(oldEmail);
            if (oldUser != null) await userManager.DeleteAsync(oldUser);
        }

        // 3. Participantes iniciales
        var participantsData = new List<(string Name, string Email, string Password)>
        {
            ("Juan Pérez", "juan.perez@example.com", "Participant123!"),
            ("María Rodríguez", "maria.rodriguez@example.com", "Participant123!"),
            ("Carlos López", "carlos.lopez@example.com", "Participant123!"),
            ("Ana Martínez", "ana.martinez@example.com", "Participant123!"),
            ("Andrés Felipe Mejía Arango", "andres.mejia@correo.com", "Participant123!"),
            ("Rosa Elena Barrios Polo", "rosa.barrios@correo.com", "Participant123!"),
            ("Mauricio Jiménez Cárdenas", "mauricio.jimenez@correo.com", "Participant123!"),
            ("Natalia Herrera Castaño", "natalia.herrera@correo.com", "Participant123!"),
            ("Paola Andrea Vargas Mora", "paola.vargas@correo.com", "Participant123!"),
            ("Jorge Andrés Pérez Rojas", "jorge.perez@correo.com", "Participant123!"),
            ("Luz Marina Ospina Ríos", "luz.ospina@correo.com", "Participant123!"),
            ("Carlos Eduardo Rincón Díaz", "carlos.rincon@correo.com", "Participant123!"),
            ("Valentina Gómez Ruiz", "valentina.gomez@correo.com", "Participant123!"),
            ("Juan Camilo Restrepo Vélez", "juan.restrepo@correo.com", "Participant123!"),
            ("María Fernanda Torres León", "maria.torres@correo.com", "Participant123!"),
            ("Santiago Ramírez Henao", "santiago.ramirez@correo.com", "Participant123!"),
            ("Daniela Morales Pineda", "daniela.morales@correo.com", "Participant123!"),
            ("Sebastián Castro Duque", "sebastian.castro@correo.com", "Participant123!"),
            ("Laura Sofía Martínez Suárez", "laura.martinez@correo.com", "Participant123!"),
            ("Andrés Mauricio Salazar Ortiz", "andres.salazar@correo.com", "Participant123!"),
            ("Camila Andrea Cárdenas Ruiz", "camila.cardenas@correo.com", "Participant123!"),
            ("Diego Alejandro Muñoz Peña", "diego.munoz@correo.com", "Participant123!"),
            ("Juliana Ortega Sánchez", "juliana.ortega@correo.com", "Participant123!"),
            ("Felipe Agudelo Marín", "felipe.agudelo@correo.com", "Participant123!"),
            ("Natalia Montoya Correa", "natalia.montoya@correo.com", "Participant123!"),
            ("Óscar Iván Guerrero Lozano", "oscar.guerrero@correo.com", "Participant123!"),
            ("Paula Andrea Zapata Ríos", "paula.zapata@correo.com", "Participant123!"),
            ("Cristian David Becerra Mejía", "cristian.becerra@correo.com", "Participant123!"),
            ("Mariana Londoño Álvarez", "mariana.londono@correo.com", "Participant123!"),
            ("Javier Alonso Cuéllar Pardo", "javier.cuellar@correo.com", "Participant123!"),
            ("Isabella Navarro Fuentes", "isabella.navarro@correo.com", "Participant123!"),
            ("Kevin Stiven Mosquera Palacios", "kevin.mosquera@correo.com", "Participant123!"),
            ("Ana María Jaramillo Gaviria", "ana.jaramillo@correo.com", "Participant123!"),
            ("Luis Fernando Arias Cano", "luis.arias@correo.com", "Participant123!"),
            ("Sara Valencia Bermúdez", "sara.valencia@correo.com", "Participant123!"),
            ("Esteban Rodríguez Molina", "esteban.rodriguez@correo.com", "Participant123!"),
            ("Manuela Escobar Toro", "manuela.escobar@correo.com", "Participant123!"),
            ("Wilmer Alfonso Benítez Cruz", "wilmer.benitez@correo.com", "Participant123!"),
            ("Yuliana Patiño Osorio", "yuliana.patino@correo.com", "Participant123!")
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

        // 4. Talleres iniciales (los 6 primeros talleres requeridos)
        var allowedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "FOT-001", "COC-001", "ORA-001", "DIS-001", "MUS-001", "MKT-001"
        };

        var andres = await context.Instructors.FirstAsync(i => i.Email == "andres.mejia@correo.com");
        var rosa = await context.Instructors.FirstAsync(i => i.Email == "rosa.barrios@correo.com");
        var mauricio = await context.Instructors.FirstAsync(i => i.Email == "mauricio.jimenez@correo.com");
        var natalia = await context.Instructors.FirstAsync(i => i.Email == "natalia.herrera@correo.com");
        var hernan = await context.Instructors.FirstAsync(i => i.Email == "hernan.quintero@correo.com");
        var paola = await context.Instructors.FirstAsync(i => i.Email == "paola.vargas@correo.com");

        var baseDate = DateTime.UtcNow.Date.AddDays(7);

        var officialWorkshops = new List<Workshop>
        {
            new()
            {
                Code = "FOT-001",
                Title = "Fotografía Urbana y Manejo de Luz Natural",
                Description = "Taller práctico sobre composición visual, captura callejera y aprovechamiento de luz ambiental.",
                InstructorId = andres.Id,
                Instructor = andres,
                InstructorEmail = andres.Email,
                StartDate = DateTime.SpecifyKind(baseDate.AddHours(9), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(baseDate.AddHours(13), DateTimeKind.Utc),
                MaxCapacity = 15
            },
            new()
            {
                Code = "COC-001",
                Title = "Cocina Mediterránea Básica",
                Description = "Aprende técnicas fundamentales de preparación, pastas frescas y salsas clásicas.",
                InstructorId = rosa.Id,
                Instructor = rosa,
                InstructorEmail = rosa.Email,
                StartDate = DateTime.SpecifyKind(baseDate.AddDays(1).AddHours(14), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(baseDate.AddDays(1).AddHours(18), DateTimeKind.Utc),
                MaxCapacity = 12
            },
            new()
            {
                Code = "ORA-001",
                Title = "Oratoria y Comunicación Asertiva",
                Description = "Técnicas de modulación de voz, lenguaje no verbal y persuasión para hablar en público con confianza.",
                InstructorId = mauricio.Id,
                Instructor = mauricio,
                InstructorEmail = mauricio.Email,
                StartDate = DateTime.SpecifyKind(baseDate.AddDays(2).AddHours(10), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(baseDate.AddDays(2).AddHours(14), DateTimeKind.Utc),
                MaxCapacity = 20
            },
            new()
            {
                Code = "DIS-001",
                Title = "Fundamentos de Diseño Visual y Branding",
                Description = "Creación de identidades de marca, teoría del color, tipografía y diseño con propósito.",
                InstructorId = natalia.Id,
                Instructor = natalia,
                InstructorEmail = natalia.Email,
                StartDate = DateTime.SpecifyKind(baseDate.AddDays(3).AddHours(15), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(baseDate.AddDays(3).AddHours(19), DateTimeKind.Utc),
                MaxCapacity = 18
            },
            new()
            {
                Code = "MUS-001",
                Title = "Guitarra desde cero",
                Description = "Acordes básicos y primeras canciones.",
                InstructorId = hernan.Id,
                Instructor = hernan,
                InstructorEmail = hernan.Email,
                StartDate = DateTime.SpecifyKind(baseDate.AddDays(4).AddHours(19), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(baseDate.AddDays(4).AddHours(22), DateTimeKind.Utc),
                MaxCapacity = 10
            },
            new()
            {
                Code = "MKT-001",
                Title = "Marketing Digital y Estrategia en Redes",
                Description = "Fundamentos de marketing de contenidos, pauta digital, analítica web y crecimiento de marcas en redes sociales.",
                InstructorId = paola.Id,
                Instructor = paola,
                InstructorEmail = paola.Email,
                StartDate = DateTime.SpecifyKind(baseDate.AddDays(5).AddHours(10), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(baseDate.AddDays(5).AddHours(14), DateTimeKind.Utc),
                MaxCapacity = 20
            }
        };

        foreach (var ow in officialWorkshops)
        {
            var existing = await context.Workshops.FirstOrDefaultAsync(w => w.Code == ow.Code);
            if (existing == null)
            {
                context.Workshops.Add(ow);
            }
            else
            {
                existing.Title = ow.Title;
                existing.Description = ow.Description;
                existing.InstructorId = ow.InstructorId;
                existing.InstructorEmail = ow.InstructorEmail;
                existing.StartDate = ow.StartDate;
                existing.EndDate = ow.EndDate;
                existing.MaxCapacity = ow.MaxCapacity;
            }
        }
        await context.SaveChangesAsync();

        // Si existen talleres adicionales fuera de los 6 permitidos, eliminarlos
        var extraWorkshops = await context.Workshops
            .Where(w => !allowedCodes.Contains(w.Code))
            .ToListAsync();

        if (extraWorkshops.Count > 0)
        {
            var extraIds = extraWorkshops.Select(w => w.Id).ToList();
            var extraEnrollments = await context.Enrollments
                .Where(e => extraIds.Contains(e.WorkshopId))
                .ToListAsync();

            if (extraEnrollments.Count > 0)
            {
                context.Enrollments.RemoveRange(extraEnrollments);
            }

            context.Workshops.RemoveRange(extraWorkshops);
            await context.SaveChangesAsync();
        }

        // Si existe un archivo Excel local (por ejemplo en la raíz o Descargas), cargar datos adicionales sólo si están dentro de los 6 permitidos
        await TrySeedFromExcelFileAsync(context, userManager, allowedCodes);
    }

    private static async Task TrySeedFromExcelFileAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, HashSet<string> allowedCodes)
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
            ExcelPackage.License.SetNonCommercialOrganization("SkillHub");
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

                    if (!allowedCodes.Contains(code)) continue;
                    
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
                                    StartDate = DateTime.SpecifyKind(start.ToUniversalTime(), DateTimeKind.Utc),
                                    EndDate = DateTime.SpecifyKind(end.ToUniversalTime(), DateTimeKind.Utc),
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

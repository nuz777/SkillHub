using OfficeOpenXml;
using SkillHub.Application.Interfaces;

namespace SkillHub.Infrastructure.Services;

public class ExcelReader : IExcelReader
{
    static ExcelReader()
    {
        ExcelPackage.License.SetNonCommercialOrganization("SkillHub");
    }

    public async Task<IReadOnlyList<(int RowNumber, string ParticipantEmail, string WorkshopCode)>> ReadEnrollmentRowsAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        var result = new List<(int RowNumber, string ParticipantEmail, string WorkshopCode)>();

        ExcelPackage.License.SetNonCommercialOrganization("SkillHub");
        using var package = new ExcelPackage();
        await package.LoadAsync(fileStream, cancellationToken);

        if (package.Workbook.Worksheets.Count == 0)
        {
            return result;
        }

        // 1. Buscar primero hojas con nombres típicos de inscripciones
        ExcelWorksheet? worksheet = null;
        string[] preferredNames = ["Enrollments", "Inscripciones", "Enrollment", "Inscripcion", "Inscritos", "Datos"];
        foreach (var name in preferredNames)
        {
            var ws = package.Workbook.Worksheets[name];
            if (ws?.Dimension != null)
            {
                worksheet = ws;
                break;
            }
        }

        // Si no se encontró por nombre preferido, buscar alguna hoja que contenga columnas de inscripciones
        if (worksheet == null)
        {
            foreach (var ws in package.Workbook.Worksheets)
            {
                if (ws.Dimension == null) continue;
                for (int c = 1; c <= ws.Dimension.Columns; c++)
                {
                    var h = ws.Cells[1, c].Text?.Trim();
                    if (!string.IsNullOrEmpty(h) && 
                        (h.Contains("email", StringComparison.OrdinalIgnoreCase) || 
                         h.Contains("correo", StringComparison.OrdinalIgnoreCase) ||
                         h.Contains("participante", StringComparison.OrdinalIgnoreCase)))
                    {
                        worksheet = ws;
                        break;
                    }
                }
                if (worksheet != null) break;
            }
        }

        // Si aún es nulo, tomar la primera con dimensión
        worksheet ??= package.Workbook.Worksheets.FirstOrDefault(w => w.Dimension != null) 
                      ?? package.Workbook.Worksheets.FirstOrDefault();

        if (worksheet == null || worksheet.Dimension == null)
        {
            return result;
        }

        var rowCount = worksheet.Dimension.Rows;
        var colCount = worksheet.Dimension.Columns;

        var emailCol = -1;
        var codeCol = -1;

        // Detectar encabezados en la fila 1
        for (var col = 1; col <= colCount; col++)
        {
            var header = worksheet.Cells[1, col].Text?.Trim();
            if (string.IsNullOrEmpty(header)) continue;

            if (header.Equals("ParticipantEmail", StringComparison.OrdinalIgnoreCase) ||
                header.Equals("Email", StringComparison.OrdinalIgnoreCase) ||
                header.Equals("Correo", StringComparison.OrdinalIgnoreCase) ||
                header.Equals("CorreoParticipante", StringComparison.OrdinalIgnoreCase) ||
                header.Equals("ParticipanteEmail", StringComparison.OrdinalIgnoreCase) ||
                header.Equals("Participante", StringComparison.OrdinalIgnoreCase) ||
                header.Equals("Usuario", StringComparison.OrdinalIgnoreCase))
            {
                emailCol = col;
            }
            else if (header.Equals("WorkshopCode", StringComparison.OrdinalIgnoreCase) ||
                     header.Equals("Code", StringComparison.OrdinalIgnoreCase) ||
                     header.Equals("Codigo", StringComparison.OrdinalIgnoreCase) ||
                     header.Equals("CodigoTaller", StringComparison.OrdinalIgnoreCase) ||
                     header.Equals("TallerCodigo", StringComparison.OrdinalIgnoreCase) ||
                     header.Equals("Taller", StringComparison.OrdinalIgnoreCase) ||
                     header.Equals("Curso", StringComparison.OrdinalIgnoreCase))
            {
                codeCol = col;
            }
        }

        // Si no se detectó emailCol por nombre, inspeccionar filas 2..5 buscando la columna con '@'
        if (emailCol == -1)
        {
            for (var col = 1; col <= colCount; col++)
            {
                if (col == codeCol) continue;
                for (var r = 2; r <= Math.Min(6, rowCount); r++)
                {
                    var val = worksheet.Cells[r, col].Text?.Trim() ?? string.Empty;
                    if (val.Contains('@'))
                    {
                        emailCol = col;
                        break;
                    }
                }
                if (emailCol != -1) break;
            }
        }

        // Asignación segura garantizando que no coincidan en la misma columna
        if (emailCol == -1 && codeCol == -1)
        {
            emailCol = 1;
            codeCol = 2;
        }
        else if (emailCol == -1)
        {
            emailCol = (codeCol == 1) ? 2 : 1;
        }
        else if (codeCol == -1)
        {
            codeCol = (emailCol == 1) ? 2 : 1;
        }

        if (emailCol == codeCol)
        {
            if (emailCol == 1) codeCol = 2;
            else emailCol = 1;
        }

        for (var row = 2; row <= rowCount; row++)
        {
            var email = worksheet.Cells[row, emailCol].Text?.Trim() ?? string.Empty;
            var code = worksheet.Cells[row, codeCol].Text?.Trim() ?? string.Empty;

            // Ignorar filas totalmente vacías
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(code))
                continue;

            // Detección inteligente: Si email no tiene '@' pero code sí lo tiene, intercambiar
            if (!email.Contains('@') && code.Contains('@'))
            {
                (email, code) = (code, email);
            }

            result.Add((row, email, code));
        }

        return result;
    }
}

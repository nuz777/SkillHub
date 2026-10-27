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

        // Si no se encontró por nombre preferido, buscar alguna hoja que contenga columnas de inscripciones (y que no sea de talleres)
        if (worksheet == null)
        {
            foreach (var ws in package.Workbook.Worksheets)
            {
                if (ws.Dimension == null) continue;

                // Si la hoja tiene columna Title o Titulo, es una hoja de definición de talleres
                bool isWorkshopSheet = false;
                for (int c = 1; c <= ws.Dimension.Columns; c++)
                {
                    var h = ws.Cells[1, c].Text?.Trim();
                    if (!string.IsNullOrEmpty(h) && (h.Equals("Title", StringComparison.OrdinalIgnoreCase) || h.Equals("Titulo", StringComparison.OrdinalIgnoreCase)))
                    {
                        isWorkshopSheet = true;
                        break;
                    }
                }
                if (isWorkshopSheet) continue;

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

        // Si aún es nulo, tomar la primera con dimensión que no sea de talleres
        if (worksheet == null)
        {
            foreach (var ws in package.Workbook.Worksheets)
            {
                if (ws.Dimension == null) continue;
                bool isWorkshopSheet = false;
                for (int c = 1; c <= ws.Dimension.Columns; c++)
                {
                    var h = ws.Cells[1, c].Text?.Trim();
                    if (!string.IsNullOrEmpty(h) && (h.Equals("Title", StringComparison.OrdinalIgnoreCase) || h.Equals("Titulo", StringComparison.OrdinalIgnoreCase)))
                    {
                        isWorkshopSheet = true;
                        break;
                    }
                }
                if (!isWorkshopSheet)
                {
                    worksheet = ws;
                    break;
                }
            }
        }

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

    public async Task<IReadOnlyList<ExcelWorkshopRow>> ReadWorkshopRowsAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        var result = new List<ExcelWorkshopRow>();

        ExcelPackage.License.SetNonCommercialOrganization("SkillHub");
        using var package = new ExcelPackage();
        await package.LoadAsync(fileStream, cancellationToken);

        if (package.Workbook.Worksheets.Count == 0)
        {
            return result;
        }

        // 1. Buscar hojas con nombres relacionados a talleres
        ExcelWorksheet? worksheet = null;
        string[] workshopSheetNames = ["Workshops", "Talleres", "Workshop", "Taller", "Cursos"];
        foreach (var name in workshopSheetNames)
        {
            var ws = package.Workbook.Worksheets[name];
            if (ws?.Dimension != null)
            {
                worksheet = ws;
                break;
            }
        }

        // Si no se encontró por nombre, buscar cualquier hoja que contenga columnas de talleres (Code/Title)
        if (worksheet == null)
        {
            foreach (var ws in package.Workbook.Worksheets)
            {
                if (ws.Dimension == null) continue;
                bool hasCode = false, hasTitle = false;
                for (int c = 1; c <= ws.Dimension.Columns; c++)
                {
                    var h = ws.Cells[1, c].Text?.Trim();
                    if (string.IsNullOrEmpty(h)) continue;
                    if (h.Equals("Code", StringComparison.OrdinalIgnoreCase) || h.Equals("Codigo", StringComparison.OrdinalIgnoreCase)) hasCode = true;
                    if (h.Equals("Title", StringComparison.OrdinalIgnoreCase) || h.Equals("Titulo", StringComparison.OrdinalIgnoreCase) || h.Equals("Nombre", StringComparison.OrdinalIgnoreCase)) hasTitle = true;
                }
                if (hasCode && hasTitle)
                {
                    worksheet = ws;
                    break;
                }
            }
        }

        if (worksheet == null || worksheet.Dimension == null)
        {
            return result;
        }

        var rowCount = worksheet.Dimension.Rows;
        var colCount = worksheet.Dimension.Columns;

        int codeCol = -1, titleCol = -1, descCol = -1, emailCol = -1, startCol = -1, endCol = -1, capCol = -1;

        for (int col = 1; col <= colCount; col++)
        {
            var h = worksheet.Cells[1, col].Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(h)) continue;

            if (h.Equals("Code", StringComparison.OrdinalIgnoreCase) || h.Equals("Codigo", StringComparison.OrdinalIgnoreCase) || h.Equals("CodigoTaller", StringComparison.OrdinalIgnoreCase))
                codeCol = col;
            else if (h.Equals("Title", StringComparison.OrdinalIgnoreCase) || h.Equals("Titulo", StringComparison.OrdinalIgnoreCase) || h.Equals("Nombre", StringComparison.OrdinalIgnoreCase) || h.Equals("Taller", StringComparison.OrdinalIgnoreCase))
                titleCol = col;
            else if (h.Equals("Description", StringComparison.OrdinalIgnoreCase) || h.Equals("Descripcion", StringComparison.OrdinalIgnoreCase) || h.Equals("Detalle", StringComparison.OrdinalIgnoreCase))
                descCol = col;
            else if (h.Equals("InstructorEmail", StringComparison.OrdinalIgnoreCase) || h.Equals("EmailInstructor", StringComparison.OrdinalIgnoreCase) || h.Equals("CorreoInstructor", StringComparison.OrdinalIgnoreCase) || h.Equals("Email", StringComparison.OrdinalIgnoreCase) || h.Equals("Correo", StringComparison.OrdinalIgnoreCase))
                emailCol = col;
            else if (h.Equals("StartDate", StringComparison.OrdinalIgnoreCase) || h.Equals("FechaInicio", StringComparison.OrdinalIgnoreCase) || h.Equals("StartDateTime", StringComparison.OrdinalIgnoreCase) || h.Equals("Inicio", StringComparison.OrdinalIgnoreCase))
                startCol = col;
            else if (h.Equals("EndDate", StringComparison.OrdinalIgnoreCase) || h.Equals("FechaFin", StringComparison.OrdinalIgnoreCase) || h.Equals("EndDateTime", StringComparison.OrdinalIgnoreCase) || h.Equals("Fin", StringComparison.OrdinalIgnoreCase))
                endCol = col;
            else if (h.Equals("Capacity", StringComparison.OrdinalIgnoreCase) || h.Equals("MaxCapacity", StringComparison.OrdinalIgnoreCase) || h.Equals("Cupos", StringComparison.OrdinalIgnoreCase) || h.Equals("Cupo", StringComparison.OrdinalIgnoreCase))
                capCol = col;
        }

        if (codeCol == -1) codeCol = 1;
        if (titleCol == -1) titleCol = 2;
        if (descCol == -1) descCol = 3;
        if (emailCol == -1) emailCol = 4;

        for (int row = 2; row <= rowCount; row++)
        {
            var code = worksheet.Cells[row, codeCol].Text?.Trim() ?? string.Empty;
            var title = titleCol != -1 ? (worksheet.Cells[row, titleCol].Text?.Trim() ?? string.Empty) : string.Empty;
            var desc = descCol != -1 ? (worksheet.Cells[row, descCol].Text?.Trim() ?? string.Empty) : string.Empty;
            var email = emailCol != -1 ? (worksheet.Cells[row, emailCol].Text?.Trim() ?? string.Empty) : string.Empty;

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(title))
                continue;

            DateTime? startDate = null;
            if (startCol != -1 && DateTime.TryParse(worksheet.Cells[row, startCol].Text, out var s))
                startDate = DateTime.SpecifyKind(s.ToUniversalTime(), DateTimeKind.Utc);

            DateTime? endDate = null;
            if (endCol != -1 && DateTime.TryParse(worksheet.Cells[row, endCol].Text, out var e))
                endDate = DateTime.SpecifyKind(e.ToUniversalTime(), DateTimeKind.Utc);

            int? cap = null;
            if (capCol != -1 && int.TryParse(worksheet.Cells[row, capCol].Text, out var c))
                cap = c;

            result.Add(new ExcelWorkshopRow(row, code, title, desc, email, startDate, endDate, cap));
        }

        return result;
    }
}

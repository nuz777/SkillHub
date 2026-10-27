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

        // Buscar hoja 'Enrollments' o usar la primera disponible
        var worksheet = package.Workbook.Worksheets["Enrollments"] 
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
                header.Equals("Correo", StringComparison.OrdinalIgnoreCase))
            {
                emailCol = col;
            }
            else if (header.Equals("WorkshopCode", StringComparison.OrdinalIgnoreCase) ||
                     header.Equals("Code", StringComparison.OrdinalIgnoreCase) ||
                     header.Equals("Codigo", StringComparison.OrdinalIgnoreCase))
            {
                codeCol = col;
            }
        }

        // Si no se encontraron por nombre, asumir columnas 1 y 2
        if (emailCol == -1) emailCol = 1;
        if (codeCol == -1) codeCol = 2;

        for (var row = 2; row <= rowCount; row++)
        {
            var email = worksheet.Cells[row, emailCol].Text?.Trim() ?? string.Empty;
            var code = worksheet.Cells[row, codeCol].Text?.Trim() ?? string.Empty;

            // Ignorar filas totalmente vacías
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(code))
                continue;

            result.Add((row, email, code));
        }

        return result;
    }
}

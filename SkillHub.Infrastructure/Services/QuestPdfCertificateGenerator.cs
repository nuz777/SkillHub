using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkillHub.Application.DTOs;
using SkillHub.Application.Interfaces;

namespace SkillHub.Infrastructure.Services;

public class QuestPdfCertificateGenerator : ICertificatePdfGenerator
{
    static QuestPdfCertificateGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateAsync(CertificateDataDto data, CancellationToken cancellationToken = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11).FontColor(Colors.Grey.Darken3));

                // Borde decorativo
                page.Content().Border(3).BorderColor("#0d6efd").Padding(30).Column(column =>
                {
                    column.Spacing(10);

                    // Encabezado
                    column.Item().AlignCenter().Text("SkillHub")
                        .Bold().FontSize(28).FontColor("#0d6efd");

                    column.Item().AlignCenter().Text("Centro de Formación Presencial")
                        .FontSize(12).FontColor(Colors.Grey.Medium);

                    column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // Título Oficial Requerido
                    column.Item().AlignCenter().Text("Constancia de inscripción")
                        .Bold().FontSize(22).FontColor(Colors.Grey.Darken4);

                    column.Item().PaddingTop(10).AlignCenter().Text(text =>
                    {
                        text.Span("Por medio de la presente, se certifica que ").FontSize(12);
                        text.Span(data.ParticipantName).Bold().FontSize(14).FontColor("#0d6efd");
                        text.Span($" ({data.ParticipantEmail})").FontSize(11).Italic();
                        text.Span(" se encuentra formalmente inscrito(a) en el siguiente taller:");
                    });

                    // Detalles del Taller
                    column.Item().PaddingVertical(15).Background(Colors.Grey.Lighten4).Padding(15).Column(details =>
                    {
                        details.Spacing(6);
                        details.Item().Text(text =>
                        {
                            text.Span("Código del Taller: ").Bold();
                            text.Span(data.WorkshopCode);
                        });

                        details.Item().Text(text =>
                        {
                            text.Span("Título: ").Bold();
                            text.Span(data.WorkshopTitle).FontSize(13).Bold();
                        });

                        details.Item().Text(text =>
                        {
                            text.Span("Instructor: ").Bold();
                            text.Span(data.InstructorName);
                        });

                        details.Item().Text(text =>
                        {
                            text.Span("Fecha y Horario: ").Bold();
                            text.Span($"{data.StartDate:dd/MM/yyyy HH:mm} - {data.EndDate:dd/MM/yyyy HH:mm} (UTC)");
                        });

                        details.Item().Text(text =>
                        {
                            text.Span("Fecha de Inscripción: ").Bold();
                            text.Span($"{data.EnrollmentDate:dd/MM/yyyy HH:mm} (UTC)");
                        });
                    });

                    // Pie de página con fecha de generación e ID
                    column.Item().AlignRight().Text(text =>
                    {
                        text.Span("Fecha de generación del documento: ").Bold();
                        text.Span($"{data.GeneratedAt:dd/MM/yyyy HH:mm:ss} UTC");
                    });

                    column.Item().AlignRight().Text($"ID de Constancia: {data.EnrollmentId}")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }
}

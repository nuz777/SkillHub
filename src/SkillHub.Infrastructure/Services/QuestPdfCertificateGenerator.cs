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
        QuestPDF.Settings.ThrowOnMissingFontFamilies = false;
    }

    public Task<byte[]> GenerateAsync(CertificateDataDto data, CancellationToken cancellationToken = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10).FontColor("#1E293B"));

                // Marco ornamental de seguridad y acreditación
                page.Content()
                    .Border(2)
                    .BorderColor("#0F172A")
                    .Padding(3)
                    .Border(1)
                    .BorderColor("#D97706")
                    .Padding(22)
                    .Column(column =>
                    {
                        column.Spacing(8);

                        // 1. Encabezado Institucional
                        column.Item().Row(row =>
                        {
                            row.RelativeItem(3).Row(logoRow =>
                            {
                                logoRow.AutoItem().Background("#0F172A").PaddingHorizontal(10).PaddingVertical(4).Text("SKILLHUB")
                                    .Bold().FontSize(15).FontColor(Colors.White);

                                logoRow.RelativeItem().PaddingLeft(10).Column(c =>
                                {
                                    c.Item().Text("CENTRO DE FORMACIÓN PRESENCIAL").Bold().FontSize(9).FontColor("#0F172A");
                                    c.Item().Text("Acreditación y Registro Académico Continuo").FontSize(7.5f).FontColor("#64748B");
                                });
                            });

                            row.RelativeItem(2).AlignRight().Border(1).BorderColor("#CBD5E1").Background("#F8FAFC").PaddingHorizontal(10).PaddingVertical(4).Column(folio =>
                            {
                                folio.Item().Text("FOLIO OFICIAL DE CONSTANCIA").Bold().FontSize(7).FontColor("#64748B");
                                folio.Item().Text($"SKH-2026-{data.EnrollmentId.ToString()[..8].ToUpperInvariant()}").Bold().FontSize(10).FontColor("#0F172A");
                                folio.Item().Text("ESTADO: ACTIVA Y CONFIRMADA").Bold().FontSize(7.5f).FontColor("#059669");
                            });
                        });

                        column.Item().PaddingVertical(4).LineHorizontal(1).LineColor("#E2E8F0");

                        // 2. Título Central Oficial
                        column.Item().AlignCenter().Column(titleCol =>
                        {
                            titleCol.Item().AlignCenter().Text("CONSTANCIA OFICIAL DE INSCRIPCIÓN").Bold().FontSize(20).FontColor("#0F172A");
                            titleCol.Item().AlignCenter().Text("Certificado de asignación formal de cupo presencial").Italic().FontSize(9.5f).FontColor("#475569");
                        });

                        // 3. Declaración de Certificación
                        column.Item().PaddingTop(4).AlignCenter().Text(t =>
                        {
                            t.Span("La Coordinación Académica de SkillHub hace constar formalmente que el/la participante:").FontSize(10).FontColor("#334155");
                        });

                        // 4. Nombre Destacado del Participante
                        column.Item().AlignCenter().Column(pCol =>
                        {
                            pCol.Item().AlignCenter().Text(data.ParticipantName).Bold().FontSize(20).FontColor("#1E3A8A");
                            pCol.Item().AlignCenter().Text($"Identificación Digital / Correo: {data.ParticipantEmail}").FontSize(9).FontColor("#64748B").Italic();
                        });

                        column.Item().AlignCenter().Text(t =>
                        {
                            t.Span("Cuenta con cupo asegurado y confirmado en el siguiente taller presencial:").FontSize(9.5f).FontColor("#334155");
                        });

                        // 5. Tarjeta de Detalles del Taller
                        column.Item().PaddingVertical(4).Border(1).BorderColor("#CBD5E1").Background("#F8FAFC").Padding(10).Column(card =>
                        {
                            card.Item().Row(topRow =>
                            {
                                topRow.AutoItem().Background("#1E3A8A").PaddingHorizontal(8).PaddingVertical(3).Text(data.WorkshopCode)
                                    .Bold().FontSize(10).FontColor(Colors.White);

                                topRow.RelativeItem().PaddingLeft(10).Text(data.WorkshopTitle)
                                    .Bold().FontSize(13).FontColor("#0F172A");
                            });

                            card.Item().PaddingVertical(4).LineHorizontal(1).LineColor("#E2E8F0");

                            card.Item().Row(metaRow =>
                            {
                                metaRow.RelativeItem().Column(c1 =>
                                {
                                    c1.Item().Text("INSTRUCTOR(A) RESPONSABLE").Bold().FontSize(7.5f).FontColor("#64748B");
                                    c1.Item().Text(data.InstructorName).Bold().FontSize(9.5f).FontColor("#0F172A");
                                    c1.Item().Text("Facilitador Especialista").FontSize(7.5f).FontColor("#64748B");
                                });

                                metaRow.RelativeItem().Column(c2 =>
                                {
                                    c2.Item().Text("FECHA Y HORARIO").Bold().FontSize(7.5f).FontColor("#64748B");
                                    c2.Item().Text($"{data.StartDate:dd/MM/yyyy HH:mm} - {data.EndDate:dd/MM/yyyy HH:mm}").Bold().FontSize(9.5f).FontColor("#0F172A");
                                    c2.Item().Text("Sesión Presencial Programada").FontSize(7.5f).FontColor("#64748B");
                                });

                                metaRow.RelativeItem().Column(c3 =>
                                {
                                    c3.Item().Text("SEDE Y MODALIDAD").Bold().FontSize(7.5f).FontColor("#64748B");
                                    c3.Item().Text("Presencial • Campus SkillHub").Bold().FontSize(9.5f).FontColor("#0F172A");
                                    c3.Item().Text("Medellín, Colombia").FontSize(7.5f).FontColor("#64748B");
                                });
                            });
                        });

                        // 6. Firmas Institucionales y Sello de Validación
                        column.Item().PaddingTop(8).Row(sigRow =>
                        {
                            sigRow.RelativeItem().Column(s1 =>
                            {
                                s1.Item().LineHorizontal(1).LineColor("#94A3B8");
                                s1.Item().PaddingTop(2).Text("Dirección de Capacitación y Registro").Bold().FontSize(8).FontColor("#1E293B");
                                s1.Item().Text("SkillHub Centro de Formación").FontSize(7).FontColor("#64748B");
                                s1.Item().Text("✓ Firma Digitalizada y Validada").FontSize(7).Italic().FontColor("#059669");
                            });

                            sigRow.ConstantItem(15);

                            sigRow.RelativeItem().Border(1).BorderColor("#CBD5E1").Background("#F1F5F9").Padding(5).Column(seal =>
                            {
                                seal.Item().AlignCenter().Text("SELLO DIGITAL OFICIAL").Bold().FontSize(7).FontColor("#0F172A");
                                seal.Item().AlignCenter().Text("VALIDACIÓN SISTÉMICA VIGENTE").Bold().FontSize(7.5f).FontColor("#059669");
                                seal.Item().AlignCenter().Text($"ID: {data.EnrollmentId}").FontSize(6).FontColor("#64748B");
                            });

                            sigRow.ConstantItem(15);

                            sigRow.RelativeItem().Column(s2 =>
                            {
                                s2.Item().LineHorizontal(1).LineColor("#94A3B8");
                                s2.Item().PaddingTop(2).Text(data.InstructorName).Bold().FontSize(8).FontColor("#1E293B");
                                s2.Item().Text("Instructor(a) Titular Asignado(a)").FontSize(7).FontColor("#64748B");
                                s2.Item().Text("✓ Confirmación de Asistencia en Sistema").FontSize(7).Italic().FontColor("#059669");
                            });
                        });

                        // 7. Pie de Página con Fecha de Emisión y Seguridad
                        column.Item().PaddingTop(6).Row(footRow =>
                        {
                            footRow.RelativeItem().Text($"Fecha de Inscripción: {data.EnrollmentDate:dd/MM/yyyy HH:mm} • Emisión del Certificado: {data.GeneratedAt:dd/MM/yyyy HH:mm:ss} UTC")
                                .FontSize(7).FontColor("#94A3B8");

                            footRow.AutoItem().Text("Documento oficial con validez académica • SkillHub Platform")
                                .FontSize(7).FontColor("#94A3B8").Italic();
                        });
                    });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }
}

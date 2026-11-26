using System.Globalization;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using FixnSeven.Application.Features.Orders.Pdf;
namespace FixnSeven.Infrastructure.Pdf;

// HU-12: PDF de la orden con QuestPDF; si la orden tiene certificado, lleva el QR de verificacion
public class QuestOrderPdfGenerator : IOrderPdfGenerator
{
    private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-HN");
    private static readonly TimeZoneInfo Zone = FindZone();

    static QuestOrderPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;   // gratis para proyectos academicos y pequenos
    }

    private static TimeZoneInfo FindZone() =>
        TimeZoneInfo.TryFindSystemTimeZoneById("America/Tegucigalpa", out var tz) ? tz : TimeZoneInfo.Utc;

    private static string Local(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone).ToString("dd/MM/yyyy HH:mm", Es);

    private static string Money(decimal value) => value.ToString("N2", Es);

    private static byte[] Qr(string text)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        return new PngByteQRCode(data).GetGraphic(10);
    }

    public byte[] Generate(OrderPdfModel model)
    {
        var o = model.Order;
        var eq = o.Equipment;
        var cert = o.Certificate;
        var shortId = o.Id.ToString()[^8..].ToUpperInvariant();

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontSize(10));

                // ---------- Encabezado ----------
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(model.EnterpriseName).FontSize(18).Bold();
                        col.Item().Text("Orden de servicio").FontSize(12).FontColor(Colors.Grey.Darken1);
                    });
                    row.ConstantItem(170).AlignRight().Column(col =>
                    {
                        col.Item().AlignRight().Text($"N.° {shortId}").FontSize(14).Bold();
                        col.Item().AlignRight().Text($"Estado: {o.Status}");
                        col.Item().AlignRight().Text($"Ingreso: {Local(o.CreatedAt)}");
                        if (o.ClosedAt is not null)
                            col.Item().AlignRight().Text($"Cierre: {Local(o.ClosedAt.Value)}");
                    });
                });

                // ---------- Contenido ----------
                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Element(Box).Column(c =>
                        {
                            c.Item().Text("Cliente").Bold();
                            c.Item().Text(o.CustomerName ?? "Equipo propio de la empresa");
                            c.Item().Text($"Técnico: {o.TechnicianName ?? "Sin asignar"}");
                        });
                        row.ConstantItem(10);
                        row.RelativeItem().Element(Box).Column(c =>
                        {
                            c.Item().Text("Equipo").Bold();
                            c.Item().Text($"{eq.CategoryName}: {eq.Brand} {eq.Model}");
                            c.Item().Text($"{eq.IdentifierLabel}: {eq.Identifier}");
                            if (eq.CurrentUsage is not null) c.Item().Text($"Uso actual: {eq.CurrentUsage}");
                        });
                    });

                    col.Item().Element(Box).Column(c =>
                    {
                        c.Item().Text("Problema reportado").Bold();
                        c.Item().Text(o.ReportedIssue);
                        if (!string.IsNullOrWhiteSpace(o.ReceivedCondition))
                        {
                            c.Item().PaddingTop(4).Text("Estado en que se recibió").Bold();
                            c.Item().Text(o.ReceivedCondition);
                        }
                        if (!string.IsNullOrWhiteSpace(o.WorkDone))
                        {
                            c.Item().PaddingTop(4).Text("Trabajo realizado").Bold();
                            c.Item().Text(o.WorkDone);
                        }
                        if (!string.IsNullOrWhiteSpace(o.Notes))
                        {
                            c.Item().PaddingTop(4).Text("Notas").Bold();
                            c.Item().Text(o.Notes);
                        }
                    });

                    // Trabajos y repuestos
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(65);
                            c.RelativeColumn();
                            c.ConstantColumn(50);
                            c.ConstantColumn(75);
                            c.ConstantColumn(80);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Element(Head).Text("Tipo");
                            h.Cell().Element(Head).Text("Descripción");
                            h.Cell().Element(Head).AlignRight().Text("Cant.");
                            h.Cell().Element(Head).AlignRight().Text("Precio");
                            h.Cell().Element(Head).AlignRight().Text("Subtotal");
                        });
                        if (o.Items.Count == 0)
                            table.Cell().ColumnSpan(5).Element(Cell).Text("Sin trabajos registrados.").Italic();
                        foreach (var i in o.Items)
                        {
                            table.Cell().Element(Cell).Text(i.Kind);
                            table.Cell().Element(Cell).Text(i.Description);
                            table.Cell().Element(Cell).AlignRight().Text(i.Quantity.ToString(Es));
                            table.Cell().Element(Cell).AlignRight().Text(Money(i.UnitPrice));
                            table.Cell().Element(Cell).AlignRight().Text(Money(i.Subtotal));
                        }
                    });
                    col.Item().AlignRight().Text($"Total: {Money(o.Total)}").FontSize(13).Bold();

                    // Certificado + QR (HU-14/16)
                    if (cert is not null && model.VerifyUrl is not null)
                    {
                        col.Item().Element(Box).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Certificado de calidad").FontSize(12).Bold();

                                // Revocado o Vencido en rojo, con aviso, para que nadie lo tome por valido
                                var isValid = cert.Status == "Vigente";
                                var statusColor = isValid ? Colors.Green.Darken2 : Colors.Red.Darken2;
                                c.Item().Text(t =>
                                {
                                    t.Span($"Nivel: {cert.Grade} ({cert.Score}/100) · Estado: ");
                                    t.Span(cert.Status.ToUpperInvariant()).Bold().FontColor(statusColor);
                                });
                                if (!isValid)
                                    c.Item().PaddingVertical(4).Background(Colors.Red.Lighten4).Padding(4)
                                        .Text(cert.Status == "Revocado"
                                            ? $"CERTIFICADO NO VÁLIDO: fue revocado el {Local(cert.RevokedAt ?? cert.ExpiresAt)}."
                                            : "CERTIFICADO NO VÁLIDO: ya venció.")
                                        .FontSize(9).Bold().FontColor(Colors.Red.Darken3);
                                c.Item().Text($"Emitido: {Local(cert.IssuedAt)} · Vence: {Local(cert.ExpiresAt)}");
                                c.Item().Text($"Código de verificación: {cert.VerificationCode}").Bold();
                                if (!string.IsNullOrWhiteSpace(cert.PublicSummary))
                                    c.Item().Text(cert.PublicSummary);
                                c.Item().PaddingTop(4).Text("Escanee el QR para verificarlo. La consulta no muestra datos personales.")
                                    .FontSize(8).FontColor(Colors.Grey.Darken1);
                            });
                            row.ConstantItem(110).Image(Qr(model.VerifyUrl));
                        });
                    }

                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().PaddingTop(25).LineHorizontal(0.5f);
                            c.Item().AlignCenter().Text("Firma del cliente").FontSize(9);
                        });
                        row.ConstantItem(40);
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().PaddingTop(25).LineHorizontal(0.5f);
                            c.Item().AlignCenter().Text("Firma del taller").FontSize(9);
                        });
                    });
                });

                // ---------- Pie ----------
                page.Footer().AlignCenter().Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(8).FontColor(Colors.Grey.Darken1));
                    t.Span("Generado por Fix n' Seven · Página ");
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static IContainer Box(IContainer c) =>
        c.Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(8);

    private static IContainer Head(IContainer c) =>
        c.BorderBottom(1).BorderColor(Colors.Grey.Darken1).PaddingVertical(4).DefaultTextStyle(t => t.Bold());

    private static IContainer Cell(IContainer c) =>
        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4);
}

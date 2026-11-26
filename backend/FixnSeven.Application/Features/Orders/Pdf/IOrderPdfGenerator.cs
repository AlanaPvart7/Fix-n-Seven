using FixnSeven.Application.Features.Orders.DTOs;
namespace FixnSeven.Application.Features.Orders.Pdf;

// HU-12: la capa Application solo dice QUE se necesita; Infrastructure decide COMO (QuestPDF)
public interface IOrderPdfGenerator
{
    byte[] Generate(OrderPdfModel model);
}

public record OrderPdfModel(
    string EnterpriseName,
    OrderDetailResponseDto Order,
    string? VerifyUrl          // URL publica del certificado (va en el QR), si la orden tiene uno
);

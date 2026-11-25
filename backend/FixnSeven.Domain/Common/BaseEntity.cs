namespace FixnSeven.Domain.Common;

public abstract class BaseEntity
{
    // UUID v7: ordenado por tiempo, no fragmenta los índices de PostgreSQL
    public Guid Id { get; set; } = Guid.CreateVersion7();
}

// Toda tabla que pertenece a una empresa hereda de esta
public abstract class TenantEntity : BaseEntity
{
    public Guid EnterpriseId { get; set; }
}
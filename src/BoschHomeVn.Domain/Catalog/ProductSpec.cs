namespace BoschHomeVn.Domain.Catalog;

// Một dòng trong bảng thông số kỹ thuật ("Sức chứa" → "13 bộ")
public sealed record ProductSpec(string Name, string Value);

using System.ComponentModel.DataAnnotations;

namespace BoschHomeVn.Contracts.Admin;

public sealed record AdminCategoryResponse(string Id, string Name, string ShortName, IReadOnlyList<AdminProductTypeResponse> Types);

// ProductCount: số sản phẩm mọi trạng thái — còn sản phẩm thì không xóa được loại
public sealed record AdminProductTypeResponse(string Id, string Name, string? Group, int ProductCount);

public sealed record SaveCategoryRequest([StringLength(200)] string Name, [StringLength(100)] string ShortName);

// Group: nhóm con trong danh mục (vd. "Thiết bị đun nấu"), để trống nếu không chia nhóm
public sealed record SaveProductTypeRequest([StringLength(200)] string Name, [StringLength(200)] string? Group);

// Thứ tự mới theo danh sách id
public sealed record ReorderRequest(IReadOnlyList<string> Ids);

public sealed record CreatedResponse(string Id);

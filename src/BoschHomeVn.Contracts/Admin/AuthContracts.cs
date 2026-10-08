namespace BoschHomeVn.Contracts.Admin;

// UserName: tên đăng nhập hoặc email · Remember: giữ đăng nhập sau khi đóng trình duyệt
public sealed record LoginRequest(string UserName, string Password, bool Remember);

public sealed record AdminUserResponse(string UserName);

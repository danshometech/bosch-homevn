namespace BoschHomeVn.Domain.Common;

// Vi phạm quy tắc nghiệp vụ (hết hàng, mã giảm giá hết hạn…) — Api trả 400 kèm Message
public class DomainException(string message) : Exception(message);

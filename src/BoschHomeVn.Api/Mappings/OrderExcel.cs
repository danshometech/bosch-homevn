using BoschHomeVn.Application.Common;
using BoschHomeVn.Domain.Orders;
using ClosedXML.Excel;

namespace BoschHomeVn.Api.Mappings;

// File Excel (.xlsx) đơn hàng: ngày giờ theo giờ Việt Nam là ô ngày, số tiền là ô số (#,##0, cộng được), số điện thoại là ô
// chữ (giữ số 0 đầu). Chữ khách nhập ghi thành ô chữ nên không chạy được thành công thức.
internal static class OrderExcel
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private const string DateFormat = "dd/mm/yyyy hh:mm";
    private const string MoneyFormat = "#,##0";

    private static readonly Dictionary<OrderStatus, string> StatusNames = new()
    {
        [OrderStatus.New] = "Mới",
        [OrderStatus.Confirmed] = "Đã xác nhận",
        [OrderStatus.Shipping] = "Đang giao",
        [OrderStatus.Completed] = "Hoàn tất",
        [OrderStatus.Cancelled] = "Đã hủy",
    };

    // [tiêu đề, độ rộng cột, định dạng số]
    private static readonly (string Title, double Width, string? Format)[] OrderColumns =
    [
        ("Mã đơn", 16, null), ("Ngày đặt", 17, DateFormat), ("Trạng thái", 13, null), ("Đã thu tiền", 11, null),
        ("Khách hàng", 22, null), ("Điện thoại", 13, "@"), ("Email", 24, null), ("Tỉnh / TP", 18, null),
        ("Phường / Xã", 18, null), ("Địa chỉ", 40, null), ("Ghi chú của khách", 30, null), ("Cách giao", 22, null),
        ("Thanh toán", 24, null), ("Trả góp (tháng)", 10, null), ("Mã sản phẩm", 15, null), ("Tên sản phẩm", 45, null),
        ("Đơn giá", 13, MoneyFormat), ("Số lượng", 9, null), ("Thành tiền", 14, MoneyFormat), ("Phí giao", 12, MoneyFormat),
        ("Tạm tính", 14, MoneyFormat), ("Tổng đơn", 14, MoneyFormat),
    ];

    // Sheet chi tiết một đơn: độ rộng 5 cột dùng chung cho cả ba phần (thông tin / sản phẩm / lịch sử)
    private static readonly double[] DetailWidths = [20, 50, 24, 18, 16];

    // Nhiều đơn, một sheet: mỗi dòng sản phẩm một hàng, thông tin đơn lặp lại ở mọi hàng để lọc được; Phí giao / Tạm tính /
    // Tổng đơn chỉ ghi ở hàng đầu của mỗi đơn để cộng cột không bị tính trùng
    public static byte[] Build(IEnumerable<Order> orders)
    {
        using var book = new XLWorkbook();
        var sheet = book.AddWorksheet("Đơn hàng");
        Header(sheet, OrderColumns);

        var r = 2;
        foreach (var o in orders)
        {
            var first = true;
            foreach (var i in o.Items)
            {
                var row = sheet.Row(r++);
                row.Cell(1).Value = o.Code;
                row.Cell(2).Value = VietnamTime.FromUtc(o.CreatedAt);
                row.Cell(3).Value = StatusNames[o.Status];
                row.Cell(4).Value = o.IsPaid ? "Có" : "Chưa";
                row.Cell(5).Value = o.CustomerName;
                row.Cell(6).Value = o.CustomerPhone;
                row.Cell(7).Value = o.CustomerEmail ?? "";
                row.Cell(8).Value = o.Province ?? "";
                row.Cell(9).Value = o.Ward ?? "";
                row.Cell(10).Value = o.Address;
                row.Cell(11).Value = o.CustomerNote ?? "";
                row.Cell(12).Value = o.ShippingMethodName;
                row.Cell(13).Value = o.PaymentMethodName;
                row.Cell(14).Value = o.InstallmentMonths is { } m ? m : Blank.Value;
                row.Cell(15).Value = i.Model;
                row.Cell(16).Value = i.Name;
                row.Cell(17).Value = (double)i.UnitPrice;
                row.Cell(18).Value = i.Quantity;
                row.Cell(19).Value = (double)i.LineTotal;
                if (first)
                {
                    row.Cell(20).Value = (double)o.ShippingFee;
                    row.Cell(21).Value = (double)o.Subtotal;
                    row.Cell(22).Value = (double)o.Total;
                }
                first = false;
            }
        }

        if (r > 2)
        {
            sheet.Range(1, 1, r - 1, OrderColumns.Length).SetAutoFilter();
        }
        return Save(book);
    }

    // Một đơn, một sheet "Chi tiết đơn" đọc từ trên xuống: thông tin đơn hàng → sản phẩm (kèm tạm tính, phí giao, tổng) →
    // lịch sử xử lý (cũ → mới; mỗi lần đổi trạng thái / thu tiền / thêm ghi chú một dòng, ghi chú đầy đủ)
    public static byte[] BuildDetail(Order o)
    {
        using var book = new XLWorkbook();
        var sheet = book.AddWorksheet("Chi tiết đơn");
        sheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        for (var c = 0; c < DetailWidths.Length; c++)
        {
            sheet.Column(c + 1).Width = DetailWidths[c];
        }

        var title = sheet.Cell(1, 1);
        title.Value = $"ĐƠN HÀNG {o.Code}";
        title.Style.Font.Bold = true;
        title.Style.Font.FontSize = 14;
        var r = 3;

        r = Section(sheet, r, "THÔNG TIN ĐƠN HÀNG");
        (string, XLCellValue, string?)[] fields =
        [
            ("Mã đơn", o.Code, null),
            ("Ngày đặt", VietnamTime.FromUtc(o.CreatedAt), DateFormat),
            ("Cập nhật lần cuối", VietnamTime.FromUtc(o.UpdatedAt), DateFormat),
            ("Trạng thái hiện tại", StatusNames[o.Status], null),
            ("Đã thu tiền", o.IsPaid ? "Có" : "Chưa", null),
            ("Khách hàng", o.CustomerName, null),
            ("Điện thoại", o.CustomerPhone, "@"),
            ("Email", o.CustomerEmail ?? "", null),
            ("Tỉnh / TP", o.Province ?? "", null),
            ("Phường / Xã", o.Ward ?? "", null),
            ("Địa chỉ", o.Address, null),
            ("Ghi chú của khách", o.CustomerNote ?? "", null),
            ("Cách giao", o.ShippingMethodName, null),
            ("Thanh toán", o.PaymentMethodName, null),
            ("Trả góp (tháng)", o.InstallmentMonths is { } m ? m : Blank.Value, null),
        ];
        foreach (var (label, value, format) in fields)
        {
            sheet.Cell(r, 1).Value = label;
            sheet.Cell(r, 1).Style.Font.Bold = true;
            var cell = sheet.Cell(r, 2);
            cell.Value = value;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            cell.Style.Alignment.WrapText = true;
            if (format is not null)
            {
                cell.Style.NumberFormat.Format = format;
            }
            FitHeight(sheet.Row(r), (value.IsText ? value.GetText() : "", DetailWidths[1]));
            r++;
        }

        r = Section(sheet, r + 1, "SẢN PHẨM");
        r = TableHeader(sheet, r, ["Mã sản phẩm", "Tên sản phẩm", "Đơn giá", "Số lượng", "Thành tiền"]);
        foreach (var i in o.Items)
        {
            sheet.Cell(r, 1).Value = i.Model;
            sheet.Cell(r, 2).Value = i.Name;
            sheet.Cell(r, 2).Style.Alignment.WrapText = true;
            Money(sheet.Cell(r, 3), i.UnitPrice);
            sheet.Cell(r, 4).Value = i.Quantity;
            Money(sheet.Cell(r, 5), i.LineTotal);
            FitHeight(sheet.Row(r), (i.Name, DetailWidths[1]));
            r++;
        }
        foreach (var (label, amount) in new[] { ("Tạm tính", o.Subtotal), ("Phí giao", o.ShippingFee), ("Tổng cộng", o.Total) })
        {
            sheet.Cell(r, 4).Value = label;
            sheet.Cell(r, 4).Style.Font.Bold = true;
            Money(sheet.Cell(r, 5), amount);
            sheet.Cell(r, 5).Style.Font.Bold = label == "Tổng cộng";
            r++;
        }

        var events = o.Events.OrderBy(e => e.CreatedAt).ThenBy(e => e.Id).ToList();
        r = Section(sheet, r + 1, $"LỊCH SỬ XỬ LÝ ({events.Count} lần)");
        r = TableHeader(sheet, r, ["Thời gian", "Ghi chú", "Thao tác", "Người thực hiện"]);
        foreach (var e in events)
        {
            var action = Action(e);
            var note = e.Note ?? "";
            sheet.Cell(r, 1).Value = VietnamTime.FromUtc(e.CreatedAt);
            sheet.Cell(r, 1).Style.NumberFormat.Format = DateFormat;
            sheet.Cell(r, 2).Value = note;
            sheet.Cell(r, 2).Style.Alignment.WrapText = true;
            sheet.Cell(r, 3).Value = action;
            sheet.Cell(r, 3).Style.Alignment.WrapText = true;
            if (e.ToStatus is not null)
            {
                sheet.Cell(r, 3).Style.Font.Bold = true;
            }
            sheet.Cell(r, 4).Value = e.Actor ?? "Khách hàng";
            FitHeight(sheet.Row(r), (note, DetailWidths[1]), (action, DetailWidths[2]));
            r++;
        }

        return Save(book);
    }

    // Hàng tiêu đề in đậm, nền xám, cố định khi cuộn; độ rộng + định dạng số cho từng cột
    private static void Header(IXLWorksheet sheet, (string Title, double Width, string? Format)[] columns)
    {
        for (var c = 0; c < columns.Length; c++)
        {
            var (title, width, format) = columns[c];
            var column = sheet.Column(c + 1);
            column.Width = width;
            if (format is not null)
            {
                column.Style.NumberFormat.Format = format;
            }
            var cell = sheet.Cell(1, c + 1);
            cell.Value = title;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F4F6");
        }
        sheet.SheetView.FreezeRows(1);
    }

    // Tiêu đề một phần trong sheet chi tiết: chữ đậm, nền xám đậm kéo hết 5 cột; trả về hàng kế tiếp
    private static int Section(IXLWorksheet sheet, int r, string title)
    {
        sheet.Cell(r, 1).Value = title;
        var range = sheet.Range(r, 1, r, DetailWidths.Length);
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#E5E7EB");
        return r + 1;
    }

    private static int TableHeader(IXLWorksheet sheet, int r, string[] titles)
    {
        for (var c = 0; c < titles.Length; c++)
        {
            var cell = sheet.Cell(r, c + 1);
            cell.Value = titles[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F4F6");
        }
        return r + 1;
    }

    private static void Money(IXLCell cell, decimal amount)
    {
        cell.Value = (double)amount;
        cell.Style.NumberFormat.Format = MoneyFormat;
    }

    // Excel không tự giãn chiều cao hàng có chữ xuống dòng khi mở file → ước lượng số dòng theo độ rộng cột (~1,2 ký tự / đơn vị
    // rộng) rồi đặt chiều cao hàng (15pt mỗi dòng)
    private static void FitHeight(IXLRow row, params (string Text, double Width)[] cells)
    {
        var lines = cells.Max(c => c.Text.Split('\n').Sum(part => Math.Max(1, (int)Math.Ceiling(part.Length / (c.Width * 1.2)))));
        if (lines > 1)
        {
            row.Height = 15 * lines;
        }
    }

    // Thao tác của một dòng lịch sử, vd. "Mới → Đã xác nhận; Đã thu tiền"
    private static string Action(OrderEvent e)
    {
        if (e.Actor is null)
        {
            return "Đặt hàng";
        }

        var parts = new List<string>();
        if (e.FromStatus is { } from && e.ToStatus is { } to)
        {
            parts.Add($"{StatusNames[from]} → {StatusNames[to]}");
        }
        if (e.IsPaid is { } paid)
        {
            parts.Add(paid ? "Đã thu tiền" : "Bỏ đánh dấu đã thu tiền");
        }
        return parts.Count > 0 ? string.Join("; ", parts) : "Ghi chú";
    }

    private static byte[] Save(XLWorkbook book)
    {
        using var stream = new MemoryStream();
        book.SaveAs(stream);
        return stream.ToArray();
    }
}

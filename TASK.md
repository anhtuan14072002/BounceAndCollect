# Task

File này lưu các thay đổi đã hoàn thành để có thể tiếp tục công việc trên máy khác.

## Quy ước cập nhật

- Chỉ thêm task sau khi thay đổi đã hoàn thành và được kiểm tra.
- Mỗi task phải ghi ngày, kết quả, file đã thay đổi và cách kiểm tra.
- Dùng đường dẫn tương đối từ root project để có thể sử dụng trên máy khác.
- Khi hoàn tác một task, xóa toàn bộ entry của task đó khỏi file này.
- Nếu chỉ hoàn tác một phần, cập nhật entry để chỉ giữ lại phần vẫn còn trong project.
- Không ghi các thử nghiệm chưa hoàn thành vào mục `Đã hoàn thành`.

## Đã hoàn thành

### 2026-07-31 - Đơn giản hóa cách truy cập config

- Chuyển `SheetConfig` thành `partial` để mỗi loại config tự khai báo API truy cập.
- `RateSummonConfig` là data class thuần, không còn kế thừa post-load interface.
- `RandomCard` không còn giữ `SheetTable<RateSummonConfig>` và chỉ lấy rate thông qua `SheetConfig`.
- Loader hỗ trợ đọc mảng được phân cách bằng ký tự `|`.
- File liên quan:
  - `Assets/_Project/Script/Config/SheetConfig.cs`
  - `Assets/_Project/Script/Config/SkillConfig.cs`
  - `Assets/_Project/Script/Config/RateSummonConfig.cs`
  - `Assets/_Project/Script/Game/Visual/RandomCard.cs`
- Kiểm tra: `Assembly-CSharp.csproj` build thành công với `0 errors`.

### 2026-07-31 - Thêm comment mô tả các hàm trong Google Sheet tool

- Thêm comment ngắn ở đầu các hàm để mô tả trách nhiệm của chúng.
- Comment chỉ thuộc phạm vi Google Sheet tool, không thêm vào config gameplay.
- File có hàm được comment:
  - `Assets/_Project/Editor/GoogleSheetBulkImporterWindow.cs`
  - `Assets/_Project/Editor/GoogleSheetDataWindow.cs`
  - `Assets/_Project/Script/Config/SheetConfig.cs`
- Hai file data model không có hàm nên không cần thêm comment:
  - `Assets/_Project/Script/Config/GoogleSheetBulkImportConfig.cs`
  - `Assets/_Project/Script/Config/GoogleSheetData.cs`
- Kiểm tra: `Assembly-CSharp-Editor.csproj` build thành công với `0 errors`.

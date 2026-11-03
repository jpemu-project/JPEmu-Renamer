// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
namespace JPEmuRenamer.Services;

// Hướng dẫn（Tiếng Việt）。書式は HelpContent を参照
internal static class HelpVi
{
    public const string Play = """
# Bắt đầu
JPEmu Renamer sắp xếp các ROM do chính bạn trích xuất và cho phép bạn chơi ngay – bằng trình giả lập tích hợp (lõi tích hợp) hoặc trình giả lập của riêng bạn (exe).

## Từ cài đặt đến chơi
- Ban đầu chưa có hệ máy nào. Dùng "+ Thêm hệ máy" ở góc dưới bên trái để tạo hệ máy với tên tùy ý (ví dụ: Famicom, NES)
- Chọn hệ máy đó và đặt thư mục ROM trong "Cài đặt hệ máy"
- Ở thẻ "Chơi", chọn một game
- Bấm "▶ Chơi". Nếu lõi chưa được cài, bạn sẽ được hỏi có muốn tải không
- Bấm `Esc` để quay về trình khởi chạy

> Hãy dùng ROM trích xuất từ phần mềm bạn sở hữu.

# Cài đặt hệ máy
Chọn hệ máy bên trái rồi mở "Cài đặt hệ máy" ở góc trên bên phải. Thêm hệ máy bằng "+ Thêm hệ máy". Chọn mẫu sẽ tự điền phần mở rộng, biểu tượng, màu và lõi tích hợp.

| Mục | Mô tả |
| Tên hệ máy | Tên hiển thị trong danh sách |
| Biểu tượng / màu | Biểu tượng trong danh sách hệ máy. Có thể dùng ảnh riêng (PNG/ICO/JPG) |
| Tệp DAT | Danh sách game dùng để đổi tên (định dạng OfflineList). Không cần nếu chỉ chơi |
| Thư mục ROM | Thư mục chứa ROM. Nếu nhiều thì cách nhau bằng `;` |
| Phần mở rộng | Phần mở rộng sẽ đọc (ví dụ `.nes;.zip`) |
| Cách chạy mặc định | Lõi tích hợp hoặc trình giả lập |
| Lõi tích hợp | Lõi dùng cho hệ máy này. Lấy bằng "Tải xuống" / "Cập nhật" |
| Trình giả lập / tham số | Dùng khi chơi bằng trình giả lập bên ngoài |

## Hệ máy cần BIOS
Famicom Disk System, Mega-CD, PC Engine CD-ROM², Saturn, Neo Geo và một số hệ khác cần tệp BIOS. Tên tệp hiển thị bên dưới lõi tích hợp trong "Cài đặt hệ máy". Hãy đặt chúng vào "Cài đặt chơi" → "Nâng cao" → "Thư mục BIOS" (mặc định: `system` trong thư mục cài đặt).

# Chạy game
## Cách chạy
- Chọn game rồi bấm "▶ Chơi"
- Nhấp đúp vào một game
- Chọn game rồi bấm `Enter`

## Đổi cách chạy
Nhấp chuột phải vào game rồi chọn "Chạy bằng lõi tích hợp" hoặc "Chạy bằng trình giả lập (exe)". Cách mặc định được đặt trong "Cài đặt hệ máy".

## Tham số của trình giả lập
| Ký hiệu | Được thay bằng |
| `{rom}` | Đường dẫn đầy đủ của ROM |
| `{romdir}` | Thư mục chứa ROM |
| `{romname}` | Tên tệp ROM không có phần mở rộng |

# Điều khiển
## Bố cục bàn phím mặc định
Tên nút theo bố cục Super Famicom (A bên phải, B bên dưới).

| Thao tác | Phím |
| Phím hướng | `↑` `↓` `←` `→` |
| A / B | `X` / `Z` |
| X / Y | `S` / `A` |
| L / R | `Q` / `W` |
| L2 / R2 | `E` / `R` |
| Select / Start | `Shift phải` / `Enter` |

## Tay cầm
Tay cầm đã kết nối được nhận diện tự động và gán bố cục chuẩn: Xbox (XInput), tay cầm USB thông thường (DirectInput), Switch Pro Controller, DualSense và các loại khác. Người chơi 1 dùng tay cầm kết nối đầu tiên, người chơi 2 dùng tay cầm thứ hai. Cần trái cũng dùng được như phím hướng. Rung hoạt động trong các game có hỗ trợ.

## Phím tắt
| Thao tác | Phím | Tay cầm |
| Menu | `F1` | Nút Guide |
| Tua nhanh (giữ) | `Tab` | |
| Lưu nhanh | `F2` | |
| Tải bản lưu nhanh | `F4` | |
| Ô − / + | `F6` / `F7` | |
| Chụp màn hình | `F12` | |
| Bật/tắt toàn màn hình | `F11` hoặc `Alt+Enter` | |
| Tạm dừng | `P` | |
| Khởi động lại | `F10` | |
| Thoát | `Esc` | |

> Mọi phím và nút đều đổi được trong "Cài đặt chơi". Khi đang toàn màn hình, `Esc` sẽ quay về cửa sổ trước.

# Menu trong game
`F1` (hoặc nút Guide trên tay cầm) mở menu và tạm dừng game. Dùng `↑` `↓` để chọn, `Enter` (A) để xác nhận và `Esc` (B) để đóng.

| Mục | Mô tả |
| Quay lại game | Đóng menu |
| Lưu nhanh / tải | Ở ô hiện tại |
| Đổi ô | Chuyển giữa các ô |
| Chụp màn hình | Lưu màn hình game thành PNG |
| Toàn màn hình / cửa sổ | Đổi cách hiển thị |
| Cài đặt chơi… | Đổi cài đặt khi đang chơi; áp dụng ngay |
| Khởi động lại | Chơi lại từ đầu |
| Về trình khởi chạy | Kết thúc game |

# Dữ liệu lưu và lưu nhanh
## Dữ liệu lưu (pin)
Những gì bạn lưu trong game được tự động lưu lại (khoảng mỗi 10 giây và khi thoát) và tải lại vào lần sau.

## Lưu nhanh
Lưu đúng khoảnh khắc hiện tại của game. `F2` để lưu, `F4` để tải, `F6` / `F7` để đổi ô (mặc định 10 ô, từ 0 đến 9).

## Vị trí lưu
| Loại | Vị trí mặc định |
| Dữ liệu lưu | `saves\<hệ máy>` trong thư mục cài đặt |
| Lưu nhanh | `states\<hệ máy>` trong thư mục cài đặt |
| Ảnh chụp màn hình | `screenshots\<hệ máy>` trong thư mục cài đặt |

Thư mục cài đặt là `%APPDATA%\JPEmuRenamer` (hoặc `data` cạnh tệp exe nếu có `portable.txt`). Có thể đổi vị trí trong "Cài đặt chơi".

# Cài đặt chơi
Mở bằng nút "Cài đặt chơi" ở thẻ Chơi, nhấp chuột phải vào game, từ menu trong game hoặc từ "Cài đặt" của ứng dụng.

## Tất cả, hệ máy và game
Cài đặt chồng lên nhau theo thứ tự "Tất cả → Hệ máy → Game". Chọn cấp muốn sửa ở phía trên. Cấp cao hơn có thể thay thế giá trị của cấp thấp hơn. Mục bị thay thế có dấu ●, và ↺ để hủy thay thế. Ví dụ: cửa sổ cho mọi thứ, toàn màn hình chỉ cho một game.

## Thẻ "Cơ bản"
| Nhóm | Mô tả |
| Tay cầm & bàn phím | Điều khiển của người chơi 1. Nhấp vào mục rồi bấm nút hoặc phím (`Esc` để hủy, `Delete` để xóa) |
| Nơi lưu | Thư mục của dữ liệu lưu, lưu nhanh và ảnh chụp màn hình |
| Hình ảnh | Cửa sổ/toàn màn hình, kích thước cửa sổ (1x/2x/3x/vừa màn hình), tỷ lệ khung hình |
| Âm thanh | Âm lượng, tắt tiếng, thiết bị phát |

## Thẻ "Nâng cao"
| Nhóm | Mô tả |
| Tay cầm & bàn phím | Người chơi 1–4, turbo và tốc độ, vùng chết của cần, phím tắt. Hỗ trợ tổ hợp phím (ví dụ `Alt+Enter`) |
| Nơi lưu | Thư mục con (không chia/theo hệ máy/theo game), lưu cạnh ROM, số ô lưu nhanh, thư mục BIOS |
| Hình ảnh | Độ phân giải và tần số quét khi toàn màn hình, phóng theo số nguyên, lọc hình, shader (đường quét/CRT), V-Sync |
| Âm thanh | Tần số lấy mẫu, chế độ phát (WASAPI dùng chung/độc quyền), độ trễ, hiệu chỉnh đồng bộ âm thanh |
| Tùy chọn lõi | Tùy chọn riêng của lõi (ví dụ bảng màu NES, tỷ lệ khung hình, ép xung). Xuất hiện sau khi chạy game bằng lõi đó một lần |

"Đặt lại thẻ này" đưa thẻ về giá trị mặc định ở cấp đang chọn.

# Xem trước tự động
Ở thẻ "Chơi", chọn một game và chờ một chút – cảnh trong game sẽ bắt đầu phát ở góc trên bên phải. Lõi tích hợp chạy không có âm thanh và điều khiển, hiển thị đoạn demo sau màn hình tiêu đề. Cần cài lõi tích hợp của hệ máy.

| Mục | Mặc định |
| Bắt đầu | 0,3 giây sau khi chọn (không bắt đầu khi đang cuộn nhanh) |
| Tốc độ | 4x |
| Bỏ qua đoạn đầu | 10 giây đầu (logo…) |
| Phát lại sau | 60 giây |
| Âm thanh / điều khiển | Không có (không ảnh hưởng dữ liệu lưu) |

Đổi các giá trị này, hoặc tắt xem trước, trong "Cài đặt" → "Xem trước tự động".

## Ảnh chụp
- Khi đang xem trước, "Lưu màn hình này" ở góc dưới bên phải lưu khoảnh khắc đó thành ảnh chụp
- Nhấp chuột phải vào game → "Tạo ảnh chụp cho hệ máy này…" sẽ chạy lần lượt mọi ROM của hệ máy ở chế độ nền và lưu màn hình tại một thời điểm cố định (mặc định 30 giây). Có thể bỏ qua game đã có ảnh chụp
- Ảnh chụp hiển thị trước khi xem trước bắt đầu và với các game có màn hình đứng yên
- Được lưu thành `snapshots\<hệ máy>\<tên game>.png` trong thư mục cài đặt

> Chạy game khi đang xem trước hoặc đang tạo ảnh chụp sẽ dừng xem trước trước.

# Tải và cập nhật lõi
Mở "Cài đặt" → "Tải / cập nhật lõi tích hợp".

- Danh sách hiển thị hệ máy, dung lượng và trạng thái của từng lõi (chưa cài / mới nhất / có cập nhật)
- Chọn lõi bằng "Lõi của hệ máy của tôi", "Chưa cài", "Có cập nhật"… rồi bấm "Tải"
- "Cập nhật tất cả" cập nhật mọi lõi có bản mới

> Lõi được tải từ trang chính thức của libretro. Không thể cập nhật lõi đang được game sử dụng.

# Khắc phục sự cố
| Vấn đề | Cần kiểm tra |
| Không chạy được / "không tải được game" | Nếu hệ máy cần BIOS, hãy kiểm tra thư mục BIOS. Thử lõi khác cũng có thể được |
| "Không dùng được OpenGL" | Lõi dùng OpenGL (ví dụ N64). Cập nhật driver đồ họa hoặc chọn lõi khác |
| Âm thanh rè hoặc bị trễ | Tăng độ trễ hoặc bật hiệu chỉnh đồng bộ âm thanh trong "Cài đặt chơi" → "Nâng cao" → "Âm thanh" |
| Hình bị giật | Tắt V-Sync hoặc đóng ứng dụng khác. Lõi nặng như N64 cần máy tính mạnh |
| Tay cầm không phản hồi | Tay cầm được nhận diện sẽ hiện ở đầu "Cài đặt chơi". Nếu không thấy, hãy cắm lại. Tay cầm đầu tiên là người chơi 1 |
| Độ phân giải không trở lại | Thoát game hoặc bấm `Esc` để quay về cửa sổ |
| Lần bấm đầu không ăn | Một số game không nhận điều khiển trong vài giây sau khi khởi động. Hãy chờ một chút |
| Không có xem trước tự động | Kiểm tra đã cài lõi tích hợp của hệ máy và đã bật xem trước tự động trong "Cài đặt". Xem trước tạm dừng khi ứng dụng chạy nền |
""";

    public const string Rename = """
# Cách đổi tên hoạt động
Ứng dụng tính CRC32 (dấu vân tay nội dung) của từng ROM rồi so với tệp DAT (danh sách tên game và CRC32). Chỉ những tệp khớp mới được đổi sang tên đúng trong DAT, bất kể tên hiện tại là gì.

> Bạn luôn kiểm tra danh sách trước khi đổi tên, và "Hoàn tác" khôi phục toàn bộ lần chạy sau đó.

# Chuẩn bị
## Lấy tệp DAT
Hãy dùng DAT định dạng OfflineList (XML) cho từng hệ máy. DAT cũ mã Shift-JIS cũng được hỗ trợ.

## Cài đặt hệ máy
Chọn hệ máy bên trái và đặt các mục sau trong "Cài đặt hệ máy".

| Mục | Mô tả |
| Tệp DAT | Tệp DAT |
| Thư mục ROM | Thư mục chứa ROM (cách nhau bằng `;`) |
| Phần mở rộng | Phần mở rộng sẽ đọc (ví dụ `.nes;.zip`) |
| Xử lý header | Có tính header iNES/FDS vào CRC32 hay không (thường để "Tự động") |

Việc đọc và so khớp bắt đầu tự động. Từ lần thứ hai, các tệp không thay đổi được bỏ qua nên rất nhanh.

# Thẻ "Đổi tên"
Chuyển sang thẻ "Đổi tên" ở trên để xem bảng.

| Cột | Mô tả |
| ✓ | Có đổi tên hay không (chỉ chọn được các dòng "Cần đổi tên") |
| Trạng thái | Kết quả so khớp (xem bên dưới) |
| Tên tệp hiện tại | Tên hiện giờ |
| → Tên trong DAT | Tên sau khi đổi |
| CRC32 / dung lượng / định dạng | Thông tin ROM |

## Trạng thái
| Trạng thái | Ý nghĩa | Đổi tên |
| Khớp | CRC32 khớp và tên đã đúng | Không cần |
| Cần đổi tên | CRC32 khớp nhưng tên khác | Có |
| Trùng lặp | Đã có tệp cho cùng tên game | Không |
| Không có trong DAT | CRC32 không có trong DAT | Không |
| Còn thiếu | Có trong DAT nhưng bạn chưa có tệp | ― |

# Đổi tên từng bước
- Ở thẻ "Đổi tên", kiểm tra các dòng "Cần đổi tên" (ban đầu đều được chọn)
- Bỏ chọn những dòng không muốn đổi. Ô ở tiêu đề cột chọn/bỏ tất cả
- Bên phải hiện "Trước" và "Sau" của dòng đang chọn
- Bấm "Đổi tên" ở dưới và xác nhận bằng "Có"
- Số lượng được hiển thị và danh sách tự cập nhật

> Chỉ những tệp có CRC32 khớp với DAT mới bị thay đổi. Tệp sẽ bị bỏ qua nếu đã có tệp mang tên mới.

## Tên ROM trong tệp ZIP
Khi bật "Cài đặt" → "Đổi tên cả ROM bên trong tệp ZIP", ROM trong mỗi ZIP sẽ được đổi tên cùng với ZIP.

## Ký tự Windows không cho phép
`\` `/` `:` `*` `?` `"` `<` `>` `|` được thay bằng ký tự toàn chiều rộng tương ứng.

# Hoàn tác
"Hoàn tác" ở cuối thẻ "Đổi tên" khôi phục toàn bộ lần chạy gần nhất. Bấm lần nữa để khôi phục lần trước đó (tối đa 50 lần gần nhất).

> "Hoàn tác" luôn khôi phục lần chạy mới nhất, bất kể hệ máy. Hộp xác nhận hiển thị ngày và số tệp.

# Tính năng tiện ích
| Tính năng | Cách dùng |
| Đã có / Tất cả / Còn thiếu | Lọc danh sách. "Còn thiếu" hiển thị game bạn chưa có |
| Tìm kiếm | Gõ vào ô tìm kiếm để lọc theo tên game hoặc tên tệp |
| So khớp lại | Dùng sau khi thêm hoặc xóa ROM |
| Ẩn phần mở rộng | "Cài đặt" → "Ẩn phần mở rộng tệp (.zip…) trong danh sách game" |
| Đọc thư mục con | "Cài đặt" → "Đọc cả thư mục con" |
| Chỉ hiển thị tên DAT | Dành cho arcade, nơi đổi tên tệp làm trình giả lập không chạy được. Bật trong "Cài đặt hệ máy" để giữ tên tệp và hiển thị tên DAT trong danh sách |

# Khắc phục sự cố
| Vấn đề | Cần kiểm tra |
| Không có gì khớp | Kiểm tra DAT có đúng hệ máy không. Đổi "Xử lý header" sang "Có header" hoặc "Không có header" có thể giúp ích |
| "Không đọc được tệp DAT" | Hãy chắc đó là DAT OfflineList (XML) |
| "Trùng lặp" | Bạn có từ hai ROM trở lên của cùng một game. Hãy chuyển một tệp sang thư mục khác |
| Bị bỏ qua kèm "(đã tồn tại)" | Đã có tệp mang tên mới. Hãy quyết định giữ tệp nào rồi sắp xếp thủ công |
| Tên bên trong ZIP không đổi | Nếu tên của chính ZIP đã đúng, tên bên trong được giữ nguyên |
""";
}

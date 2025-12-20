# Hướng Dẫn Cài Đặt và Cấu Hình Jenkins cho PartTimeJob

Tài liệu này hướng dẫn chi tiết cách thiết lập hệ thống CI/CD sử dụng Jenkins cho dự án PartTimeJob.

## 1. Yêu cầu hệ thống (Prerequisites)
Đảm bảo máy của bạn đã cài đặt:
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/) (cho Mac/Windows) hoặc Docker Engine (cho Linux).
*   Git.

## 2. Khởi động Jenkins Server
Chúng tôi đã chuẩn bị sẵn cấu hình Docker Compose để chạy Jenkins với đầy đủ môi trường cần thiết (bao gồm Docker CLI để build image).

1.  Mở Terminal tại thư mục gốc của dự án.
2.  Chạy lệnh sau để khởi động Jenkins:
    ```bash
    docker compose -f jenkins/docker-compose.yml up -d
    ```
3.  Kiểm tra xem container đã chạy chưa:
    ```bash
    docker ps
    ```
    Bạn sẽ thấy container tên là `jenkins_server` đang chạy.

## 3. Mở khóa Jenkins (Unlock Jenkins)
Lần đầu truy cập, Jenkins yêu cầu mật khẩu quản trị viên (Admin Password).

1.  Truy cập trình duyệt: [http://localhost:8080](http://localhost:8080)
2.  Lấy mật khẩu bằng lệnh sau trong Terminal:
    ```bash
    docker logs jenkins_server
    ```
    *Tìm dòng text nằm giữa 2 hàng dấu sao `***`, ví dụ: `b2f341f9c3df42a690d5fc18b21079af`*
3.  Copy mật khẩu và dán vào ô **Administrator password** trên trình duyệt -> Nhấn **Continue**.

## 4. Cấu hình ban đầu
1.  **Customize Jenkins**: Chọn **Install suggested plugins**.
2.  Đợi quá trình cài đặt plugin hoàn tất (có thể mất vài phút).
3.  **Create First Admin User**: Điền thông tin tài khoản admin bạn muốn sử dụng (Username, Password, Full name, Email) -> Nhấn **Save and Continue**.
4.  **Instance Configuration**: Giữ nguyên `http://localhost:8080` -> Nhấn **Save and Finish**.
5.  Nhấn **Start using Jenkins**.

## 5. Tạo Pipeline Job
Bây giờ chúng ta sẽ kết nối Jenkins với dự án của bạn.

1.  Tại trang chủ (Dashboard), nhấn **New Item** (ở menu bên trái).
2.  **Enter an item name**: Điền tên Job (ví dụ: `PartTimeJob-Pipeline`).
3.  Chọn loại **Pipeline**.
4.  Nhấn **OK**.

> [!IMPORTANT]
> **Cài đặt Plugin Docker:**
> Để Jenkinsfile hoạt động (do có dùng `agent { docker ... }`), bạn cần cài thêm plugin **Docker Pipeline**.
> 1.  Vào **Manage Jenkins** -> **Plugins**.
> 2.  Chọn tab **Available plugins**.
> 3.  Tìm kiếm từ khóa: `Docker Pipeline`.
> 4.  Tích chọn nó và nhấn **Install**.
> 5.  Đợi cài xong (có thể khởi động lại Jenkins nếu cần), sau đó mới chạy Job.

### Cấu hình Job:
Trong trang cấu hình vừa mở ra, kéo xuống phần **Pipeline**:
*   **Definition**: Chọn `Pipeline script from SCM`. (Điều này bảo Jenkins đọc file `Jenkinsfile` từ code).
*   **SCM**: Chọn `Git`.
*   **Repository URL**:
    *   *Nếu chạy Local (trên máy cá nhân)*: Bạn có thể điền đường dẫn tuyệt đối tới thư mục dự án (ví dụ: `/Users/vanthe/Sites/job/partimejob`).
    *   *Lưu ý quan trọng*: Để Jenkins (đang chạy trong Docker) nhìn thấy folder này, folder đó cần được mount vào container hoặc bạn nên đẩy code lên **GitHub** và dùng link HTTPS (ví dụ: `https://github.com/username/repo.git`). **Khuyến khích dùng GitHub.**
    
    > [!TIP]
    > **Cách lấy Token GitHub và Setup Credentials (nếu dùng Private Repo):**
    >
    > 1.  **Lấy Token GitHub:**
    >     *   Truy cập [GitHub Settings > Developer settings > Personal access tokens > Tokens (classic)](https://github.com/settings/tokens).
    >     *   Chọn **Generate new token (classic)**.
    >     *   Đặt Note (ví dụ: `Jenkins Token`).
    >     *   Chọn scopes: tích chọn **repo** (Full control of private repositories).
    >     *   Nhấn **Generate token** và **COPY** token đó ngay (bạn sẽ không thấy lại nó nữa).
    >
    > 2.  **Setup Credentials trong Jenkins:**
    >     *   Quay lại trang cấu hình Job.
    >     *   Dưới dòng **Credentials**, nhấn nút **Add** -> **Jenkins**.
    >     *   **Domain**: Global credentials (unrestricted).
    >     *   **Kind**: Chọn **Username with password**.
    >     *   **Username**: Tên tài khoản GitHub của bạn.
    >     *   **Password**: Dán cái **Token** vừa copy ở bước 1 vào đây.
    >     *   **ID**: Đặt tên dễ nhớ (ví dụ: `github-auth`).
    >     *   **Description**: Mô tả (ví dụ: `GitHub Access Token`).
    >     *   Nhấn **Add**.
    >
    > 3.  **Chọn Credentials:**
    >     *   Tại dòng **Credentials**, bấm vào dropdown và chọn cái bạn vừa tạo (`github-auth`).
*   **Branch Specifier**: Điền `*/main` (hoặc tên branch bạn đang làm việc).
*   **Script Path**: Điền `Jenkinsfile` (đã mặc định).

Nhấn **Save**.

## 6. Vận hành quy trình CI/CD (Running CI/CD)

Hiện tại, Pipeline của bạn đã xác định 2 giai đoạn chính:
*   **CI (Continuous Integration)**: Build Code (.NET) và chạy Unit Test.
*   **CD (Continuous Deployment)**: Đóng gói Docker Image và Deploy container mới.

### Cách 1: Kích hoạt thủ công (Manual Trigger)
Đây là cách chúng ta đang dùng hiện tại.

1.  **Push Code**: Commit và push code thay đổi của bạn lên GitHub (`main` branch).
2.  **Truy cập Jenkins**: Vào trang Job bạn đã tạo.
3.  **Bấm "Build Now"**: Nút này ở menu bên trái.
4.  **Theo dõi**: Nhìn vào phần **Build History** bên dưới, bạn sẽ thấy một bản build mới (có thanh tiến trình nhấp nháy).

### Cách 2: Tự động kích hoạt (Webhook) - Khuyên dùng
Cách này giúp Pipeline tự chạy ngay khi có code mới được merge vào nhánh `main`.

#### 1. Cấu hình trên Jenkins
1.  Vào Job của bạn -> **Configure**.
2.  Tìm mục **Build Triggers**.
3.  Tích chọn: **GitHub hook trigger for GITScm polling**.
4.  Nhấn **Save**.

#### 2. Cấu hình Webhook trên GitHub
Để GitHub "báo" cho Jenkins biết, Jenkins cần một địa chỉ IP công khai (Public IP). Vì bạn đang chạy **Localhost**, GitHub không thể gọi trực tiếp được.
> **Giải pháp**: Sử dụng **ngrok** để public localhost ra internet.

**Bước A: Cài đặt và chạy ngrok**
1.  Tải và cài đặt [ngrok](https://ngrok.com/).
2.  Mở Terminal (máy thật), chạy lệnh:
    ```bash
    ngrok http 8080
    ```
3.  Copy đường dẫn HTTPS mà ngrok sinh ra (ví dụ: `https://abcd-123.ngrok-free.app`).

**Bước B: Add Webhook trên GitHub**
1.  Vào Repository trên GitHub -> **Settings**.
2.  Chọn menu **Webhooks** (bên trái) -> **Add webhook**.
3.  **Payload URL**: Điền đường dẫn của Jenkins theo format:
    `[LINK_NGROK]/github-webhook/`
    *   Ví dụ: `https://abcd-123.ngrok-free.app/github-webhook/` (Lưu ý dấu `/` ở cuối rất quan trọng).
4.  **Content type**: Chọn `application/json`.
5.  **Which events would you like to trigger this webhook?**: Chọn `Just the push event`.
6.  Nhấn **Add webhook**.

Bây giờ, mỗi khi bạn `git push`, GitHub sẽ gọi tới ngrok -> ngrok chuyển tiếp vào Jenkins Local -> Jenkins tự động chạy Build!

### Theo dõi kết quả (Monitoring)
*   **Console Output**: Bấm vào số thứ tự của Build (ví dụ `#5`) -> Chọn **Console Output** để xem log chi tiết từng bước.
*   **Stage View**: Trên trang chính của Job, bạn sẽ thấy biểu đồ các bước (Checkout, Build & Test, Deploy).
    *   **Màu Xanh lá**: Thành công.
    *   **Màu Đỏ**: Thất bại (hãy xem logs để fix).

### Kiểm tra sau khi Deploy (Verification)
Sau khi Pipeline chạy xong (màu xanh toàn bộ):
1.  API Server (`ptj_api`) đã được cập nhật code mới nhất.
2.  Truy cập Swagger để kiểm tra: [http://localhost:5000/swagger](http://localhost:5000/swagger)
3.  Thử gọi một vài API để đảm bảo server hoạt động ổn định.

## 7. Troubleshooting (Gỡ lỗi)
*   **Lỗi "Permission denied" với `/var/run/docker.sock`**:
    *   Chúng tôi đã cấu hình Jenkins chạy dưới quyền `root` trong `jenkins/docker-compose.yml` để khắc phục điều này. Nếu vẫn bị, hãy đảm bảo Docker Desktop đã cấp quyền truy cập file socket.
*   **Lỗi không tìm thấy file**:
    *   Đảm bảo bạn đã commit file `Jenkinsfile` và `docker-compose.yml` lên Git nếu dùng GitHub URL.

## 8. Cấu trúc thư mục liên quan
*   `jenkins/Dockerfile`: File cấu hình image Jenkins tùy chỉnh (có cài Docker CLI).
*   `jenkins/docker-compose.yml`: File chạy Jenkins server.
*   `Jenkinsfile`: File định nghĩa các bước chạy CI/CD.

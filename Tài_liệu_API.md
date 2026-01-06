# **Tài liệu API - Job Students Platform**

**Base URL:** `/api`

---

## **1. Authentication (Xác thực)**

### 1.1 Đăng ký tài khoản

**Endpoint:** `POST /api/auth/register`
**Quyền:** Public
**Mô tả:** Đăng ký tài khoản mới (STUDENT hoặc EMPLOYER)

**Request Body:**

```json
{
  "email": "user@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "fullName": "Nguyen Van A",
  "phoneNumber": "0912345678"
}
```

**Response:**

```json
{
  "userId": 1,
  "email": "user@example.com",
  "fullName": "Nguyen Van A",
  "roles": ["STUDENT"],
  "accessToken": "eyJhbGci...",
  "refreshToken": "abc123xyz...",
  "expiresAt": "2026-10-27T10:00:00Z"
}
```

### 1.2 Đăng nhập

**Endpoint:** `POST /api/auth/login`
**Quyền:** Public

**Request Body:**

```json
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Response:** Tương tự Đăng ký

### 1.3 Refresh Token

**Endpoint:** `POST /api/auth/refresh`
**Quyền:** Public
**Mô tả:** Lấy Access Token mới khi token cũ hết hạn

**Request Body:**

```json
{
  "refreshToken": "abc123xyz..."
}
```

**Response:** Tương tự Đăng nhập

### 1.4 Đăng xuất

**Endpoint:** `POST /api/auth/revoke`
**Quyền:** Authenticated
**Mô tả:** Thu hồi Refresh Token

**Request Body:**

```json
{
  "refreshToken": "abc123xyz..."
}
```

---

## **2. Job Posts (Tin tuyển dụng)**

### 2.1 Lấy chi tiết tin tuyển dụng

**Endpoint:** `GET /api/jobposts/{id}`
**Quyền:** Public

### 2.2 Lấy danh sách tin tuyển dụng

**Endpoint:** `GET /api/jobposts`
**Quyền:** Public
**Query Params:**

- `pageNumber` (default: 1)
- `pageSize` (default: 10)

### 2.3 Tìm kiếm tin tuyển dụng

**Endpoint:** `GET /api/jobposts/search`
**Quyền:** Public
**Query Params:**

- `searchTerm`: Từ khóa tìm kiếm
- `pageNumber` (default: 1)
- `pageSize` (default: 10)
- `sortDescending` (default: true)

### 2.4 Lấy tin tuyển dụng theo công ty

**Endpoint:** `GET /api/jobposts/company/{companyId}`
**Quyền:** Public
**Query Params:**

- `pageNumber` (default: 1)
- `pageSize` (default: 10)

### 2.5 Tạo tin tuyển dụng

**Endpoint:** `POST /api/jobposts`
**Quyền:** `EMPLOYER`, `ADMIN`

**Request Body:**

```json
{
  "title": "Part-time English Tutor",
  "description": "Teaching English for kids...",
  "requirements": "IELTS 6.5+",
  "benefits": "Flexible hours",
  "salaryMin": 50000,
  "salaryMax": 100000,
  "salaryPeriod": "Hour",
  "location": "Ho Chi Minh City",
  "workType": "Part-time",
  "category": "Education",
  "numberOfPositions": 5,
  "applicationDeadline": "2026-12-31T23:59:59",
  "shifts": [
    { "startTime": "18:00", "endTime": "20:00", "dayOfWeek": "Monday" }
  ],
  "requiredSkills": ["English", "Teaching"]
}
```

### 2.6 Cập nhật tin tuyển dụng

**Endpoint:** `PUT /api/jobposts/{id}`
**Quyền:** `EMPLOYER`, `ADMIN`
**Request Body:** Tương tự Create (dùng `UpdateJobPostDto`)

### 2.7 Xóa tin tuyển dụng

**Endpoint:** `DELETE /api/jobposts/{id}`
**Quyền:** `EMPLOYER`, `ADMIN`

### 2.8 Thay đổi trạng thái tin tuyển dụng

**Endpoint:** `PATCH /api/jobposts/{id}/status`
**Quyền:** `EMPLOYER`, `ADMIN`

**Request Body:**

```json
0 // JobPostStatus: 0=Draft, 1=Open, 2=Closed
```

---

## **3. Applications (Đơn ứng tuyển)**

### 3.1 Lấy chi tiết đơn ứng tuyển

**Endpoint:** `GET /api/applications/{id}`
**Quyền:** Authenticated

### 3.2 Lấy đơn ứng tuyển theo Job Post (Employer)

**Endpoint:** `GET /api/applications/job/{jobPostId}`
**Quyền:** `EMPLOYER`, `ADMIN`
**Query Params:**

- `pageNumber` (default: 1)
- `pageSize` (default: 10)

### 3.3 Lấy danh sách đơn của tôi (Student)

**Endpoint:** `GET /api/applications/me`
**Quyền:** `STUDENT`, `ADMIN`
**Query Params:**

- `pageNumber` (default: 1)
- `pageSize` (default: 10)

### 3.4 Nộp đơn ứng tuyển (Student)

**Endpoint:** `POST /api/applications`
**Quyền:** `STUDENT`, `ADMIN`

**Request Body:**

```json
{
  "jobPostId": 123,
  "coverLetter": "I am very interested in this position...",
  "resumeUrl": "https://example.com/cv.pdf"
}
```

### 3.5 Cập nhật trạng thái đơn (Employer)

**Endpoint:** `PATCH /api/applications/{id}/status`
**Quyền:** `EMPLOYER`, `ADMIN`

**Request Body:**

```json
{
  "statusId": 2, // 1=Pending, 2=Approved, 3=Rejected
  "notes": "Good candidate"
}
```

### 3.6 Rút đơn ứng tuyển (Student)

**Endpoint:** `POST /api/applications/{id}/withdraw`
**Quyền:** `STUDENT`, `ADMIN`

---

## **4. Companies (Công ty)**

### 4.1 Lấy chi tiết công ty

**Endpoint:** `GET /api/companies/{id}`
**Quyền:** Public

### 4.2 Lấy danh sách công ty

**Endpoint:** `GET /api/companies`
**Quyền:** Public
**Query Params:**

- `pageNumber` (default: 1)
- `pageSize` (default: 10)

### 4.3 Tìm kiếm công ty

**Endpoint:** `GET /api/companies/search`
**Quyền:** Public
**Query Params:**

- `searchTerm`: Từ khóa tìm kiếm (name, description, industry, address)
- `pageNumber` (default: 1)
- `pageSize` (default: 10)
- `sortDescending` (default: true)

### 4.4 Lấy công ty của tôi (Employer)

**Endpoint:** `GET /api/companies/me`
**Quyền:** `EMPLOYER`, `ADMIN`

### 4.5 Đăng ký công ty

**Endpoint:** `POST /api/companies`
**Quyền:** Authenticated
**Mô tả:** Gửi yêu cầu đăng ký công ty (cần Admin duyệt)

**Request Body:**

```json
{
  "name": "Tech Solutions Ltd.",
  "description": "Software Outsourcing",
  "address": "District 1, HCMC",
  "website": "https://techsolutions.com",
  "taxCode": "0123456789",
  "industry": "IT",
  "employeeCount": 50,
  "foundedYear": "2015-01-01T00:00:00"
}
```

### 4.6 Cập nhật thông tin công ty

**Endpoint:** `PUT /api/companies/{id}`
**Quyền:** `EMPLOYER`, `ADMIN`
**Request Body:** Tương tự Create

### 4.7 Xóa công ty

**Endpoint:** `DELETE /api/companies/{id}`
**Quyền:** `EMPLOYER`, `ADMIN`

---

## **5. Company Requests (Duyệt công ty - Admin)**

### 5.1 Lấy danh sách yêu cầu đang chờ duyệt

**Endpoint:** `GET /api/companyrequests/pending`
**Quyền:** `ADMIN`
**Query Params:**

- `pageNumber` (default: 1)
- `pageSize` (default: 10)

### 5.2 Lấy chi tiết yêu cầu

**Endpoint:** `GET /api/companyrequests/{id}`
**Quyền:** `ADMIN`

### 5.3 Duyệt yêu cầu

**Endpoint:** `POST /api/companyrequests/approve`
**Quyền:** `ADMIN`

**Request Body:**

```json
{
  "requestId": 123
}
```

### 5.4 Từ chối yêu cầu

**Endpoint:** `POST /api/companyrequests/reject`
**Quyền:** `ADMIN`

**Request Body:**

```json
{
  "requestId": 123,
  "rejectionReason": "Missing tax documents"
}
```

---

## **6. Profiles (Hồ sơ ứng viên)**

### 6.1 Lấy chi tiết profile

**Endpoint:** `GET /api/profiles/{id}`
**Quyền:** Public

### 6.2 Lấy profile của tôi

**Endpoint:** `GET /api/profiles/me`
**Quyền:** `STUDENT`, `ADMIN`

### 6.3 Tạo/Cập nhật profile

**Endpoint:** `POST /api/profiles`
**Quyền:** `STUDENT`, `ADMIN`

**Request Body:**

```json
{
  "firstName": "Nguyen",
  "lastName": "An",
  "dateOfBirth": "2000-01-01T00:00:00",
  "gender": 0,
  "address": "123 Nguyen Hue",
  "city": "Ho Chi Minh",
  "district": "District 1",
  "studentId": "SE12345",
  "university": "FPT University",
  "major": "Software Engineering",
  "gpa": 3.5,
  "yearOfStudy": 3,
  "expectedGraduationDate": "2027-06-01T00:00:00",
  "resumeUrl": "https://example.com/cv.pdf",
  "bio": "Hardworking student...",
  "linkedInUrl": "https://linkedin.com/in/...",
  "gitHubUrl": "https://github.com/...",
  "skills": [
    {
      "skillName": "C#",
      "proficiencyLevel": 4,
      "yearsOfExperience": 2
    }
  ],
  "experiences": [
    {
      "companyName": "ABC Tech",
      "position": "Intern Developer",
      "description": "Developed web apps...",
      "startDate": "2024-01-01T00:00:00",
      "endDate": "2024-06-30T00:00:00",
      "isCurrentlyWorking": false
    }
  ],
  "educations": [
    {
      "institutionName": "FPT University",
      "degree": "Bachelor",
      "fieldOfStudy": "Software Engineering",
      "startDate": "2021-09-01T00:00:00",
      "endDate": "2025-06-30T00:00:00",
      "gpa": 3.5,
      "description": "Specialized in AI"
    }
  ],
  "certificates": [
    {
      "name": "AWS Certified Developer",
      "issuingOrganization": "Amazon",
      "issueDate": "2024-01-01T00:00:00",
      "expiryDate": "2027-01-01T00:00:00",
      "credentialId": "ABC123",
      "credentialUrl": "https://aws.amazon.com/verify",
      "certificateFileUrl": "https://example.com/cert.pdf"
    }
  ]
}
```

### 6.4 Xóa profile

**Endpoint:** `DELETE /api/profiles/{id}`
**Quyền:** `STUDENT`, `ADMIN`

---

## **7. Chat (Tin nhắn)**

### 7.1 Tạo/Lấy conversation

**Endpoint:** `POST /api/chat/conversations`
**Quyền:** Authenticated

**Request Body:**

```json
{
  "recipientId": 5,
  "jobPostId": 10 // Optional
}
```

### 7.2 Lấy danh sách conversations

**Endpoint:** `GET /api/chat/conversations`
**Quyền:** Authenticated
**Query Params:**

- `pageNumber` (default: 1)
- `pageSize` (default: 20)

### 7.3 Lấy tin nhắn trong conversation

**Endpoint:** `GET /api/chat/conversations/{conversationId}/messages`
**Quyền:** Authenticated
**Query Params:**

- `pageNumber` (default: 1)
- `pageSize` (default: 50)

### 7.4 Gửi tin nhắn

**Endpoint:** `POST /api/chat/messages`
**Quyền:** Authenticated

**Request Body:**

```json
{
  "conversationId": 10,
  "recipientId": 5, // Optional nếu đã có conversationId
  "jobPostId": 3, // Optional
  "content": "Hello, is the job still available?"
}
```

### 7.5 Đánh dấu đã đọc

**Endpoint:** `POST /api/chat/conversations/{conversationId}/read`
**Quyền:** Authenticated

### 7.6 Lấy số tin nhắn chưa đọc

**Endpoint:** `GET /api/chat/unread-count`
**Quyền:** Authenticated

---

## **8. AI Chat (Trợ lý AI)**

### 8.1 Gửi tin nhắn AI

**Endpoint:** `POST /api/aichat/message`
**Quyền:** Authenticated

**Request Body:**

```json
{
  "message": "Tìm việc làm part-time cho sinh viên IT"
}
```

**Response:**

```json
{
  "response": "Dựa trên yêu cầu của bạn, tôi tìm thấy các công việc sau..."
}
```

### 8.2 Khởi động lại phiên chat

**Endpoint:** `POST /api/aichat/restart`
**Quyền:** Authenticated

---

## **9. Files (Quản lý File)**

### 9.1 Upload File

**Endpoint:** `POST /api/files/upload`
**Quyền:** Authenticated
**Content-Type:** `multipart/form-data`

**Form Data:**

- `file`: Binary file
- `folder`: "avatars" | "cvs" | "logos" | "certificates" | "general"

**Response:**

```json
{
  "success": true,
  "data": {
    "url": "/uploads/avatars/filename.jpg"
  }
}
```

### 9.2 Xóa File

**Endpoint:** `DELETE /api/files?fileUrl=/uploads/avatars/filename.jpg`
**Quyền:** Authenticated

### 9.3 Download File

**Endpoint:** `GET /api/files/download?fileUrl=/uploads/cvs/resume.pdf`
**Quyền:** Public
**Response:** Binary file stream

---

## **10. Admin (Quản trị)**

### 10.1 Khóa tài khoản user

**Endpoint:** `POST /api/admin/users/{id}/lock`
**Quyền:** `ADMIN`

### 10.2 Mở khóa tài khoản user

**Endpoint:** `POST /api/admin/users/{id}/unlock`
**Quyền:** `ADMIN`

### 10.3 Lấy danh sách users

**Endpoint:** `GET /api/admin/users`
**Quyền:** `ADMIN`
**Query Params:**

- `search`: Từ khóa tìm kiếm
- `pageNumber` (default: 1)
- `pageSize` (default: 10)

### 10.4 Cập nhật trạng thái Job Post (Admin)

**Endpoint:** `PUT /api/admin/jobs/{id}/status`
**Quyền:** `ADMIN`

**Request Body:**

```json
{
  "status": 1 // 0=Draft, 1=Open, 2=Closed
}
```

### 10.5 Xóa Job Post (Admin)

**Endpoint:** `DELETE /api/admin/jobs/{id}`
**Quyền:** `ADMIN`

### 10.6 Lấy thống kê Dashboard

**Endpoint:** `GET /api/admin/stats`
**Quyền:** `ADMIN`

**Response:**

```json
{
  "totalUsers": 1000,
  "totalJobPosts": 250,
  "totalApplications": 5000,
  "activeJobs": 180
}
```

---

## **11. Logs (Nhật ký hệ thống - Admin)**

### 11.1 Lấy Activity Logs

**Endpoint:** `GET /api/logs/activities`
**Quyền:** `ADMIN`
**Query Params:**

- `userId`: Lọc theo user
- `startDate`: Từ ngày
- `endDate`: Đến ngày
- `pageNumber` (default: 1)
- `pageSize` (default: 50, max: 100)

### 11.2 Lấy Error Logs

**Endpoint:** `GET /api/logs/errors`
**Quyền:** `ADMIN`
**Query Params:**

- `level`: Mức độ lỗi (Critical/Error/Warning)
- `startDate`: Từ ngày
- `endDate`: Đến ngày
- `pageNumber` (default: 1)
- `pageSize` (default: 50, max: 100)

### 11.3 Thống kê Activity Logs

**Endpoint:** `GET /api/logs/activities/stats`
**Quyền:** `ADMIN`
**Query Params:**

- `startDate`: Từ ngày
- `endDate`: Đến ngày

### 11.4 Thống kê Error Logs

**Endpoint:** `GET /api/logs/errors/stats`
**Quyền:** `ADMIN`
**Query Params:**

- `startDate`: Từ ngày
- `endDate`: Đến ngày

---

## **Phụ lục: Enums và Status Codes**

### JobPostStatus

- `0`: Draft (Nháp)
- `1`: Open (Đang mở)
- `2`: Closed (Đã đóng)

### ApplicationStatus

- `1`: Pending (Chờ duyệt)
- `2`: Approved (Đã duyệt)
- `3`: Rejected (Từ chối)
- `4`: Withdrawn (Đã rút)

### CompanyRequestStatus

- `0`: Pending (Chờ duyệt)
- `1`: Approved (Đã duyệt)
- `2`: Rejected (Từ chối)

### Gender

- `0`: Male
- `1`: Female
- `2`: Other

### UserRole

- `STUDENT`: Sinh viên
- `EMPLOYER`: Nhà tuyển dụng
- `ADMIN`: Quản trị viên

---

## **Response Format**

Tất cả responses đều có format:

```json
{
  "success": true,
  "message": "Success message",
  "data": {
    /* actual data */
  }
}
```

Hoặc khi có lỗi:

```json
{
  "success": false,
  "message": "Error message",
  "errors": ["Error 1", "Error 2"]
}
```

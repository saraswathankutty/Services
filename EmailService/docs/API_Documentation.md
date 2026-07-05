# Email Service API Documentation

This document outlines the Request and Response formats for every endpoint available in the Email Service API.

---

## 1. Authentication

### `POST /api/Auth`
Authenticates a user and returns a JWT token.

**Auth Required:** `false`

#### Request Body (JSON)
```json
{
  "email": "user@example.com",
  "password": "your_secure_password"
}
```

#### Response (200 OK)
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR..."
}
```

#### Response (401 Unauthorized)
Returned if the credentials do not match or the user is not found.

---

## 2. Email Endpoints

### `POST /api/Email/SendContact`
Dedicated endpoint for the Portfolio Contact Form. Parses a JSON object and places it into an HTML template before sending.

**Auth Required:** `false`

#### Request Body (JSON)
```json
{
  "name": "John Doe",
  "hr_Name": "Jane Smith",
  "company_Name": "Tech Corp",
  "email": "john@techcorp.com",
  "subject": "Job Inquiry",
  "message": "I would like to discuss a potential opportunity."
}
```

#### Response (200 OK)
```json
{
  "status": true,
  "message": "Thank You! Your message has been sent.",
  "data": null
}
```

---

### `POST /api/Email/SendEmail`
General purpose endpoint for sending emails.

**Auth Required:** `true` (Bearer Token)

#### Request Body (JSON)
```json
{
  "to_Address": ["client@example.com"],
  "cc_Address": ["manager@example.com"],
  "bcc_Address": [],
  "subject": "Monthly Report",
  "body": "<h1>Report Data</h1><p>Here is the data for this month.</p>"
}
```
*(Note: The `file` array for attachments must be submitted as `multipart/form-data` if you wish to attach physical files).*

#### Response (200 OK)
```json
{
  "status": true,
  "message": "Email sent successfully.",
  "data": null
}
```

---

### `GET /api/Email/History`
Retrieves a paginated list of email logs from the database.

**Auth Required:** `true` (Bearer Token)

#### Query Parameters
- `pageNumber` (int, default: 1): The page number to retrieve.
- `pageSize` (int, default: 10): The number of records per page.

*Example:* `/api/Email/History?pageNumber=1&pageSize=5`

#### Response (200 OK)
```json
{
  "status": true,
  "message": "",
  "data": {
    "totalRecords": 150,
    "pageNumber": 1,
    "pageSize": 5,
    "logs": [
      {
        "id": 14,
        "fromAddress": "adtmomo2@gmail.com",
        "toAddress": "saraswathandev@gmail.com",
        "subject": "Portfolio Contact - John Doe",
        "messageBody": "...",
        "createdDate": "2026-07-04T14:30:00Z",
        "status": "Sent",
        "errorMessage": ""
      }
    ]
  }
}
```

---

## 3. System

### `GET /health`
Checks if the API and Database are running correctly.

**Auth Required:** `false`

#### Response (200 OK)
```text
Healthy
```

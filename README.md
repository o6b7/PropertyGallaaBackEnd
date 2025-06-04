# PropertyGalla Backend

## Overview

**PropertyGalla** is a backend system for a real estate platform that supports user authentication, property listings, feedback, and agent management via a RESTful API.  
Built with **ASP.NET Core** and deployed on various **AWS services**, including Elastic Beanstalk, S3, Lambda, RDS, and API Gateway. JWT is used for secure authentication and role-based access control.

---

## Technologies Used

- **Backend Framework**: ASP.NET Core
- **Database**: Amazon RDS (MySQL)
- **Authentication**: JWT (JSON Web Tokens)
- **File Storage**: Amazon S3
- **Compute**: AWS Lambda
- **API Gateway**: AWS API Gateway
- **Deployment**: AWS Elastic Beanstalk
- **Networking**: Amazon VPC
- **Security**: Role-based access control

---

## Getting Started

### Prerequisites

- .NET 8.0 SDK  
- AWS Account with required permissions  
- MySQL database  
- AWS CLI configured  

### Installation

1. **Clone the Repository**
   ```bash
   git clone https://github.com/o6b7/PropertyGallaaBackEnd.git
   cd PropertyGallaBackend
   ```

2. **Install Dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure**
   - Update `appsettings.json` with:
     - MySQL connection string
     - JWT secret key
     - AWS credentials

4. **Run the Application**
   ```bash
   dotnet run
   ```

---

## API Documentation

### Authentication

#### `POST /api/users/login`

Authenticate user and return JWT token.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "token": "jwt_token_here",
  "user": {
    "userId": "USR0001",
    "name": "John Doe",
    "email": "user@example.com",
    "phone": "1234567890",
    "role": "user",
    "createdAt": "2023-01-01T00:00:00"
  }
}
```

---

#### `POST /api/users/register`

Register a new user.

**Request Body:**
```json
{
  "name": "John Doe",
  "email": "user@example.com",
  "password": "password123",
  "phone": "1234567890",
  "role": "user"
}
```

---

### Properties

#### `GET /api/properties`

Get paginated list of properties with filtering.

**Query Parameters:**

- `title`, `state`, `city`, `rooms`, `bathrooms`, `parking`
- `minArea`, `maxArea`, `minPrice`, `maxPrice`
- `page` (default: 1), `pageSize` (default: 5)

---

#### `POST /api/properties/with-files`

Create a property listing with images.

**Headers:**
```
Authorization: Bearer <token>
```

**Form Data:**

- `Title`, `Description`, `Rooms`, `Bathrooms`, `Parking`
- `Area`, `State`, `City`, `Neighborhood`, `Price`
- `OwnerId`, `Images[]` (Array of image files)

---

### Feedback

#### `GET /api/feedback`

Get paginated feedback with filters.

**Query Parameters:**

- `ownerId`, `reviewerId`
- `rating`, `minRating`
- `page`, `pageSize`

---

#### `POST /api/feedback`

Submit new feedback.

**Request Body:**
```json
{
  "reviewerId": "USR0001",
  "ownerId": "USR0002",
  "rating": 5,
  "comment": "Great service!"
}
```

---

### Reports

#### `POST /api/reports`

Report a property (authentication required).

**Request Body:**
```json
{
  "reporterId": "USR0001",
  "propertyId": "PRO0001",
  "reason": "Incorrect information"
}
```

---

#### `PUT /api/reports`

Update report status (admin only).

**Request Body:**
```json
{
  "reportId": "REP0001",
  "status": "resolved",
  "note": "Issue has been fixed"
}
```

---

### View Requests

#### `POST /api/viewrequests`

Request to view a property.

**Request Body:**
```json
{
  "userId": "USR0001",
  "propertyId": "PRO0001",
  "text": "I'd like to view this property on Saturday"
}
```

---

#### `PATCH /api/viewrequests/{id}/status`

Update view request status (property owner only).

**Request Body:**
```json
{
  "status": "approved"
}
```

---

## AWS Deployment

### Elastic Beanstalk Setup

- Create new application in Elastic Beanstalk  
- Use .NET platform  
- Set environment variables (DB, JWT, etc.)  
- Deploy application via ZIP package

---

### RDS Configuration

- Create a MySQL database  
- Set security group to allow EB environment access  
- Update connection string in `appsettings.json`

---

### S3 Configuration

- Create S3 bucket for storing property images  
- Add proper CORS policy  
- Configure IAM roles/permissions

---

### API Gateway Setup

- Create new REST API  
- Set up resources and HTTP methods  
- Point integration to Elastic Beanstalk endpoint

---

## Database Schema

- **Users**
- **Properties**
- **PropertyImages**
- **Feedbacks**
- **Reports**
- **SavedProperties**
- **ViewRequests**

---

## Security

- JWT authentication with 1-hour expiration
- Role-based authorization (user, agent, admin)
- Password hashing using bcrypt
- CORS policy for cross-origin requests
- Input validation on all endpoints

---

## Summary

This README provides:

1. A comprehensive project overview  
2. Detailed API documentation  
3. AWS deployment instructions  
4. Security best practices

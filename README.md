# PropertyGallaaBackEnd
Backend for PropertyGalla – a real estate platform supporting user authentication, property listings, and agent management via a RESTful API. It uses ASP.NET and AWS (Elastic Beanstalk, S3, Lambda, RDS) with JWT-based authentication.

PropertyGalla Backend Documentation
Table of Contents

Project Overview
Technology Stack
API Documentation
Authentication
Users
Properties
Feedback
Reports
Saved Properties
View Requests
AWS Services Integration
Deployment
Development Setup
Security Considerations
Project Overview

PropertyGalla is a comprehensive real estate platform backend that provides:

User authentication and authorization
Property listing management
Feedback and rating system
Reporting mechanism for inappropriate content
Property saving functionality
View request scheduling
The backend is built with ASP.NET Core and leverages various AWS services for scalability and reliability.

Technology Stack

Backend Framework: ASP.NET Core
Database: Amazon RDS (Relational Database Service)
Authentication: JWT (JSON Web Tokens)
File Storage: Amazon S3 (for property images)
Serverless Functions: AWS Lambda
Deployment: AWS Elastic Beanstalk
Additional Services:
AWS CloudWatch (Logging)
AWS IAM (Identity and Access Management)
API Documentation

Authentication

POST /api/users/login

Authenticates a user and returns a JWT token.

Request Body:

json
{
  "email": "user@example.com",
  "password": "securePassword123"
}
Response:

json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "userId": "USR0001",
    "name": "John Doe",
    "email": "user@example.com",
    "phone": "+1234567890",
    "role": "user",
    "createdAt": "2023-01-01T00:00:00Z"
  }
}
POST /api/users/change-password

Changes the password for an authenticated user.

Request Headers:

Authorization: Bearer <token>
Request Body:

json
{
  "oldPassword": "currentPassword",
  "newPassword": "newSecurePassword"
}
Users

POST /api/users

Registers a new user.

Request Body:

json
{
  "name": "Jane Smith",
  "email": "jane@example.com",
  "password": "password123",
  "phone": "+1234567890",
  "role": "user"
}
GET /api/users

Retrieves a paginated list of users (admin only).

Query Parameters:

name: Filter by name
email: Filter by email
page: Page number (default: 1)
pageSize: Items per page (default: 5)
Properties

GET /api/properties

Retrieves properties with filtering and pagination.

Query Parameters:

title: Filter by title
state: Filter by state
city: Filter by city
rooms: Filter by number of rooms
bathrooms: Filter by number of bathrooms
parking: Filter by parking spaces
minArea: Minimum area
maxArea: Maximum area
minPrice: Minimum price
maxPrice: Maximum price
startDate: Properties created after this date
endDate: Properties created before this date
page: Page number (default: 1)
pageSize: Items per page (default: 5)
POST /api/properties/with-files

Creates a new property with images (multipart form data).

Request Headers:

Authorization: Bearer <token>
Content-Type: multipart/form-data
Form Data:

Title: Property title
Description: Property description
Rooms: Number of rooms
Bathrooms: Number of bathrooms
Parking: Number of parking spaces
Area: Property area
State: State location
City: City location
Neighborhood: Neighborhood
Price: Property price
OwnerId: Owner user ID
Images[]: Array of image files
Feedback

GET /api/feedback

Retrieves feedback with filtering options.

Query Parameters:

ownerId: Filter by owner ID
reviewerId: Filter by reviewer ID
rating: Filter by exact rating
minRating: Filter by minimum rating
page: Page number (default: 1)
pageSize: Items per page (default: 5)
POST /api/feedback

Submits new feedback.

Request Headers:

Authorization: Bearer <token>
Request Body:

json
{
  "reviewerId": "USR0001",
  "ownerId": "USR0002",
  "rating": 5,
  "comment": "Great service!"
}
Reports

POST /api/reports

Submits a report about a property.

Request Headers:

Authorization: Bearer <token>
Request Body:

json
{
  "reporterId": "USR0001",
  "propertyId": "PRO0001",
  "reason": "Inaccurate information"
}
Saved Properties

POST /api/savedproperties

Saves a property for a user.

Request Headers:

Authorization: Bearer <token>
Request Body:

json
{
  "userId": "USR0001",
  "propertyId": "PRO0001"
}
View Requests

POST /api/viewrequests

Creates a new property viewing request.

Request Headers:

Authorization: Bearer <token>
Request Body:

json
{
  "userId": "USR0001",
  "propertyId": "PRO0001",
  "text": "I'd like to view this property on Friday afternoon"
}
AWS Services Integration

Amazon RDS

Hosts the MYSQL database
Configuration in appsettings.json:
json
"ConnectionStrings": {
  "PropertyGallaContext": "Host=rds-instance.endpoint.us-east-1.rds.amazonaws.com;Database=propertygalla;Username=admin;Password=securepassword"
}
Amazon S3

Stores property images
Configured via AWS SDK:
csharp
services.AddAWSService<IAmazonS3>();
AWS Lambda

Handles image processing and other serverless functions
Triggered via S3 events or API Gateway
Elastic Beanstalk

Deployment platform for the ASP.NET application
Environment variables configured in the EB console
Deployment

Prerequisites:
AWS account with necessary permissions
AWS CLI configured
Elastic Beanstalk CLI tools installed
Deployment Steps:
bash
# Package the application
dotnet publish -c Release -o ./publish

# Zip the published files
cd ./publish
zip -r ../propertygalla.zip .

# Deploy to Elastic Beanstalk
eb init -p asp.net-core-linux propertygalla-backend
eb create propertygalla-prod
eb deploy
Development Setup

Prerequisites:
.NET 8 SDK
MYSQL
AWS CLI (for local development with AWS services)
Setup:
bash
# Clone the repository
git clone https://github.com/yourusername/PropertyGallaBackEnd.git
cd PropertyGallaBackEnd

# Install dependencies
dotnet restore

# Configure database connection
# Update appsettings.Development.json with your local DB credentials

# Run migrations
dotnet ef database update

# Run the application
dotnet run
Environment Variables:
ASPNETCORE_ENVIRONMENT: Development/Production
JWT__Key: Secret key for JWT tokens
JWT__Issuer: Token issuer
JWT__Audience: Token audience
AWS__AccessKey: AWS access key
AWS__SecretKey: AWS secret key
Security Considerations

# JWT Configuration:
Tokens expire after 1 hour
Strong secret key required
HTTPS mandatory in production

# Data Protection:
All passwords are hashed using PBKDF2 with HMAC-SHA256
Sensitive data encrypted in transit and at rest

# API Security:
Role-based authorization
Input validation on all endpoints
Rate limiting recommended (implement via AWS API Gateway)

# AWS Best Practices:
Least privilege IAM roles
RDS in private subnet
S3 bucket policies with proper access controls
Support

For issues or questions, please contact me at qusaii.abdullah@gmail.com

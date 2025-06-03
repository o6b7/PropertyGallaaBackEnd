## Overview
Backend for PropertyGalla – a real estate platform supporting user authentication, property listings, and agent management via a RESTful API. Built with ASP.NET Core and deployed on AWS services including Elastic Beanstalk, S3, Lambda, RDS, and API Gateway with JWT-based authentication.

## Technologies Used
- **Backend Framework**: ASP.NET Core
- **Database**: Amazon RDS (MySQL)
- **Authentication**: JWT (JSON Web Tokens)
- **File Storage**: Amazon S3
- **Compute**: AWS Lambda for servless functions
- **API Gateway**: AWS API Gateway
- **Deployment**: AWS Elastic Beanstalk
- **Networking**: Amazon VPC
- **Security**: Role-based access control

## Getting Started

### Prerequisites
- .NET 8.0 SDK 
- AWS Account with necessary permissions
- MySQL database
- AWS CLI configured

### Installation
1. Clone the repository:
   git clone https://github.com/yourusername/PropertyGallaBackend.git
Navigate to the project directory:
bash
cd PropertyGallaBackend
Install dependencies:
bash
dotnet restore
Configure appsettings.json with your database and AWS credentials
Run the application:
bash
dotnet run
API Documentation

Authentication

POST /api/users/login

Description: Authenticate user and get JWT token
Request Body:
json
{
  "email": "user@example.com",
  "password": "password123"
}
Response:
json
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
POST /api/users/register

Description: Register a new user
Request Body:
json
{
  "name": "John Doe",
  "email": "user@example.com",
  "password": "password123",
  "phone": "1234567890",
  "role": "user"
}
Properties

GET /api/properties

Description: Get paginated list of properties with filtering options
Query Parameters:
title: Filter by property title
state: Filter by state
city: Filter by city
rooms: Filter by number of rooms
bathrooms: Filter by number of bathrooms
parking: Filter by parking spaces
minArea/maxArea: Filter by area range
minPrice/maxPrice: Filter by price range
page: Page number (default: 1)
pageSize: Items per page (default: 5)
POST /api/properties/with-files

Description: Create a new property with images (multipart form)
Headers: Authorization: Bearer <token>
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

Description: Get paginated feedback with filtering
Query Parameters:
ownerId: Filter by owner ID
reviewerId: Filter by reviewer ID
rating: Filter by exact rating
minRating: Filter by minimum rating
page: Page number (default: 1)
pageSize: Items per page (default: 5)
POST /api/feedback

Description: Submit new feedback (requires authentication)
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

Description: Report a property (requires authentication)
Request Body:
json
{
  "reporterId": "USR0001",
  "propertyId": "PRO0001",
  "reason": "Incorrect information"
}
PUT /api/reports

Description: Update report status (admin only)
Request Body:
json
{
  "reportId": "REP0001",
  "status": "resolved",
  "note": "Issue has been fixed"
}
View Requests

POST /api/viewrequests

Description: Request a property viewing
Request Body:
json
{
  "userId": "USR0001",
  "propertyId": "PRO0001",
  "text": "I'd like to view this property on Saturday"
}
PATCH /api/viewrequests/{id}/status

Description: Update view request status (property owner only)
Request Body:
json
{
  "status": "approved"
}
AWS Deployment

Elastic Beanstalk Setup

Create new application in Elastic Beanstalk
Configure environment with .NET platform
Set environment variables for database and JWT configuration
Deploy application package
RDS Configuration

Create PostgreSQL/MySQL database
Configure security group to allow connections from EB environment
Set connection string in appsettings.json
S3 Configuration

Create bucket for property images
Configure CORS policy
Set up IAM permissions for application
API Gateway Setup

Create new REST API
Configure resources and methods
Set up integration with EB environment
Deploy API
Database Schema

The database includes tables for:

Users
Properties
PropertyImages
Feedbacks
Reports
SavedProperties
ViewRequests
Security

JWT authentication with 1-hour expiration
Role-based authorization (user, agent, admin)
Password hashing with bcrypt
Input validation on all endpoints
CORS policy configuration
Contributing

Fork the repository
Create your feature branch (git checkout -b feature/AmazingFeature)
Commit your changes (git commit -m 'Add some AmazingFeature')
Push to the branch (git push origin feature/AmazingFeature)
Open a Pull Request
License

This project is licensed under the MIT License - see the LICENSE file for details.


This README provides:
1. Comprehensive project overview
2. Detailed API documentation for all endpoints
3. AWS deployment instructions
4. Security information
5. Contribution guidelines

The markdown format ensures proper rendering on GitHub with clear section headings and code blocks for API examples. You can extend any section as needed, particularly the AWS deployment details which can be made more specific to your actual configuration.

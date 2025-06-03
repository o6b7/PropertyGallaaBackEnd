# PropertyGallaaBackEnd
Backend for PropertyGalla – a real estate platform supporting user authentication, property listings, and agent management via a RESTful API. It uses ASP.NET and AWS (Elastic Beanstalk, S3, Lambda, RDS) with JWT-based authentication.

graph TD
    A[Client] --> B[CloudFront CDN]
    B --> C[API Gateway]
    C --> D[Elastic Beanstalk]
    D --> E[Application Load Balancer]
    E --> F[EC2 Auto Scaling Group]
    F --> G[VPC]
    G --> H[Private Subnet]
    H --> I[RDS PostgreSQL]
    H --> J[ElastiCache Redis]
    G --> K[Public Subnet]
    K --> L[NAT Gateway]
    F --> M[S3 Buckets]
    F --> N[Lambda Functions]

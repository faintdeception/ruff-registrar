# Security Policy

## Reporting Security Vulnerabilities

If you discover a security vulnerability in this project, please report it responsibly:

1. **Do NOT** create a public GitHub issue for security vulnerabilities
2. Email security reports to: [your-email@example.com]
3. Include as much detail as possible about the vulnerability
4. Allow time for the issue to be resolved before public disclosure

## Security Features

This project implements several security best practices:

### Authentication & Authorization
- JWT-based authentication via Keycloak
- Role-based access control (RBAC)
- Secure token validation
- HTTPS redirection enforced

### Password Management
- Auto-generated passwords via .NET Aspire
- No hardcoded secrets in source code
- Secure credential rotation
- Environment-based configuration

### Data Protection
- Encrypted database connections
- Secure CORS configuration
- Input validation and sanitization
- Protection against common attacks (XSS, CSRF, SQL injection)

### Infrastructure Security
- Docker container security
- Network isolation
- Secure defaults for all services
- Regular dependency updates

## Security Updates

This project is regularly updated to address security vulnerabilities:

- Dependencies are monitored for known vulnerabilities
- Security patches are applied promptly
- Breaking changes are documented in release notes

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.x     | ✅ Active support  |
| < 1.0   | ❌ Not supported   |

## Security Configuration

### Development Environment

For development, the application uses:
- Auto-generated passwords via .NET Aspire
- HTTPS redirection
- Secure default configurations
- User secrets for sensitive data

### Production Environment

For production deployment, ensure:
- Strong, unique passwords for all services
- HTTPS with valid certificates
- Database connections over SSL/TLS
- Proper firewall and network security
- Regular security updates
- Monitoring and alerting

## Best Practices

When contributing to this project:

1. Follow secure coding practices
2. Validate all inputs
3. Use parameterized queries
4. Implement proper error handling
5. Don't log sensitive information
6. Keep dependencies updated
7. Review code for security issues

## Third-Party Dependencies

This project uses several third-party components:

- .NET 9 - Microsoft's web framework
- PostgreSQL - Database system
- Keycloak - Identity and access management
- Next.js - Frontend framework
- Entity Framework Core - ORM

All dependencies are regularly updated and monitored for security vulnerabilities.

## Compliance

This project follows:
- OWASP security guidelines
- Microsoft security best practices
- Industry standard security practices
- Secure development lifecycle (SDL)

## Contact

For security-related questions or concerns:
- Email: [your-email@example.com]
- GitHub Issues: For non-security related issues only

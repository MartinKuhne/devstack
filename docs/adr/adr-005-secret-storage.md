# ADR-005: Secret Storage

## Status
Accepted

## Context
DevStack stores sensitive credentials (GitHub tokens, model API keys) that must not be persisted as plaintext. We need a reusable encryption mechanism that works for both production deployments and local development scenarios.

## Decision
Implement AES-256 encryption for secret storage with the following approach:

### Encryption Scheme
- **Algorithm**: AES-256-CBC
- **Key Derivation**: PBKDF2 with SHA256, 10,000 iterations
- **Version Prefix**: `v1:` prepended to all encrypted values for forward compatibility

### Secret Key Management
1. **Primary**: `DEVSTACK_SECRET_KEY` environment variable
2. **Fallback**: Development fallback to environment variable (no DPAPI in cross-platform .NET 10)

### Implementation
- `ISecretService` interface with `Encrypt(string)` and `Decrypt(string)` methods
- `AesSecretService` implementation registered as singleton in DI
- All token fields (`GithubToken_Encrypted`, `ApiKey_Encrypted`) are encrypted before database writes and decrypted on read

### API Configuration
The API requires `DEVSTACK_SECRET_KEY` to be set. In Docker Compose, this is passed via environment variable or can be mounted from a secrets management system.

## Consequences

### Positive
- Secrets are never stored in plaintext
- Version prefix enables future key rotation
- Works cross-platform without platform-specific dependencies

### Negative
- Key must be managed securely in production
- Loss of key makes encrypted data unrecoverable

## Notes
- Consider adding key rotation support in future iterations
- The design allows adding HSM/KMS integration later without breaking existing encrypted data
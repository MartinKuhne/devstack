namespace DevStack.Infrastructure.Services;

public interface ISecretService
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}

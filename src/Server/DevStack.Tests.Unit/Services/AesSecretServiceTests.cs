using DevStack.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace DevStack.Tests.Unit.Services;

public class AesSecretServiceTests
{
    private readonly AesSecretService _service;

    public AesSecretServiceTests()
    {
        _service = new AesSecretService("test-secret-key-for-unit-tests");
    }

    [Fact]
    public void Encrypt_ReturnsNonEmptyString()
    {
        var result = _service.Encrypt("my-secret-value");

        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("v1:");
    }

    [Fact]
    public void Decrypt_ReturnsOriginalValue()
    {
        var plaintext = "my-secret-value";

        var encrypted = _service.Encrypt(plaintext);
        var decrypted = _service.Decrypt(encrypted);

        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public void EncryptRoundTrip_WorksForLongStrings()
    {
        var plaintext = new string('A', 1000);

        var encrypted = _service.Encrypt(plaintext);
        var decrypted = _service.Decrypt(encrypted);

        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public void Encrypt_SpecialCharacters()
    {
        var plaintext = "special!@#$%^&*()_+-=[]{}|;':\",./<>?`~";

        var encrypted = _service.Encrypt(plaintext);
        var decrypted = _service.Decrypt(encrypted);

        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public void Encrypt_UnicodeCharacters()
    {
        var plaintext = "Hello, 世界! 🌍";

        var encrypted = _service.Encrypt(plaintext);
        var decrypted = _service.Decrypt(encrypted);

        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public void Encrypt_EmptyString_ReturnsEmpty()
    {
        var result = _service.Encrypt(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Encrypt_NullString_ReturnsEmpty()
    {
        var result = _service.Encrypt(null!);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_EmptyString_ReturnsEmpty()
    {
        var result = _service.Decrypt(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_NullString_ReturnsEmpty()
    {
        var result = _service.Decrypt(null!);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_InvalidFormat_Throws()
    {
        var invalidCiphertext = "invalid-ciphertext";

        var action = () => _service.Decrypt(invalidCiphertext);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Invalid encrypted format");
    }

    [Fact]
    public void Constructor_WithoutKey_UsesEnvironmentVariable()
    {
        Environment.SetEnvironmentVariable("DEVSTACK_SECRET_KEY", "env-secret-key");
        try
        {
            var serviceFromEnv = new AesSecretService();
            var encrypted = serviceFromEnv.Encrypt("test");
            var decrypted = serviceFromEnv.Decrypt(encrypted);

            decrypted.Should().Be("test");
        }
        finally
        {
            Environment.SetEnvironmentVariable("DEVSTACK_SECRET_KEY", null);
        }
    }

    [Fact]
    public void DifferentServices_DifferentKeys_CannotDecryptEachOther()
    {
        var service1 = new AesSecretService("key-one");
        var service2 = new AesSecretService("key-two");

        var encrypted = service1.Encrypt("secret");

        var action = () => service2.Decrypt(encrypted);

        action.Should().Throw<Exception>();
    }
}
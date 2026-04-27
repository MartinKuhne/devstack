using System.Security.Cryptography;

using DevStack.Infrastructure.Services;

using FluentAssertions;

using Xunit;

public class AesSecretServiceTests
{
    private const string TestSecretKey = "test-secret-key-for-unit-tests-12345";

    [Fact]
    public void Constructor_WithValidKey_CreatesService()
    {
        var exception = Record.Exception(() => new AesSecretService(TestSecretKey));

        exception.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullKey_ThrowsInvalidOperationException()
    {
        var exception = Record.Exception(() => new AesSecretService(null!));

        exception.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("No secret key available: set DEVSTACK_SECRET_KEY or ensure DPAPI is available.");
    }

    [Fact]
    public void Constructor_WithEmptyKey_ThrowsInvalidOperationException()
    {
        var exception = Record.Exception(() => new AesSecretService(string.Empty));

        exception.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("No secret key available: set DEVSTACK_SECRET_KEY or ensure DPAPI is available.");
    }

    [Fact]
    public void Encrypt_WithNullPlaintext_ReturnsEmptyString()
    {
        var service = new AesSecretService(TestSecretKey);

        var result = service.Encrypt(null!);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Encrypt_WithEmptyPlaintext_ReturnsEmptyString()
    {
        var service = new AesSecretService(TestSecretKey);

        var result = service.Encrypt(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Encrypt_WithValidPlaintext_ReturnsBase64StringWithV1Prefix()
    {
        var service = new AesSecretService(TestSecretKey);

        var result = service.Encrypt("hello world");

        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("v1:");
        var base64Part = result.Substring(3);
        var decoded = Convert.FromBase64String(base64Part);
        decoded.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Encrypt_Deterministic_SameKeyProducesSameOutput()
    {
        var service = new AesSecretService(TestSecretKey);

        var encrypted1 = service.Encrypt("test message");
        var encrypted2 = service.Encrypt("test message");

        encrypted1.Should().Be(encrypted2);
    }

    [Fact]
    public void Encrypt_DifferentKeysProduceDifferentOutput()
    {
        var service1 = new AesSecretService("key-one");
        var service2 = new AesSecretService("key-two");

        var encrypted1 = service1.Encrypt("same message");
        var encrypted2 = service2.Encrypt("same message");

        encrypted1.Should().NotBe(encrypted2);
    }

    [Fact]
    public void Decrypt_WithNullCiphertext_ReturnsEmptyString()
    {
        var service = new AesSecretService(TestSecretKey);

        var result = service.Decrypt(null!);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_WithEmptyCiphertext_ReturnsEmptyString()
    {
        var service = new AesSecretService(TestSecretKey);

        var result = service.Decrypt(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_WithInvalidFormat_ThrowsInvalidOperationException()
    {
        var service = new AesSecretService(TestSecretKey);

        var exception = Record.Exception(() => service.Decrypt("not-a-valid-encrypted-string"));

        exception.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("Invalid encrypted format");
    }

    [Fact]
    public void Decrypt_WithInvalidBase64_ThrowsFormatException()
    {
        var service = new AesSecretService(TestSecretKey);

        var exception = Record.Exception(() => service.Decrypt("v1:invalidbase64!!!"));

        exception.Should().BeOfType<FormatException>();
    }

    [Fact]
    public void EncryptThenDecrypt_RoundTrip_PreservesOriginalMessage()
    {
        var service = new AesSecretService(TestSecretKey);
        var original = "hello world";

        var encrypted = service.Encrypt(original);
        var decrypted = service.Decrypt(encrypted);

        decrypted.Should().Be(original);
    }

    [Fact]
    public void EncryptThenDecrypt_RoundTrip_PreservesSpecialCharacters()
    {
        var service = new AesSecretService(TestSecretKey);
        var original = "Special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?";

        var encrypted = service.Encrypt(original);
        var decrypted = service.Decrypt(encrypted);

        decrypted.Should().Be(original);
    }

    [Fact]
    public void EncryptThenDecrypt_RoundTrip_PreservesUnicodeCharacters()
    {
        var service = new AesSecretService(TestSecretKey);
        var original = "Unicode: Hello 世界 \u00e9\u00e0\u00fc\u00f1 \ud83c\udf0d";

        var encrypted = service.Encrypt(original);
        var decrypted = service.Decrypt(encrypted);

        decrypted.Should().Be(original);
    }

    [Fact]
    public void EncryptThenDecrypt_RoundTrip_PreservesLongMessage()
    {
        var service = new AesSecretService(TestSecretKey);
        var original = new string('x', 10000);

        var encrypted = service.Encrypt(original);
        var decrypted = service.Decrypt(encrypted);

        decrypted.Should().Be(original);
    }

    [Fact]
    public void EncryptThenDecrypt_RoundTrip_PreservesNewlinesAndTabs()
    {
        var service = new AesSecretService(TestSecretKey);
        var original = "Line 1\nLine 2\twith\ttabs\r\nwith CRLF";

        var encrypted = service.Encrypt(original);
        var decrypted = service.Decrypt(encrypted);

        decrypted.Should().Be(original);
    }

    [Fact]
    public void EncryptThenDecrypt_DifferentKey_ThrowsInvalidOperationException()
    {
        var service1 = new AesSecretService(TestSecretKey);
        var service2 = new AesSecretService("different-key");
        var original = "secret message";

        var encrypted = service1.Encrypt(original);

        var exception = Record.Exception(() => service2.Decrypt(encrypted));

        exception.Should().BeOfType<CryptographicException>();
    }

    [Fact]
    public void Encrypt_EmptyString_RoundTrip_PreservesEmptyString()
    {
        var service = new AesSecretService(TestSecretKey);

        var encrypted = service.Encrypt(string.Empty);
        var decrypted = service.Decrypt(encrypted);

        decrypted.Should().BeEmpty();
    }

    [Fact]
    public void Encrypt_WithSingleCharacter_PreservesCharacter()
    {
        var service = new AesSecretService(TestSecretKey);
        var original = "a";

        var encrypted = service.Encrypt(original);
        var decrypted = service.Decrypt(encrypted);

        decrypted.Should().Be(original);
    }

    [Fact]
    public void Encrypt_WithWhitespaceOnly_PreservesWhitespace()
    {
        var service = new AesSecretService(TestSecretKey);
        var original = "   \t\n  ";

        var encrypted = service.Encrypt(original);
        var decrypted = service.Decrypt(encrypted);

        decrypted.Should().Be(original);
    }

    [Fact]
    public void Encrypt_MultipleMessages_AllDecryptionSuccessful()
    {
        var service = new AesSecretService(TestSecretKey);
        var messages = new[]
        {
            "message one",
            "message two",
            "message three with special chars: !@#$%",
            "\u00e9\u00e0\u00fc\u00f1 unicode test",
            new string('z', 5000),
            "",
            "a",
            "\r\n\t  mixed whitespace  \t\r\n"
        };

        foreach (var message in messages)
        {
            var encrypted = service.Encrypt(message);
            var decrypted = service.Decrypt(encrypted);
            decrypted.Should().Be(message, $"Round-trip failed for message: {message}");
        }
    }
}

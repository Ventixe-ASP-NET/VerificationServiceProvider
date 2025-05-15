using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Tests.Services;


// These tests were written with AI assistance
public class VerificationService_Tests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<EmailClient> _emailClientMock;
    private readonly IMemoryCache _cache;
    private readonly VerificationService _verificationService;

    public VerificationService_Tests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _emailClientMock = new Mock<EmailClient>();
        _cache = new MemoryCache(new MemoryCacheOptions());

        _configurationMock.Setup(c => c["ACS:SenderAddress"]).Returns("sender@mail.com");

        _verificationService = new VerificationService(_configurationMock.Object, _emailClientMock.Object, _cache);
    }


    [Fact]
    public async Task SendVerificationCodeAsync_ValidRequest_SendsEmail()
    {
        // arrange
        var request = new SendVerificationCodeRequest { Email = "test@example.com" };

        var emailSendOperationMock = new Mock<EmailSendOperation>();
        _emailClientMock.Setup(ec => ec.SendAsync(It.IsAny<WaitUntil>(), It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emailSendOperationMock.Object);


        // act
        var result = await _verificationService.SendVerificationCodeAsync(request);


        // assert
        Assert.True(result.Succeeded);
        Assert.Equal("Verification email sent successfully", result.Message);
        Assert.True(_cache.TryGetValue(request.Email.ToLowerInvariant(), out string? storedCode));
        Assert.NotNull(storedCode);
    }


    [Fact]
    public async Task SendVerificationCodeAsync_EmailSendFails_ReturnsError()
    {
        // arrange
        var request = new SendVerificationCodeRequest { Email = "test@example.com" };

        _emailClientMock.Setup(ec => ec.SendAsync(It.IsAny<WaitUntil>(), It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email sending failed"));


        // act
        var result = await _verificationService.SendVerificationCodeAsync(request);


        // assert
        Assert.False(result.Succeeded);
        Assert.Equal("Failed to send verification email", result.Error);
        Assert.False(_cache.TryGetValue(request.Email.ToLowerInvariant(), out string? storedCode));
    }


    [Fact]
    public void VerifyVerificationCode_ValidCode_ReturnsSuccess()
    {
        // arrange
        var email = "test@example.com";
        var code = "123456";

        _verificationService.SaveVerificationCode(new SaveVerificationCodeRequest
        {
            Email = email,
            Code = code,
            ValidFor = TimeSpan.FromMinutes(5)
        });

        var request = new VerifyVerificationCodeRequest { Email = email, Code = code };


        // act
        var result = _verificationService.VerifyVerificationCode(request);


        // assert
        Assert.True(result.Succeeded);
        Assert.Equal("Verification successful", result.Message);
    }


    [Fact]
    public void VerifyVerificationCode_InvalidCode_ReturnsError()
    {
        // arrange
        var email = "test@example.com";
        var code = "123456";

        _verificationService.SaveVerificationCode(new SaveVerificationCodeRequest
        {
            Email = email,
            Code = code,
            ValidFor = TimeSpan.FromMinutes(5)
        });

        var request = new VerifyVerificationCodeRequest { Email = email, Code = "654321" };


        // act
        var result = _verificationService.VerifyVerificationCode(request);


        // assert
        Assert.False(result.Succeeded);
        Assert.Equal("Invalid or expired verification code", result.Error);
    }
}

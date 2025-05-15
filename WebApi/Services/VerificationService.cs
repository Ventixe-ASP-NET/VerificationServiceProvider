using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using WebApi.Interfaces;
using WebApi.Models;

namespace WebApi.Services;

public class VerificationService(IConfiguration configuration, EmailClient emailClient, IMemoryCache cache) : IVerificationService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly EmailClient _emailClient = emailClient;
    private readonly IMemoryCache _cache = cache;
    private static readonly Random _random = new();

    public async Task<VerificationServiceResult> SendVerificationCodeAsync(SendVerificationCodeRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return new VerificationServiceResult { Succeeded = false, Error = "Recipient email address is required" };

            var verificationCode = GenerateVerificationCode();

            var subject = $"Your verification code is {verificationCode}";
            var plainTextContent = $@"
            Verify you email address for Ventixe
            
            To complete your verification, please enter the following code:

            {verificationCode}
            ";

            var emailMessage = new EmailMessage(
                senderAddress: _configuration["ACS:SenderAddress"],
                recipients: new EmailRecipients([new(request.Email)]),
                content: new EmailContent(subject)
                {
                    PlainText = plainTextContent
                });

            var emailSendOperation = await _emailClient.SendAsync(WaitUntil.Started, emailMessage);
            SaveVerificationCode(new SaveVerificationCodeRequest { Email = request.Email, Code = verificationCode, ValidFor = TimeSpan.FromMinutes(5) });

            return new VerificationServiceResult { Succeeded = true, Message = "Verification email sent successfully" };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return new VerificationServiceResult { Succeeded = false, Error = "Failed to send verification email" };
        }
    }


    public string GenerateVerificationCode()
    {
        return _random.Next(100000, 999999).ToString();
    }


    public void SaveVerificationCode(SaveVerificationCodeRequest request)
    {
        _cache.Set(request.Email.ToLowerInvariant(), request.Code, request.ValidFor);
    }


    public VerificationServiceResult VerifyVerificationCode(VerifyVerificationCodeRequest request)
    {
        var key = request.Email.ToLowerInvariant();

        if (_cache.TryGetValue(key, out string? storedCode))
        {
            if (storedCode == request.Code)
            {
                _cache.Remove(key);
                return new VerificationServiceResult { Succeeded = true, Message = "Verification successful" };
            }
        }

        return new VerificationServiceResult { Succeeded = false, Error = "Invalid or expired verification code" };
    }
}

using WebApi.Models;

namespace WebApi.Interfaces;

public interface IVerificationService
{
    string GenerateVerificationCode();
    void SaveVerificationCode(SaveVerificationCodeRequest request);
    Task<VerificationServiceResult> SendVerificationCodeAsync(SendVerificationCodeRequest request);
    VerificationServiceResult VerifyVerificationCode(VerifyVerificationCodeRequest request);
}

using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using WebApi.Documentation;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VerificationController(IVerificationService verificationService) : ControllerBase
{
    private readonly IVerificationService _verificationService = verificationService;

    [SwaggerRequestExample(typeof(SendVerificationCodeRequest), typeof(SendExample))]
    [HttpPost("send")]
    public async Task<IActionResult> Send(SendVerificationCodeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Error = "Recipient email address is required" });

        var result = await _verificationService.SendVerificationCodeAsync(request);
        return result.Succeeded
            ? Ok(result)
            : StatusCode(500, result);
    }


    [SwaggerRequestExample(typeof(VerifyVerificationCodeRequest), typeof(VerifyExample))]
    [HttpPost("verify")]
    public IActionResult Verify(VerifyVerificationCodeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Error = "Invalid or expired verification code" });

        var result = _verificationService.VerifyVerificationCode(request);
        return result.Succeeded
            ? Ok(result)
            : StatusCode(500, result);
    }
}

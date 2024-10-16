using Contracts;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace API.Controllers;
public class AccountController(IAuthService authService) : ApiController(authService)
{

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        var registrationCheckUp = await _authService.Register(registerDto);
       
        if (!registrationCheckUp.Succeeded)
        {
            _response = new ApiResponse(registrationCheckUp.Errors.First().Description, false, null, Convert.ToInt32(HttpStatusCode.BadRequest));
            return StatusCode(_response.StatusCode, _response);
        }

        _response = new ApiResponse("Registration successful. Please check your email to confirm your account.", true, null, Convert.ToInt32(HttpStatusCode.Created));
        return StatusCode(_response.StatusCode, _response);
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var loginCheckUp = await _authService.Login(loginDto);

        if (!loginCheckUp.Succeeded)
        {
            _response = new ApiResponse(loginCheckUp.Errors.First().Description, false, null, Convert.ToInt32(HttpStatusCode.BadRequest));
            return StatusCode(_response.StatusCode, _response);
        }

        _response = new ApiResponse("User logged in successfully",
            true,
            await _authService.Authenticate(user => user.UserName == loginDto.Username),
            Convert.ToInt32(HttpStatusCode.OK));
        return StatusCode(_response.StatusCode, _response);
    }



}

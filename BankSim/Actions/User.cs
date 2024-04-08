using BankSim.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace BankSim.Actions;

public interface IUser
{
    ResponseHeader Login(LoginDto loginDto);
}
public class User : IUser
{
    private readonly IConfiguration configuration;
    


    public User(IConfiguration configuration)
    {
        this.configuration = configuration;        
    }

    public  ResponseHeader Login(LoginDto loginDto)
    {
        try
        {
            var bankCode = configuration.GetValue<string>("BankCode");

            var username = configuration.GetValue<string>("Email");
            var password = configuration.GetValue<string>("Password");

            var lowUser = username.ToLower();

            if (loginDto.Username != lowUser || loginDto.Password != password)
            {
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Invalid username/password"
                };
            }

            var value = username + password;
            var claims = new List<Claim>
                {
                    new Claim (bankCode, value),
                };


            return new ResponseHeader
            {
                ResponseCode = "00",
                ResponseMessage = "Success"
            };
        }
        catch (Exception ex)
        {
            return new ResponseHeader
            {
                ResponseCode = "01",
                ResponseMessage = "Something went wrong"
            };
        }
    }
}

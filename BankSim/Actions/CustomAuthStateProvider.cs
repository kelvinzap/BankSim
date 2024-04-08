using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Security.Claims;
using System.Security.Principal;

namespace BankSim.Actions
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage _protectedLocalStorage;
        private readonly IConfiguration configuration;
        private readonly ILocalStorageService LocalStorage;


        public CustomAuthStateProvider(ProtectedLocalStorage protectedLocalStorage, IConfiguration configuration, ILocalStorageService localStorage)
        {
            _protectedLocalStorage = protectedLocalStorage;
            this.configuration = configuration;
            LocalStorage = localStorage;
        }



        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var bankCode = configuration.GetValue<string>("BankCode");
            var key = await LocalStorage.GetItemAsync<string>(bankCode);
            var identity = new ClaimsIdentity();

            if (string.IsNullOrEmpty(key))
            {
                identity = new ClaimsIdentity();
                var badUser = new ClaimsPrincipal(identity);
                var badState = new AuthenticationState(badUser);

                NotifyAuthenticationStateChanged(Task.FromResult(badState));

                return badState;
            }
            var username = configuration.GetValue<string>("Email");
            var password = configuration.GetValue<string>("Password");


            var lowUser = username.ToLower();

            var validKey = lowUser + password;

            if(key != validKey)
            {
                await LocalStorage.RemoveItemAsync(bankCode);

                identity = new ClaimsIdentity();
                var badUser = new ClaimsPrincipal(identity);
                var badState = new AuthenticationState(badUser);

                NotifyAuthenticationStateChanged(Task.FromResult(badState));

                return badState;
            }


            var claims = new List<Claim>
            {
                new Claim (bankCode, key)
            };

            identity = new ClaimsIdentity(claims, "authCheck");
                        
          
            var user = new ClaimsPrincipal(identity);


            var state = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(state));

            
            return state;
        }


        public async Task MarkUserAsAuthenticated(string value)
        {
            var bankCode = configuration.GetValue<string>("BankCode");

            var claims = new List<Claim>
            {
                new Claim (bankCode, value)
            };
            var identity = new ClaimsIdentity(claims, "authCheck");
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }



        public async Task MarkUserAsAnonymous()
        {
            var bankCode = configuration.GetValue<string>("BankCode");

            await _protectedLocalStorage.DeleteAsync(bankCode);
         
            var claimsPrincipal = new ClaimsPrincipal();
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));

        }
    }
}

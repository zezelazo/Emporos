using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Emporos.Core.Data;
using Emporos.Core.Identity;
using Emporos.Core.Services;
using Emporos.Model.Dto.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Emporos.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger<AuthenticationService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;

        public AuthenticationService(IOptions<AppSettings> tokenManagement, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<AuthenticationService> logger)
        {
            _jwt = tokenManagement.Value.JWT;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<OperationResult<string>> RegisterAsync(RegisterModel model)
        {
            try
            {
                _logger.LogInformation("Started RegisterAsync.");
                var userWithSameEmail = await _userManager.FindByEmailAsync(model.Email);

                if (userWithSameEmail == null)
                {
                    var user = new ApplicationUser
                    { 
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "user");
                        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        return OperationResult.Ok(token);
                    }
                    else
                    {
                        return OperationResult.Fail<string>(string.Join(". ", result.Errors.Select(e => e.Description)));
                    }
                }

                return OperationResult.Fail<string>($"Email {model.Email} is already registered.");

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in RegisterAsync mehod. {e}");
                return OperationResult.Fail<string>(string.Empty);
            }

        }

        public async Task<OperationResult<string>> GenerateNewRegisterToken(string userEmail)
        {

            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                return OperationResult.Fail<string>($"Email {userEmail} is not registered.");
            }
            else
            {
                string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                return OperationResult.Ok(token);
            }

        }

        public async Task<OperationResult> ConfirmEmailAsync(string token, string email)
        {
            try
            {
                _logger.LogInformation("Started ConfirmEmailAsync.");
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    return OperationResult.Fail($"No Accounts registered with {email}.");
                }

                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    return OperationResult.Ok();
                }

                return OperationResult.Fail($"Failed to confirm {email}");

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in ConfirmEmailAsync mehod. {e}");
                return OperationResult.Fail(string.Empty);
            }
        }

        public async Task<OperationResult> LoginAsync(LoginModel model, bool emailConfirmationEnabled)
        {
            var loginResultModel = new LoginResultModel();

            try
            {
                _logger.LogInformation("Started LoginAsync.");

                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    return OperationResult.Fail($"No Accounts registered with {model.Email}.");
                }

                if (await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    if (emailConfirmationEnabled && (!await _userManager.IsEmailConfirmedAsync(user)))
                    {
                        return OperationResult.Fail($"Email {user.Email} is not confirmed.");
                    }

                    JwtSecurityToken jwtSecurityToken = await CreateJwtToken(user);
                    loginResultModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                    loginResultModel.Email = user.Email;
                    loginResultModel.UserName = user.UserName;

                    return OperationResult.Ok(loginResultModel);
                }

                return OperationResult.Fail($"Incorrect Credentials for user {user.Email}.");

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in LoginAsync method. {e}");
                return OperationResult.Fail(string.Empty);
            }
        }

        public async Task<OperationResult<ApplicationUser>> GetUserByEmail(string email)
        {
            try
            {
                _logger.LogInformation("Started GetUsers.");

                var user = await _userManager.FindByEmailAsync(email);
                return user is null ? OperationResult.Fail<ApplicationUser>("User not found") : OperationResult.Ok(user);
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in GetUsers method. {e}");
                return OperationResult.Fail<ApplicationUser>(string.Empty);
            } 
        }

        public async Task<OperationResult<List<ApplicationUser>>> GetUsers()
        {
            try
            {
                _logger.LogInformation("Started GetUsers.");

                var users = await _userManager.Users.ToListAsync();

                return OperationResult.Ok(users);

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in GetUsers method. {e}");
                return OperationResult.Fail<List<ApplicationUser>>(string.Empty);
            } 
        }

        public async Task<OperationResult<List<ApplicationUser>>> GetUsersFromRole(string role)
        {
            try
            {
                _logger.LogInformation("Started GetUsers from Role.");

                var users = await _userManager.GetUsersInRoleAsync(role) ;

                return OperationResult.Ok(users.ToList());

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in GetUsers from role method. {e}");
                return OperationResult.Fail<List<ApplicationUser>>(string.Empty);
            } 
        }

        public async Task<OperationResult<List<string>>> GetRoles()
        {
            try
            {
                _logger.LogInformation("Started Get Roles.");

                var roles= (await _roleManager.Roles.ToListAsync()).Select(r=>r.Name).ToList();

                return OperationResult.Ok(roles);

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in GetRoles method. {e}");
                return OperationResult.Fail<List<string>>(string.Empty);
            } 
        }
        public async Task<OperationResult<List<string>>> SearchRoles(string term)
        {
            try
            {
                _logger.LogInformation("Started SearchRoles.");

                var roles= (await _roleManager.Roles.Where(r=>r.Name.ToLower().Trim()==term.ToLower().Trim()).ToListAsync()).Select(r=>r.Name).ToList();

                return OperationResult.Ok(roles);

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in SearchRoles method. {e}");
                return OperationResult.Fail<List<string>>(string.Empty);
            } 
        }
        
        public async Task<OperationResult> CreateRole(string roleName)
        {
            try
            {
                _logger.LogInformation("Started CreateRole.");

                if (await _roleManager.RoleExistsAsync(roleName)) 
                    return OperationResult.Fail("Role already exists");
                
                await _roleManager.CreateAsync(new IdentityRole( roleName));

                return OperationResult.Ok( );

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in CreateRole method. {e}");
                return OperationResult.Fail<List<string>>(string.Empty);
            } 
        }

        public async Task<OperationResult> DeleteRole(string roleName)
        {
            try
            {
                _logger.LogInformation("Started DeleteRole.");

                if (!await _roleManager.RoleExistsAsync(roleName)) 
                    return OperationResult.Fail("Role not exists");

                var role =await  _roleManager.Roles.FirstAsync(r => r.Name == roleName);
                var users = await _userManager.GetUsersInRoleAsync(roleName);
                foreach (var user in users)
                {
                    await _userManager.RemoveFromRoleAsync(user, roleName);
                }
                await _roleManager.DeleteAsync(role);
                return OperationResult.Ok( );

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in DeleteRole method. {e}");
                return OperationResult.Fail<List<string>>(string.Empty);
            } 
        }

        public async Task<OperationResult> DeleteUser(string email)
        {
            try
            {
                _logger.LogInformation("Started DeleteUser.");

                var user = await GetUserByEmail(email);
                if (!user.Success || user.Value is null || user.Value.Email != email)
                    return OperationResult.Fail("User not exists");
                
                await _userManager.DeleteAsync(user.Value);
                return OperationResult.Ok( );

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in DeleteUser method. {e}");
                return OperationResult.Fail(string.Empty);
            } 
        }

        public async Task<OperationResult> AddUserToRole(ApplicationUser user, string role)
        {
            try
            {
                _logger.LogInformation("Started AddUserToRole.");

                if (!await _roleManager.RoleExistsAsync(role)) 
                    return OperationResult.Fail("Role not exists");

                await _userManager.AddToRoleAsync(user, role);

                return OperationResult.Ok();
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in AddUserToRole method. {e}");
                return OperationResult.Fail<List<string>>(string.Empty);
            } 
        }
        public async Task<OperationResult> RemoveUserOfRole(ApplicationUser user, string role)
        {
            try
            {
                _logger.LogInformation("Started RemoveUserOfRole.");

                if (!await _roleManager.RoleExistsAsync(role)) 
                    return OperationResult.Fail("Role not exists");

                await _userManager.RemoveFromRoleAsync(user, role);

                return OperationResult.Ok();
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in RemoveUserOfRole method. {e}");
                return OperationResult.Fail<List<string>>(string.Empty);
            } 
        }



        public async Task<OperationResult> CreateUser(RegisterModel model)
        {
            try
            {
                _logger.LogInformation("Started RegisterAsync.");
                var userWithSameEmail = await _userManager.FindByEmailAsync(model.Email);

                if (userWithSameEmail == null)
                {
                    var user = new ApplicationUser
                    { 
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "user");
                        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        await _userManager.ConfirmEmailAsync(user, token);

                        return OperationResult.Ok();
                    }
                    else
                    {
                        return OperationResult.Fail(string.Join(". ", result.Errors.Select(e => e.Description)));
                    }
                }

                return OperationResult.Fail($"Email {model.Email} is already registered.");

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in RegisterAsync mehod. {e}");
                return OperationResult.Fail<string>(string.Empty);
            }
        }
        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);


            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
            .Union(userClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);
        }

    }
}

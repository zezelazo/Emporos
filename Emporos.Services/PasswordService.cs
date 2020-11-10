using System;
using System.Linq;
using System.Threading.Tasks;
using Emporos.Core.Identity;
using Emporos.Core.Services;
using Emporos.Model.Dto.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Emporos.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly ILogger<PasswordService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public PasswordService(UserManager<ApplicationUser> userManager, ILogger<PasswordService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        
        public async Task<OperationResult<string>> RequestResetAsync(RequestPasswordResetModel model)
        {
            try
            {
                _logger.LogInformation("Started RequestResetAsync.");
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    string resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                    return OperationResult.Ok(resetPasswordToken);
                }

                return OperationResult.Fail<string>($"No Accounts registered with mail {model.Email}");

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in RequestResetAsync mehod. {e}");
                return OperationResult.Fail<string>(string.Empty);
            }

        }

        public async Task<OperationResult> ResetAsync(PasswordResetModel model)
        {
            try
            {
                _logger.LogInformation("Started ResetAsync.");
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var validToken = await _userManager.VerifyUserTokenAsync(
                        user,
                        TokenOptions.DefaultProvider,
                        "ResetPassword",
                        model.Token);

                    if (validToken)
                    {
                        if (string.IsNullOrEmpty(model.NewPassword))
                        {
                            return OperationResult.Fail($"New password cannot be empty.");
                        }

                        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

                        if (result.Succeeded)
                        {
                            return OperationResult.Ok();
                        }
                        else
                        {
                            return OperationResult.Fail(string.Join(". ", result.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        return OperationResult.Fail($"Invalid password reset token for email {model.Email}.");
                    }
                }

                return OperationResult.Fail($"No Accounts registered with mail {model.Email}");

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in ResetAsync mehod. {e}");
                return OperationResult.Fail(string.Empty);
            }

        }

        public async Task<OperationResult> ChangeAsync(PasswordChangeModel model)
        {
            try
            {
                _logger.LogInformation("Started ChangeAsync.");
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    if (string.IsNullOrEmpty(model.NewPassword))
                    {
                        return OperationResult.Fail($"New password cannot be empty.");
                    }

                    if (await _userManager.CheckPasswordAsync(user, model.OldPassword))
                    {
                        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                        if (result.Succeeded)
                        {
                            return OperationResult.Ok();
                        }
                        else
                        {
                            return OperationResult.Fail(string.Join(". ", result.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        return OperationResult.Fail($"Old password did not match for user with mail {model.Email}");
                    }
                }

                return OperationResult.Fail($"No Accounts registered with mail {model.Email}");

            }
            catch (Exception e)
            {
                _logger.LogError($"Exception caught in ChangeAsync mehod. {e}");
                return OperationResult.Fail(string.Empty);
            }

        }
    }
}
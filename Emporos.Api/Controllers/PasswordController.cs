using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Emporos.Core.Data;
using Emporos.Core.Services;
using Emporos.Services;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Emporos.Model.Dto.Auth;

namespace Emporos.Api.Controllers
{
    /// <summary>
    /// Password controller
    /// </summary>   
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordController> _log;
        private readonly Core.Data.Email _emailSettings;

        /// <summary>
        /// Password controller constructor
        /// </summary>
        /// <param name="passwordService">Instance of IPasswordService</param>
        /// <param name="emailService">Instance of IIdentityMessageService</param>
        /// <param name="config">Instance of the config mapping object</param>
        /// <param name="log">Logger</param> 
        public PasswordController(IPasswordService passwordService, IEmailService emailService, IOptions<AppSettings> config,ILogger<PasswordController> log)
        {
            _emailSettings= config.Value.Email;
            _passwordService = passwordService;
            _emailService = emailService;
            _log = log;
        }

        /// <summary>
        /// Method request reset user account password
        /// </summary>
        /// <returns>Result</returns>
        [ProducesResponseType(typeof(OperationResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost(nameof(RequestReset))]
        public async Task<IActionResult> RequestReset(RequestPasswordResetModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var output = await _passwordService.RequestResetAsync(model);

            if (output.Success)
            { 
                string resetPasswordLink =
                    $"{_emailSettings.UIServer}resetPassword?token={HttpUtility.UrlEncode(output.Value)}&email={HttpUtility.UrlEncode(model.Email)}";
                 
                try
                {
                    var resetPasswordEmailResponse = await _emailService.SendEmail(
                        model.Email,
                        message: $"{_emailSettings.HtmlContent_ResetPassword}{resetPasswordLink}{_emailSettings.HtmlContent_ResetPasswordEnding}",
                        subject: _emailSettings.Subject_ResetPassword);

                    return Ok(resetPasswordEmailResponse);

                }
                catch (Exception e)
                {
                    _log.LogError(e,"Request Reset Exception");
                    return StatusCode(500);
                }

            }

            return Ok(output);
        }

        /// <summary>
        /// Method reset user account password
        /// </summary>
        /// <returns>Result</returns>
        [ProducesResponseType(typeof(OperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet(nameof(Reset))]
        public async Task<IActionResult> Reset(string token, string email, string newPassword)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var output = await _passwordService.ResetAsync(new PasswordResetModel()
                {
                    Email = email,
                    Token = token,
                    NewPassword = newPassword
                });

                return Ok(output);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Reset Exception");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Method change user account password
        /// </summary>
        /// <returns>Result</returns>
        [ProducesResponseType(typeof(OperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize()]
        [HttpGet(nameof(Change))]
        public async Task<IActionResult> Change(string email, string oldPassword, string newPassword)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var output = await _passwordService.ChangeAsync(new PasswordChangeModel()
                {
                    Email = email,
                    OldPassword = oldPassword,
                    NewPassword = newPassword
                }); ;

                return Ok(output);

            }
            catch (Exception e)
            {

                _log.LogError(e, "Change Exception");
                return StatusCode(500);
            }

        }
    }
}

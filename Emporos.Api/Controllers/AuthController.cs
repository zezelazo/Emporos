using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Emporos.Core.Data;
using Emporos.Core.Services; 
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Emporos.Model.Dto.Auth;
using Emporos.Services;

namespace Emporos.Api.Controllers
{
    /// <summary>
    /// Authentication controller
    /// </summary>   
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly IEmailService _emailService;
        private readonly Email _emailSettings;

        /// <summary>
        /// Authentication controller constructor
        /// </summary>
        /// <param name="authService">Instance of IAuthenticateService</param>
        /// <param name="emailService">Instance of IIdentityMessageService</param>
        /// <param name="config">Instance of IOptions</param>
        public AuthenticationController(IAuthenticationService authService, IEmailService emailService, IOptions<AppSettings> config)
        {
            _authService = authService;
            _emailService = emailService;
            _emailSettings = config.Value.Email;
        }

        /// <summary>
        /// Method login user
        /// </summary>
        /// <param name="model">Login model</param>
        /// <returns>Result</returns>
        [ProducesResponseType(typeof(OperationResult<LoginResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost(nameof(Login))]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(model, _emailSettings.EmailConfirmationEnabled);

            return Ok(result);
        }

        /// <summary>
        /// Method register user
        /// </summary>
        /// <param name="model">Register model</param> 
        /// <returns>Result</returns>
        [ProducesResponseType(typeof(OperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost(nameof(Register))]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var output = await _authService.RegisterAsync(model);

            if (output.Success && _emailSettings.EmailConfirmationEnabled)
            {

                try
                {
                    string confirmationLink =
                        $"{_emailSettings.UIServer}ConfirmEmail?token={HttpUtility.UrlEncode(output.Value)}&email={HttpUtility.UrlEncode(model.Email)}";
                    var emailText = _emailSettings.HtmlContent_Welcome.Replace("@pass@", model.Password);
                    var sendWelcomeEmailResponse = await _emailService.SendEmail(
                        model.Email,
                        message: $"{emailText}{confirmationLink}",
                        subject: _emailSettings.Subject_Welcome);

                    return Ok(sendWelcomeEmailResponse);

                }
                catch (Exception)
                {
                    return Ok(OperationResult.Fail(string.Empty));
                }

            }

            return Ok(output);
        }

        /// <summary>
        /// Method generate new registration user email
        /// </summary>
        /// <param name="model">Register model</param> 
        /// <returns>Result</returns>
        [ProducesResponseType(typeof(OperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost(nameof(ReIssueRegisterToken))]
        [AllowAnonymous]
        public async Task<IActionResult> ReIssueRegisterToken(string model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var output = await _authService.GenerateNewRegisterToken(model);

            if (output.Success && _emailSettings.EmailConfirmationEnabled)
            {

                try
                { 
                    string confirmationLink =
                        $"{_emailSettings.UIServer}ConfirmEmail?token={HttpUtility.UrlEncode(output.Value)}&email={HttpUtility.UrlEncode(model)}";
  
                    var sendWelcomeEmailResponse = await _emailService.SendEmail(
                        model,
                        message: $"{_emailSettings.HtmlContent_Welcome1}{confirmationLink}",
                        subject: _emailSettings.Subject_Welcome);

                    return Ok(sendWelcomeEmailResponse);

                }
                catch (Exception)
                {
                    return Ok(OperationResult.Fail(string.Empty));
                }

            }

            return Ok(output);
        }

        /// <summary>
        /// Method email confirmation
        /// </summary>
        /// <param name="token">Confirmation token</param> 
        /// <param name="email">Confirmation email</param> 
        /// <returns>Result</returns>
        [ProducesResponseType(typeof(OperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpGet(nameof(ConfirmEmail))]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ConfirmEmailAsync(token, email);

            return Ok(result);
        }



    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Emporos.Core.Identity;
using Emporos.Core.Services;
using Emporos.Data.Context;
using Emporos.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Emporos.Api.Controllers
{
    /// <summary>
    /// Users controller
    /// </summary>   
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly EmporosContext _ctx; 
        private readonly ILogger<UsersController> _log;
        private readonly IAuthenticationService _authService;

        public UsersController(EmporosContext ctx, ILogger<UsersController> log, IAuthenticationService authService)
        {
            _ctx = ctx; 
            _log = log;
            _authService = authService;
        }
     

       
        /// <summary>
        /// Method for getting of all users
        /// </summary>
        /// <returns>List of users</returns>
        [ProducesResponseType(typeof(OperationResult<List<ApplicationUser>>), (int)HttpStatusCode.OK)]
        [Authorize()]
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _log.LogInformation("Trying to get all users");
                var result = await _authService.GetUsers();

                if (result.Success) return Ok(result);
                
                return Ok(OperationResult.Fail("Unhandled Error"));
            }
            catch (Exception e)
            {
                _log.LogError(e, "Get all users");
                return Ok(OperationResult.Fail("Unhandled Error"));
            }
        }

        /// <summary>
        /// Get users by rol
        /// </summary>
        /// <param name="rol">Rol </param>
        /// <returns>Requested users in the role </returns>
        [ProducesResponseType(typeof(OperationResult<List<ApplicationUser>>), (int)HttpStatusCode.OK)]
        [Authorize()]
        [HttpGet("{rol}")]
        public async Task<IActionResult> GetByRol(string rol)
        {
            try
            {
                _log.LogInformation("Trying to get users by role");
                var result = await _authService.GetUsersFromRole(rol);
                if(result.Success) return Ok(result);
                return Ok(OperationResult.Fail("Unhandled Error"));
            }
            catch (Exception e)
            {
                _log.LogError(e, $"GetByRol {rol}");
                return Ok(OperationResult.Fail("Unhandled Error"));
            }
        }

        /// <summary>
        /// Method add user to rol 
        /// </summary>
        /// <param name="userEmail">User email</param>
        /// <param name="rol">Rol to add the user</param>
        /// <returns>Accepted or error</returns>
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [Authorize()]
        [HttpPut("addtorol")]
        public async Task<IActionResult> AddUserToRol([FromQuery]string userEmail, [FromQuery] string rol )
        {
            try
            {
                _log.LogInformation("Trying to Add a user to rol");

                var user = await _authService.GetUserByEmail(userEmail);
                if (!user.Success || user.Value is null ) return NotFound();

                var result = await _authService.AddUserToRole(user.Value,rol);

               if(result.Success) return Accepted(); 

               return NotFound();
            }
            catch (Exception e)
            {
                _log.LogError(e, $"Update user");
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Method remove a user from  rol 
        /// </summary>
        /// <param name="userEmail">User email</param>
        /// <param name="rol">Rol to delete the user</param>
        /// <returns>Accepted or error</returns>
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [Authorize()]
        [HttpPut("removefromrol")]
        public async Task<IActionResult> RemoveUserFromRol([FromQuery]string userEmail, [FromQuery] string rol )
        {
            try
            {
                _log.LogInformation("Trying to remove a user to rol");

                var user = await _authService.GetUserByEmail(userEmail);
                if (!user.Success || user.Value is null ) return NotFound();

                var result = await _authService.RemoveUserOfRole(user.Value,rol);

                if(result.Success) return Accepted(); 

                return NotFound();
            }
            catch (Exception e)
            {
                _log.LogError(e, $"Update user");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Method Delete user  
        /// </summary>
        /// <param name="userEmail">User email</param> 
        /// <returns>Accepted or error</returns>
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [Authorize()]
        [HttpDelete("{userEmail}")]
        public async Task<IActionResult> Delete(string userEmail)
        {
            try
            {
                _log.LogInformation("Trying to remove a user"); 
                var result = await _authService.DeleteUser(userEmail);

                if(result.Success) return Accepted(); 

                return NotFound();
            }
            catch (Exception e)
            {
                _log.LogError(e, $"Delete user");
                return StatusCode(500);
            }
        }

    }
}
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
    /// Roles controller
    /// </summary>   
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class RolesController : ControllerBase
    {
        private readonly EmporosContext _ctx; 
        private readonly ILogger<UsersController> _log;
        private readonly IAuthenticationService _authService;

        public RolesController(EmporosContext ctx, ILogger<UsersController> log, IAuthenticationService authService)
        {
            _ctx = ctx; 
            _log = log;
            _authService = authService;
        }
     
       
        /// <summary>
        /// Method for getting of all roles
        /// </summary>
        /// <returns>List of roles</returns>
        [ProducesResponseType(typeof(OperationResult<List<ApplicationUser>>), (int)HttpStatusCode.OK)]
        [Authorize()]
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _log.LogInformation("Trying to get all roles");
                var result = await _authService.GetRoles();

                if (result.Success) return Ok(result);
                
                return Ok(OperationResult.Fail("Unhandled Error"));
            }
            catch (Exception e)
            {
                _log.LogError(e, "Get all roles");
                return Ok(OperationResult.Fail("Unhandled Error"));
            }
        }

        /// <summary>
        /// Search Roles
        /// </summary>
        /// <param name="term">Term to search</param>
        /// <returns>Requested roles</returns>
        [ProducesResponseType(typeof(OperationResult<List<ApplicationUser>>), (int)HttpStatusCode.OK)]
        [Authorize()]
        [HttpGet("{term}")]
        public async Task<IActionResult> Search(string term)
        {
            try
            {
                _log.LogInformation("Trying to search roles");
                var result = await _authService.SearchRoles(term);
                if(result.Success) return Ok(result);
                return Ok(OperationResult.Fail("Unhandled Error"));
            }
            catch (Exception e)
            {
                _log.LogError(e, $"Search {term}");
                return Ok(OperationResult.Fail("Unhandled Error"));
            }
        }

        /// <summary>
        /// Method add rol 
        /// </summary> 
        /// <param name="rol">Rol name</param>
        /// <returns>Accepted or error</returns>
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [Authorize()]
        [HttpPost("{rol}")]
        public async Task<IActionResult> Add(  string rol )
        {
            try
            {
                _log.LogInformation("Trying to Add a   rol");
                 

                var result = await _authService.CreateRole( rol);

               if(result.Success) return Accepted(); 

               return NotFound();
            }
            catch (Exception e)
            {
                _log.LogError(e, $"Create rol");
                return StatusCode(500);
            }
        }
         
        /// <summary>
        /// Method Delete rol  
        /// </summary>
        /// <param name="rol">Rol name</param> 
        /// <returns>Accepted or error</returns>
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [Authorize()]
        [HttpDelete("{rol}")]
        public async Task<IActionResult> Delete(string rol)
        {
            try
            {
                _log.LogInformation("Trying to remove a rol"); 
                var result = await _authService.DeleteRole(rol);

                if(result.Success) return Accepted(); 

                return NotFound();
            }
            catch (Exception e)
            {
                _log.LogError(e, $"Delete rol");
                return StatusCode(500);
            }
        }

    }
}
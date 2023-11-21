using ApiElTiempo.Logic.Interface;
using ApiElTiempo.Models;
using ApiElTiempo.Models.CtlErr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiElTiempo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUser _user;

        public UserController(IUser user)
        {
            _user = user;
        }

        [HttpGet]
        public async Task<IActionResult> ListUsers()
        {
            ReturnData returnData = await _user.ListUsers();

            if (returnData.IsSuccess)
            {
                return Ok(returnData);
            }
            else
            {
                return BadRequest(returnData);
            }
        }

        [HttpGet]
        [Route("GetUser/{iduser}")]
        public async Task<IActionResult> GetUser([FromRoute] int iduser)
        {
            TbUser returnData = await _user.Obtener(iduser);

            if (returnData != null)
            {
                return Ok(returnData);
            }
            else
            {
                return BadRequest(returnData);
            }
        }


        [HttpPost]
        [Route("SaveUser")]
        public async Task<IActionResult> PostSaveUser([FromBody] TbUser tbUser)
        {
            ReturnData returnData = await _user.SaveUser(tbUser);

            if (returnData.IsSuccess)
            {
                return Ok(returnData);
            }
            else
            {
                return BadRequest(returnData);
            }
        }

        [HttpPut]
        [Route("UpdateUser")]
        public async Task<IActionResult> PutUpdateUser([FromBody] TbUser tbUser)
        {
            ReturnData returnData = await _user.UpdateUser(tbUser);

            if (returnData.IsSuccess)
            {
                return Ok(returnData);
            }
            else
            {
                return BadRequest(returnData);
            }
        }

        [HttpDelete]
        [Route("DeleteUser/{idUser}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int idUser)
        {
            ReturnData returnData = await _user.DeleteUser(idUser);

            if (returnData.IsSuccess)
            {
                return Ok(returnData);
            }
            else
            {
                return BadRequest(returnData);
            }
        }
    }
}

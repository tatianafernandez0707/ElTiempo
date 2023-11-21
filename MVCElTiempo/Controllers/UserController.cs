using Microsoft.AspNetCore.Mvc;
using MVCElTiempo.DataManagement.Interface;
using MVCElTiempo.Models;
using MVCElTiempo.Models.CtlErr;

namespace MVCElTiempo.Controllers
{
    public class UserController : Controller
    {
        private readonly IUser _user;

        public UserController(IUser user)
        {
            _user = user;
        }

        public async Task<IActionResult> Listar()
        {
            //Mostrara una vista de todos los usuarios

            ReturnData returnData = await _user.ListUsers();

            return View(returnData.Data);
        }

        public IActionResult Guardar()
        {
            //Metodo que devuelve la vista
            return View();
        }

        public async Task<IActionResult> Editar(int idUser)
        {
            //Metodo que devuelve la vista
            ReturnData returnData = await _user.Obtener(idUser);

            return View(returnData.Data);
        }


        [HttpPost]
        public async Task<IActionResult> Guardar(TbUser tbUser)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }

            //Metodo que recibe el objeto de usuarios y guardarlo en la base de datos

            ReturnData returnData = await _user.SaveUser(tbUser);

            if (returnData.IsSuccess)
            {
                return RedirectToAction("Listar");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TbUser tbUser)
        {
            //Metodo que recibe el objeto de usuarios y guardarlo en la base de datos

            ReturnData returnData = await _user.UpdateUser(tbUser);

            if (returnData.IsSuccess)
            {
                return RedirectToAction("Listar");
            }
            else
            {
                return View();
            }
        }

        public async Task<IActionResult> Eliminar(int idUser)
        {
            //Metodo que devuelve la vista
            ReturnData returnData = await _user.Obtener(idUser);

            return View(returnData.Data);
        }


        [HttpPost]
        public async Task<IActionResult> Eliminar(TbUser tbUser)
        {
            ReturnData returnData = await _user.DeleteUser(tbUser.IdUser);

            if (returnData.IsSuccess)
            {
                return RedirectToAction("Listar");
            }
            else
            {
                return View();
            }
        }
    }
}



using ApiElTiempo.Models;
using ApiElTiempo.Models.CtlErr;

namespace ApiElTiempo.Logic.Interface
{
    public interface IUser
    {
        Task<ReturnData> SaveUser(TbUser tbUser);
        Task<ReturnData> UpdateUser(TbUser tbUser);
        Task<ReturnData> ListUsers();
        Task<TbUser> Obtener(int idUser);
        Task<ReturnData> DeleteUser(int idUser);
    }
}

using MVCElTiempo.Models.CtlErr;
using MVCElTiempo.Models;

namespace MVCElTiempo.DataManagement.Interface
{
    public interface IUser
    {
        Task<ReturnData> SaveUser(TbUser tbUser);
        Task<ReturnData> UpdateUser(TbUser tbUser);
        Task<ReturnData> ListUsers();
        Task<ReturnData> Obtener(int idUser);
        Task<ReturnData> DeleteUser(int idUser);
    }
}

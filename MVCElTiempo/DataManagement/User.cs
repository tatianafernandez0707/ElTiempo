using Isopoh.Cryptography.Argon2;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MVCElTiempo.DataManagement.Interface;
using MVCElTiempo.Models;
using MVCElTiempo.Models.ContextEntityFramework;
using MVCElTiempo.Models.CtlErr;
using System.Collections.Immutable;
using System.Data;
using System.Text;

namespace MVCElTiempo.DataManagement
{
    public class User : IUser
    {
        private readonly MvcContext _mvcContext;
        public User(MvcContext mvcContext) 
        {
            _mvcContext = mvcContext;
        }

        /// <summary>
        /// Metodo para guardar el usuario, este metodo recibe el modelo de la tabla de usuario
        /// </summary>
        /// <param name="tbUser"></param>
        /// <returns>Objeto ReturnData</returns>
        /// NOTA: En este mismo metodo se podria manejar la actualización 
        public async Task<ReturnData> SaveUser(TbUser tbUser)
        {
            try
            {
                //Validar existencia de un usuario 

                ReturnData returnData = new();

                ReturnData existsUser = await ConsultUser(tbUser.UserName, tbUser.Email, tbUser.IdUser);

                if (existsUser.IsSuccess)
                {
                    //Encriptación de la contraseña por seguridad se convertira a bytes para enviar la cadena con seguridad

                    byte[] passwordUser = Encoding.UTF8.GetBytes(tbUser.PasswordUser);

                    string codify = EncryptKey(passwordUser);
                    tbUser.PasswordUser = codify;

                    tbUser.CreateDate = DateTime.Now;
                    _mvcContext.TbUser.Add(tbUser);
                    _mvcContext.SaveChanges();

                    returnData.IsSuccess = true;
                    returnData.Message = "Usuario creado exitosamente.";
                }
                else
                {
                    returnData.IsSuccess = false;
                    returnData.Message = "The email already exists, please enter a different one.";
                }

                return returnData;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Metodo para actualizar el usuario, este metodo recibe el modelo de la tabla de usuario
        /// </summary>
        /// <param name="tbUser"></param>
        /// <returns>Objeto ReturnData</returns>
        public async Task<ReturnData> UpdateUser(TbUser tbUser)
        {
            try
            {
                ReturnData returnData = new();

                TbUser? user = await _mvcContext.TbUser.FindAsync(tbUser.IdUser);

                if (user != null)
                {
                    if (user.Email != tbUser.Email || tbUser.UserName != user.UserName)
                    {
                        ReturnData exists = await ConsultUser(tbUser.UserName, tbUser.Email, tbUser.IdUser);

                        if (exists.IsSuccess)
                        {
                            //Actualiza el usuario

                            SqlConnection sqlConnection = (SqlConnection)_mvcContext.Database.GetDbConnection();

                            if (sqlConnection.State == ConnectionState.Closed)
                            {
                                sqlConnection.Open();
                            }

                            //Por seguridad se utiliza parametros dentro del sql command para proteger los datos 
                            // y que estos no vayan concatenados dentro del string.
                            string scriptSql = @"Update TB_User set UpdateDate = @updateDate, FullName = @fullname, UserName = @username, 
                                            Email = @email WHERE IdUser = @IdUser";

                            SqlCommand sqlCommand = new()
                            {
                                Connection = sqlConnection,
                                CommandText = scriptSql
                            };

                            tbUser.UpdateDate = DateTime.Now;
                            sqlCommand.Parameters.AddWithValue("@fullname", tbUser.FullName);
                            sqlCommand.Parameters.AddWithValue("@username", tbUser.UserName);
                            sqlCommand.Parameters.AddWithValue("@email", tbUser.Email);
                            sqlCommand.Parameters.AddWithValue("@IdUser", tbUser.IdUser);
                            sqlCommand.Parameters.AddWithValue("@updateDate", tbUser.UpdateDate);

                            int update = await sqlCommand.ExecuteNonQueryAsync();
                            await sqlCommand.DisposeAsync();

                            if(update > 0)
                            {
                                returnData.IsSuccess = true;
                                returnData.Message = "El usuario fue actualizado satisfactoriamente";
                            }
                            else
                            {
                                returnData.IsSuccess = false;
                                returnData.Message = "No se altero ningun dato al momento de actualizar el usuario";
                            }
                        }
                        else
                        {
                            returnData.IsSuccess = false;
                            returnData.Message = "El usuario a actualizar o correo electronico ya existe en el sistema";
                        }
                    }
                }
                else
                {
                    returnData.IsSuccess = false;
                    returnData.Message = "El usuario no existe en el sistema.";
                }

                return returnData;
            }
            catch(SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            catch(Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }

        #region Encriptación de la contraseña
        public string EncryptKey(byte[] passwordUser)
        {
            string clave = Encoding.ASCII.GetString(passwordUser);

            string? claveEncriptada = Argon2.Hash(clave);
            return claveEncriptada;
        }
        #endregion

        #region Verifica que la contraseña introducida existe
        public bool VerifyPassword(string passwordUserA, string passwordUserI)
        {
            var verificaClave = Argon2.Verify(passwordUserA, passwordUserI);
            return verificaClave;
        }
        #endregion

        /// <summary>
        /// Metodo que consultara si existe un usuario por medio del correo electronico
        /// en este proceso se utiliza la manera de consulta por medio de ADO.NET obteniendo 
        /// solo la conexion por medio del contexto
        /// </summary>
        /// <param name="userName" name="email"></param>
        /// <returns></returns>
        #region Metodo encargado de verificar la existencia de un usuario por medio del correo electronico
        public async Task<ReturnData> ConsultUser(string userName, string email, int idUser)
        {
            try
            {
                ReturnData returnData = new();

                SqlConnection sqlConnection = (SqlConnection)_mvcContext.Database.GetDbConnection();

                if (sqlConnection.State == ConnectionState.Closed)
                {
                    sqlConnection.Open();
                }

                //Por seguridad se utiliza parametros dentro del sql command para proteger los datos 
                // y que estos no vayan concatenados dentro del string.
                string scriptSql = "SELECT IdUser FROM TB_User WHERE UserName = @USERNAME OR Email = @EMAIL ";

                SqlCommand sqlCommand = new()
                {
                    Connection = sqlConnection,
                    CommandText = scriptSql
                };

                sqlCommand.Parameters.AddWithValue("@USERNAME", userName);
                sqlCommand.Parameters.AddWithValue("@EMAIL", email);

                int? value = (int?)await sqlCommand.ExecuteScalarAsync();
                await sqlCommand.DisposeAsync();

                if (idUser != 0)
                {
                    if (value != idUser)
                    {
                        returnData.IsSuccess = false;
                        returnData.Message = "existing email or username.";
                    }
                    else
                    {
                        returnData.IsSuccess = true;
                        returnData.Message = "non-existent username, you can continue with the creation of the user.";
                    }
                }
                else
                {
                    if (value == null)
                    {
                        returnData.IsSuccess = true;
                        returnData.Message = "non-existent username, you can continue with the creation of the user.";
                    }
                    else
                    {
                        returnData.IsSuccess = false;
                        returnData.Message = "The user exists, the user creation is not continued";
                    }
                }

                return returnData;
            }
            catch(SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        /// <summary>
        /// Metodo que consulta todos los usuarios del sistema
        /// </summary>
        /// <returns>Objeto tipo returndata con una lista de arrays de todos los usuarios</returns>
        #region Metodo encargado de listar todos los usuarios existentes en el sistema
        public async Task<ReturnData> Obtener(int idUser)
        {
            try
            {
                ReturnData returnData = new();

                TbUser tbUsers = await _mvcContext.TbUser.FindAsync(idUser);

                if (tbUsers != null)
                {
                    returnData.IsSuccess = true;
                    returnData.Message = "El usuario ha sido encontrado satisfactoriamente.";
                    returnData.Data = tbUsers;
                }
                else
                {
                    returnData.IsSuccess = false;
                    returnData.Message = "No se encontraron encontro datos del usuario a consultar.";
                }

                return returnData;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion


        /// <summary>
        /// Metodo que consulta todos los usuarios del sistema
        /// </summary>
        /// <returns>Objeto tipo returndata con una lista de arrays de todos los usuarios</returns>
        #region Metodo encargado de listar todos los usuarios existentes en el sistema
        public async Task<ReturnData> ListUsers()
        {
            try
            {
                ReturnData returnData = new();

                List<TbUser> tbUsers = await _mvcContext.TbUser.ToListAsync();

                if (tbUsers.Count > 0)
                {
                    returnData.IsSuccess = true;
                    returnData.Message = "Usuarios encontrados y listados correctamente";
                    returnData.Data = tbUsers;
                }
                else
                {
                    returnData.IsSuccess = false;
                    returnData.Message = "No se encontraron datos de usuarios en el sistema.";
                }

                return returnData;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        /// <summary>
        /// Metodo para eliminar el usuario requerido
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns>Objeto ReturnData</returns>
        public async Task<ReturnData> DeleteUser(int idUser)
        {
            try
            {
                ReturnData returnData = new();
                
                SqlConnection sqlConnection = (SqlConnection)_mvcContext.Database.GetDbConnection();

                if (sqlConnection.State == ConnectionState.Closed)
                {
                    sqlConnection.Open();
                }

                //Por seguridad se utiliza parametros dentro del sql command para proteger los datos 
                // y que estos no vayan concatenados dentro del string.
                string scriptSql = @"DELETE FROM TB_User WHERE IdUser = @IdUser";

                SqlCommand sqlCommand = new()
                {
                    Connection = sqlConnection,
                    CommandText = scriptSql
                };

                sqlCommand.Parameters.AddWithValue("@IdUser", idUser);

                int update = await sqlCommand.ExecuteNonQueryAsync();
                await sqlCommand.DisposeAsync();

                if (update > 0)
                {
                    returnData.IsSuccess = true;
                    returnData.Message = "El usuario fue eliminado satisfactoriamente";
                }
                else
                {
                    returnData.IsSuccess = false;
                    returnData.Message = "No se altero ningun dato al momento de eliminar el usuario";
                }
                
                return returnData;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

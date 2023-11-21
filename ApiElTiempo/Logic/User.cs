using Isopoh.Cryptography.Argon2;
using Microsoft.Data.SqlClient;
using ApiElTiempo.Models;
using ApiElTiempo.Models.CtlErr;
using System.Collections.Immutable;
using System.Data;
using System.Text;
using ApiElTiempo.Logic.Interface;
using Finbuckle.MultiTenant;
using System.Reflection;

namespace ApiElTiempo.DataManagement
{
    public class User : IUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public readonly IConfiguration _configuration;
        public User(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
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

                    TenantInfo? tenantInfo = new();

                    //Cuando es nulo se setea una conexion por default por medio del archivo de configuración sino toma el valor que se envia en el header
                    if (_httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>() == null)
                    {
                        tenantInfo.ConnectionString = _configuration.GetConnectionString("ElTiempo1");
                    }
                    else
                    {
                        tenantInfo = _httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>().TenantInfo;
                    }

                    using SqlConnection sqlConnection = new(tenantInfo.ConnectionString);

                    if (sqlConnection.State == ConnectionState.Closed)
                    {
                        sqlConnection.Open();
                    }

                    //Por seguridad se utiliza parametros dentro del sql command para proteger los datos 
                    // y que estos no vayan concatenados dentro del string.
                    string scriptSql = @"INSERT INTO TB_User values(@fullname, @username, @email, @passworduser, @createdate, null);";

                    SqlCommand sqlCommand = new()
                    {
                        Connection = sqlConnection,
                        CommandText = scriptSql
                    };

                    tbUser.UpdateDate = DateTime.Now;
                    sqlCommand.Parameters.AddWithValue("@fullname", tbUser.FullName);
                    sqlCommand.Parameters.AddWithValue("@username", tbUser.UserName);
                    sqlCommand.Parameters.AddWithValue("@email", tbUser.Email);
                    sqlCommand.Parameters.AddWithValue("@passworduser", tbUser.PasswordUser);
                    sqlCommand.Parameters.AddWithValue("@createdate", tbUser.CreateDate);

                    int insert = await sqlCommand.ExecuteNonQueryAsync();
                    await sqlCommand.DisposeAsync();

                    if (insert > 0)
                    {
                        returnData.IsSuccess = true;
                        returnData.Message = "El usuario fue creado satisfactoriamente";
                    }
                    else
                    {
                        returnData.IsSuccess = false;
                        returnData.Message = "No se altero ningun dato al momento de crear el usuario";
                    }
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

                TbUser existsUser = await Obtener(tbUser.IdUser);
                if (existsUser != null)
                {
                    if (existsUser.Email != tbUser.Email || tbUser.UserName != existsUser.UserName)
                    {
                        ReturnData exists = await ConsultUser(tbUser.UserName, tbUser.Email, tbUser.IdUser);

                        if (exists.IsSuccess)
                        {
                            //Actualiza el usuario

                            TenantInfo? tenantInfo = new();

                            //Cuando es nulo se setea una conexion por default por medio del archivo de configuración sino toma el valor que se envia en el header
                            if (_httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>() == null)
                            {
                                tenantInfo.ConnectionString = _configuration.GetConnectionString("ElTiempo1");
                            }
                            else
                            {
                                tenantInfo = _httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>().TenantInfo;
                            }

                            using SqlConnection sqlConnection = new(tenantInfo.ConnectionString);

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

                TenantInfo? tenantInfo = new();

                //Cuando es nulo se setea una conexion por default por medio del archivo de configuración sino toma el valor que se envia en el header
                if (_httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>() == null)
                {
                    tenantInfo.ConnectionString = _configuration.GetConnectionString("ElTiempo1");
                }
                else
                {
                    tenantInfo = _httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>().TenantInfo;
                }

                using SqlConnection sqlConnection = new(tenantInfo.ConnectionString);

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
                    if (value != null)
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
        public async Task<TbUser> Obtener(int idUser)
        {
            try
            {
                TenantInfo? tenantInfo = new();

                //Cuando es nulo se setea una conexion por default por medio del archivo de configuración sino toma el valor que se envia en el header
                if (_httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>() == null)
                {
                    tenantInfo.ConnectionString = _configuration.GetConnectionString("ElTiempo1");
                }
                else
                {
                    tenantInfo = _httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>().TenantInfo;
                }

                using SqlConnection sqlConnection = new(tenantInfo.ConnectionString);

                if (sqlConnection.State == ConnectionState.Closed)
                {
                    sqlConnection.Open();
                }

                TbUser tbUser = new();

                string scriptSql = @"SELECT * FROM TB_User WHERE IdUser = @IdUser ";

                SqlCommand sqlCommand = new()
                {
                    Connection = sqlConnection,
                    CommandText = scriptSql
                };

                sqlCommand.Parameters.AddWithValue("IdUser", idUser);

                SqlDataReader dataReader = await sqlCommand.ExecuteReaderAsync();

                if(dataReader.HasRows)
                {
                    while(await dataReader.ReadAsync())
                    {
                        tbUser = new()
                        {
                            IdUser = Convert.ToInt32(dataReader["IdUser"]),
                            FullName = Convert.ToString(dataReader["FullName"]),
                            UserName = Convert.ToString(dataReader["UserName"]),
                            Email = Convert.ToString(dataReader["Email"])
                        };
                    }
                }
                else
                {
                    tbUser = null;
                }

                await dataReader.CloseAsync();
                await dataReader.DisposeAsync();
                await sqlCommand.DisposeAsync();

                return tbUser;

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

                TenantInfo? tenantInfo = new();

                //Cuando es nulo se setea una conexion por default por medio del archivo de configuración sino toma el valor que se envia en el header
                if (_httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>() == null)
                {
                    tenantInfo.ConnectionString = _configuration.GetConnectionString("ElTiempo1");
                }
                else
                {
                    tenantInfo = _httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>().TenantInfo;
                }

                using SqlConnection sqlConnection = new(tenantInfo.ConnectionString);

                if (sqlConnection.State == ConnectionState.Closed)
                {
                    sqlConnection.Open();
                }

                string scriptSql = @"SELECT * FROM TB_User ";

                SqlCommand sqlCommand = new()
                {
                    Connection = sqlConnection,
                    CommandText = scriptSql
                };

                DataTable dataTable = new();

                SqlDataReader dataReader = await sqlCommand.ExecuteReaderAsync();
                dataTable.Load(dataReader);
                await dataReader.CloseAsync();
                await dataReader.DisposeAsync();
                await sqlCommand.DisposeAsync();

                if (dataTable.Rows.Count > 0)
                {
                    returnData.IsSuccess = true;
                    returnData.Message = "Usuarios encontrados y listados correctamente";

                    List<TbUser> tbUsers = new();

                    tbUsers = (from DataRow dr in dataTable.Rows
                                   select new TbUser()
                                   {
                                       IdUser = Convert.ToInt32(dr["IdUser"]),
                                       FullName = dr["FullName"].ToString(),
                                       UserName = dr["UserName"].ToString(),
                                       Email = dr["Email"].ToString(),
                                       PasswordUser = dr["PasswordUser"].ToString()
                                   }).ToList();

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

        //Convertir un datatable a una lista generica
        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

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

                TenantInfo? tenantInfo = new();

                //Cuando es nulo se setea una conexion por default por medio del archivo de configuración sino toma el valor que se envia en el header
                if (_httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>() == null)
                {
                    tenantInfo.ConnectionString = _configuration.GetConnectionString("ElTiempo1");
                }
                else
                {
                    tenantInfo = _httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>().TenantInfo;
                }

                using SqlConnection sqlConnection = new(tenantInfo.ConnectionString);

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

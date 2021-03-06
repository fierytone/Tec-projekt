﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Data.Interface;
using MySql.Data;
using Model;
using Data.Classes;
using MySql.Data.MySqlClient;


namespace Data.Service
{
    public class UserService : IUserService
    {
        private readonly IConnectionInformationService _connectionInformationService;

        public UserService(IConnectionInformationService connectionInformationService)
        {
            _connectionInformationService = connectionInformationService;
        }

        public int UserId
        {
            get
            {
                var requestCookie = HttpContext.Current.Request
                    .Cookies[FormsAuthentication.FormsCookieName];
                if (requestCookie != null)
                {
                    var authTicket = FormsAuthentication.Decrypt(requestCookie.Value);
                    if (authTicket != null)
                    {
                        return int.Parse(authTicket.UserData.Split(':')[0]);
                    }
                }

                return -1;
            }
        }

        public User FindByRoomAndPassword(int roomnr, string password)
        {
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select Id, Fornavn, Efternavn, VærelseNr, Email, KodeHash, Billede, Type, (select Type from BrugerType where id = Bruger.Type) as TypeName from Bruger where VærelseNr = @roomnr";
                    cmd.Parameters.AddWithValue("@roomnr", roomnr);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (PasswordHash.Validate(password, (string)dr["KodeHash"]))
                            {
                                return BindUser(dr);
                            }
                        }
                    }

                }
            }
            return null;
        }

        public bool SaveProfile(int id, string firstname, string lastname, string email, int roleId)
        {
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "update Bruger set Fornavn = @firstname, Efternavn = @lastname, Email = @email, Type = @roleId where id = @id";
                    cmd.Parameters.AddWithValue("@firstname", firstname);
                    cmd.Parameters.AddWithValue("@lastname", lastname);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@roleId", roleId);
                    cmd.Parameters.AddWithValue("@id", id);
                    var count = cmd.ExecuteNonQuery();
                    if (count > 0)
                    {
                        return true;
                    }

                }
            }
            return false;
        }

        public bool SaveCredentials(int roomnr, string password, int id)
        {
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "update Bruger set VærelseNr = @roomnr, KodeHash = @password  where id = @id";
                    cmd.Parameters.AddWithValue("@roomnr", roomnr);
                    cmd.Parameters.AddWithValue("@password", PasswordHash.Hash(password));
                    cmd.Parameters.AddWithValue("@id", id);
                    var count = cmd.ExecuteNonQuery();
                    if (count > 0)
                    {
                        return true;
                    }

                }
            }
            return false;
        }


        public bool SaveImage(byte[] file, int id)
        {
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "update Bruger set Billede = @image  where id = @id";
                    cmd.Parameters.AddWithValue("@image", file);
                    cmd.Parameters.AddWithValue("@id", id);
                    var count = cmd.ExecuteNonQuery();
                    if (count > 0)
                    {
                        return true;
                    }

                }
            }
            return false;
        }

        public User FindById(int id, bool bindPicture = false)
        {
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select Id, Fornavn, Efternavn, VærelseNr, Email, KodeHash, Billede, Type, (select Type from BrugerType where id = Bruger.Type) as TypeName from Bruger where Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            return BindUser(dr, bindPicture);
                        }
                    }

                }
            }
            return null;
        }

        public IEnumerable<User> GetUsers()
        {
            var result = new List<User>();
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select Id, Fornavn, Efternavn, VærelseNr, Email, KodeHash, Billede, Type, (select Type from BrugerType where id = Bruger.Type) as TypeName from Bruger";
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(BindUser(dr));
                        }
                    }

                }
            }
            return result;
        }

        public IEnumerable<Role> GetRoles()
        {
            var result = new List<Role>();
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select Id, Type from BrugerType";
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(new Role
                            {
                                Id = (int)dr["Id"],
                                Name = (string)dr["Type"]
                            });
                        }
                    }

                }
            }
            return result;
        }

        public bool CreateUser(User user, string password)
        {
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "insert into Bruger (Fornavn, Efternavn, Email, VærelseNr, KodeHash, Billede, Type) values(@Fornavn, @Efternavn, @Email, @VærelseNr, @KodeHash, @Billede, @Type)";
                    cmd.Parameters.AddWithValue("@Fornavn", user.Fornavn);
                    cmd.Parameters.AddWithValue("@Efternavn", user.Efternavn);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@VærelseNr", user.VærelseNr);
                    cmd.Parameters.AddWithValue("@KodeHash", PasswordHash.Hash(password));
                    cmd.Parameters.AddWithValue("@Billede", user.Billede);
                    cmd.Parameters.AddWithValue("@Type", user.Role.Id);
                    var count = cmd.ExecuteNonQuery();
                    if (count > 0)
                    {
                        return true;
                    }

                }
            }
            return false;
        }

        public bool DeleteUser(int id)
        {
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "delete from Bruger where id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    var count = cmd.ExecuteNonQuery();
                    if (count > 0)
                    {
                        return true;
                    }

                }
            }
            return false;
        }

        public IEnumerable<User> SearchUsers(string searchText, UserSort sort, int pageSize, int offSet)
        {
            var sql = "select Id, Fornavn, Efternavn, VærelseNr, Email, KodeHash, Billede, Type, (select Type from BrugerType where id = Bruger.Type) as TypeName from Bruger [SEARCHTEXT] [SORTNAME] limit [Offset],[PageSize]";


            switch (sort)
            {
                case UserSort.RoomNrAsc:
                    sql = sql.Replace("[SORTNAME]", "order by VærelseNr asc");
                    break;
                case UserSort.RoomNrDesc:
                    sql = sql.Replace("[SORTNAME]", "order by VærelseNr desc");
                    break;
                case UserSort.FirstnameAsc:
                    sql = sql.Replace("[SORTNAME]", "order by Fornavn asc");
                    break;
                case UserSort.FirstnameDesc:
                    sql = sql.Replace("[SORTNAME]", "order by Fornavn desc");
                    break;
                case UserSort.LastnameAsc:
                    sql = sql.Replace("[SORTNAME]", "order by Efternavn asc");
                    break;
                case UserSort.LastnameDesc:
                    sql = sql.Replace("[SORTNAME]", "order by Efternavn desc");
                    break;
                case UserSort.EmailAsc:
                    sql = sql.Replace("[SORTNAME]", "order by Email asc");
                    break;
                case UserSort.EmailDesc:
                    sql = sql.Replace("[SORTNAME]", "order by Email desc");
                    break;
                case UserSort.TypeAsc:
                    sql = sql.Replace("[SORTNAME]", "order by Type asc");
                    break;
                case UserSort.TypeDesc:
                    sql = sql.Replace("[SORTNAME]", "order by Type desc");
                    break;
            }

            sql = sql.Replace("[Offset]", offSet.ToString()).Replace("[PageSize]", pageSize.ToString());

            var result = new List<User>();
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    if (!string.IsNullOrEmpty(searchText))
                    {

                        if (searchText.StartsWith("VærelseNr:"))
                        {
                            sql = sql.Replace("[SEARCHTEXT]", "where VærelseNr like @search");
                            cmd.Parameters.AddWithValue("@search", string.Format("%{0}%", searchText.Replace("VærelseNr:", "")));
                        }
                        if (searchText.StartsWith("Fornavn:"))
                        {
                            sql = sql.Replace("[SEARCHTEXT]", "where Fornavn like @search");
                            cmd.Parameters.AddWithValue("@search", string.Format("%{0}%", searchText.Replace("Fornavn:", "")));
                        }
                        if (searchText.StartsWith("Efternavn:"))
                        {
                            sql = sql.Replace("[SEARCHTEXT]", "where Efternavn like @search");
                            cmd.Parameters.AddWithValue("@search", string.Format("%{0}%", searchText.Replace("Efternavn:", "")));
                        }
                        if (searchText.StartsWith("Email:"))
                        {
                            sql = sql.Replace("[SEARCHTEXT]", "where Email like @search");
                            cmd.Parameters.AddWithValue("@search", string.Format("%{0}%", searchText.Replace("Email:", "")));
                        }
                        if (sql.Contains("[SEARCHTEXT]"))
                        {
                            sql = sql.Replace("[SEARCHTEXT]", "");
                        }
                    }
                    else
                    {
                        sql = sql.Replace("[SEARCHTEXT]", "");
                    }
                    cmd.CommandText = sql;
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(BindUser(dr));
                        }
                    }

                }
            }
            return result;
        }

        public int UserCount()
        {
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select count(Id) as Total from Bruger";

                    return Convert.ToInt32(cmd.ExecuteScalar());

                }
            }
        }

        public byte[] GetUserImage(int userid)
        {
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select Billede from Bruger where Id = @id";
                    cmd.Parameters.AddWithValue("@id", userid);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            return dr["Billede"] == DBNull.Value ? new byte[0] : (byte[])dr["Billede"];
                        }
                    }

                }
            }
            return new byte[0];
        }

        private User BindUser(MySqlDataReader dr, bool bindPicture = false)
        {
            var user = new User();
            if (dr != null)
            {
                user.Id = (int)dr["Id"];
                user.Fornavn = (string)dr["Fornavn"];
                user.Efternavn = (string)dr["Efternavn"];
                user.Email = (string)dr["Email"];
                user.VærelseNr = (int)dr["VærelseNr"];
                if (bindPicture)
                {
                    user.Billede = (byte[])dr["Billede"];
                }
                user.Role = new Role { Id = (int)dr["Type"], Name = (string)dr["TypeName"] ?? "" };
            }
            return user;
        }

        public List<User> GetPagedUser(UserSearchTerms terms)
        {
            var result = new List<User>();
            var sql = @"select Bruger.Id BrugerId, Bruger.Fornavn BrugerFornavn, Bruger.Efternavn BrugerEfternavn, Bruger.VærelseNr BrugerVærelseNr, Bruger.Email BrugerEmail, Bruger.Billede BrugerBillede, Bruger.Type BrugerType, BrugerType.Id BrugeTypeId, BrugerType.Type BrugerTypeType from ( select * from Bruger where Fornavn like @search [SORT] limit @offset, @pageSize) as Bruger
                     left join BrugerType on Bruger.Type=BrugerType.Id";

            switch (terms.Sort)
            {
                case SortUser.AscFornavn:
                    sql = sql.Replace("[SORT]", "order by Fornavn asc");
                    break;
                case SortUser.DescFornavn:
                    sql = sql.Replace("[SORT]", "order by Fornavn desc");
                    break;
                case SortUser.AscEfternavn:
                    sql = sql.Replace("[SORT]", "order by Efternavn asc");
                    break;
                case SortUser.DescEfternavn:
                    sql = sql.Replace("[SORT]", "order by Efternavn dsc");
                    break;
                case SortUser.AscVærelseNr:
                    sql = sql.Replace("[SORT]", "order by VærelseNr asc");
                    break;
                case SortUser.DescVærelseNr:
                    sql = sql.Replace("[SORT]", "order by VærelseNr desc");
                    break;
                case SortUser.AscEmail:
                    sql = sql.Replace("[SORT]", "order by Email asc");
                    break;
                case SortUser.DescEmail:
                    sql = sql.Replace("[SORT]", "order by Email desc");
                    break;
                case SortUser.AscType:
                    sql = sql.Replace("[SORT]", "order by Type asc");
                    break;
                case SortUser.DescType:
                    sql = sql.Replace("[SORT]", "order by type desc");
                    break;
            }
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@offset", terms.PageSize * terms.Page);
                    cmd.Parameters.AddWithValue("@pageSize", terms.PageSize);
                    cmd.Parameters.AddWithValue("@search", "%" + (terms.SearchText ?? "") + "%");
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var brugerId = (int)dr["BrugerId"];
                            var brugerFornavn = (string)dr["BrugerFornavn"];
                            var brugerEfternavn = (string)dr["BrugerEfternavn"];
                            var brugerVærelseNr = (int)dr["BrugerVærelseNr"];
                            var brugerEmail = (string)dr["BrugerEmail"];
                            var brugerBillede = dr["BrugerBillede"] != DBNull.Value ? (byte[])dr["BrugerBillede"] :new byte[0];
                            var brugerType = (int)dr["BrugerType"];
                            var brugerTypeType = (string)dr["BrugerTypeType"];

                            result.Add(new User()
                            {
                                Id = brugerId,
                                Fornavn = brugerFornavn,
                                Efternavn = brugerEfternavn,
                                VærelseNr = brugerVærelseNr,
                                Email = brugerEmail,
                                Billede=brugerBillede,
                                Role = new Role
                                {
                                    Id = brugerType,
                                    Name = brugerTypeType
                                }                               
                            });
                        }
                    }
                }
            }
            return result;
        }

        public List<ItemSold> GetTopBuyers()
        {
            var result = new List<ItemSold>();
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = @"select Bruger.Id, concat_ws(' ', Fornavn, Efternavn) as FullName, count(BrugerId) as Total from Ordre 
                    join Bruger on Bruger.Id = BrugerId
                    group by BrugerId
                        order by Total desc limit 3";
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(new ItemSold
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Navn = (string)dr["FullName"],
                                Total = Convert.ToInt32(dr["Total"])
                            });
                        }
                    }
                }
            }
            return result;
        }

        public int GetUserBuyerPlace(int userId)
        {
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = @"
set @rownr = 0;
select rownumber from (
select @rownr:=@rownr+1 as rownumber, users.* from (select Bruger.Id, ifnull(count(Ordre.Id), 0) as Total from Bruger
left join Ordre on Ordre.BrugerId = Bruger.Id
group by Bruger.Id
order by Total desc, Fornavn asc) as users
) as userrows
where Id = @userId";
                    cmd.Parameters.AddWithValue("@userId", userId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int GetUserTotal()
        {
            using (var con = new MySqlConnection(_connectionInformationService.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select count(*) as Total from Bruger";
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

    }
}

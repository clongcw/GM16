using GM16.Shared.Constants;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel.DBContext
{
    public class UserContext
    {
        public SqlSugarClient Db;
        public UserContext()
        {
            Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = Environments.UserConnectionString,
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
                AopEvents = new AopEvents()
                {
                    OnLogExecuting = (sql, p) =>
                    {
                        Console.WriteLine(sql);
                    }
                }
            });
        }

        public SimpleClient<User> Users => new SimpleClient<User>(Db);

        public SimpleClient<Role> Roles => new SimpleClient<Role>(Db);
        public SimpleClient<Privilege> Privileges => new SimpleClient<Privilege>(Db);
    }
}

using GM16.Shared.Constants;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel.DBContext
{
    public class ProtocolContext
    {
        public SqlSugarClient Db;
        public ProtocolContext()
        {
            Db = new SqlSugarClient(new List<ConnectionConfig>()
            {
                new ConnectionConfig()
                {
                ConfigId = "ProtocolsDb",
                ConnectionString = Environments.ProtocolConnectionString,
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
                AopEvents = new AopEvents()
                {
                    OnLogExecuting = (sql, p) =>
                    {
                        Console.WriteLine(sql);
                    }
                } },
                new ConnectionConfig()
                {
                ConfigId = "StepsDb",
                ConnectionString = Environments.ProtocolConnectionString,
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
                AopEvents = new AopEvents()
                {
                    OnLogExecuting = (sql, p) =>
                    {
                        Console.WriteLine(sql);
                    }
                } },
                });
        }

        public SimpleClient<Step> StepsDb => new SimpleClient<Step>(Db);
        public SimpleClient<Protocol> ProtocolsDb => new SimpleClient<Protocol>(Db);
        public SimpleClient<MixParameter> MixParametersDb => new SimpleClient<MixParameter>(Db);
    }
}

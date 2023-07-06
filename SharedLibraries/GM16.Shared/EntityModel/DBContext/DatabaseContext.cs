using Dm.filter.rw;
using GM16.Shared.Constants;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel.DBContext
{
    public class DatabaseContext
    {
        public SqlSugarClient Db;
        public DatabaseContext()
        {
            Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = Environments.DataBaseConnectionString,
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

        public SimpleClient<MotorInfo> MotorInfos => new SimpleClient<MotorInfo>(Db);

        public SimpleClient<TipParam> TipParams => new SimpleClient<TipParam>(Db);

        public SimpleClient<TipInfo> TipInfos => new SimpleClient<TipInfo>(Db);

        public SimpleClient<LiquidTypeInfo> LiquidTypeInfos => new SimpleClient<LiquidTypeInfo>(Db);

        public SimpleClient<LiquidContainer> LiquidContainers => new SimpleClient<LiquidContainer>(Db);

        public SimpleClient<SampleType> SampleTypes => new SimpleClient<SampleType>(Db);
    }
}

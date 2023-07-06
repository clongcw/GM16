using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    public class User
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public bool Enable { get; set; }

        public bool IsAdmin { get; set; } = false;

        // 当前用户角色，每个用户只能有一个角色
        public string RoleName { get; set; }

        // 当前用户的角色
        [SugarColumn(IsIgnore = true)]
        public Role Role { get; set; }
        [SugarColumn(IsNullable = true)]
        public UserRole UserRole { get; set; }
        [SugarColumn(IsNullable = true)]
        public int Sequence { get; set; }
        [SugarColumn(IsNullable = true)]
        public string GroupName { get; set; }
        [SugarColumn(IsNullable = true)]
        public int LoginTimes { get; set; }

        public bool Enabled { get; set; } = true;
        [SugarColumn(IsNullable = true)]
        public DateTime CreateDate { get; set; }
        [SugarColumn(IsNullable = true)]
        public string Creator { get; set; }
        [SugarColumn(IsNullable = true)]
        public string Remark { get; set; }
        [SugarColumn(IsNullable = true)]
        public DateTime LastLoginTime { get; set; }
        [SugarColumn(IsNullable = true)]
        public int WrongPwdTimes { get; set; }
        [SugarColumn(IsNullable = true)]
        public bool IsDefault { get; set; }
    }
}

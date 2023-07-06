using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    public class Role
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RoleName { get; set; }

        [Navigate(NavigateType.OneToMany, nameof(Privilege.RoleId))]
        public List<Privilege> Privileges { get; set; } 
    }

    

    public class Privilege
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        public int RoleId { get; set; }
        public bool Status { get; set; } = false;
        public string Name { get; set; }

        private string visiual = "Visible";
        public string Visiual
        {
            get => visiual;
            set
            {
                string tmp = value;
                if (tmp == "显示")
                {
                    visiual = "Visible";
                }
                else if (tmp == "不显示")
                {
                    visiual = "Collapsed";
                }
                else
                {
                    visiual = tmp;
                }
            }
        }

        public bool IsEnabled { get; set; } = false;
        public bool IsReadOnly { get; set; } = true;
    }

    public class Privileges
    {
        public Privilege Lis { get; set; } = new();
        public Privilege Report { get; set; } = new();
        public Privilege Setting { get; set; } = new();
    }
}

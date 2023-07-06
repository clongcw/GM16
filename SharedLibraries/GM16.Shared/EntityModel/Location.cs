using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    /// <summary>
    /// 位置实体
    /// </summary>
    [SugarTable("Locations")]

    public class Location
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        public int Z2 { get; set; }

        public int GroupId { get; set; }

        public int DisplayId { get; set; }


        public Location Clone()
        {
            Location locaClone = new Location();
            locaClone.X = X;
            locaClone.Y = Y;
            locaClone.Z = Z;
            locaClone.Z2 = Z2;
            locaClone.Name = Name;
            locaClone.GroupId = GroupId;
            return locaClone;
        }
    }
}

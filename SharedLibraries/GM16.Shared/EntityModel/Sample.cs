using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    [SugarTable("Samples")]
    public class Sample
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public int Channel { get; set; }

        public int SubId { get; set; }
        public int SmpType { get; set; } = 4;
        public int ContainerType { get; set; }
        public bool IsValid { get; set; } = true;
    }
}

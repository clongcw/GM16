using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    [SugarTable("SampleTypes")]

    public class SampleType
    {
        public int Id { get; set; }

        public int SmpType { get; set; }

        public string ChineseName { get; set; }

        public string EnglishName { get; set; }

        public string Sign { get; set; }
        public int Group { get; set; }
        public int Sequence { get; set; }
    }
}

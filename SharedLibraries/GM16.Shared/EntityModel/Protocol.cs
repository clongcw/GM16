using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    [SugarTable("Protocols")]
    public class Protocol
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public DateTime CreateDateTime { get; set; }

        [SugarColumn(IsIgnore = true)]
        [Navigate(NavigateType.OneToMany, nameof(Step.ProtocolId))]
        public List<Step> Steps { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int StartSequence { get; set; }

        public int PcrTempCount { get; set; } = 1;

        public string Token { get; set; }
        public string PcrTemplateFile { get; set; }

        [SugarColumn(IsIgnore = true)]
        public bool IsManualAddSmp { get; set; }

        [SugarColumn(IsIgnore = true)]
        public List<Sample> SmpList { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int ExcuteResult { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int Channel { get; set; }

        public Protocol()
        {
            Steps = new List<Step>();
            SmpList = new List<Sample>();
        }
    }
}

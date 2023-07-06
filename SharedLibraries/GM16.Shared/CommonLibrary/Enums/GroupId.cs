using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommonLibrary
{
    public enum GroupId
    {
        Pipette900_ChannelA = 0,//排枪 900tip头
        Pipette900_ChannelB = 1,
        Pipette900_ChannelC = 2,
        Pipette900_ChannelD = 3,

        Pipette175_ChannelA = 4,//排枪 175tip头
        Pipette175_ChannelB = 5,
        Pipette175_ChannelC = 6,
        Pipette175_ChannelD = 7,

        Adp175_ChannelA = 10,//Adp 175tip头
        Adp175_ChannelB = 11,
        Adp175_ChannelC = 12,
        Adp175_ChannelD = 13,
        TipBox175 = 14,
        Adp900_ChannelA = 15,//Adp 900tip头
        Adp900_ChannelB = 16,
        Adp900_ChannelC = 17,
        Adp900_ChannelD = 18,




        Scanner_ChannelA = 30,//扫码枪A通道
        Scanner_ChannelB = 31,
        Scanner_ChannelC = 32,
        Scanner_ChannelD = 33,

        ReagentScanner_ChannelA = 40,//试剂扫码枪A通道
        ReagentScanner_ChannelB = 41,
        ReagentScanner_ChannelC = 42,
        ReagentScanner_ChannelD = 43,

        SealingYCollection = 50,//热封Y收集位置
        SealingYSealing = 51,//热封Y热封位置
        SealingYHome = 52,
        SealingZSealing = 53,
        SealingZHome = 54,
        //Adp900_ChannelA = 14,//Adp 900tip头
        //Adp900_ChannelB = 15,
        //Adp900_ChannelC = 16,
        //Adp900_ChannelD = 17,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommonLibrary.Enums
{
    public enum RetryFlag
    {
        /// <summary>
        /// 终止
        /// </summary>
        Abort,
        /// <summary>
        /// 重试
        /// </summary>
        Retry,
        /// <summary>
        /// 忽略
        /// </summary>
        Ignore,
    }
}

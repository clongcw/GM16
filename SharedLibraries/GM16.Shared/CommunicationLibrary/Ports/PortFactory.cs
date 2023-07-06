using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommunicationLibrary
{
    public class PortFactory
    {
        /// <summary>
        /// 创建实列
        /// </summary>
        /// <returns></returns>
        public static Port GetInstance(string portName)
        {
            Port instance = null;
            if (portName != "")
            {
                instance = (Port)Assembly.Load("GM16.Shared.CommunicationLibrary").CreateInstance("GM16.Shared.CommunicationLibrary." + portName);
            }
            return instance;
        }
    }
}

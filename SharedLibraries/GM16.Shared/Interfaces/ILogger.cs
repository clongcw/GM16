using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.Interfaces
{
    public interface ILogger
    {
        void Debug(string message);

        void Debug(string messageTemplate, params object[] args);

        void Information(string message);

        void Information(string messageTemplate, params object[] args);

        void Warning(string message);

        void Warning(string messageTemplate, params object[] args);

        void Error(Exception exception, string messageTemplate);

        void Error(Exception exception, string messageTemplate, params object[] args);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEAR.Domain.Telemetry
{
    public interface IFEARTelemetrySignalService
    {
        Guid Id { get; }
        void SendSignal(IFEARTelemetrySignal signal);
    }
}

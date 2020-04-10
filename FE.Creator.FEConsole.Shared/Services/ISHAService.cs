using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.FEConsole.Shared.Services.Cryptography
{
   public interface ISHAService
    {
        string CalculateSHA256(byte[] content);
    }
}

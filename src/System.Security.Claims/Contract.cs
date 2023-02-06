#if NET35 || NET30 || NET20
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace System.Diagnostics.Contracts
{
    internal static class Contract
    {
        /// <summary>Marks the end of the contract section when a method's contracts contain only preconditions in the <see langword="if" />-<see langword="then" />-<see langword="throw" /> form.</summary>
        [Conditional("CONTRACTS_FULL")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void EndContractBlock()
        {
        }
    }
}
#endif
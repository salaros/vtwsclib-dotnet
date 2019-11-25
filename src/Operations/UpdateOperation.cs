using System;
using System.Collections.Generic;
using System.Text;

namespace Salaros.vTiger.WebService.Operations
{
    public class UpdateOperation : OperationBase
    {
        internal UpdateOperation(Client client, string moduleName)
            : base(client, "update")
        {
        }
    }
}

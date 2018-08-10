using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebService.SE

{
    interface IDocumento<t>
    {
        bool inserirDocumento(t Indice);
        bool uploadDocumento(t Indice, string id);

    }
}

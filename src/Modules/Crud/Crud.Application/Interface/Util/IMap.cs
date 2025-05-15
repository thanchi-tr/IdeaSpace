using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crud.Application.Interface.Util
{
    public interface IMap
    {
        DomainModel Map<ModelDTO, DomainModel>(ModelDTO dto)
            where ModelDTO : class
            where DomainModel : new();
    }
}

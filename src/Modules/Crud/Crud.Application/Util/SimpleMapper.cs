
using Crud.Application.Interface.Util;
using Crud.Application.Util.Attribute;
using System.Reflection;

namespace Crud.Application.Util
{
    public class SimpleMapper : IMap
    {
        public DomainModel Map<ModelDTO, DomainModel>(ModelDTO dto) 
            where ModelDTO : class
            where DomainModel : new()
        {
            var model = new DomainModel();
            var domainModelProps = typeof(DomainModel).GetProperties();
            var dtoProps = typeof(ModelDTO).GetProperties();

            foreach (var prop in dtoProps)
            {
                // Skip if property has [DoNotMap]
                if (prop.GetCustomAttribute<DoNotMapAttribute>() != null)
                    continue;
                
                var destProp = domainModelProps.FirstOrDefault(p => p.Name == prop.Name && p.PropertyType == prop.PropertyType);
                if (destProp != null && destProp.CanWrite)
                {
                    var value = prop.GetValue(dto) ?? default;
                    destProp.SetValue(model, value);
                }
            }
            return model;
        }
    }
}

using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xrm.Core
{
    public static class Extensions
    {
        public static T Merge<T>(this T baseEntity, T entity)
            where T : Entity
        {
            if (baseEntity == null)
            {
                return entity;
            }
            if (entity != null)
            {
                foreach (var attribute in entity.Attributes)
                {
                    if(attribute.Value != null)
                    {
                        baseEntity[attribute.Key] = attribute.Value;
                    }
                }
            }

            return baseEntity;
        }
    }
}

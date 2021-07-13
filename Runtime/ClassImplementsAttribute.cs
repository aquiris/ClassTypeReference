using System;

namespace Aquiris.ClassTypeReference.Reflection
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ClassImplementsAttribute : ClassTypeConstraintAttribute
    {
        public Type InterfaceType { get; private set; }

        public ClassImplementsAttribute()
        {
        }

        public ClassImplementsAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }

        public override bool IsConstraintSatisfied(Type type)
        {
            if (base.IsConstraintSatisfied(type))
            {
                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (interfaceType == InterfaceType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

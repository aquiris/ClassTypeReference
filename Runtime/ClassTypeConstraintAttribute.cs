using System;
using UnityEngine;

namespace Aquiris.ClassTypeReference.Reflection
{
    public abstract class ClassTypeConstraintAttribute : PropertyAttribute
    {
        private bool _allowAbstract = false;
        public bool AllowAbstract
        {
            get => _allowAbstract;
            set => _allowAbstract = value;
        }

        private EClassGrouping _grouping = EClassGrouping.ByNamespaceFlat;
        public EClassGrouping Grouping
        {
            get => _grouping;
            set => _grouping = value;
        }

        public virtual bool IsConstraintSatisfied(Type type)
        {
            return AllowAbstract || !type.IsAbstract;
        }
    }
}

using System;
using UnityEngine;

namespace Aquiris.ClassTypeReference
{
    [Serializable]
    public sealed class ClassTypeRef : ISerializationCallbackReceiver
    {
        [SerializeField]
        private string _classRef = string.Empty;

        private Type _type;
        public Type Type
        {
            get => _type;
            set
            {
                if (value != null && !value.IsClass)
                {
                    throw new ArgumentException($"'{value.FullName}' is not a class type.", "value");
                }

                _type = value;
                _classRef = GetClassRef(value);
            }
        }

        public ClassTypeRef()
        {
        }

        public ClassTypeRef(string assemblyQualifiedClassName)
        {
            Type = !string.IsNullOrEmpty(assemblyQualifiedClassName)
                ? Type.GetType(assemblyQualifiedClassName)
                : null;
        }

        public ClassTypeRef(Type type)
        {
            Type = type;
        }

        public static string GetClassRef(Type type)
        {
            return type != null
                ? $"{type.FullName}, {type.Assembly.GetName().Name}"
                : string.Empty;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(_classRef))
            {
                _type = System.Type.GetType(_classRef);
                if (_type == null)
                {
                    Debug.LogWarning($"'{_classRef} was referenced but class type was not found.");
                }
            }
            else
            {
                _type = null;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        public static implicit operator string(ClassTypeRef typeRef)
        {
            return typeRef._classRef;
        }

        public static implicit operator Type(ClassTypeRef typeRef)
        {
            return typeRef.Type;
        }

        public static implicit operator ClassTypeRef(Type type)
        {
            return new ClassTypeRef(type);
        }

        public override string ToString()
        {
            return Type != null
                ? Type.FullName
                : "(None)";
        }
    }
}

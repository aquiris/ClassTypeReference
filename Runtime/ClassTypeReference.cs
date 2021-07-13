using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Aquiris.ClassTypeReference.Reflection
{
    [Serializable]
    public sealed class ClassTypeReference : ISerializationCallbackReceiver
    {
        private Type _type;

        [SerializeField]
        private string _classRef = string.Empty;

        public ClassTypeReference()
        {
        }

        public ClassTypeReference(string assemblyQualifiedClassName)
        {
            Type = !string.IsNullOrEmpty(assemblyQualifiedClassName)
                ? Type.GetType(assemblyQualifiedClassName)
                : null;
        }

        public ClassTypeReference(Type type)
        {
            Type = type;
        }

        public static string GetClassRef(Type type)
        {
            return type != null
                ? $"{type.FullName}, {type.Assembly.GetName().Name}"
                : string.Empty;
        }

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

        public static implicit operator string(ClassTypeReference typeReference)
        {
            return typeReference._classRef;
        }

        public static implicit operator Type(ClassTypeReference typeReference)
        {
            return typeReference.Type;
        }

        public static implicit operator ClassTypeReference(Type type)
        {
            return new ClassTypeReference(type);
        }

        public override string ToString()
        {
            return Type != null
                ? Type.FullName
                : "(None)";
        }
    }
}

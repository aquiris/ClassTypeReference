// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEngine;

namespace Aquiris.ClassType.Reflection
{
    /// <summary>
    /// Base class for class selection constraints that can be applied when selecting
    /// a <see cref="ClassTypeReference"/> with the Unity inspector.
    /// </summary>
    public abstract class ClassTypeConstraintAttribute : PropertyAttribute
    {
        private EClassGrouping _grouping = EClassGrouping.ByNamespaceFlat;
        private bool _allowAbstract = false;

        /// <summary>
        /// Gets or sets grouping of selectable classes. Defaults to <see cref="EClassGrouping.ByNamespaceFlat"/>
        /// unless explicitly specified.
        /// </summary>
        public EClassGrouping Grouping
        {
            get => _grouping;
            set => _grouping = value;
        }

        /// <summary>
        /// Gets or sets whether abstract classes can be selected from drop-down.
        /// Defaults to a value of <c>false</c> unless explicitly specified.
        /// </summary>
        public bool AllowAbstract
        {
            get => _allowAbstract;
            set => _allowAbstract = value;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Type"/> satisfies filter constraint.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <returns>
        /// A <see cref="bool"/> value indicating if the type specified by <paramref name="type"/>
        /// satisfies this constraint and should thus be selectable.
        /// </returns>
        public virtual bool IsConstraintSatisfied(Type type)
        {
            return AllowAbstract || !type.IsAbstract;
        }
    }
}

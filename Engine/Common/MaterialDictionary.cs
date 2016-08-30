﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Common
{
    /// <summary>
    /// Material by name dictionary
    /// </summary>
    [Serializable]
    public class MaterialDictionary : Dictionary<string, MeshMaterial>
    {
        /// <summary>
        /// Gets material description by name
        /// </summary>
        /// <param name="material">Material name</param>
        /// <returns>Return material description by name if exists</returns>
        public new MeshMaterial this[string material]
        {
            get
            {
                if (!string.IsNullOrEmpty(material))
                {
                    if (base.ContainsKey(material))
                    {
                        return base[material];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MaterialDictionary()
            : base()
        {

        }
        /// <summary>
        /// Constructor de serialización
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="context">Context</param>
        protected MaterialDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
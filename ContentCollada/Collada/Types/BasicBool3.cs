﻿using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicBool3 : BasicBoolArray
    {
        public BasicBool3()
        {

        }

        public BasicBool3(bool a, bool b, bool c)
        {
            this.Values = new bool[] { a, b, c };
        }

        public override string ToString()
        {
            if (this.Values != null && this.Values.Length == 3)
            {
                return string.Format("({0}, {1}, {2})", this.Values[0], this.Values[1], this.Values[2]);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}

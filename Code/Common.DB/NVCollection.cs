using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
namespace Common.DB
{


    [Serializable]
    public class NVCollection : Dictionary<string, object>
    {


        public NVCollection()
            : base()
        {
        }

        public NVCollection(int capacity)
            : base(capacity)
        {
        }

        protected NVCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public new NVCollection Add(string key, object value)
        {
            base.Add(key, value);
            return this;
        }

        public new object this[string key]
        {
            get
            {
                if (base.ContainsKey(key))
                {
                    return base[key];
                }

                return null;
            }
            set
            {
                base[key] = value;
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
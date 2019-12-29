using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiTsEd.Model
{
    interface ITiTSSaveable
    {
        AmfObject SaveableObject
        {
            get;
            set;
        }
    }

    interface ITiTSVersionedSaveable : ITiTSSaveable
    {
        int Version
        {
            get;
            set;
        }
    }


    public class TiTSSaveable : ITiTSSaveable
    {
        public TiTSSaveable()
        {
            SaveableObject = new AmfObjectObject();
        }

        public TiTSSaveable(AmfObject saveObject)
        {
            SaveableObject = saveObject;
        }


        [field: NonSerialized]
        public AmfObject SaveableObject
        {
            get;
            set;
        }

    }

    public class TiTSVersionedSaveable : TiTSSaveable, ITiTSVersionedSaveable
    {
        public TiTSVersionedSaveable() : base()
        { }

        public TiTSVersionedSaveable(AmfObject saveObject) : base(saveObject)
        { }

        public int Version
        {
            get { return (null != SaveableObject) ? SaveableObject.GetInt("version", 0) : 0; }
            set
            {
                if (null == SaveableObject)
                {
                    SaveableObject = new AmfObject(AmfTypes.Object);
                }
                SaveableObject["version"] = value;
            }
        }
    }

    public class TiTSSaveableClass : TiTSSaveable
    {
        public TiTSSaveableClass() : base()
        { }

        public TiTSSaveableClass(AmfObject saveObject) : base(saveObject)
        { }


        public String ClassInstance
        {
            get { return SaveableObject.GetString("classInstance"); }
            set { SaveableObject["classInstance"] = value; }
        }

    }

}

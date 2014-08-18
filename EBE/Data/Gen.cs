using System;

namespace EBE.Data
{
    public class Gen
    {
        public virtual Guid Id
        {
            get;
            set;
        }
        public virtual int ParenId
        {
            get;
            set;
        }
        public virtual int VariableId
        {
            get;
            set;
        }
        public virtual int OperatorId
        {
            get;
            set;
        }
        public virtual string Expression
        {
            get;
            set;
        }

        public virtual DateTime Created
        {
            get;
            set;
        }
        public virtual DateTime Modified
        {
            get;
            set;
        }
    }
}


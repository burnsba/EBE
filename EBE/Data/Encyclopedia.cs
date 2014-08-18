using System;

namespace EBE.Data
{
    public class Encyclopedia
    {
        public virtual Guid Id
        {
            get;
            set;
        }
        public virtual Guid GenId
        {
            get;
            set;
        }
        public virtual string RawInput
        {
            get;
            set;
        }
        public virtual string CleanedInput
        {
            get;
            set;
        }
        public virtual string ParsedInput
        {
            get;
            set;
        }
        public virtual int Variables
        {
            get;
            set;
        }
        public virtual int Slots
        {
            get;
            set;
        }
        public virtual int MaxBits
        {
            get;
            set;
        }
        public virtual string RawEval
        {
            get;
            set;
        }
        public virtual Guid EvalId
        {
            get;
            set;
        }

        public virtual Gen Gen
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


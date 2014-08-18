using System;
using System.Collections;
using System.Runtime.Serialization;

namespace EBE.Core
{
    [DataContract]
    public abstract class IteratorBase : IEnumerator
    {
        [DataMember(Name = "Id", Order = 1)]
        private int _id;

        [DataMember(Name = "IterationCount", Order = 2)]
        private int _iterationCount = 1;

        [DataMember(Name = "DoneIterating", Order = 10)]
        private bool _doneIterating = false;

        public IteratorBase()
        {
        }

        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        public int Id
        {
            get
            {
                return _id;
            }

            protected set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Gets or sets the iteration count.
        /// </summary>
        public int IterationCount
        {
            get
            {
                return _iterationCount;
            }

            protected set
            {
                _iterationCount = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="EBE.Core.IteratorBase"/> is done iterating.
        /// </summary>
        protected bool DoneIterating
        {
            get
            {
                return _doneIterating;
            }
            set
            {
                _doneIterating = value;
            }
        }

        /// <summary>
        /// Resets iterator back to initial conditions.
        /// </summary>
        public virtual void Reset()
        {
            _doneIterating = false;
            _iterationCount = 1;
        }

        /// <summary>
        /// Gets the current state.
        /// </summary>
        public string Current
        {
            get
            {
                return ToString();
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return ToString();
            }
        }

        /// <summary>
        /// Gets a value indicating whether there additional items to iterate over.
        /// </summary>
        public bool CanIterate
        {
            get
            {
                return !_doneIterating;
            }
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <remarks>
        /// Must be implemented by concrete implementation.
        /// </remarks>
        public abstract bool MoveNext();

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            // nothing to do
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            OnCreated();
        }

        /// <summary>
        /// Called by OnDeserializing.
        /// </summary>
        protected virtual void OnCreated()
        {
            // nothing to do
        }
    }
}


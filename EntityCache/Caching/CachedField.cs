using System;
using EntityCache.Interfaces;

namespace EntityCache.Caching
{
    /// <summary>
    ///     If this is used for fields or properties on a class, the fields and properties should
    ///     never be null! This type should actually be a struct, since this is a non-nullable
    ///     value type, but because we are not able to access properties by reference when using
    ///     reflection, this type had to be defined as a class. Make sure to initialize fields and
    ///     properties with a valid instance inside the class definition and use the Value property
    ///     or the Set method to initialize/change the underlying value.
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    [Serializable]
    public sealed class CachedField<TType> : ICachedField, IEquatable<CachedField<TType>>
    {
        public enum State
        {
            Uninitialized, // field is yet to be initialized
            Initialized, // field is initialized but not modified
            Modified, // field is initialized and modified
            Conflicted // field has been initialized and modified at one point, but now there is a conflict with the source of truth
        }

        /// <summary>
        ///     this is the first value that has been explicitly assigned to this field
        /// </summary>
        private TType _cachedSourceOfTruth;

        /// <summary>
        ///     if this field had been modified and we tried to pull from the database and noticed a conflict,
        ///     this contains the value that is written to the database.
        ///     the user now needs to decide: does he keep his update? does he keep the database value? or does he reset to the original value?
        /// </summary>
        private TType _externalChange;

        /// <summary>
        ///     this is the value that has been set by the user
        /// </summary>
        private TType _localChange;

        private State _state;

        /// <summary>
        ///     Creates a new uninitialized field.
        /// </summary>
        public CachedField()
        {
            _cachedSourceOfTruth = default(TType);
            _localChange = default(TType);
            _externalChange = default(TType);
            _state = State.Uninitialized;
        }

        /// <summary>
        ///     Creates a field that is initialized with the given value
        /// </summary>
        public CachedField(TType value)
        {
            _cachedSourceOfTruth = value;
            _localChange = default(TType);
            _externalChange = default(TType);
            _state = State.Initialized;
        }

        /// <summary>
        ///     Gives access to the underyling value much like the Value property for nullable types.
        /// </summary>
        public TType Value
        {
            get => (TType)GetValueAsObject();
            set => Set(value);
        }

        public void Reset()
        {
            _state = State.Initialized;
            _localChange = default(TType);
            _externalChange = default(TType);
        }

        public void Reset(object value)
        {
            _cachedSourceOfTruth = (TType)value;
            Reset();
        }

        public void Apply()
        {
            Reset(_localChange);
        }

        public object GetValueAsObject()
        {
            return _state == State.Modified ? _localChange : _cachedSourceOfTruth;
        }

        public void SetValueFromObject(object value) => Set((TType)value);

        /// <inheritdoc cref="SetValueFromObject(object)"/>
        public void Set(TType value)
        {
            switch (_state)
            {
                case State.Uninitialized:
                    _cachedSourceOfTruth = value;
                    _state = State.Initialized;
                    break;
                case State.Initialized:
                    if (!Equals(value, _cachedSourceOfTruth))
                    {
                        _localChange = value;
                        _state = State.Modified;
                    }

                    break;
                case State.Modified:
                    // if the new value is the same as the old value, we basically have no local change, so undo all changes
                    if (Equals(value, _cachedSourceOfTruth))
                    {
                        Reset();
                    }
                    else
                    {
                        _localChange = value;
                    }

                    break;
                case State.Conflicted:
                    // if there is a conflict, we dont do anything. resolve the conflict first!
                    break;
            }
        }

        public void PullFromObject(object value) => Pull((TType)value);
        /// <inheritdoc cref="PullFromObject(object)"/>
        public void Pull(TType value)
        {
            if (_state == State.Conflicted)
            {
                // if this is already conflicted, lets forget about the previous conflict and try pulling anew
                _externalChange = default(TType);
                _state = State.Modified;
            }

            switch (_state)
            {
                // if we have no local change, we can just take the new value as the new source of truth
                case State.Uninitialized:
                case State.Initialized:
                    Reset(value);
                    break;
                case State.Modified:
                    // if the new value is equal to the cached source of truth, we do nothing.
                    // there is no conflict, because the local change still applies.
                    if (!Equals(value, _cachedSourceOfTruth))
                    {
                        // if the new value and the local change are the same, we can solve the conflict here
                        if (Equals(value, _localChange))
                        {
                            Reset(value);
                        }
                        // otherwise we have a conflict!
                        // the cached source of truth, local change and the new value are all different
                        else
                        {
                            _externalChange = value;
                            _state = State.Conflicted;
                        }
                    }

                    break;
            }
        }
        public void TakeSource()
        {
            if (IsConflicted())
            {
                // when solving a conflict, we apply the external change as a local change, because we still remain dirty
                _state = State.Modified;
                _localChange = _externalChange;
                _externalChange = default(TType);
            }
        }

        public void TakeLocal()
        {
            if (IsConflicted())
            {
                _state = State.Modified;
                _externalChange = default(TType);
            }
        }

        public bool IsDirty()
        {
            return _state == State.Modified || IsConflicted();
        }

        public bool IsConflicted()
        {
            return _state == State.Conflicted;
        }

        public bool Equals(CachedField<TType> other)
        {
            return other != null && Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case TType value: return Equals(Value, value);
                case CachedField<TType> backingField: return Equals(backingField);
                default: return false;
            }
        }

        public override string ToString()
        {
            return GetValueAsObject().ToString();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator TType(CachedField<TType> field)
        {
            return field.Value;
        }

        /// <summary>
        ///     Do not use this implicit type conversion to set/update the value of a backing field,
        ///     because a conversion always creates a new instance.
        /// </summary>
        /// <param name="value">The value to initialize a new <see cref="CachedField{TType}" /> instance with</param>
        public static implicit operator CachedField<TType>(TType value)
        {
            return new CachedField<TType>(value);
        }
    }
}

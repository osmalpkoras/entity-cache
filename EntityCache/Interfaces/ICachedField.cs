namespace EntityCache.Interfaces
{
    /// <summary>
    ///     A cached field allows to undo any changes that have been made to it.
    ///     It supports a source of truth and handles conflicts, which may occur,
    ///     when the source of truth and the local value have changed simultaneously.
    /// </summary>
    public interface ICachedField
    {
        /// <summary>
        ///     Resets this field to the value it has been initialized with.
        /// </summary>
        void Reset();
        /// <summary>
        ///     Resets this field to the given value.
        /// </summary>
        void Reset(object value);
        /// <summary>
        ///     Applies the local change. This basically calls Reset(localChange), where localChange is the current local change.
        /// </summary>
        void Apply();
        /// <summary>
        ///     Returns the current value of this field.
        /// </summary>
        object GetValueAsObject();
        /// <summary>
        ///     Sets this field to the given value. Nothing is done, when there is a conflict.
        ///     A conflict needs to be resolved beforehand.
        /// </summary>
        void SetValueFromObject(object value);
        /// <summary>
        ///     Pulls this field from the source of truth. If the source of truth and this field
        ///     have been changed, a conflict is raised, that must be solved. Otherwise the field
        ///     is reset to the new value of the source of truth.
        /// </summary>
        /// <param name="value">The current value of the source of truth.</param>
        void PullFromObject(object value);
        /// <summary>
        ///     Solves an existing conflict by throwing away the local change and taking the new
        ///     value as the new local change.
        /// </summary>
        void TakeSource();
        /// <summary>
        ///     Solves an existing conflict by throwing away the new value of the source of truth
        ///     and keeping the local change.
        /// </summary>
        void TakeLocal();
        /// <summary>
        ///     Returns true, when the field has been changed after initialization.
        ///     This will always return true when <see cref="IsConflicted"/> returns true.
        /// </summary>
        bool IsDirty();
        /// <summary>
        ///     Returns true, when the field has an unresolved conflict.
        ///     A conflict occurs, when the source of truth and the field have
        ///     both been changed to different values at the same time.
        /// </summary>
        bool IsConflicted();
    }
}
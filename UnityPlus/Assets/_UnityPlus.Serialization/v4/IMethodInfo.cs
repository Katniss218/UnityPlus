namespace UnityPlus.Serialization
{
    /// <summary>
    /// Describes a callable method on a type.
    /// </summary>
    public interface IMethodInfo
    {
        string Name { get; }
        string DisplayName { get; }
        bool IsStatic { get; }
        bool IsGeneric { get; }
        string[] GenericTypeParameters { get; }
        IParameterInfo[] Parameters { get; }

        object Invoke( object target, object[] parameters );
    }
}
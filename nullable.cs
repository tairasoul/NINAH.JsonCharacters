// this file is required because il2cpp generates an empty class for this and that breaks compilation entirely

namespace System.Runtime.CompilerServices
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.GenericParameter | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited=false)]
  [ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never)]
  public sealed partial class NullableAttribute : Attribute
  {
    public readonly byte[] NullableFlags;
    public NullableAttribute(byte value) { }
    public NullableAttribute(byte[] value) { }
  }
}
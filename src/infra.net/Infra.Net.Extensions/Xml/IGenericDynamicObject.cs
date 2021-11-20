namespace Infra.Net.Extensions.Xml;

public interface IGenericDynamicObject
{
    object GetDynamicMemberValue(string key);
    object SetDynamicMember(string key, object value);
    object GetIndexValue(GetIndexBinder binder, object[] indexes);
}
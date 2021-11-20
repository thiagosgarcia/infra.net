
namespace Infra.Net.Extensions.Xml;

public class GenericDynamicMetaObject<TDynamicType> : DynamicMetaObject
    where TDynamicType : DynamicObject, IGenericDynamicObject
{
    public GenericDynamicMetaObject(Expression expression, TDynamicType value)
        : base(expression, BindingRestrictions.Empty, value)
    { }

    public TDynamicType SourceObject => base.Value as TDynamicType;

    public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
    {
        var methodName = this.MemberName<GetIndexBinder, object[], object>(() => SourceObject.GetIndexValue);

        var parameters = new Expression[] { Expression.Constant(binder), Expression.Constant(indexes) };
        var tryGetIndex = new DynamicMetaObject(Expression.Call(Expression.Convert(Expression, LimitType),
            typeof(TDynamicType).GetMethod(methodName)!, parameters), BindingRestrictions.GetTypeRestriction(Expression, LimitType));

        return tryGetIndex;
    }

    public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
    {
        var methodName = this.MemberName<string, object>(() => SourceObject.GetDynamicMemberValue);

        var parameters = new Expression[] { Expression.Constant(binder.Name) };
        var getDictionaryEntry = new DynamicMetaObject(Expression.Call(Expression.Convert(Expression, LimitType),
            typeof(TDynamicType).GetMethod(methodName)!, parameters), BindingRestrictions.GetTypeRestriction(Expression, LimitType));

        return getDictionaryEntry;
    }

    public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
    {
        var methodName = this.MemberName<string, object, object>(() => SourceObject.SetDynamicMember);

        var restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);

        var args = new Expression[2];

        args[0] = Expression.Constant(binder.Name);

        args[1] = Expression.Convert(value.Expression, typeof(object));

        Expression self = Expression.Convert(Expression, LimitType);

        Expression methodCall = Expression.Call(self, typeof(TDynamicType).GetMethod(methodName)!, args);

        var setDictionaryEntry = new DynamicMetaObject(methodCall, restrictions);

        return setDictionaryEntry;
    }
}

public class DynamicGetMemberBinder : GetMemberBinder
{
    public DynamicGetMemberBinder(string name)
        : base(name, true)
    {
    }

    public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
    {
        return base.FallbackGetMember(target);
    }
}
using System.Dynamic;
using System.Linq.Expressions;

namespace Infra.Net.Extensions.Xml
{
    public class GenericDynamicMetaObject<dynamicType> : DynamicMetaObject
        where dynamicType : DynamicObject, IGenericDynamicObject
    {
        public GenericDynamicMetaObject(Expression expression, dynamicType value)
            : base(expression, BindingRestrictions.Empty, value)
        { }

        public dynamicType SourceObject { get { return base.Value as dynamicType; } }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            string methodName = this.MemberName<GetIndexBinder, object[], object>(() => SourceObject.GetIndexValue);

            Expression[] parameters = new Expression[] { Expression.Constant(binder), Expression.Constant(indexes) };
            DynamicMetaObject tryGetIndex = new DynamicMetaObject(Expression.Call(Expression.Convert(Expression, LimitType),
                typeof(dynamicType).GetMethod(methodName), parameters), BindingRestrictions.GetTypeRestriction(Expression, LimitType));

            return tryGetIndex;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            string methodName = this.MemberName<string, object>(() => SourceObject.GetDynamicMemberValue);

            Expression[] parameters = new Expression[] { Expression.Constant(binder.Name) };
            DynamicMetaObject getDictionaryEntry = new DynamicMetaObject(Expression.Call(Expression.Convert(Expression, LimitType),
                typeof(dynamicType).GetMethod(methodName), parameters), BindingRestrictions.GetTypeRestriction(Expression, LimitType));

            return getDictionaryEntry;
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            string methodName = this.MemberName<string, object, object>(() => SourceObject.SetDynamicMember);

            BindingRestrictions restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);

            Expression[] args = new Expression[2];

            args[0] = Expression.Constant(binder.Name);

            args[1] = Expression.Convert(value.Expression, typeof(object));

            Expression self = Expression.Convert(Expression, LimitType);

            Expression methodCall = Expression.Call(self, typeof(dynamicType).GetMethod(methodName), args);

            DynamicMetaObject setDictionaryEntry = new DynamicMetaObject(methodCall, restrictions);

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
}

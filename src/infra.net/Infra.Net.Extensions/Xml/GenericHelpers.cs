namespace Infra.Net.Extensions.Xml;

public static class GenericHelpers
{
    public static IDictionary<string, object> ToDynamicExpando(this object anonymousObject, bool includeAtSign = false, bool stringInQuotes = false)
    {
        var anonymousDictionary = new Dictionary<string, object>();
        if (anonymousObject != null)
        {
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(anonymousObject))
            {
                var propertyValue = prop.GetValue(anonymousObject);
                if (prop.PropertyType == typeof(string) && stringInQuotes)
                    propertyValue = $"'{propertyValue}'";
                anonymousDictionary.Add(
                    $"{(includeAtSign ? "@" : string.Empty)}{prop.Name}",
                    propertyValue);
            }
        }

        IDictionary<string, object> resultExpando = new ExpandoObject();
        foreach (var item in anonymousDictionary)
            resultExpando.Add(item);

        return resultExpando;
    }

    public static resultType Cast_AnyType<resultType>(this object sourceValue, params string[] skippingPropertyNames)
        where resultType : new()
    {
        var result = new resultType();
        if (sourceValue == null)
            return result;

        var propertiesFromResult = TypeDescriptor.GetProperties(result).OfType<PropertyDescriptor>().ToList();

        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(sourceValue))
        {
            if (skippingPropertyNames.Contains(property.Name))
                continue;

            if (propertiesFromResult.Any(
                    p => p.Name == property.Name
                         && p.PropertyType == property.PropertyType
                         && (!p.IsReadOnly)))
                continue;

            if (propertiesFromResult.All(p => p.Name != property.Name))
                continue;

            propertiesFromResult.First(p => p.Name == property.Name)
                .SetValue(result, property.GetValue(sourceValue));
        }

        return result;
    }

    public static resultType ConvertTo<resultType>(this IDictionary<string, object> sourceDynObject, params string[] skippingPropertyNames)
        where resultType : new()
    {
        if (sourceDynObject == null)
            return default(resultType);

        var resultObject = new resultType();
        var propertiesFromResult = TypeDescriptor.GetProperties(resultObject)
            .OfType<PropertyDescriptor>()
            .Where(p => (!p.IsReadOnly) && (!skippingPropertyNames.Contains(p.Name)));

        foreach (var sourceValue in sourceDynObject
                     .Where(s => (!skippingPropertyNames.Contains(s.Key))))
        {
            var targetProperty = propertiesFromResult
                .FirstOrDefault(p => string.Equals(sourceValue.Key, p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (targetProperty == null)
                continue;

            resultObject.GenericAssign(targetProperty, sourceValue.Value);
        }

        return resultObject;
    }

    public static resultType ConvertTo<resultType>(this DynamicXml sourceDynamicXml, params string[] skippingPropertyNames)
    {
        if (sourceDynamicXml == null)
            return default(resultType);

        var resultObject = Activator.CreateInstance<resultType>();

        var propertiesFromResult = TypeDescriptor.GetProperties(resultObject)
            .OfType<PropertyDescriptor>()
            .Where(p => (!p.IsReadOnly) && (!skippingPropertyNames.Contains(p.Name)));

        foreach (var targetProperty in propertiesFromResult)
        {
            var memberValue = sourceDynamicXml.GetDynamicMemberValue(targetProperty.Name);
            if (memberValue == null)
                continue;

            if (memberValue.GetType().Equals(targetProperty.PropertyType))
                targetProperty.SetValue(resultObject, memberValue);
            else
                resultObject.GenericAssign(targetProperty, memberValue);
        }

        return resultObject;
    }

    public static void GenericAssign(this object target, string propertyName, object value)
    {
        var targetProperty = TypeDescriptor.GetProperties(target).Find(propertyName, true);
        target.GenericAssign(targetProperty, value);
    }

    private static void GenericAssign(this object target, PropertyDescriptor targetProperty, object value)
    {
        if ((targetProperty == null) || (targetProperty.IsReadOnly)) return;
        System.Reflection.MethodInfo parseMethod = null;
        try
        {
            parseMethod = targetProperty.PropertyType.GetMethod("Parse", new Type[] { typeof(string) });
        }
        catch
        {
        }

        object convertedValue = null;
        try
        {
            if (value != null)
                convertedValue = ConvertValue(targetProperty, value, parseMethod);
            else if (targetProperty.PropertyType.IsValueType)
                convertedValue = Activator.CreateInstance(targetProperty.PropertyType);
        }
        catch
        {
        }
        if (convertedValue != null)
        {
            targetProperty.SetValue(target, convertedValue);
            return;
        }

        try
        {
            targetProperty.SetValue(target, value);
        }
        catch
        {
        }
    }

    private static object ConvertValue(PropertyDescriptor targetProperty, object value, MethodInfo parseMethod)
    {
        object convertedValue;
        if (parseMethod != null)
        {
            var stringToConvert = parseMethod.DeclaringType == typeof(DateTime)
                ? value.ToString().Replace("UTC", string.Empty)
                : value.ToString();
            convertedValue = parseMethod.Invoke(null, new object[] { stringToConvert });
        }
        else
            convertedValue = DoValueConversion(targetProperty, value);

        return convertedValue;
    }

    private static object DoValueConversion(PropertyDescriptor targetProperty, object value)
    {
        if (targetProperty.PropertyType == typeof(string))
            return value.ToString();
        if (targetProperty.PropertyType == typeof(object))
            return value;

        return null;
    }

    public static resultType GenericGet<resultType>(this object target, string propertyName)
    {
        var resultValue = default(resultType);
        if (target == null)
            return resultValue;

        var propertyToGet = TypeDescriptor.GetProperties(target).Find(propertyName, true);

        if (propertyToGet != null)
        {
            var gotValue = propertyToGet.GetValue(target);
            System.Reflection.MethodInfo parseMethod = null;
            try
            {
                parseMethod = typeof(resultType).GetMethod("Parse", new Type[] { typeof(string) });
            }
            catch
            {
            }
            object convertedValue = null;
            try
            {
                if (parseMethod != null)
                    convertedValue = parseMethod.Invoke(null, new object[] { gotValue.ToString() });
                else if (typeof(resultType) == typeof(string))
                    convertedValue = gotValue.ToString();
                else
                    convertedValue = gotValue is resultType ? (resultType)gotValue : default(resultType);
            }
            catch
            {
            }
            if (convertedValue != null)
                resultValue = (resultType)convertedValue;
        }
        return resultValue;
    }

    public static returnType IsNull<returnType>(this returnType anyObject, returnType alternateResult)
    {
        if (CheckNull(anyObject))
            return anyObject;
        return alternateResult;
    }

    private static bool CheckNull<returnType>(returnType anyObject)
    {
        return (anyObject != null && anyObject is returnType) ||
               (anyObject is string && !string.IsNullOrEmpty(anyObject.ToString()));
    }

    public static DynamicXml IsNotNullGetElementByName(this DynamicXml xmlObject, string elementName)
    {
        if (xmlObject != null)
            return xmlObject.GetElementByName(elementName);
        return null;
    }

    public static IEnumerable<DynamicXml> IsNotNullGetXmlElementsCollection(this DynamicXml xmlObject)
    {
        if (xmlObject != null)
            return xmlObject.XmlElementsCollection;
        return Enumerable.Empty<DynamicXml>();
    }

    public static string MemberName(this object objectValue, Expression<Func<Action>> voidParameterLessMethod)
    {
        return MemberName(voidParameterLessMethod);
    }

    public static string MemberName<TMember>(this object objectValue, Expression<Func<Func<TMember>>> expression)
    {
        return MemberName(expression);
    }

    public static string MemberName<Tin, TMember>(this object objectValue, Expression<Func<Func<Tin, TMember>>> expression)
    {
        return MemberName(expression);
    }

    public static string MemberName<Tin, Tin2, TMember>(this object objectValue, Expression<Func<Func<Tin, Tin2, TMember>>> expression)
    {
        return MemberName(expression);
    }

    public static string MemberName<TMember>(this object objectValue, Expression<Func<TMember>> expression)
    {
        return MemberName(expression);
    }

    public static string MemberName<TMember>(Expression<Func<TMember>> expression)
    {
        switch (expression.Body)
        {
            case MethodCallExpression callExpression:
                return callExpression.Method.Name;
            case MemberExpression memberExpression:
                return memberExpression.Member.Name;
            case UnaryExpression unaryExpression:
                return unaryExpression.Operand
                    .GenericGet<object>("Object")
                    .GenericGet<object>("Value")
                    .GenericGet<string>("Name");
            default:
                return null;
        }
    }
}
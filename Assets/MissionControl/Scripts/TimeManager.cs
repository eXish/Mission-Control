﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/** TimeRemaining Class
  * Used in Mystery Module
  * Credits: Timwi
  */
public static class TimeRemaining {
    public static void FromModule(KMBombModule module, float time) {
        module.GetComponent("BombComponent").GetValue<object>("Bomb").CallMethod<object>("GetTimer").SetValue("TimeRemaining", time);
    }

    public static void FromModule(KMNeedyModule module, float time) {
        module.GetComponent("BombComponent").GetValue<object>("Bomb").CallMethod<object>("GetTimer").SetValue("TimeRemaining", time);
    }
}

/** TimerRate Class
  * Used in Training Text
  * Credits: Samfundev
  */
public static class TimerRate {
    public static float FromModule(KMBombModule module) {
        return module.GetComponent("BombComponent").GetValue<object>("Bomb").CallMethod<object>("GetTimer").GetValue<float>("GetRate");
    }

    public static float FromModule(KMNeedyModule module) {
        return module.GetComponent("BombComponent").GetValue<object>("Bomb").CallMethod<object>("GetTimer").GetValue<float>("GetRate");
    }

    public static void SetFromModule(KMBombModule module, float v)
    {
        module.GetComponent("BombComponent").GetValue<object>("Bomb").CallMethod<object>("GetTimer").CallMethod("SetRateModifier", v);
    }
}

public static class ReflectionHelper {
    public const string GameAssembly = "Assembly-CSharp";

    public static Type FindType(string fullName) {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetSafeTypes).FirstOrDefault(t => t.FullName == null ? false : t.FullName.Equals(fullName));
    }

    public static Type FindType(string fullName, string assemblyName) {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetSafeTypes).FirstOrDefault(t => t.FullName == null ? false : t.FullName.Equals(fullName) && t.Assembly.GetName().Name.Equals(assemblyName));
    }

    public static Type FindGameType(string fullName)
    {
        return FindType(fullName, GameAssembly);
    }

    public static IEnumerable<Type> GetSafeTypes(this Assembly assembly) {
        try {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e) {
            return e.Types.Where(x => x != null);
        }
        catch (Exception) {
            return new List<Type>();
        }
    }

    static readonly Dictionary<string, MemberInfo> MemberCache = new Dictionary<string, MemberInfo>();
    public static T GetCachedMember<T>(this Type type, string member) where T : MemberInfo {
        // Use AssemblyQualifiedName and the member name as a unique key to prevent a collision if two types have the same member name
        var key = type.AssemblyQualifiedName + member;
        if (MemberCache.ContainsKey(key)) return MemberCache[key] is T ? (T)MemberCache[key] : null;

        MemberInfo memberInfo = type.GetMember(member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).FirstOrDefault();
        MemberCache[key] = memberInfo;

        return memberInfo is T ? (T)memberInfo : null;
    }

    public static T GetValue<T>(this Type type, string member, object target = null) {
        var fieldMember = type.GetCachedMember<FieldInfo>(member);
        var propertyMember = type.GetCachedMember<PropertyInfo>(member);

        return (T)((fieldMember != null ? fieldMember.GetValue(target) : default(T)) ?? (propertyMember != null ? propertyMember.GetValue(target, null) : default(T)));
    }

    public static void SetValue(this Type type, string member, object value, object target = null) {
        var fieldMember = type.GetCachedMember<FieldInfo>(member);
        if (fieldMember != null)
            fieldMember.SetValue(target, value);

        var propertyMember = type.GetCachedMember<PropertyInfo>(member);
        if (propertyMember != null)
            propertyMember.SetValue(target, value, null);
    }

    public static T CallMethod<T>(this Type type, string method, object target = null, params object[] arguments) {
        var member = type.GetCachedMember<MethodInfo>(method);
        if (member != null)
            return (T)(member.Invoke(target, arguments));

        return default(T);
    }

    public static void CallMethod(this Type type, string method, object target = null, params object[] arguments) {
        var member = type.GetCachedMember<MethodInfo>(method);
        if (member != null)
            member.Invoke(target, arguments);
    }

    public static T GetValue<T>(this object @object, string member) {
        return @object.GetType().GetValue<T>(member, @object);
    }

    public static void SetValue(this object @object, string member, object value) {
        @object.GetType().SetValue(member, value, @object);
    }

    public static T CallMethod<T>(this object @object, string member, params object[] arguments) {
        return @object.GetType().CallMethod<T>(member, @object, arguments);
    }

    public static void CallMethod(this object @object, string member, params object[] arguments) {
        @object.GetType().CallMethod(member, @object, arguments);
    }
}
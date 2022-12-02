using UnityEngine;
using XLua;

namespace AIDesigner
{
    public class Variable
    {
        public const float Unit = 1000f;

        public string Key { get; set; }
        public string Desc { get; set; }
        public VarType Type { get; protected set; }
        public object Value { get; protected set; }
        public Options Options { get; private set; }

        public Variable(string key, VarType type, string desc, Options options)
        {
            Key = key;
            Type = type;
            Desc = desc;
            Options = options;
            SetValue(null);
        }

        public Variable(string key, VarType type, object value, string desc, Options options)
        {
            Key = key;
            Type = type;
            Desc = desc;
            Options = options;
            SetValue(value);
        }

        public void SetType(VarType type)
        {
            if (type == Type)
            {
                return;
            }

            Type = type;
            Value = GetDefault(type);
        }

        public void SetValue(object value)
        {
            if (null == value)
            {
                Value = GetDefault(Type);
            }
            else
            {
                var type = value.GetType();
                if (Type == VarType.Float && type == typeof(long))
                {
                    Value = (float) (long) value;
                }
                else if (Type == VarType.Float && type == typeof(double))
                {
                    Value = (float) (double) value;
                }
                else if (Type == VarType.Int && type == typeof(long))
                {
                    Value = (int) (long) value;
                }
                else if (Type == VarType.Int && type == typeof(double))
                {
                    Value = (int) (double) value;
                }
                else if ((Type == VarType.Object)
                         || (Type == VarType.Float && type == typeof(float))
                         || (Type == VarType.Int && type == typeof(int))
                         || (Type == VarType.String && type == typeof(string))
                         || (Type == VarType.Boolean && type == typeof(bool))
                         || (Type == VarType.Vector2 && type == typeof(Vector2))
                         || (Type == VarType.Vector2Int && type == typeof(Vector2Int))
                         || (Type == VarType.Vector3 && type == typeof(Vector3))
                         || (Type == VarType.Vector3Int && type == typeof(Vector3Int))
                         || (Type == VarType.Vector4 && type == typeof(Vector4)))
                {
                    Value = value;
                }
                else
                {
                    Value = GetDefault(Type);
                }
            }

            Options?.SetValue(value);
        }

        public void VarFromLua(object luaVar)
        {
            if (null == luaVar)
            {
                return;
            }

            Value = ParseToCS(luaVar, Type);
        }

        public string VarToLua()
        {
            return ParseToStr(Value, Type);
        }

        public Variable DeepCopy()
        {
            return new Variable(Key, Type, Value, Desc, Options?.DeepCopy());
        }

        public static object GetDefault(VarType type)
        {
            switch (type)
            {
                case VarType.Boolean: return false;
                case VarType.Float: return 0f;
                case VarType.Int: return 0;
                case VarType.String: return string.Empty;
                case VarType.Vector2: return Vector2.zero;
                case VarType.Vector2Int: return Vector2Int.zero;
                case VarType.Vector3: return Vector3.zero;
                case VarType.Vector3Int: return Vector3Int.zero;
                case VarType.Vector4: return Vector4.zero;
                case VarType.Object: return null;
                default: break;
            }

            return null;
        }

        public static object ParseToCS(object refObj, VarType type)
        {
            if (null != refObj)
            {
                if (refObj.GetType() == typeof(long))
                {
                    if (type == VarType.Float)
                    {
                        refObj = (float) (long) refObj / Unit;
                    }
                    else if (type == VarType.Int)
                    {
                        refObj = (int) (long) refObj;
                    }
                    else
                    {
                        refObj = null;
                    }
                }
                else if (refObj.GetType() == typeof(double))
                {
                    if (type == VarType.Float)
                    {
                        refObj = (float) (double) refObj / Unit;
                    }
                    else if (type == VarType.Int)
                    {
                        refObj = (int) (double) refObj;
                    }
                    else
                    {
                        refObj = null;
                    }
                }
                else if (refObj.GetType() == typeof(LuaTable))
                {
                    LuaTable tab = refObj as LuaTable;
                    if (type == VarType.Vector2)
                    {
                        refObj = new Vector2(GetLuaValue<float>(tab, "x") / Unit, GetLuaValue<float>(tab, "y") / Unit);
                    }
                    else if (type == VarType.Vector2Int)
                    {
                        refObj = new Vector2Int(GetLuaValue<int>(tab, "x"), GetLuaValue<int>(tab, "y"));
                    }
                    else if (type == VarType.Vector3)
                    {
                        refObj = new Vector3(GetLuaValue<float>(tab, "x") / Unit, GetLuaValue<float>(tab, "y") / Unit, GetLuaValue<float>(tab, "z") / Unit);
                    }
                    else if (type == VarType.Vector3Int)
                    {
                        refObj = new Vector3Int(GetLuaValue<int>(tab, "x"), GetLuaValue<int>(tab, "y"), GetLuaValue<int>(tab, "z"));
                    }
                    else if (type == VarType.Vector4)
                    {
                        refObj = new Vector4(GetLuaValue<float>(tab, "x") / Unit, GetLuaValue<float>(tab, "y") / Unit, GetLuaValue<float>(tab, "z") / Unit, GetLuaValue<float>(tab, "w") / Unit);
                    }
                    else
                    {
                        refObj = null;
                    }
                }
            }

            if (null == refObj)
            {
                refObj = GetDefault(type);
            }

            return refObj;
        }

        public static string ParseToStr(object refObj, VarType type)
        {
            if (null == refObj || type == VarType.Object)
            {
                return "nil";
            }

            if (type == VarType.String)
            {
                return $"'{(string) refObj}'";
            }

            if (type == VarType.Int)
            {
                return $"{(int) refObj}";
            }

            if (type == VarType.Float)
            {
                return $"{Mathf.RoundToInt((float) refObj * Unit)}";
            }

            if (type == VarType.Boolean)
            {
                return $"{((bool) refObj ? "true" : "false")}";
            }

            if (type == VarType.Vector2)
            {
                var v2 = (Vector2) refObj;
                return $"{{ x={Mathf.RoundToInt(v2.x * Unit)},y={Mathf.RoundToInt(v2.y * Unit)} }}";
            }

            if (type == VarType.Vector2Int)
            {
                var v2 = (Vector2Int) refObj;
                return $"{{ x={v2.x},y={v2.y} }}";
            }

            if (type == VarType.Vector3)
            {
                var v3 = (Vector3) refObj;
                return $"{{ x={Mathf.RoundToInt(v3.x * Unit)},y={Mathf.RoundToInt(v3.y * Unit)},z={Mathf.RoundToInt(v3.z * Unit)} }}";
            }

            if (type == VarType.Vector3Int)
            {
                var v3 = (Vector3Int) refObj;
                return $"{{ x={v3.x},y={v3.y},z={v3.z} }}";
            }

            if (type == VarType.Vector4)
            {
                var v4 = (Vector4) refObj;
                return $"{{ x={Mathf.RoundToInt(v4.x * Unit)},y={Mathf.RoundToInt(v4.y * Unit)},z={Mathf.RoundToInt(v4.z * Unit)},w={Mathf.RoundToInt(v4.w * Unit)} }}";
            }

            return "nil";
        }

        public static T GetLuaValue<T>(LuaTable tab, string key)
        {
            if (null == tab || string.IsNullOrEmpty(key))
            {
                return default(T);
            }

            return tab.ContainsKey(key) ? tab.GetInPath<T>(key) : default(T);
        }
    }
}
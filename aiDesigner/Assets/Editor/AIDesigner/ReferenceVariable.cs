using System.Collections.Generic;
using System.Text;
using XLua;

namespace AIDesigner
{
    public class SharedVariable : Variable
    {
        public bool IsAnyType { get; }
        public bool IsShared { get; }
        public bool SharedFlag { get; set; }
        public string SharedKey { get; set; }

        public SharedVariable(string key, VarType type, string desc, bool isShared, bool isAnyType, Options options) : base(key, type, desc, options)
        {
            IsShared = isShared;
            IsAnyType = isAnyType;
        }

        public SharedVariable(string key, VarType type, object value, string desc, bool isShared, string sharedKey, bool isAnyType, Options options) : base(key, type, value, desc, options)
        {
            IsShared = isShared;
            IsAnyType = isAnyType;
            SetSharedKey(sharedKey);
        }

        public bool IsSharedKey(string name)
        {
            return SharedFlag && IsShared && !string.IsNullOrEmpty(SharedKey) && SharedKey == name;
        }

        public void SetSharedKey(string name)
        {
            if (!IsShared)
            {
                return;
            }

            SharedKey = name;
            SharedFlag = !string.IsNullOrEmpty(name);
        }

        public void ChangeSharedKey(string fromName, string toName)
        {
            if (IsShared && SharedKey == fromName)
            {
                SharedKey = toName;
            }
        }

        public void ChangeSharedType(string name, VarType type)
        {
            if (IsShared && SharedKey == name && Type != type)
            {
                SharedKey = null;
            }
        }

        public new SharedVariable DeepCopy()
        {
            return new SharedVariable(Key, Type, Value, Desc, IsShared, SharedKey, IsAnyType, Options?.DeepCopy());
        }
    }

    public class ReferenceVariable : SharedVariable
    {
        public float PosY { get; set; }
        public bool IsSelected { get; set; }
        public bool IsArrayExpanded { get; set; }
        public bool IsArray => ArrayType != ArrayType.None;
        public ArrayType ArrayType { get; }
        public List<SharedVariable> ArrayVar { get; }

        public ReferenceVariable(string key, VarType type, string desc, ArrayType arrayType, bool isShared, bool isAnyType, Options options) : base(key, type, desc, isShared, isAnyType, options)
        {
            IsArrayExpanded = true;
            ArrayType = arrayType;
            ArrayVar = new List<SharedVariable>();
        }

        public ReferenceVariable(string key, VarType type, object value, string desc, ArrayType arrayType, bool isShared, string sharedKey, bool isAnyType, Options options) : base(key, type, value, desc, isShared, sharedKey, isAnyType, options)
        {
            IsArrayExpanded = true;
            ArrayType = arrayType;
            ArrayVar = new List<SharedVariable>();
        }

        public bool ContainSharedKey(string name)
        {
            if (!IsShared)
            {
                return false;
            }

            var result = IsSharedKey(name);
            if (!IsArray)
            {
                return result;
            }

            if (!result)
            {
                foreach (var v in ArrayVar)
                {
                    if (v.IsSharedKey(name))
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        public new void ChangeSharedKey(string fromName, string toName)
        {
            if (!IsShared)
            {
                return;
            }

            base.ChangeSharedKey(fromName, toName);
            foreach (var v in ArrayVar)
            {
                v.ChangeSharedKey(fromName, toName);
            }
        }

        public new void ChangeSharedType(string name, VarType type)
        {
            if (!IsShared)
            {
                return;
            }

            base.ChangeSharedType(name, type);
            foreach (var v in ArrayVar)
            {
                v.ChangeSharedType(name, type);
            }
        }

        public new void SetType(VarType type)
        {
            if (type == Type)
            {
                return;
            }

            base.SetType(type);
            foreach (var var in ArrayVar)
            {
                var.SetType(type);
            }
        }

        public void SetArraySize(int size)
        {
            if (ArrayVar.Count == size)
            {
                return;
            }

            for (var i = ArrayVar.Count - 1; i >= size; i--)
            {
                ArrayVar.RemoveAt(i);
            }

            for (var i = ArrayVar.Count; i < size; i++)
            {
                ArrayVar.Add(new SharedVariable(Key, Type, Desc, IsShared, IsAnyType, Options?.DeepCopy()));
            }
        }

        public new ReferenceVariable DeepCopy()
        {
            var var = new ReferenceVariable(Key, Type, Value, Desc, ArrayType, IsShared, SharedKey, IsAnyType, Options?.DeepCopy());
            foreach (var v in ArrayVar)
            {
                var.ArrayVar.Add(v.DeepCopy());
            }

            return var;
        }

        public void VarFromLua(object luaVar)
        {
            //{key = "key", type = 7, isShared = true, arrayType = 0, sharedKey = "sharedKey", value = {value = {x = 0, y = 0, z = 0}, sharedKey= "key"}}
            if (!(luaVar is LuaTable luaTab))
            {
                return;
            }

            if (IsAnyType && luaTab.ContainsKey("type"))
            {
                SetType(GetLuaValue<VarType>(luaTab, "type"));
            }

            SetSharedKey(GetLuaValue<string>(luaTab, "sharedKey"));
            if (!IsArray)
            {
                SetValue(ParseToCS(GetLuaValue<object>(luaTab, "value"), Type));
            }
            else
            {
                ArrayVar.Clear();
                var luaValue = GetLuaValue<LuaTable>(luaTab, "value");
                if (null == luaValue) return;
                
                for (var i = 0; i < luaValue.Length; i++)
                {
                    var luaSubValue = luaValue.Get<int, object>(i + 1);
                    if (IsShared)
                    {
                        var luaSubTab = luaSubValue as LuaTable;
                        var var = new SharedVariable(Key, Type, ParseToCS(GetLuaValue<object>(luaSubTab, "value"), Type), Desc, true, GetLuaValue<string>(luaSubTab, "sharedKey"), false, Options?.DeepCopy());
                        ArrayVar.Add(var);
                    }
                    else
                    {
                        var var = new SharedVariable(Key, Type, ParseToCS(luaSubValue, Type), Desc, false, null, false, Options?.DeepCopy());
                        ArrayVar.Add(var);
                    }
                }
            }
        }

        public string VarToLua()
        {
            //{key = "key", type = 7, isShared = true, arrayType = 0, sharedKey = "sharedKey", value = {value = {x = 0, y = 0, z = 0}, sharedKey= "key"}}
            var sb = new StringBuilder();

            sb.Append("{");
            sb.Append($"key='{Key}',");
            sb.Append($"type={(int) Type},");
            if (IsShared) sb.Append("isShared=true,");
            if (IsArray) sb.Append($"arrayType={(int) ArrayType},");
            if (!string.IsNullOrEmpty(SharedKey)) sb.Append($"sharedKey='{SharedKey}',");

            sb.Append("value=");
            if (!IsArray)
            {
                sb.Append(ParseToStr(Value, Type));
            }
            else
            {
                sb.Append("{");
                if (IsShared)
                {
                    foreach (var v in ArrayVar)
                    {
                        sb.Append("{");
                        sb.Append($"value={ParseToStr(v.Value, Type)},");
                        if (!string.IsNullOrEmpty(v.SharedKey)) sb.Append($"sharedKey='{v.SharedKey}',");
                        sb.Append("},");
                    }
                }
                else
                {
                    foreach (var v in ArrayVar)
                    {
                        sb.Append($"{ParseToStr(v.Value, Type)},");
                    }
                }

                sb.Append("}");
            }

            sb.Append("}");
            return sb.ToString();
        }
    }
}

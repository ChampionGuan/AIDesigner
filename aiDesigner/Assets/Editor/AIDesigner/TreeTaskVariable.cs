namespace AIDesigner
{
    public class TreeTaskVariable : ReferenceVariable
    {
        public TreeTaskVariable(string key, VarType type, string desc, ArrayType arrayType, bool isShared, bool isAnyType, Options options) : base(key, type, desc, arrayType, isShared, isAnyType, options)
        {
        }

        public TreeTaskVariable(string key, VarType type, object value, string desc, ArrayType arrayType, bool isShared, string sharedKey, bool isAnyType, Options options) : base(key, type, value, desc, arrayType, isShared, sharedKey, isAnyType, options)
        {
        }

        public new void SetType(VarType type)
        {
        }

        public new TreeTaskVariable DeepCopy()
        {
            var var = new TreeTaskVariable(Key, Type, Value, Desc, ArrayType, IsShared, SharedKey, IsAnyType, Options?.DeepCopy());
            foreach (var v in ArrayVar)
            {
                var.ArrayVar.Add(v.DeepCopy());
            }

            return var;
        }
    }
}
namespace AIDesigner
{
    public class TreeRefVariable : ReferenceVariable
    {
        public TreeRefVariable(string key, VarType type, string desc, bool isArray) : base(key, type, desc, isArray ? ArrayType.Regular : ArrayType.None, false, false, null)
        {
            IsArrayExpanded = false;
        }

        public TreeRefVariable(string key, VarType type, object value, string desc, bool isArray) : base(key, type, value, desc, isArray ? ArrayType.Regular : ArrayType.None, false, null, false, null)
        {
            IsArrayExpanded = false;
        }

        public new TreeRefVariable DeepCopy()
        {
            var var = new TreeRefVariable(Key, Type, Value, Desc, IsArray);
            foreach (var v in ArrayVar)
            {
                var.ArrayVar.Add(v.DeepCopy());
            }

            return var;
        }
    }
}
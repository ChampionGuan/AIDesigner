namespace AIDesigner
{
    public class EditorTaskVariable : Variable
    {
        public bool IsShared { get; }
        public bool IsAnyType { get; }
        public ArrayType ArrayType { get; }

        public EditorTaskVariable(string key, VarType type, string desc, ArrayType arrayType, bool isShared, bool isAnyType, Options options) : base(key, type, desc, options)
        {
            IsShared = isShared;
            IsAnyType = isAnyType;
            ArrayType = arrayType;
        }

        public EditorTaskVariable DeepCopy()
        {
            return new EditorTaskVariable(Key, Type, Desc, ArrayType, IsShared, IsAnyType, Options?.DeepCopy());
        }
    }
}
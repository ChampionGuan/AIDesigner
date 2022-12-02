namespace AIDesigner
{
    public class Options
    {
        public string Name { get; }
        public string[] Keys { get; }
        public object[] Values { get; }
        public int SelectedIndex { get; set; }

        public Options(string name, string[] keys, object[] values)
        {
            Name = name;
            Keys = keys;
            Values = values;
            SelectedIndex = 0;
        }

        public Options(string name, string[] keys, object[] values, int selectedIndex)
        {
            Name = name;
            Keys = keys;
            Values = values;
            SelectedIndex = selectedIndex;
        }

        public object GetValue()
        {
            return Values[SelectedIndex];
        }

        public void SetValue(object value)
        {
            if (null == value)
            {
                return;
            }

            for (var i = 0; i < Values.Length; i++)
            {
                if (Values[i].Equals(value))
                {
                    SelectedIndex = i;
                    break;
                }
            }
        }

        public int GetValueSelectedIndex(object value)
        {
            if (null != value)
            {
                for (var i = 0; i < Values.Length; i++)
                {
                    if (Values[i].Equals(value))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public Options DeepCopy()
        {
            return new Options(Name, Keys, Values, SelectedIndex);
        }
    }
}
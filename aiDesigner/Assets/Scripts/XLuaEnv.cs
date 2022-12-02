using XLua;

namespace Test
{
    public class XLuaEnv : LuaEnv
    {
        public static XLuaEnv Instance;

        public XLuaEnv()
        {
            Instance = this;
        }
    }
}
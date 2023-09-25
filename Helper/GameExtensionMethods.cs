namespace AntMe_2_Lib.Helper
{

    namespace ExtensionMethods
    {
        public static class GameExtensionMethods
        {
            public static Godot.Vector3 ToGodot(this System.Numerics.Vector3 v3)
            {
                return new Godot.Vector3(v3.X, v3.Y, v3.Z);
            }
        }
    }
}

namespace dev.susy_baka.xivAnim.EtoGui
{
    public static class Utility
    {
        public static T Also<T>(this T control, Action<T> configure)
        {
            configure(control);
            return control;
        }
    }
}

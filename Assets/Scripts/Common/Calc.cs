namespace ET.Client.Platform
{
    public static class Calc
    {
        public static bool OnInterval(float val, float preVal, float interval)
        {
            return (int)(preVal / interval) != (int)(val / interval);
        }
    }
}
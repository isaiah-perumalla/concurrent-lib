namespace concurrent_collections.conurrent.utils
{
    public static class Extensions
    {
        public static uint NextPowerOfTwo(this uint num)
        {
            var n = num > 0 ? num - 1 : 0;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return ++n;
        }

        public static bool IsPowerOf2(this uint x)
        {
            return (x & (x - 1)) == 0;
        }
    }
}
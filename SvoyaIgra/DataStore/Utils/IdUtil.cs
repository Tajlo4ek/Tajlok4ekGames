namespace DataStore.Utils
{
    public static class IdUtil
    {
        private static long id = 0;

        public static long GetId()
        {
            return id++;
        }

    }
}

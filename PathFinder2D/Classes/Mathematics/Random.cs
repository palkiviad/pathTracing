namespace PathFinder.Mathematics {
    public static class Random {
        
        private static readonly System.Random rnd = new System.Random(); 
        
        // Как у Unity, возвразает в интевале [min, max) т.е. Random.Range(0,2) может вернуть либо 0, либо 1
        public static int Range(int min, int max)
        {
            return RandomInteger(min, max);
        }
        
        public static float Range(float min, float max)
        {
            return RandomFloat(min, max);
        }
        
        // Отдолжено отсюда
        //https://answers.unity.com/questions/585266/question-about-randomrange.html
        private static int RandomInteger(int minInclusive, int maxExclusive) {
            int randomInteger = rnd.Next();
            int range = maxExclusive - minInclusive;
            return minInclusive + randomInteger % range;
        }
        
        private static float RandomFloat(float minInclusive, float maxInclusive) {
            int randomInteger = rnd.Next();
            // Convert to a value between 0f and 1f:
            float randomFloat = (float)randomInteger / int.MaxValue;
            float range = maxInclusive - minInclusive;
            return minInclusive + randomFloat * range;
        }
    }
}
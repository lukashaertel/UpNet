namespace UpNet.Graphics.Lang
{
    /// <summary>
    /// Extends arrays.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Deconstruction operator for <paramref name="array"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="array">The array to deconstruct</param>
        /// <param name="item0">The first item of the array.</param>
        /// <typeparam name="T">The type of the items.</typeparam>
        public static void Deconstruct<T>(this T[] array, out T item0)
        {
            item0 = array[0];
        }

        /// <summary>
        /// Deconstruction operator for <paramref name="array"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="array">The array to deconstruct</param>
        /// <param name="item0">The first item of the array.</param>
        /// <param name="item1">The second item of the array.</param>
        /// <typeparam name="T">The type of the items.</typeparam>
        public static void Deconstruct<T>(this T[] array, out T item0, out T item1)
        {
            item0 = array[0];
            item1 = array[1];
        }

        /// <summary>
        /// Deconstruction operator for <paramref name="array"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="array">The array to deconstruct</param>
        /// <param name="item0">The first item of the array.</param>
        /// <param name="item1">The second item of the array.</param>
        /// <param name="item2">The third item of the array.</param>
        /// <typeparam name="T">The type of the items.</typeparam>
        public static void Deconstruct<T>(this T[] array, out T item0, out T item1, out T item2)
        {
            item0 = array[0];
            item1 = array[1];
            item2 = array[2];
        }

        /// <summary>
        /// Deconstruction operator for <paramref name="array"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="array">The array to deconstruct</param>
        /// <param name="item0">The first item of the array.</param>
        /// <param name="item1">The second item of the array.</param>
        /// <param name="item2">The third item of the array.</param>
        /// <param name="item3">The fourth item of the array.</param>
        /// <typeparam name="T">The type of the items.</typeparam>
        public static void Deconstruct<T>(this T[] array, out T item0, out T item1, out T item2, out T item3)
        {
            item0 = array[0];
            item1 = array[1];
            item2 = array[2];
            item3 = array[3];
        }
    }
}
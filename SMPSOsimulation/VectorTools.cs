using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPSOsimulation
{
    public static class VectorTools
    {
        public static double[] Multiply(double scalar, double[] array)
        {
            double[] result = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = scalar * array[i];
            }
            return result;
        }

        public static double[] AddArrays(params double[][] arrays)
        {
            // Ensure there is at least one array passed in
            if (arrays.Length == 0)
            {
                throw new ArgumentException("At least one array must be provided");
            }

            // Find the maximum length of the arrays
            int maxLength = arrays.Max(arr => arr.Length);

            // Create the result array with the same length
            double[] result = new double[maxLength];

            // Add elements from each array
            foreach (var array in arrays)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    result[i] += array[i];
                }
            }

            return result;
        }

    }

}

using System.Collections.Generic;

namespace Codenet.Collections;

public static class ListHelper
{
    public static List<T[]> Split<T>(IList<T> input, int splitCount)
    {
        int rows;
        List<T> column;
        List<T[]> columns = new List<T[]>();

        rows = input.Count / splitCount;

        if (splitCount * rows != input.Count)
        {
            rows++;
        }

        int index = 0;
        for (var matrixX = 0; matrixX < splitCount && index < input.Count; matrixX++)
        {
            column = new List<T>();
            for (var matrixY = 0; matrixY < rows && index < input.Count; matrixY++)
            {
                if (input.Count + matrixX + 1 - (index + splitCount) == 0 && matrixY != 0)
                {
                    continue;
                }

                column.Add(input[index]);
                index++;
            }
            columns.Add(column.ToArray());
        }

        return columns;
    }
}

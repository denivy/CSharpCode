/*
    https://www.hackerrank.com/challenges/candies
    solves with dynamic programming approach
*/

using System;
using System.IO;

class CandiesDP
{
    static void Main(String[] args)
    {
        System.IO.StreamReader file = new System.IO.StreamReader(@"file_path.txt");
        Console.SetIn(file);

        int numKids, next, answer, j;
        numKids = int.Parse(Console.ReadLine());
        int[] ratings = new int[numKids];
        int[] candy = new int[numKids];
        answer = 0;
        candy[0] = 1;
        for (int i = 0; i < numKids; i++)
            ratings[i] = int.Parse(Console.ReadLine());
        for (int i = 1; i < ratings.Length; i++)
        {//look at each childs ranking
            next = 0;
            j = i;
            while (j < ratings.Length - 1 && ratings[j] > ratings[j + 1])
            {
                next++;
                j++;
            }
            if (ratings[i] > ratings[i - 1])    //if current child has a better rating than previous child
                candy[i] = candy[i - 1] + 1;    //add one to previous
            else if (ratings[i] == ratings[i - 1]) candy[i] = 1;
            else
            {
                candy[i - 1] = Math.Max(candy[i - 1], next + 2);
                candy[i] = 1;
            }
        }//end for
        
        for (int i = 0; i < ratings.Length; i++)
        {
            //Console.Write(candy[i] + " + ");
            answer += candy[i];
        }//end for
        Console.WriteLine(answer);

        file.Close();
        throw new Exception();        //pause for debug
    }//end main
}//end class Candies DP

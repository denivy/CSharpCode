using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class AngryChildren
{
    static void Main(String[] args)
    {
        System.IO.StreamReader file = new System.IO.StreamReader(@"file_path_here.txt");
        Console.SetIn(file);
        
        List<long> sums = new List<long>();
        int numPackets, numKids;
        long diff;
        numPackets = int.Parse(Console.ReadLine());
        numKids = int.Parse(Console.ReadLine());
        Console.WriteLine("numKids=" + numKids);

        int[] packets = new int[numPackets];
        for (int i = 0; i < numPackets; i++)
        {//for each packet
            packets[i] = int.Parse(Console.ReadLine());
            //Console.WriteLine("packets[" + i + "]=" + packets[i]);
        }
        Array.Sort(packets);

        sums.Add(packets[0]);

        for (int i = 1; i < numPackets; i++)
        {
            diff = sums[i - 1] + packets[i];
            sums.Add(diff);
        }
        int value = 1 - numKids;
        long answer = 0;

        for (int i = 0; i < numKids; i++)
        {
            answer += value * packets[i];
            value += 2;
        }
        long finalAnswer = answer;
        for (int i = numKids; i < numPackets; i++)
        {
            long newAnswer = answer + (numKids - 1) * packets[i] + (numKids - 1) * packets[i - numKids] - 2 * (sums[i - 1] - sums[i - numKids]);
            finalAnswer = Math.Min(newAnswer, finalAnswer);
            answer = newAnswer;
        }
        Console.WriteLine(finalAnswer);
        
        file.Close();
    }
}

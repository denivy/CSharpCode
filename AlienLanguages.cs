using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class AlienLanguages
{
    static void Main(String[] args)
    {
        System.IO.StreamReader file = new System.IO.StreamReader(@"File_Path_Here.txt");
        Console.SetIn(file);

        int numCases, numLetters, wordLength, numWords;
        bool last;
        numCases = int.Parse(Console.ReadLine());

        for (int i = 0; i < numCases; i++) // for each case
        {
            string[] line = Console.ReadLine().Split();
            numLetters = int.Parse(line[0]);
            wordLength = int.Parse(line[1]);
            Console.WriteLine("numLetters=" + numLetters + ", wordLength=" + wordLength);
            //long[] word = new long[wordLength];
            long[] alphabet = new long[numLetters];
            List<int> possibleChoices;
            List<int>[] word = new List<int>[wordLength];
            for (int x = 0; x < numLetters; x++)
            {
                alphabet[x] = x + 1;
            }
            string AlphabetStr = "";
            for (int x = 0; x < numLetters; x++)
            {
                AlphabetStr += (x + 1) + ", ";
            }
            Console.WriteLine("'alphabet'=[" + AlphabetStr.Substring(0, AlphabetStr.Length - 2) + "]");
            numWords = 0;

            for (int x = 0; x < wordLength; x++)
            {//look at each letter in the word.
                possibleChoices = new List<int>();
                Console.Write("word[x={0}]=", x);
                for (int y = 1; y <= numLetters; y++)
                {//look at each letter of the alphabet and determine if it can be used in this position of the word.
                    if (y * 2 > numLetters)
                    {
                        Console.Write(y + " ");
                        possibleChoices.Add(y);
                        // contains[x,y] = true;       //this slot can contain this letter
                        //last = true;
                    }
                    else
                    {//this slot can contain only a smaller subset                                                
                        //contains[x, y] = false;
                        if (x == wordLength - 1)
                        {
                            Console.Write("!{0} ", y);
                        }
                        else if ((x - 1) >= 0)    //for all items except the first item
                        {
                            if (word[x - 1].Contains(y))    //
                            {
                                Console.Write("!{0} ", y);  //it cannot follow itself
                            }
                            else
                            {
                                Console.Write("{0} ", y);
                                possibleChoices.Add(y);
                            }
                        }
                        else
                        {
                            //its the first item
                            Console.Write("{0} ", y);
                            possibleChoices.Add(y);
                        }
                        //Console.Write("?y={0}?,", y);
                    }
                }
                word[x] = possibleChoices;
                Console.WriteLine();
            }
        }

        file.Close();
        throw new Exception();
    }

    public class Combination
    {
        private long n = 0;
        private long k = 0;
        private long[] data = null;

        public Combination(long n, long k)
        {
            if (n < 0 || k < 0) // normally n >= k
                throw new Exception("Negative parameter in constructor");

            this.n = n;
            this.k = k;
            this.data = new long[k];
            for (long i = 0; i < k; ++i)
                this.data[i] = i;
        } // Combination(n,k)

        public Combination(long n, long k, long[] a) // Combination from a[]
        {
            if (k != a.Length)
                throw new Exception("Array length does not equal k");

            this.n = n;
            this.k = k;
            this.data = new long[k];
            for (long i = 0; i < a.Length; ++i)
                this.data[i] = a[i];

            if (!this.IsValid())
                throw new Exception("Bad value from array");
        } // Combination(n,k,a)

        public bool IsValid()
        {
            if (this.data.Length != this.k)
                return false; // corrupted

            for (long i = 0; i < this.k; ++i)
            {
                if (this.data[i] < 0 || this.data[i] > this.n - 1)
                    return false; // value out of range

                for (long j = i + 1; j < this.k; ++j)
                    if (this.data[i] >= this.data[j])
                        return false; // duplicate or not lexicographic
            }

            return true;
        } // IsValid()

        public override string ToString()
        {
            string s = "{ ";
            for (long i = 0; i < this.k; ++i)
                s += this.data[i].ToString() + " ";
            s += "}";
            return s;
        } // ToString()

        public Combination Successor()
        {
            if (this.data[0] == this.n - this.k)
                return null;

            Combination ans = new Combination(this.n, this.k);

            long i;
            for (i = 0; i < this.k; ++i)
                ans.data[i] = this.data[i];

            for (i = this.k - 1; i > 0 && ans.data[i] == this.n - this.k + i; --i)
                ;

            ++ans.data[i];

            for (long j = i; j < this.k - 1; ++j)
                ans.data[j + 1] = ans.data[j] + 1;

            return ans;
        } // Successor()

        public static long Choose(long n, long k)
        {
            if (n < 0 || k < 0)
                throw new Exception("Invalid negative parameter in Choose()");
            if (n < k)
                return 0;  // special case
            if (n == k)
                return 1;

            long delta, iMax;

            if (k < n - k) // ex: Choose(100,3)
            {
                delta = n - k;
                iMax = k;
            }
            else         // ex: Choose(100,97)
            {
                delta = k;
                iMax = n - k;
            }

            long ans = delta + 1;

            for (long i = 2; i <= iMax; ++i)
            {
                checked { ans = (ans * (delta + i)) / i; }
            }

            return ans;
        } // Choose()
    } // Combination class
}


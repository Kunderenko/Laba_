using System;
using System.Threading.Tasks;

namespace laba2
{
    class Program
    {
        static void Main(string[] args)
        {
            Implementation imp = new Implementation();
            imp.Start();
        }
    }

    class Implementation
    {
        int n;
        public Matrix A1, A2, B2, C2, A, b1, c1, b, Y3, y1, y2, Y3squared, result;
        //матриці всіх обчисленнь для програми
        //(5b1 - c1), (B2 - 10C2), (Y3 * y2), (y1 * y2'), (y1' * Y3) etc
        Matrix tmp2_1, tmp2_2, tmp4_1, tmp4_2, tmp4_3, tmp5_1, tmp5_2, tmp5_3, tmp6_1, tmp6_2, tmp7_1;

        public void Start()
        {

            Console.WriteLine("Enter n: ");
            n = Convert.ToInt32(Console.ReadLine());
            firstLevel();
        }

        private void firstLevel()
        {
            b = new Matrix(n);
            var createb = Task.Factory.StartNew(() => { b.Createb(); });
            C2 = new Matrix(n);
            var createC2 = Task.Factory.StartNew(() => { C2.CreateC2(); });

            Console.WriteLine("Use random values? y/n");
            ConsoleKey response = Console.ReadKey(true).Key;

            if (response == ConsoleKey.Y)
            {
                A = new Matrix(n);
                A1 = new Matrix(n);
                A2 = new Matrix(n);
                B2 = new Matrix(n);
                b1 = new Matrix(n);
                c1 = new Matrix(n);
                Parallel.Invoke(
                    () => { A.CreateMatrix(); },
                    () => { A1.CreateMatrix(); },
                    () => { A2.CreateMatrix(); },
                    () => { B2.CreateMatrix(); },
                    () => { b1.CreateVector(); },
                    () => { c1.CreateVector(); }
                    );
            }
            else
            {
                Console.WriteLine("Write elements row-wise");
                A = new Matrix(n);
                A1 = new Matrix(n);
                A2 = new Matrix(n);
                B2 = new Matrix(n);
                b1 = new Matrix(n);
                c1 = new Matrix(n);
                A.FromKeyboard("A");
                A1.FromKeyboard("A1");
                A2.FromKeyboard("A2");
                B2.FromKeyboard("B2");
                b1.FromKeyboard("b1");
                c1.FromKeyboard("c1");
            }
            Core();
        }

        public void Core()
        {
            //level 2 -> (5b1 - c1), (B2 - 10C2), y1
            Parallel.Invoke(
                () => { tmp2_1 = (b1 * 5) + c1; },
                () => { tmp2_2 = B2 - (C2 * 10); },
                () => { y1 = A * b; }
                );

            //level 3 -> y2, Y3
            Parallel.Invoke(
                () => { y2 = A1 * tmp2_1; },
                () => { Y3 = A2 * tmp2_2; }
                );

            //level 4 -> Y3squared, tmp4_1 = (Y3 * y2), (y1 * y2'), (y1' * Y3)
            Parallel.Invoke(
                () => { Y3squared = Y3 * Y3; },
                () => { tmp4_1 = Y3 * y2; },
                () => { tmp4_2 = y1 * y2.Transposed(); },
                () => { tmp4_3 = y1.Transposed() * Y3; }
                );

            //level 5 -> tmp5_1 = (Y3squared * y2), (tmp4_1 + y1), (tmp4_3 * y1)
            Parallel.Invoke(
                () => { tmp5_1 = Y3squared * y2; },
                () => { tmp5_2 = tmp4_1 + y1; },
                () => { tmp5_3 = tmp4_3 * y1; }
                );

            //level 6 -> tmp6_1 = (tmp4_2 * tmp5_1), tmp6_2 = (tmp5_3 + y2)
            Parallel.Invoke(
                () => { tmp6_1 = tmp4_2 * tmp5_1; },
                () => { tmp6_2 = tmp5_3 + y2; }
                );

            //level 7 -> tmp7_1 = (tmp5_2 + tmp6_1)
            //без Parallel.Invoke тому що 1 завдання
            tmp7_1 = tmp5_2 + tmp6_1;

            //result
            result = tmp6_2 * tmp7_1;


            Final();
        }

        private void Final()
        {
            Console.WriteLine("Show all? y/n");
            ConsoleKey response = Console.ReadKey(true).Key;

            if (response == ConsoleKey.Y)
            {
                Console.WriteLine("If started");
                A1.ShowMatrix("A1");
                A2.ShowMatrix("A2");
                B2.ShowMatrix("B2");
                C2.ShowMatrix("C2");
                A.ShowMatrix("A");
                b1.ShowMatrix("b1");
                c1.ShowMatrix("c1");
                b.ShowMatrix("b");
                Y3.ShowMatrix("Y3");
                y1.ShowMatrix("y1");
                y2.ShowMatrix("y2");
                result.ShowMatrix("result");
                Console.WriteLine("If ended");
            }
            Console.WriteLine("Press key to exit");
            Console.ReadKey();
        }
    }


    public class Matrix
    {
        //масимальне ціле число яке може попасти в матрицю
        public static int MaxRnd = 10;
        int n;
        private double[,] matrix;

        public Matrix(int n)
        {
            if (n < 0) throw new Exception("Invalid size");
            this.n = n;
            matrix = new double[n, n];
        }

        public double this[int i, int j]
        {
            get
            {
                return matrix[i, j];
            }
            protected set
            {
                matrix[i, j] = value;
            }
        }

        public void ShowMatrix(string message)
        {
            Console.WriteLine("{0}:", message);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write(string.Format("{0} ", matrix[i, j]));
                }
                Console.WriteLine();
            }
        }

        public Matrix Transposed()
        {
            Matrix result = new Matrix(n);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result.matrix[i, j] = matrix[j, i];
                }
            }
            return result;
        }

        public void FromKeyboard(string name)
        {
            //Вводити рядково
            Console.WriteLine("Entering {0}", name);
            for (int i = 0; i < n; i++)
            {
                var values = (Console.ReadLine().Split(' '));
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = double.Parse(values[j]);
                }
            }
        }

        public void CreateMatrix()
        {
            Random rnd = new Random();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = rnd.Next(1, MaxRnd);
                }
            }
        }

        public void CreateVector()
        {
            Random rnd = new Random();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (j == 0)
                    {
                        matrix[i, j] = rnd.Next(0, MaxRnd);
                    }
                    else
                    {
                        matrix[i, j] = 0;
                    }

                }
            }
        }

        public void CreateC2()
        {

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = 1.0 / (Math.Pow(i + 1, 2) + (j + 1));
                }
            }
        }

        public void Createb()
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (j == 0)
                    {
                        matrix[i, j] = 5.0 * Math.Pow(i + 1, 3);
                    }
                    else
                    {
                        matrix[i, j] = 0;
                    }
                }
            }
        }

        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            if (m1.n != m2.n) return new Matrix(2);

            Matrix result = new Matrix(m1.n);

            for (int i = 0; i < m1.n; i++)
            {
                for (int j = 0; j < m1.n; j++)
                {
                    result[i, j] = m1[i, j] + m2[i, j];
                }
            }

            return result;
        }

        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            if (m1.n != m2.n) return new Matrix(2);

            Matrix result = new Matrix(m1.n);

            for (int i = 0; i < m1.n; i++)
            {
                for (int j = 0; j < m1.n; j++)
                {
                    result[i, j] = m1[i, j] - m2[i, j];
                }
            }

            return result;
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.n != m2.n) return new Matrix(2);

            Matrix result = new Matrix(m1.n);

            for (int i = 0; i < m1.n; i++)
            {
                for (int j = 0; j < m1.n; j++)
                {
                    double tmp = 0;
                    for (int k = 0; k < m1.n; k++)
                    {
                        tmp += m1[i, k] * m2[k, j];
                    }
                    result[i, j] = tmp;
                }
            }
            return result;
        }

        public static Matrix operator *(Matrix m1, double value)
        {
            Matrix result = new Matrix(m1.n);

            for (int i = 0; i < m1.n; i++)
            {
                for (int j = 0; j < m1.n; j++)
                {
                    double tmp = 0;
                    for (int k = 0; k < m1.n; k++)
                    {
                        tmp += m1[i, k] * value;
                    }
                    result[i, j] = tmp;
                }
            }
            return result;
           
        }
       
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace NBGAC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Start();


            //Config.Threshold = 0;

            //int step = 1000 / 30;


            //for (int i = 0; i <= 1000; i += step)
            //{
            //    Console.Clear();
            //    Console.WriteLine($"Generating treshold: {i}");
            //    Config.Threshold = i;
            //    Start();
            //}


        }

        public static void Start()
        {
            Console.WriteLine("Starting the job.");
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            var job = Engine.Start();

            if (job.IsCompleted)
            {
                stopwatch.Stop();
                var ticks = stopwatch.ElapsedTicks;
                Console.WriteLine("Tasks completed.");

                //foreach (var i in job.Result)
                //{
                //    Console.WriteLine($"Result: {i}");
                //}

                Console.WriteLine($"Ticks: {ticks}");

                Painter.Draw(job.Result);
            }
        }
    }

    

    public class Config
    {
        public static MyPoint PointA = new MyPoint(-2.0, 2.0);
        public static MyPoint PointB = new MyPoint(PointA.Y, PointA.X);
        public static int Size = 2800;
        public static double Threshold = 100.0;
        public static int Iterations = 20;
    }

    public struct MyPoint
    {
        public double X;
        public double Y;

        public MyPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public struct Cell
    {
        public int X;
        public int Y;
        public int Z;

        public Cell(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"Cell [x: {X}, y: {Y}, z: {Z}]";
        }
    }

    class Engine
    {
        private static Task<Cell> AtomicTask(int x, int y, Complex input)
        {
            Cell result = new Cell(x, y, 0);
            int z = Calculator.MainCalculate(input);

            result.Z = z;

            return Task.FromResult(result);
        }

        public static async Task<IEnumerable<Cell>> Start()
        {
            double step = Math.Abs(Config.PointA.X - Config.PointB.X) / Config.Size;

            List<Task<Cell>> listOfTasks = new List<Task<Cell>>();

            for (int i = 0; i < Config.Size; i++)
            {
                for (int j = 0; j < Config.Size; j++ )
                {
                    double x = i * step + Config.PointA.X;
                    //Math.Round(i * step + Config.PointA.X, 10, MidpointRounding.ToEven);
                    double y = j * step - Config.PointA.Y;
                    //Math.Round(j * step - Config.PointA.Y, 10, MidpointRounding.ToEven);

                    listOfTasks.Add(AtomicTask(i, j, new Complex(x, y)));
                }
            }
            Console.WriteLine("Tasks initiated - waiting for completion.");
            return await Task.WhenAll<Cell>(listOfTasks);
        }


    }

    class Calculator
    {
        public static int MainCalculate(Complex input)
        {
            Complex result = input;
            int iteration = -1;

            for (int i = 0; i < Config.Iterations; i++)
            {
                result = InnerCalculate(result);
                result = InnerIteration(result, 0.6, 0.8, 0.0 );

                if (MagnitudeCheck(result))
                {
                    if (Math.Abs(result.Real) < Config.Threshold || Math.Abs(result.Imaginary) < Config.Threshold)
                    {
                        return -1;
                    }

                    iteration = i;
                    break;
                }
            }

            return iteration;
        }

        private static Complex InnerCalculate(Complex input)
        {
            //Complex result = (input / input / input / input / input) * 0.25 + (input * input * input * input) * 0.60 + (input / input / input) * 1.25 + (input * input) * 0.75 + 0.5;

            Complex result = (input * input * input) + 0.5;
            return result;
        }

        private static Complex InnerIteration(Complex input, double alpha, double beta, double gamma)
        {
            Complex x = input;

            Complex z = input * (1.0 - gamma) + x * gamma;

            Complex y = z * (1 - alpha - beta) + z * alpha + x * beta;

            return y * (1 - alpha - beta) + (y * alpha) + (z * beta);
        }

        private static bool MagnitudeCheck(Complex input)
        {
            return Math.Abs(input.Real) + Math.Abs(input.Imaginary) > Config.Threshold;
        }
    }

    class Painter
    {
        public static void Draw(IEnumerable<Cell> cells)
        {
            Bitmap result = new Bitmap(Config.Size, Config.Size);
            int step = (int)Math.Round(255.0 / Config.Iterations);

            foreach (var c in cells)
            {
                int value = c.Z >= 0 ? c.Z : 0;
                value = value * step;
                //Color color = Color.FromArgb(value, value, value);

                //int red = value * 2 - 255;
                //int green = value;
                //int blue = 100 - value * 3;

                int red = value;
                int green = value;
                int blue = value * 2;

                red = red < 0 ? 0 : red > 255 ? 255 : red;
                green = green < 0 ? 0 : green > 255 ? 255 : green;
                blue = blue < 0 ? 0 : blue > 255 ? 255 : blue;

                Color color = Color.FromArgb(red, green, blue);
                result.SetPixel(c.X, c.Y, color);
            }

            result.Save($"biomorph_{DateTime.Now.Ticks}.bmp");
            
        }
    }
}

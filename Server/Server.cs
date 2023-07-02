using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    class Server
    {
        static bool MillerRabinTest(BigInteger n)
        {
            double K = BigInteger.Log(n, 2);
            int k = (int)K;

            if (n == 2 || n == 3)
                return true;

            if (n < 2 || n % 2 == 0)
                return false;

            BigInteger t = n - 1;

            int s = 0;

            while (t % 2 == 0)
            {
                t /= 2;
                s += 1;
            }

            for (int i = 0; i < k; i++)
            {
                //случайное целое число a в отрезке [2, n − 2]
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                byte[] _a = new byte[n.ToByteArray().LongLength];

                BigInteger a;

                do
                {
                    rng.GetBytes(_a);
                    a = new BigInteger(_a);
                }
                while (a < 2 || a >= n - 2);

                // x = a^t mod n, возведение в степень по модулю
                BigInteger x = BigInteger.ModPow(a, t, n);

                if (x == 1 || x == n - 1)
                    continue;

                for (int r = 1; r < s; r++)
                {
                    // x = x^2 mod n
                    x = BigInteger.ModPow(x, 2, n);

                    if (x == 1)
                        return false;

                    if (x == n - 1)
                        break;
                }

                if (x != n - 1)
                    return false;
            }
            // вернуть "вероятно простое"
            return true;
        }

        static BigInteger GeneratePrimeNumber()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            int sizeArr = 4;// 32 бит = 4 байта
            byte[] _a = new byte[sizeArr];
            BigInteger randNumb;

            rng.GetBytes(_a);
            randNumb = new BigInteger(_a);

            if (randNumb < 0)
            {
                randNumb = -randNumb;
            }

            if (randNumb % 2 == 0)
            {
                randNumb += 1;
            }
            while (MillerRabinTest(randNumb) == false)
            {
                randNumb += 2;
            }
            return randNumb;
        }

        static BigInteger CalculateKey(string data, BigInteger privateNumb)
        {
            string[] dataArr = data.Split(' ');
            BigInteger primeNumber = 0, publicNumber = 0;

            for (int i = 0; i < dataArr.Length; i++)
            {
                if (dataArr[i] == "publicNumber")
                    publicNumber = BigInteger.Parse(dataArr[++i]);
                if (dataArr[i] == "primeNumber")
                    primeNumber = BigInteger.Parse(dataArr[++i]);
            }
                
            return BigInteger.ModPow(publicNumber, privateNumb, primeNumber);
        }

        static BigInteger CalculatePublicNumb(string data, BigInteger privateNumb)
        {
            string[] dataArr = data.Split(' ');
            BigInteger primitiveRoot = 0, primeNumber = 0;

            for (int i = 0; i < dataArr.Length; i++)
            {
                if (dataArr[i] == "primitiveRoot")
                    primitiveRoot = BigInteger.Parse(dataArr[++i]);
                if (dataArr[i] == "primeNumber")
                    primeNumber = BigInteger.Parse(dataArr[++i]);
            }

            return BigInteger.ModPow(primitiveRoot, privateNumb, primeNumber);
        }

        static void Main(string[] args)
        {
            const string IP = "127.0.0.1";
            const int port = 8080;
            int numQueue = 5;


            var data = new StringBuilder();
            var tcpEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);

            //tcp-сокет в режиме ожидания
            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Bind(tcpEndPoint);
            tcpSocket.Listen(numQueue);


            BigInteger privateNumber = GeneratePrimeNumber();

            while (true)
            {
                var listener = tcpSocket.Accept();
                var buffer = new byte[256];
                var size = 0;

                do
                {
                    size = listener.Receive(buffer);
                    data.Append(Encoding.UTF8.GetString(buffer, 0, size));
                }
                while (tcpSocket.Available > 0);

                BigInteger numberB = CalculatePublicNumb(data.ToString(), privateNumber);
                
                listener.Send(Encoding.UTF8.GetBytes(numberB.ToString()));

                Console.WriteLine(CalculateKey(data.ToString(), privateNumber));

                data.Clear();
                listener.Shutdown(SocketShutdown.Both);
                listener.Close();


            }
        }
    }
}

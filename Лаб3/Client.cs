using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Лаб3
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
        }
        public bool MillerRabinTest(BigInteger n)
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
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                byte[] _a = new byte[n.ToByteArray().LongLength];

                BigInteger a;

                do
                {
                    rng.GetBytes(_a);
                    a = new BigInteger(_a);
                }
                while (a < 2 || a >= n - 2);

                BigInteger x = BigInteger.ModPow(a, t, n);

                if (x == 1 || x == n - 1)
                    continue;

                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, n);

                    if (x == 1)
                        return false;

                    if (x == n - 1)
                        break;
                }

                if (x != n - 1)
                    return false;
            }
            return true;
        }

        private BigInteger GeneratePrimeNumber()
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


        private void FindPrimeFactors(List<BigInteger> s, BigInteger n)
        {

            while (n % 2 == 0)
            {
                s.Add(2);
                n = n / 2;
            }


            for (int i = 3; i * i <= n; i = i + 2)
            {

                while (n % i == 0)
                {
                    s.Add(i);
                    n = n / i;
                }
            }

            if (n > 2)
            {
                s.Add(n);
            }
        }

        private BigInteger GeneratePrimitiveRoot(BigInteger primeNumber)
        {
            List<BigInteger> s = new List<BigInteger>();


            if (MillerRabinTest(primeNumber) == false)
            {
                return -1;
            }
            BigInteger phi = primeNumber - 1;


            FindPrimeFactors(s, phi);


            for (int r = 2; r <= phi; r++)
            {
                bool flag = false;
                foreach (int a in s)
                {

                    if (BigInteger.ModPow(r, phi / (a), primeNumber) == 1)
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag == false)
                {
                    return r;
                }
            }
            return -1;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            BigInteger privateNumber = rand.Next(0, 100000000);
            BigInteger primeNumber = GeneratePrimeNumber();
            BigInteger primitiveRoot = GeneratePrimitiveRoot(primeNumber);

            textBox1.Text = privateNumber.ToString();
            textBox2.Text = primeNumber.ToString();
            textBox3.Text = primitiveRoot.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            const string IP = "127.0.0.1";
            const int port = 8080;

            BigInteger privateNumber = BigInteger.Parse(textBox1.Text);
            BigInteger primeNumber = BigInteger.Parse(textBox2.Text);
            BigInteger primitiveRoot = BigInteger.Parse(textBox3.Text);

            string resultLine = "publicNumber " + FindPublicNumb(privateNumber, primeNumber, primitiveRoot) + 
                " primeNumber " + primeNumber + 
                " primitiveRoot " + primitiveRoot;

            var tcpEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);

            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var message = resultLine;
            var data = Encoding.UTF8.GetBytes(message);

            tcpSocket.Connect(tcpEndPoint);
            tcpSocket.Send(data);

            var buffer = new byte[256];
            var size = 0;
            var answer = new StringBuilder();

            do
            {
                size = tcpSocket.Receive(buffer);
                answer.Append(Encoding.UTF8.GetString(buffer, 0, size));
            }
            while (tcpSocket.Available > 0);

            textBox4.Text = CalculateKey(answer.ToString(), privateNumber, primeNumber).ToString();
        }

        private BigInteger CalculateKey(string answer, BigInteger privateNumber, BigInteger primeNumber)
        {
            return BigInteger.ModPow(BigInteger.Parse(answer), privateNumber, primeNumber);
        }


        private BigInteger FindPublicNumb(BigInteger privateNumber, BigInteger primeNumber, BigInteger primitiveRoot)
        {
            return BigInteger.ModPow(primitiveRoot, privateNumber, primeNumber);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RC4 newForm = new RC4();
            newForm.Show();
        }
    }
}

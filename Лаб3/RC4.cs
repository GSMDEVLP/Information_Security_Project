using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Лаб3
{
    public partial class RC4 : Form
    {
        public RC4()
        {
            InitializeComponent();
            textBox2.ScrollBars = ScrollBars.Vertical;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string key = textBox1.Text;
            string input = textBox2.Text;


            StringBuilder result = new StringBuilder();
            int x, y, j = 0;
            int[] box = new int[256];

            for (int i = 0; i < 256; i++)
            {
                box[i] = i;
            }

            for (int i = 0; i < 256; i++)
            {
                j = (key[i % key.Length] + box[i] + j) % 256;
                x = box[i];
                box[i] = box[j];
                box[j] = x;
            }

            for (int i = 0; i < input.Length; i++)
            {
                y = i % 256;
                j = (box[y] + j) % 256;
                x = box[y];
                box[y] = box[j];
                box[j] = x;

                result.Append((char)(input[i] ^ box[(box[y] + box[j]) % 256]));
            }


            textBox2.Text = result.ToString();
        }
    }
}

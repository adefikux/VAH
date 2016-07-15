using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tao.OpenGl;
using Tao.FreeGlut;
using Tao.Platform.Windows; 

namespace VAH
{
    public partial class Form1 : Form
    {
        List<float[]> points = new List<float[]>();
        public Form1()
        {
            InitializeComponent();

            AnT.InitializeContexts();
            // инициализация Glut 
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);

            Gl.glClearColor(0, 0, 0, 1); // отчитка окна 
            Gl.glViewport(0, 0, AnT.Width, AnT.Height); // установка порта вывода в соответствии с размерами элемента anT 
            //Gl.glViewport(-10f, 10f, 20f, 20f);

            // настройка проекции 
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            //Glu.gluOrtho2D(0.0, (float)AnT.Width, (float)AnT.Height, 0);
            Glu.gluOrtho2D(-5f, 5f, -0.0005f, 0.0005f);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();

            

            serialPort1.Open();
        }

        void log(string s)
        {
            listLog.Items.Add(s);
            listLog.SelectedIndex = listLog.Items.Count - 1;
            listLog.SelectedIndex = -1;
            while (listLog.Items.Count > 20)
                listLog.Items.RemoveAt(0);
        }

        void drawLine(double x0, double y0, double x1, double y1)
        {
            Gl.glBegin(Gl.GL_LINE_STRIP);
            Gl.glVertex2d(x0, y0);
            Gl.glVertex2d(x1, y1);
            Gl.glEnd();
        }

        private void incomingData(string data)
        {
            //log(data);
            List<string> ar = data.Split(':').ToList();
            if (ar.Count != 4)
                return;
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT); // очищаем буфер цвета 
            Gl.glLoadIdentity(); // очищаем текущую матрицу 

            Gl.glColor3f(1f, 1f, 1f);
            //drawLine(-1, 0, 1, 0);
            for (int i = -100; i < 100; i++)
            {
                float color = ((i % 10) == 0) ? 0.5f : 0.1f;
                if (0 == i)
                    color = 1f;
                Gl.glColor3f(color, color, color);
                drawLine((float)i / 10, -1, (float)i / 10, 1);
                drawLine(-10, (float)i / 100000, 10, (float)i / 100000);
            }
                //drawLine(0.5, -0.00005, 0.5, 0.00005);
                //drawLine(-0.5, -0.00005, -0.5, 0.00005);
            //drawLine(0, -0.0001, 0, 0.0001);



            //LOG(OCR2A);LOG(":");LOG(analogRead(0));LOG(":");LOG(analogRead(1));LOG(":");LOGLN(analogRead(2));
            float aref = 5, r1 = 10000;            
            float a0 = (float)Int16.Parse(ar[1]) * aref / 1024;
            float a1 = (float)Int16.Parse(ar[2]) * aref / 1024;
            float a2 = (float)Int16.Parse(ar[3]) * aref / 1024;
            float uD = a1 - a0;
            float iD = (a0 - a2) / r1;
            //log("uD = " + uD + " iD = " + iD);

            float[] point = {uD, iD};
            points.Add(point);

            float[] prevArr = null;
            foreach (float[] curArr in points)
            {
                if (null != prevArr)
                {
                    Gl.glColor3f(1f, 0f, 0f);
                    Gl.glBegin(Gl.GL_LINE_STRIP);
                    Gl.glVertex2d(prevArr[0], prevArr[1]);
                    Gl.glVertex2d(curArr[0], curArr[1]);
                    Gl.glEnd();
                }                
                prevArr = curArr;
            }
            
            Gl.glFlush();
            AnT.Invalidate();
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            Action<string> idata = incomingData;
            while(serialPort1.BytesToRead > 0)
                BeginInvoke(idata, serialPort1.ReadLine());
        }
    }
}

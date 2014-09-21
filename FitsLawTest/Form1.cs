using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestLib;

namespace FitsLawTest
{

    public partial class Form1 : Form
    {
        public FitsMistakesTest currentTest;        
        public readonly int defaultDistance = FitsLawTestValues.ButtonDistances[4];
        public readonly int defaultSize = FitsLawTestValues.ButtonSizes[2];
        public readonly Random rand = new Random();
        public ITestHelper currentHelper;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (currentTest == null)
                return;

            if(e.Button == MouseButtons.Left && currentTest.IsTrying)
            {
                currentTest.FinishTry(false);
                
            }
            else if(e.Button == MouseButtons.Right && !currentTest.IsTrying)
            {
                SetupMouseAndButton();
                currentTest.StartTry();                
            }
        }

        private void startSizeTestClick(object sender, EventArgs e)
        {
            startTest(new FitsMistakesTest(FitsLawTestValues.ButtonSizes), new SizeTestHelper());
        }

        private void startDistanceTestClick(object sender, EventArgs e)
        {
            startTest(new FitsMistakesTest(FitsLawTestValues.ButtonDistances), new DistanceTestHelper());
        }


        public void startTest(FitsMistakesTest test, ITestHelper helper)
        {
            button1.Hide();
            button3.Hide();

            if (currentTest != null)
            {
                currentTest.OnTestCompleted -= currentTest_OnTestCompleted;
                currentTest.OnTestValueUpdated -= currentTest_OnTestValueUpdated;
                currentTest.OnTryFailed -= currentTest_OnTryFailed;
            }

            this.Text = helper.TestName;

            currentTest = test;
            currentHelper = helper;
            
            currentTest.OnTestCompleted += currentTest_OnTestCompleted;
            currentTest.OnTestValueUpdated += currentTest_OnTestValueUpdated;
            currentTest.OnTryFailed += currentTest_OnTryFailed;
        }

        void currentTest_OnTryFailed()
        {
            MessageBox.Show("Слишком медленно, попробуйте быстрее. Начните новую попытку");
        }

        void currentTest_OnTestValueUpdated(int newValue)
        {
            updateButton();
        }

        void updateButton()
        {
            currentHelper.SetupButtonPosition(this, pictureBox1);
            currentHelper.SetupButtonSize(this, pictureBox1);
        }

        void currentTest_OnTestCompleted(List<TryResult<int>> testResults)
        {
            string fileName = currentHelper.TestName + ".txt";
            Utils.SaveResults<int>(fileName, testResults);

            MessageBox.Show("Тест завершен. Результаты сохранены");
            button1.Show();
            button3.Show();
        }

        
        private void SetupMouseAndButton()
        {
            Cursor.Position = this.PointToScreen(new Point());
            updateButton();            
        }
        
        public Point GetRandPosition(int radius)
        {
            Point p = new Point();
            double angle = rand.NextDouble() * Math.PI / 2;
            p.X = (int)(radius * Math.Cos(angle));
            p.Y = (int)(radius * Math.Sin(angle));
            return p;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (currentTest != null)
                currentTest.FinishTry(true);
        }

    }

    public interface ITestHelper
    {
        void SetupButtonPosition(Form1 form, Control btn);
        void SetupButtonSize(Form1 form, Control btn);

        string TestName { get; }
    }

    public class SizeTestHelper : ITestHelper
    {
        public void SetupButtonPosition(Form1 form, Control btn)
        {
            btn.Location = form.GetRandPosition(form.defaultDistance);
        }
        public void SetupButtonSize(Form1 form, Control btn)
        {
            int value = form.currentTest.TestValue * 8;
            btn.Size = new Size(value, value);
        }
        public string TestName
        {
            get { return "Тест с изменяющейся кнопкой и дистанцией постоянного размера"; }
        }
    }

    public class DistanceTestHelper : ITestHelper
    {

        public void SetupButtonPosition(Form1 form, Control btn)
        {
            btn.Location = form.GetRandPosition(form.currentTest.TestValue);
        }
        public void SetupButtonSize(Form1 form, Control btn)
        {
            int size = form.defaultSize * 8;
            btn.Size = new Size(size, size);
        }        
        public string TestName
        {
            get { return "Тест с кнопкой постоянного размера и изменяющейся дистанцией"; }
        }
    }
}
